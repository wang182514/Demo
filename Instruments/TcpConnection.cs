// ============================================================
// TcpConnection — 基于 TCP Socket 的 SCPI 通信通道
//
// 封装原生 Socket 操作：三次握手、ASCII 编码、SCPI 终止符、
// 循环读取（解决 TCP 分包问题）、超时处理。
//
// 连接参数（IP / 端口 / 超时）在构造时传入，后续 Connect() 无参。
// 使用者无需关心 TCP 细节，只需调 Write/Query 收发 SCPI 命令。
// ============================================================

using System.Net.Sockets;
using System.Text;
using Demo.Instruments.Abstractions;

namespace Demo.Instruments;

public class TcpConnection : IScpiConnection
{
    // ---- 连接参数（构造时传入，不可变）----
    private readonly string _ip;
    private readonly int _port;
    private readonly int _timeoutMs;

    // ---- 运行时状态 ----
    private TcpClient? _tcp;
    private NetworkStream? _stream;
    private string _lastError = "";
    private bool _disposed;

    // ============================================================
    // IScpiConnection 属性
    // ============================================================

    public int WriteDelayMs { get; set; } = 30;
    public int ReadDelayMs { get; set; } = 0;

    public string LastError => _lastError;

    // ============================================================
    // 构造 — 连接参数在这里，不在 Connect()
    // ============================================================


    /// <summary>
    /// <param name="ip">仪器 IP 地址，如 "192.168.1.11"</param>
    /// <param name="port">TCP 端口号，电源默认 2268，频谱仪/信号源默认 5025</param>
    /// <param name="timeoutMs">收发超时 (ms)</param>
    /// </summary>
    public TcpConnection(string ip, int port, int timeoutMs = 3000)
    {
        _ip = ip;
        _port = port;
        _timeoutMs = timeoutMs;
    }

    // ============================================================
    // Connect — 使用构造时传入的参数，不接收运行时参数
    // ============================================================

    /// <summary>
    /// 建立 TCP 连接（三次握手）。
    /// IP、端口、超时在构造时已确定，此处无参。
    /// </summary>
    public void Connect()
    {
        // 先释放旧连接，防止重复连接资源泄漏
        Disconnect();
        _lastError = "";

        // ① 创建 TCP 客户端，设置收发超时
        _tcp = new TcpClient
        {
            ReceiveTimeout = _timeoutMs,
            SendTimeout = _timeoutMs
        };

        // ② TCP 三次握手：SYN → SYN+ACK → ACK
        _tcp.Connect(_ip, _port);

        // ③ 获取全双工数据流
        _stream = _tcp.GetStream();
    }


    // Disconnect — 释放 TCP 资源

    /// <summary>关闭网络流和 TCP 连接，置空引用帮助 GC 回收</summary>
    public void Disconnect()
    {
        _stream?.Close();
        _stream = null;
        _tcp?.Close();
        _tcp = null;
    }


    // 退出程序时调用gc,防止资源泄露

    public void Dispose()
    {
        if (!_disposed)
        {
            Disconnect();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    // ============================================================
    // Write — 发送 SCPI 命令（只发不收）
    // ============================================================

    /// <summary>
    /// <para>发送 SCPI 命令。</para>
    /// <para>自动补 \n 终止符，发送后等待 <see cref="WriteDelayMs"/> ms。</para>
    /// </summary>
    public void Write(string cmd)
    {
        if (_tcp == null || _stream == null)
            throw new InvalidOperationException("仪器未连接");

        // SCPI 命令以换行符 \n 结尾
        if (!cmd.EndsWith('\n'))
            cmd += '\n';

        // 字符串 → ASCII 字节数组 → 写入网络流
        byte[] data = Encoding.ASCII.GetBytes(cmd);
        _stream!.Write(data, 0, data.Length);

        // 等待仪器处理（不同型号响应速度不同）
        if (WriteDelayMs > 0)
            Thread.Sleep(WriteDelayMs);
    }

    // ============================================================
    // Query — 发送查询，循环读取完整响应
    // ============================================================

    /// <summary>
    /// <para>发送 SCPI 查询命令，循环读取直到 \n 终止符。</para>
    /// <para>TCP 是流协议，响应可能分多个包——循环读到 \n 保证完整性。</para>
    /// <para>超时或断连时返回已读部分，LastError 记录原因（不抛异常）。</para>
    /// </summary>
    public string Query(string cmd)
    {
        Write(cmd);

        // 等待仪器处理命令并准备回复
        if (ReadDelayMs > 0)
            Thread.Sleep(ReadDelayMs);

        var sb = new StringBuilder();
        var buf = new byte[4096];

        try
        {
            int bytesRead;
            do
            {
                bytesRead = _stream!.Read(buf, 0, buf.Length);
                if (bytesRead > 0)
                    sb.Append(Encoding.ASCII.GetChars(buf, 0, bytesRead));
            } while (bytesRead > 0 && !sb.ToString().Contains('\n'));
        }
        catch (Exception ex)
        {
            // 超时 / 断连 → 记录错误，返回已读部分
            _lastError = ex.Message;
        }

        return sb.ToString().Trim();
    }

    // ============================================================
    // ReadRaw — 读取 IE488.2 二进制块（用于截图、波形等二进制数据）
    // ============================================================
    //
    // 【为什么需要这个方法？】
    //
    // 普通的 Query() 靠读到 \n 判断一条响应结束。
    // 但截图是 PNG 二进制数据——里面到处都是 0x0A (= \n) 字节。
    // 如果用 Query() 收，读到第一个 0x0A 就停，PNG 只剩几十字节的文件头。
    //
    // IE488.2 的解决方案：在数据前面加一个"长度头"，
    // 接收方先读长度，再按长度精准读取 N 个字节——不管中间有没有 \n。
    //
    // 【IE488.2 二进制块格式】
    //
    //    # <digit> <count> <data> \n
    //    │    │       │        │    └─ 块结束符 (0x0A)
    //    │    │       │        └────── 纯二进制数据，共 count 个字节
    //    │    │       └─────────────── 十进制数字 = data 的字节数
    //    │    └────────────────────── 1 位数字 = count 占几位
    //    └─────────────────────────── 固定起始符
    //
    // 【具体例子】假如图片是 800123 字节 (≈781 KB)
    //
    //    仪器返回的字节流:
    //      #    6   8  0  0  1  2  3   [ 800123 个 PNG 字节 ]   \n
    //     0x23 0x36 ...                                  ...    0x0A
    //      ↑    ↑   └───── 6 个字符 ──────┘                    │
    //     固定   '6' = "后面用6位数字来描述数据长度"            块结束
    //     起始   "800123" → 数据体有 800123 字节
    //
    // 【解析步骤】下面逐行拆解

    /// <summary>
    /// <para>发送查询命令，读取 IE488.2 二进制块响应，返回纯净数据（已剥除头尾）。</para>
    /// <para>出错返回空数组，<see cref="LastError"/> 记录原因。</para>
    /// </summary>
    public byte[] ReadRaw(string cmd)
    {
        Write(cmd);  // 先发查询命令（如 :MMEM:DATA? "tmp.png"）

        try
        {
            // ── 步骤①: 读 '#' (0x23) 确认这是 IE488.2 二进制块 ──
            // 如果不是，说明仪器返回的不是二进制数据（可能命令错误）
            int b = _stream!.ReadByte();
            if (b != '#')
            {
                _lastError = "ReadRaw: 缺少 # 头";
                return Array.Empty<byte>();
            }

            // ── 步骤②: 读下 1 个字节，得到"count 占几位" ──
            // 这个字节是 ASCII 数字字符，比如 '6' (0x36)
            // '6' - '0' = 6 → count 字符串长度 = 6 位
            b = _stream.ReadByte();
            int digits = b - '0';
            if (digits < 1 || digits > 9)
            {
                _lastError = "ReadRaw: 无效的数字位";
                return Array.Empty<byte>();
            }

            // ── 步骤③: 读 count 字符串（digits 位），解析出数据字节数 ──
            // 例: digits=6 → 读 "800123" → byteCount = 800123
            // 这里用 ReadExactly 而非普通 Read——count 字符串很短（≤9字节），
            // TCP 不会拆分它，一次读够即可
            var countBuf = new byte[digits];
            _stream.ReadExactly(countBuf, 0, digits);
            int byteCount = int.Parse(Encoding.ASCII.GetString(countBuf));

            // ── 步骤④: 按 byteCount 精准读取全部二进制数据 ──
            // 用 while 而非单次 Read——800KB 的数据 TCP 必然会分多个包发送，
            // 每次 Read 可能只拿到几 KB，需要循环拼接直到收够 byteCount 字节。
            // 返回 0 表示对方关闭连接，提前结束。
            var data = new byte[byteCount];
            int total = 0;
            while (total < byteCount)
            {
                int n = _stream.Read(data, total, byteCount - total);
                if (n == 0) break;
                total += n;
            }

            // ── 步骤⑤: 跳过块结尾的 \n ──
            // 按 IE488.2 规范，二进制块以 \n (0x0A) 结尾。
            // 读到这行时数据已在 data[] 中，这个 \n 不属于 PNG 内容，丢弃。
            _stream.ReadByte();

            // ── 返回纯净数据 ──
            // # 头、count 字符串、结尾 \n 都已剥离，data[] 是纯净的 PNG 字节。
            // 调用方直接 File.WriteAllBytes() 即可得到可用的图片文件。
            return data;
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
            return Array.Empty<byte>();
        }
    }
}

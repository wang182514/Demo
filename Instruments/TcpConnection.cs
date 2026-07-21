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

    /// <param name="ip">仪器 IP 地址，如 "192.168.1.11"</param>
    /// <param name="port">TCP 端口号，电源默认 2268，频谱仪/信号源默认 5025</param>
    /// <param name="timeoutMs">收发超时 (ms)</param>
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

    // ============================================================
    // Disconnect — 释放 TCP 资源
    // ============================================================

    /// <summary>关闭网络流和 TCP 连接，置空引用帮助 GC 回收</summary>
    public void Disconnect()
    {
        _stream?.Close();
        _stream = null;
        _tcp?.Close();
        _tcp = null;
    }

    // ============================================================
    // Dispose — 最终清理
    // ============================================================

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
}

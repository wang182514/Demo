// ============================================================
// GWINSTEK PSW20-27E 可编程直流电源（TCP SCPI 端口 2268）
// 
// 【通信原理概述】
// 仪器后面板有网口，插网线连到电脑。电脑通过 TCP/IP 协议
// 向仪器发送 SCPI 文本命令，仪器处理后返回文本结果。
// 整个过程和浏览器访问网页是一样的底层协议（TCP）。
// ============================================================

using System.Net.Sockets;   // TcpClient, NetworkStream 在这里
using System.Text;           // Encoding 在这里
using Demo.Instruments.Abstractions;

namespace Demo.Instruments;

public class GwInstekPsw : IPowerSupply
{
    // ---- 连接参数（创建对象时指定，之后不可变）----
    private readonly string _ip;          // 仪器 IP 地址，如 "192.168.1.11"
    private readonly int _port;           // TCP 端口号，电源默认 2268
    private readonly int _timeoutMs;      // 超时时间（毫秒）

    // ---- 运行时状态 ----
    // TcpClient? 的 ? 表示"可以为 null"
    // 在调用 Connect() 之前，_tcp 就是 null
    private TcpClient? _tcp;              // TCP 客户端对象（.NET 封装好的 Socket）
    private NetworkStream? _stream;       // 网络数据流（从 TcpClient 获取）
    private string _idn = "";             // 仪器标识字符串（*IDN? 的返回值）
    private string _lastError = "";       // 最后一次错误信息
    private bool _disposed;               // 是否已释放资源

    /// <summary>
    /// 构造函数 — 只是保存参数，不建立连接
    /// </summary>
    /// <param name="ip">仪器 IP 地址</param>
    /// <param name="port">TCP 端口号，电源默认 2268</param>
    /// <param name="timeoutSec">超时秒数</param>
    public GwInstekPsw(string ip, int port = 2268, double timeoutSec = 1.0)
    {
        _ip = ip; _port = port; _timeoutMs = (int)(timeoutSec * 1000);
    }

    // ---- 公开属性（外部只读）----
    // => 是表达式体语法，等价于 get { return _idn; }
    public string Idn => _idn;

    /// <summary>
    /// 判断是否已连接
    /// _tcp?.Connected 的 ?. 是"null 条件运算符"：
    /// 如果 _tcp 是 null，直接返回 null（不会崩溃）
    /// ?? false 的 ?? 是"null 合并运算符"：
    /// 如果左边是 null，返回右边的值 false
    /// 合起来：_tcp 为 null 或已断开 → 返回 false
    /// </summary>
    public bool IsConnected => _tcp?.Connected ?? false;

    public string LastError => _lastError;

    // ============================================================
    // Connect — 三步建立 TCP 连接
    // ============================================================

    /// <summary>
    /// <para>连接仪器并返回 IDN 标识</para>
    /// <para>
    /// 步骤：
    /// 1. new TcpClient()  — 创建 TCP 客户端（类似"拿起电话"）
    /// 2. _tcp.Connect()   — 向仪器 IP:端口发起连接（"拨号"）
    /// 3. GetStream()      — 获取数据流（"电话接通后的通话线路"）
    /// 4. Query("*IDN?")   — 发送身份查询命令，确认通讯正常
    /// </para>
    /// <para>
    /// 如果连接失败（网络不通、IP 错误等），Connect() 会抛出异常，
    /// 由调用方（Form1.cs 的 TryConnect）捕获并显示红色状态灯
    /// </para>
    /// </summary>
    public string Connect()
    {
        // 先断开旧连接（如果有的话），防止重复连接
        Disconnect();
        _lastError = "";

        // ① 创建 TCP 客户端
        _tcp = new TcpClient();

        // ② 设置超时（单位毫秒）
        // ReceiveTimeout：读取数据时，超过这个时间没收到就抛异常
        // SendTimeout：发送数据时，超过这个时间发不出去就抛异常
        _tcp.ReceiveTimeout = _timeoutMs;
        _tcp.SendTimeout = _timeoutMs;

        // ③ 发起连接 —— 关键步骤
        // 这就是 TCP 的"三次握手"：客户端发 SYN → 服务器回 SYN+ACK → 客户端回 ACK
        // 如果 IP 不对或仪器没开机，这里会卡住直到超时
        _tcp.Connect(_ip, _port);

        // ④ 获取网络流（NetworkStream 是对 Socket 的封装，提供 Read/Write 方法）
        _stream = _tcp.GetStream();

        // ⑤ 发送 *IDN? 查询仪器身份，确认通信正常
        _idn = Query("*IDN?");
        return _idn;
    }

    /// <summary>
    /// 断开连接并释放网络资源
    /// _stream?.Close() 的 ?. 表示"不为 null 才调用"
    /// </summary>
    public void Disconnect()
    {
        _stream?.Close();      // 关闭数据流
        _stream = null;        // 释放引用（帮助 GC 回收）
        _tcp?.Close();         // 关闭 TCP 连接
        _tcp = null;
        _idn = "";
    }

    /// <summary>实现 IDisposable 接口 — 用 using 语句时会自动调用</summary>
    public void Dispose()
    {
        if (!_disposed) { Disconnect(); _disposed = true; }
    }

    // ============================================================
    // 电源特有操作
    // ============================================================

    /// <summary>开启/关闭电源输出。发送后等 200ms 让仪器执行。</summary>
    public void SetOutput(bool on)
    {
        // SCPI 命令格式：:OUTP 1（开）或 :OUTP 0（关）
        Send($"OUTP {(on ? "1" : "0")}");
        // 电源从收到命令到实际动作需要时间，等待 200 毫秒确保完成
        Thread.Sleep(200);
    }

    /// <summary>测量当前实际电压（伏特）</summary>
    public double MeasureVoltage()
    {
        var resp = Query("MEAS:VOLT?");

        // double.TryParse 安全解析：
        // 成功 → v 有值，返回 v
        // 失败（仪器返回乱码）→ 返回 double.NaN（Not a Number）
        return double.TryParse(resp, out var v) ? v : double.NaN;
    }

    /// <summary>测量当前实际电流（安培）</summary>
    public double MeasureCurrent()
    {
        var resp = Query("MEAS:CURR?");
        return double.TryParse(resp, out var v) ? v : double.NaN;
    }

    /// <summary>设置目标电压（伏特）。仪器会自动调节到设定值。</summary>
    public void SetVoltage(double volts) => Send($"SOUR:VOLT {volts:F3}");

    /// <summary>设置电流上限（安培）。超过此值仪器会自动限流保护。</summary>
    public void SetCurrent(double amps) => Send($"SOUR:CURR {amps:F3}");

    // ============================================================
    // 底层 SCPI 通信方法（私有，外部不可见）
    // ============================================================

    /// <summary>
    /// <para>发送 SCPI 命令（只发不收）</para>
    /// <para>
    /// TCP 只认"字节"（byte[]），不认"字符串"
    /// Encoding.ASCII.GetBytes() 把字符串转成字节数组（每个字符一个字节）
    /// 例："*IDN?\n" → [42, 73, 68, 78, 63, 10]
    /// </para>
    /// </summary>
    private void Send(string cmd)
    {
        // 未连接时抛出明确的异常
        if (_tcp == null || !_tcp.Connected)
        {
            //等价判断表达式
            // _tcp?.Connected != true
            // _tcp?.Connected is not true
            throw new InvalidOperationException("电源未连接");
        }

        // SCPI 命令以换行符 \n 结尾（仪器靠它判断"命令结束了"）

        if (!cmd.EndsWith('\n'))
            cmd += '\n';

        // 转字节数组
        byte[] data = Encoding.ASCII.GetBytes(cmd);

        // 写字节到网络流（TCP 保证数据完整送达，底层自动分包重传）
        // _stream! 的 ! 是 null 容错操作符：
        // "我确定 _stream 不是 null，别报警告了"
        _stream!.Write(data, 0, data.Length);
    }

    /// <summary>
    /// 发送查询命令并读取回复（SCPI 经典三步）
    /// 
    /// 步骤：
    /// ① Send(cmd)       — 发命令
    /// ② Thread.Sleep    — 等仪器处理（仪器不是瞬间回复的）
    /// ③ 循环 Read       — 从网络流读字节直到收到换行符 \n
    /// 
    /// 为什么循环读？TCP 是"流"协议，数据可能分几次到达。
    /// 仪器回复以 \n 结束，读到 \n 就说明这条回复完整了。
    /// </summary>
    private string Query(string cmd)
    {
        Send(cmd);

        // 等待 50ms — 给仪器一点时间处理命令并准备回复
        Thread.Sleep(50);

        // 缓冲区：每次最多读 4096 字节（4KB）
        byte[] buffer = new byte[4096];

        // StringBuilder 用于拼接多次读取的结果
        // 比 string += 效率高（string 每次 += 都会创建新对象）
        StringBuilder sb = new StringBuilder();

        try
        {
            int bytesRead;
            do
            {
                // _stream.Read(缓冲区, 起始位置, 最大长度) → 返回实际读到的字节数
                // 返回 0 表示对方关闭了连接
                bytesRead = _stream!.Read(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    // 把字节转成字符并追加到 StringBuilder
                    // GetChars 比 GetString 更高效（不需要每次创建新字符串）
                    sb.Append(Encoding.ASCII.GetChars(buffer, 0, bytesRead));
                }
                // 读到换行符 \n 就停 —— 这条回复完整了
            } while (bytesRead > 0 && !sb.ToString().Contains('\n'));
        }
        catch (IOException)
        {
            // 超时或连接断开 → 忽略异常，返回已经读到的部分
            // 这不是致命错误，调用方会处理空字符串的情况
        }

        // Trim() 去掉首尾空格和换行符
        return sb.ToString().Trim();
    }
}

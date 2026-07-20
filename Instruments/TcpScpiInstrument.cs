// ============================================================
// TcpScpiInstrument — TCP SCPI 仪器抽象基类
// 封装 TCP 连接、SCPI 文本命令收发、资源释放。
// 子类只需声明端口和仪器特有命令，无需重复 Write/Query/Connect。
// ============================================================

using System.Net.Sockets;
using System.Text;
using Demo.Instruments.Abstractions;

namespace Demo.Instruments;

public abstract class TcpScpiInstrument : IInstrument
{
    // ---- 构造参数 ----
    private readonly string _ip;
    private readonly int _port;
    private readonly int _timeoutMs;

    // ---- 运行时状态 ----
    private TcpClient? _tcp;
    private NetworkStream? _stream;
    private string _idn = "";
    private string _lastError = "";
    private bool _disposed;

    // ============================================================
    // 子类可覆盖的配置点
    // ============================================================

    /// <summary>每次 Write 后等待仪器消化命令的时间 (ms)</summary>
    protected virtual int WriteDelayMs => 30;

    /// <summary>Query 中 Write 完成后、开始 Read 前的额外等待 (ms)</summary>
    protected virtual int ReadDelayMs => 0;

    /// <summary>TCP 连接建立后、发送 *IDN? 前的初始化钩子</summary>
    protected virtual void OnConnected() { }

    // ============================================================
    // 构造
    // ============================================================

    protected TcpScpiInstrument(string ip, int port, int timeoutMs = 3000)
    {
        _ip = ip;
        _port = port;
        _timeoutMs = timeoutMs;
    }

    // ============================================================
    // IInstrument 属性
    // ============================================================

    public string Idn => _idn;
    public bool IsConnected => _tcp?.Connected ?? false;
    public string LastError => _lastError;

    // ============================================================
    // Connect / Disconnect / Dispose
    // ============================================================

    public virtual string Connect()
    {
        // 先断开旧连接，防止重复连接资源泄漏
        Disconnect();
        _lastError = "";

        _tcp = new TcpClient
        {
            ReceiveTimeout = _timeoutMs,
            SendTimeout = _timeoutMs
        };
        _tcp.Connect(_ip, _port);
        _stream = _tcp.GetStream();

        // 子类初始化钩子（如发送 *CLS 清空状态寄存器）
        OnConnected();

        // 查询仪器身份，确认通信正常
        _idn = Query("*IDN?");
        return _idn;
    }

    public virtual void Disconnect()
    {
        _stream?.Close();
        _stream = null;
        _tcp?.Close();
        _tcp = null;
        _idn = "";
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Disconnect();
            _disposed = true;
        }
    }

    // ============================================================
    // 底层 SCPI 通信（protected，子类直接调用）
    // ============================================================

    /// <summary>发送 SCPI 命令（只发不收），自动补 \n</summary>
    protected void Write(string cmd)
    {
        if (_tcp == null || !_tcp.Connected)
            throw new InvalidOperationException("仪器未连接");

        if (!cmd.EndsWith('\n'))
            cmd += '\n';

        _stream!.Write(Encoding.ASCII.GetBytes(cmd));

        if (WriteDelayMs > 0)
            Thread.Sleep(WriteDelayMs);
    }

    /// <summary>发送查询命令，循环读取直到 \n 终止符，返回去首尾空白后的字符串</summary>
    protected string Query(string cmd)
    {
        Write(cmd);

        if (ReadDelayMs > 0)
            Thread.Sleep(ReadDelayMs);

        var sb = new StringBuilder();
        var buf = new byte[4096];
        try
        {
            int n;
            do
            {
                n = _stream!.Read(buf, 0, buf.Length);
                if (n > 0) sb.Append(Encoding.ASCII.GetChars(buf, 0, n));
            } while (n > 0 && !sb.ToString().Contains('\n'));
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
        }
        return sb.ToString().Trim();
    }
}

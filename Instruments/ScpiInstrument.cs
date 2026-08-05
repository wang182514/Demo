// ============================================================
// ScpiInstrument — SCPI 仪器抽象基类（组合模式）
//
// 核心设计：仪器"持有"通信连接，而非"是"通信连接。
//   基类通过 IScpiConnection 委托所有底层 I/O，
//   子类只关注仪器特有 SCPI 命令，不感知 TCP/串口差异。
//
// 三个虚钩子供子类按需覆盖：
//   OnConnected()  — 连接建立后的初始化（如发 *CLS）
//   WriteDelayMs   — Write 后等待时间 (ms)，默认 30
//   ReadDelayMs    — Query 读前额外等待 (ms)，默认 0
//
// 新增非 TCP 仪器时：
//   1. 新建连接实现类 (如 SerialConnection : IScpiConnection)
//   2. 新型号继承本基类，构造注入新连接
//   3. 现有仪器代码零改动
// ============================================================

using Demo.Instruments.Abstractions;

namespace Demo.Instruments;

public abstract class ScpiInstrument : IInstrument
{
    // ---- 通信通道（组合，外部注入）----
    private readonly IScpiConnection _connection;

    private string _idn = "";
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

    /// <summary>
    /// 便捷构造：内部创建 TcpConnection。
    /// 适用于绝大多数场景——只要仪器走 TCP，子类传 IP + 端口即可。
    /// </summary>
    /// <param name="ip">仪器 IP 地址</param>
    /// <param name="port">TCP 端口号</param>
    /// <param name="timeoutMs">收发超时 (ms)</param>
    protected ScpiInstrument(string ip, int port, int timeoutMs = 3000)
        : this(new TcpConnection(ip, port, timeoutMs))
    {
    }

    /// <summary>
    /// 注入自定义连接实现。
    /// 用于 RS232/USB 等非 TCP 场景，或需要自行配置 TcpConnection 时。
    /// </summary>
    protected ScpiInstrument(IScpiConnection connection)
    {
        _connection = connection;

        // 将子类的延迟配置传给连接层
        _connection.WriteDelayMs = WriteDelayMs;
        _connection.ReadDelayMs = ReadDelayMs;
    }

    // ============================================================
    // IInstrument 属性 — 转发给连接层
    // ============================================================

    public string Idn => _idn;
    public string LastError => _connection.LastError;

    // ============================================================
    // Connect / Disconnect / Dispose
    // ============================================================

    /// <summary>
    /// 建立连接并查询仪器身份。
    /// 连接参数已在连接对象构造时确定，此处无参。
    /// </summary>
    public virtual string Connect()
    {
        Disconnect();
        _connection.Connect();

        // 子类初始化（如 Keysight N9020A 发 *CLS 清空状态寄存器）
        OnConnected();

        _idn = Query("*IDN?");
        return _idn;
    }

    /// <summary>断开连接，关闭底层传输通道</summary>
    public virtual void Disconnect()
    {
        _connection.Disconnect();
        _idn = "";
    }

    /// <summary>释放所有资源（关闭连接 + 抑制 Finalizer）</summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _connection.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    // ============================================================
    // Write / Query — 子类通过这两个方法发送 SCPI 命令
    // ============================================================

    /// <summary>发送 SCPI 命令（只发不收），自动补终止符</summary>
    protected void Write(string cmd) => _connection.Write(cmd);

    /// <summary>
    /// 发送查询命令并读取完整响应。
    /// 底层处理分包重组、超时、终止符判断。
    /// 异常不向上抛——出错时返回已读部分，LastError 记录原因。
    /// </summary>
    protected string Query(string cmd) => _connection.Query(cmd);

    /// <summary>发送查询命令，读取原始字节响应（IE488.2 二进制块，用于截图等）</summary>
    protected byte[] ReadRaw(string cmd) => _connection.ReadRaw(cmd);
}

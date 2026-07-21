// ============================================================
// IScpiConnection — SCPI 命令收发通道抽象
//
// 将"通信方式"与"仪器功能"解耦：
//   仪器对象持有 IScpiConnection，而非直接继承 TCP 实现。
//   新增 RS232/USB 等通信方式时，只需增加实现类，仪器代码不动。
//
// 设计原则：仪器 "has-a" 连接，而非 "is-a" 连接。
//
// 注意：连接参数（IP、端口、超时等）由实现类的构造函数接收，
//   Connect() 方法本身无参。这避免接口污染——TCP 需要 port，
//   串口需要 baudRate，接口不该关心这些差异。
// ============================================================

namespace Demo.Instruments.Abstractions;

public interface IScpiConnection : IDisposable
{
    /// <summary>建立连接。连接参数在实现类构造时传入。</summary>
    void Connect();

    /// <summary>断开连接，释放底层传输资源</summary>
    void Disconnect();

    /// <summary>最近一次通信错误信息（无错误时为空字符串）</summary>
    string LastError { get; }

    // ---- 延迟参数（SCPI 通用概念，非 TCP 特有）----

    /// <summary>每次 Write 后等待仪器消化命令的时间 (ms)</summary>
    int WriteDelayMs { get; set; }

    /// <summary>Query 中 Write 完成后、Read 前的额外等待 (ms)</summary>
    int ReadDelayMs { get; set; }

    // ---- SCPI 收发 ----

    /// <summary>
    /// 发送 SCPI 命令（只发不收）。
    /// 实现类负责追加命令终止符（TCP SCPI 为 \n）。
    /// </summary>
    void Write(string cmd);

    /// <summary>
    /// 发送查询命令并读取完整响应。
    /// 实现类负责处理分包重组、超时、终止符判断。
    /// 异常不向上抛——出错时返回已读部分，同时设置 <see cref="LastError"/>。
    /// </summary>
    string Query(string cmd);
}

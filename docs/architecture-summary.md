# Demo 代码库架构总结

> 基于 2026-07-26 的深入学习整理。

---

## 一、整体架构：三层单体 + 策略模式

```
┌──────────────────────────────────────────────────┐
│                  测试代码层                       │
│  TestBase 只认接口：IPowerSupply / ISpectrumAnalyzer... │
│  不关心仪器品牌，不关心通信方式                      │
└──────────────────┬───────────────────────────────┘
                   │ 依赖接口，不依赖实现
                   ▼
┌──────────────────────────────────────────────────┐
│                  仪器实现层                       │
│  GwInstekPsw : ScpiInstrument, IPowerSupply     │
│  KeysightN9020A : ScpiInstrument, ISpectrumAnalyzer │
│  RsSmu200A : ScpiInstrument, ISignalGenerator    │
│  Udc0624F : ISwitchMatrix（不走 SCPI）             │
│  只写 SCPI 命令，不写通信代码                       │
└──────────────────┬───────────────────────────────┘
                   │ 继承 ScpiInstrument
                   ▼
┌──────────────────────────────────────────────────┐
│              ScpiInstrument 基类                  │
│  _connection: IScpiConnection                    │
│  Write() → _connection.Write()                   │
│  Query() → _connection.Query()                   │
│  子类通过 protected 方法发命令，不直接操作连接        │
└──────────────────┬───────────────────────────────┘
                   │ 组合（has-a），而非继承（is-a）
                   ▼
┌──────────────────────────────────────────────────┐
│               IScpiConnection 接口               │
│  定义：Connect / Disconnect / Write / Query      │
│  这是"通信方式"的契约                             │
└──────┬──────────────────────────┬────────────────┘
       │ 实现                      │ 可扩展
       ▼                           ▼
┌──────────────┐          ┌──────────────────┐
│ TcpConnection │          │ SerialConnection   │
│ TCP Socket    │          │ UART 串口          │
│ 当前在用      │          │ 预留/可新增        │
└──────────────┘          └──────────────────┘
```

---

## 二、核心设计思想：组合优于继承（Has-A 而非 Is-A）

仪器对象**持有**一个通信连接，而不是**继承**通信能力。

```
┌──────────────────────┐
│   KeysightN9020A      │   ← 具体的仪器
│  （频谱分析仪）       │      只关注 SCPI 命令语义
└──┬───────────────────┘
   │ 继承
   ▼
┌──────────────────────┐
│   ScpiInstrument      │   ← 抽象基类
│   - IScpiConnection   │      委托所有 I/O 给连接对象
└──┬───────────────────┘
   │ 组合（has-a）
   ▼
┌──────────────────────┐
│   TcpConnection       │   ← 传输层实现
│  (TCP Socket SCPI)    │      只关心 TCP 收发、分包重组
└──────────────────────┘
```

---

## 三、接口层（`Instruments/Abstractions/`）—— 契约定义

### 3.1 `IInstrument` — 所有仪器的基接口

```csharp
public interface IInstrument : IDisposable
{
    string Connect();        // 建立连接，返回 *IDN? 仪器身份
    void Disconnect();
    string Idn { get; }      // 仪器身份字符串
    string LastError { get; }
}
```

### 3.2 `IScpiConnection` — SCPI 传输通道接口

**最关键**的接口——把"通信方式"从"仪器功能"中彻底剥离。

| 成员                           | 说明                           |
| ---------------------------- | ---------------------------- |
| `Connect()` / `Disconnect()` | 无参连接/断开。连接参数在构造时传入           |
| `Write(string cmd)`          | 发送 SCPI 命令，实现类自动追加 `\n` 终止符  |
| `Query(string cmd)`          | 发送查询 → 循环读取直到 `\n`，解决 TCP 分包 |
| `WriteDelayMs`               | Write 后等待仪器消化命令的时间           |
| `ReadDelayMs`                | Query 中开始 Read 前的额外等待时间      |
| `LastError`                  | 最后一次错误的描述（不抛异常，容错设计）         |

> **关键设计**：`Connect()` **不接收参数**。因为 TCP 需要 IP/端口，串口需要端口/波特率——接口不该关心这些差异，参数由实现类在构造时固定。

### 3.3 各仪器功能接口

| 接口                  | 操作                                                                                    | 对应仪器                |
| ------------------- | ------------------------------------------------------------------------------------- | ------------------- |
| `IPowerSupply`      | `SetOutput()`, `MeasureVoltage()`, `MeasureCurrent()`, `SetVoltage()`, `SetCurrent()` | GWINSTEK PSW 电源     |
| `ISignalGenerator`  | `SetCw()`, `RfOn/Off()`, `ConfigureSweep()`, `ModOff()`                               | R&S SMU200A 信号源     |
| `ISpectrumAnalyzer` | `SetModeSa/Nf/Pn()`, `SaMarkerPeak()`, `NfSetMarker()`, `PnReadSpot()`                | Keysight N9020A 频谱仪 |
| `ISwitchMatrix`     | `SetUdcSwitches(sw1~sw4)`                                                             | UDC-0624F 开关矩阵      |

---

## 四、传输层（`TcpConnection`）—— TCP SCPI 通信

### Query 的循环读取机制

```
F[Query] --> G[写命令 + 等待]
G --> H[循环 NetworkStream.Read]
H --> I{读到 \\n?}
I -- 否 --> H
I -- 是 --> J[返回完整响应]
I -- 超时/断连 --> K[返回已读部分 + 设置 LastError]
```

因为 TCP 是流协议，一个响应可能被拆成多个包到达。**必须循环读到 `\n` 终止符才能保证响应完整性。**

---

## 五、构造函数链 —— 三层接力（理解解耦的关键）

### 两个构造函数

| 构造        | 签名                                                   | 作用                          |
| --------- | ---------------------------------------------------- | --------------------------- |
| 构造 A（快捷）  | `ScpiInstrument(string ip, int port, int timeoutMs)` | new 一个 TcpConnection，传给构造 B |
| 构造 B（主构造） | `ScpiInstrument(IScpiConnection connection)`         | 存连接、同步延迟配置，真正干活             |

### 完整调用链

```
new GwInstekPsw("192.168.1.10", 2268, 1.0)
    │
    │ 第 1 层 ── 子类 GwInstekPsw 构造
    │  : base(ip, port, (int)(timeoutSec * 1000))
    │  作用：把基本参数传给父类
    ▼
┌──────────────────────────────────────────────────────┐
│ 第 2 层 ── ScpiInstrument 构造 A（快捷构造）          │
│  : this(new TcpConnection(ip, port, timeoutMs))       │
│  作用：① new TcpConnection(...)                       │
│        ② 把新连接对象传给构造 B                       │
└──────────────────────┬───────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────┐
│ 第 3 层 ── ScpiInstrument 构造 B（主构造）← 真正干活  │
│ _connection = connection;        // ① 存连接          │
│ _connection.WriteDelayMs = ...;  // ② 同步延迟        │
│ _connection.ReadDelayMs = ...;   // ③ 同步延迟        │
└──────────────────────────────────────────────────────┘
```

### 关键语法

| 语法            | 意思                | 去向    |
| ------------- | ----------------- | ----- |
| `: base(...)` | 调用**父类**的构造函数     | 往上走一层 |
| `: this(...)` | 调用**自己类**的另一个构造函数 | 同一个类内 |

`: this(new TcpConnection(...))` 先求值括号内的表达式（new 一个连接对象），然后根据结果类型匹配另一个构造——**和普通函数重载的规则相同**。

### 为什么需要这条链？

1. **复用代码**——连接初始化逻辑（那三行赋值）只写一次
2. **子类不用知道 TcpConnection**——只传 ip/port，父类负责创建
3. **为换通信方式留扩展口**——见下文

---

## 六、调用 MeasureVoltage() 时的全链路

```
ps.MeasureVoltage()
    │
    ▼
GwInstekPsw.MeasureVoltage()              ← 子类的方法
    │
    ├── var resp = Query("MEAS:VOLT?")     ← 调基类的 protected 方法
    │       │
    │       ▼
    │   ScpiInstrument.Query("MEAS:VOLT?")
    │       └── _connection.Query("MEAS:VOLT?")  ← 委托给连接对象
    │               │
    │               ▼
    │           TcpConnection.Query("MEAS:VOLT?")
    │               ├── Write → ASCII "MEAS:VOLT?\n" → TCP socket → 仪器
    │               ├── Sleep(50ms)     ← GwInstekPsw 的 ReadDelayMs=50
    │               └── 循环 Read → 收到 "12.345\n" → 返回 "12.345"
    │
    └── return double.TryParse("12.345", out var v) ? v : double.NaN
                    ↑ 返回 12.345
```

---

## 七、如何解耦合——换通信方式时不改现有代码

### 只需两步

**第一步：新增串口连接类**

```csharp
public class SerialConnection : IScpiConnection
{
    // 实现 Connect/Disconnect/Write/Query，用 SerialPort 操作
}
```

**第二步：给仪器加一个新构造**

```csharp
public class GwInstekPsw : ScpiInstrument, IPowerSupply
{
    // 原有的 TCP 构造——不动
    public GwInstekPsw(string ip, int port = 2268, ...)
        : base(ip, port, ...) { }

    // 新增的串口构造
    public GwInstekPsw(string comPort, int baudRate)
        : base(new SerialConnection(comPort, baudRate)) { }  // ← 直接走构造 B

    // 以下 SCPI 方法——全都不动
    public void SetOutput(bool on) { Write("OUTP ..."); ... }
    public double MeasureVoltage() { Query("MEAS:VOLT?"); ... }
}
```

### 改动范围

| 文件                            | 改动                         |
| ----------------------------- | -------------------------- |
| `ScpiInstrument.cs`（构造 A + B） | **不动**                     |
| `TcpConnection.cs`            | **不动**                     |
| `SerialConnection.cs`         | **新增**（实现 IScpiConnection） |
| `GwInstekPsw.cs`              | **新增 1 个重载构造**             |
| 测试代码 TestBase                 | **不动**（只认接口）               |

### 为什么能这样？

因为**构造 B** 从头到尾只认 `IScpiConnection`——`TcpConnection` 和 `SerialConnection` 都实现了这个接口。对构造 B 来说，两个东西长得一模一样，它根本不关心底层是 TCP 还是串口。

```
TCP 场景：构造 A 内部 → new TcpConnection(...)  →  传给构造 B
串口场景：新增构造    → new SerialConnection(...) →  传给构造 B  ← 一样的逻辑
```

---

## 八、三句话终极总结

> **① 创建仪器时**，子类传 ip/port → 父类构造 A 自动 new 一个 TcpConnection → 通过 `: this(...)` 传给构造 B 存起来。
> 
> **② 发送 SCPI 时**，子类调 `protected Write/Query` → 基类直接转发给 `_connection.Write/Query` → TcpConnection 把字符串打成 ASCII 字节通过 TCP socket 发出去。
> 
> **③ 整个设计的灵魂**：仪器层只写 SCPI 命令，通信层只读写字节流，中间的 `ScpiInstrument` 基类持有一个 `IScpiConnection` 接口——**"有什么"而不是"是什么"**，所以换通信方式只需新增一个接口实现类，现有代码零改动。

---

## 九、项目中涉及的 C# 语法（学习笔记）

| 语法                     | 说明                    |
| ---------------------- | --------------------- |
| `class A : B`          | 类 A 继承类 B             |
| `: base(...)`          | 子类构造中显式调用父类构造         |
| `: this(...)`          | 同一个类中一个构造调用另一个构造      |
| `protected`            | 子类可见，外部不可见            |
| `virtual` / `override` | 虚方法 / 子类重写            |
| `interface`            | 纯抽象契约，只有声明没有实现        |
| `is` / `as`            | 类型判断和转换               |
| `IDisposable`          | 提供 `Dispose()` 统一释放资源 |

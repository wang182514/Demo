# 🔍 C# 已实现部分代码审查报告

> 仅聚焦已编写完成的代码中的缺陷，不含 TODO 桩代码及未移植功能。
>
> 审查日期：2026-07-20

---

## 🔴 严重问题 (9项)

### 1. KeysightN9020A.Query() — TCP 只读一次，数据可能截断

**文件**: `Instruments/KeysightN9020A.cs:39`

```csharp
private string Query(string c) { Write(c); var b = new byte[4096]; int n = _stream!.Read(b, 0, b.Length); return Encoding.ASCII.GetString(b, 0, n).Trim(); }
```

`NetworkStream.Read()` 只调一次。TCP 是流协议，仪器响应可能分多个包到达——如果数据在第一次 Read 时未完整到达，响应被截断，后续解析必错。

**同样的问题**: `RsSmu200A.cs:33-39`，完全相同的单次 Read 模式。

**正确做法**: 参考 `GwInstekPsw.cs` 的 `Query()` 实现——循环读直到收到 `\n` 终止符。

---

### 2. KeysightN9020A / RsSmu200A — Disconnect() 资源泄漏

**文件**: `KeysightN9020A.cs:19`, `RsSmu200A.cs:21`

```csharp
public void Disconnect() { _stream?.Close(); _tcp?.Close(); }
```

三个问题：
- `Close()` 不释放底层 Socket，应改用 `Dispose()`
- 未将 `_tcp` / `_stream` 置 null，导致 `IsConnected` 在断开后仍可能返回 `true`
- `Connect()` 未先调用 `Disconnect()`，重复连接会泄漏旧连接

**对比**: `GwInstekPsw.cs:109-116` 的 `Disconnect()` 正确置 null 并在 `Connect()` 开头先断旧连。

---

### 3. ISpectrumAnalyzer 接口缺少已用方法

**文件**: `Instruments/Abstractions/ISpectrumAnalyzer.cs`

Python `SpectrumAnalyzer` 以下方法在 C# 接口中缺失，但**配置文件中的其他测试项（tx_gain、tx_flatness_pn、tx_rx_influence）明确引用了这些命令**：

| Python方法 | SCPI 命令 | 缺失影响 |
|-----------|----------|---------|
| `sa_set_offset(db)` | `:SENSe:POWer:RF:GAIN:OFFSet` | 线损补偿，增益测量必用 |
| `sa_marker_ptp()` | `:CALC:MARK:PT_Peak` | 峰峰值平坦度 |
| `sa_marker_noise(freq)` | `:CALC:MARK2:MODE NOISe` | 噪底标记 |
| `screenshot(path)` | `:MMEM:DATA?` | 所有测试用截图留证 |

---

### 4. Form1.cs — God Object，单类承担全部职责

**文件**: `Form1.cs` (329行)

一个类兼任：仪器生命周期管理、配置加载/保存、10+ 按钮事件处理、测试调度与多线程、日志格式化、结果收集。任何修改都需要触碰这个类，测试和复用困难。

**Python 对应**: `MainWindow` + `test_runner.py` + `plugin.py` + 各仪器类，职责分离清晰。

---

### 5. _allResults 无线程保护

**文件**: `Form1.cs:25,257`

```csharp
private List<Dictionary<string, object>> _allResults = new();
// 后台线程中:
_allResults.Add(...);
```

`_allResults` 在 `Task.Run` 的后台线程写入，`BtnReport_Click` 在 UI 线程读取，无锁保护——竞态条件。应改用 `ConcurrentBag<T>` 或加 `lock`。

---

### 6. SettingsDialog 是纯空壳

**文件**: `Forms/SettingsDialog.cs` (33行)

两个 TabPage（"仪器连接"、"产品信息"）里面**没有任何输入控件**。用户点"详细设置"看到空白对话框。Python 版是完整的 PySide6 设置界面。

---

### 7. RxNfTest 无 UI 入口，完全不可达

**文件**: `Form1.cs:179,225-231`

- `BtnRunAll` 的 testIds = `["rx_pn", "tx_gain", "tx_flatness_pn", "tx_rx_influence"]`，不含 `"rx_nf"`
- `testMap` 字典不含 `"rx_nf"`
- 没有独立的 `btnRxNf` 按钮

`RxNfTest.cs` 是**唯一有完整实现的测试**，但无法从 UI 触发。

---

### 8. 停止按钮未绑定事件

**文件**: `Form1.Designer.cs:193-196`, `Form1.cs:52-64`

```csharp
// Designer.cs 声明了按钮
this.btnStop = new Button();
this.btnStop.Text = "停止";

// Form1.cs HookEvents() 中没有任何绑定
```

`TestBase.StopRequested` 属性存在但永不为 true，测试一旦启动无法中断。

---

### 9. RxNfTest 缺电源状态验证和关机保护

**文件**: `Tests/RxNfTest.cs` vs `tests/rx_nf.py`

| 缺失项 | Python (rx_nf.py) |
|--------|------------------|
| 上电后验证输出状态 | `base.rx_pwr.get_output_state()` |
| 断电后验证输出状态 | `base.rx_pwr.get_output_state()` |
| `finally` 块调用 `SafeShutdown()` | 第133行 |

如果测试中途异常退出，RX 电源不会自动关闭，被测模块持续通电。

---

## 🟡 中等问题 (11项)

### 10. RxNfTest NF/Gain 读取循环合并

**文件**: `Tests/RxNfTest.cs:35-40`

Python 分两个独立循环读 NF 和 Gain。C# 合并为一个循环，功能等价但若一个频率点 NF 读取失败，Gain 也一并跳过。

---

### 11. Udc0624F 串口缺少缓冲区清理

**文件**: `Instruments/Udc0624F.cs`

Python `switch_matrix.py` 在 `__init__` 后调用 `reset_input_buffer()` / `reset_output_buffer()`。C# 的 `Connect()` 未调 `DiscardInBuffer()` / `DiscardOutBuffer()`，残留数据可能干扰。

---

### 12. IPowerSupply 接口缺 get_output_state()

**文件**: `Instruments/Abstractions/IPowerSupply.cs`

Python 有 `get_output_state()`（发 `OUTP?` 查询）。`RxNfTest` 需要此方法确认电源状态。`GwInstekPsw` 也未实现。

---

### 13. GwInstekPsw.Query() 吞掉非IOException

**文件**: `Instruments/GwInstekPsw.cs:243`

```csharp
catch (IOException)
{
    // 超时或连接断开 → 忽略异常
}
```

仅捕获 `IOException`，`SocketException`、`ObjectDisposedException` 等会直接崩溃。Python 用裸 `except` 兜底（虽然也不完美，但至少不崩）。

---

### 14. ConfigManager.Get/Set 仅处理字符串

**文件**: `Models/ConfigManager.cs:36,48`

```csharp
public string Get(string dottedKey, string fallback = "") { ... }
public void Set(string dottedKey, string value) { ... }
```

JSON 中 port 是整数、enabled 是布尔、数组存频率列表——全部被强转字符串。`Form1.cs:74` 写 `== "true"` 做布尔判断，脆弱。

---

### 15. ConfigNode 构造函数无 null 保护

**文件**: `Models/ConfigNode.cs:13`

```csharp
public ConfigNode(JToken token) => _token = token;
```

若 `token` 为 null，后续任意调用都抛 `NullReferenceException`。

---

### 16. ConfigNode 隐式转换不健壮

```csharp
public static implicit operator int(ConfigNode n) => n._token.Value<int>();
public static implicit operator double(ConfigNode n) => n._token.Value<double>();
```

如果 JSON 值是字符串 `"1.0"`，`Value<double>()` 抛异常。Python 保留原始 Python 类型，不存在此问题。

---

### 17. btnSaveCfg 按钮无事件处理器

**文件**: `Form1.Designer.cs:150-153`

"保存配置"按钮声明了但 `HookEvents()` 没绑定，点击无反应。

---

### 18. rtbDetail 从未被填充

**文件**: `Form1.Designer.cs:224-226`

`RichTextBox rtbDetail` 声明了但全文件无引用，测试结果仅走 txtLog 文本输出。

---

### 19. 进度条粒度太粗

```csharp
progressBar.Maximum = testIds.Length; // 最多4步
```

只显示完成了几个测试（0→4），Python 通过 `report_progress(current, total)` 在测试内部报告精细进度。

---

### 20. Logger 文件名精度不足

`Logger.cs` 按天分文件 (`test_log_yyyyMMdd.log`)，多次运行追加同一文件。Python 精确到秒 (`test_log_%Y%m%d_%H%M%S.log`)，每次运行独立文件。

---

## 🟢 轻微问题 (5项)

| # | 问题 | 位置 |
|---|------|------|
| 21 | `KeysightN9020A` 端口硬编码 5025，未从配置读取 | `KeysightN9020A.cs:18` |
| 22 | `Udc0624F.LastError` 硬编码返回 `""` | `Udc0624F.cs:21` |
| 23 | `Logger` 无 Debug 级别 | `Utils/Logger.cs` |
| 24 | `Console.WriteLine` 在 WinForms 中不可见（无控制台窗口） | `Utils/Logger.cs:21` |
| 25 | `KeysightN9020A` 和 `RsSmu200A` 的 `Disconnect()` 直接 `Close()` 不 `Dispose()` | 与 #2 关联 |

---

## 📊 汇总

| 严重程度 | 数量 | 核心问题 |
|---------|------|---------|
| 🔴 严重 | 9 | TCP 单次Read、资源泄漏、接口缺方法、God Object、线程安全、空壳对话框、测试不可达、无停止机制、缺断电保护 |
| 🟡 中等 | 11 | 串口缓冲、缺 get_output_state、异常吞不完全、配置类型丢失、null保护、死按钮、空控件、粗进度条、日志文件名 |
| 🟢 轻微 | 5 | 硬编码端口、空LastError、缺Debug、Console无效输出、Close vs Dispose |

## 🎯 修复优先级

1. **立即**: 修复 `KeysightN9020A.Query()` 和 `RsSmu200A.Query()` 的 TCP 读循环
2. **立即**: 修复 `Disconnect()` 资源泄漏（置 null + Dispose）
3. **立即**: 给 `RxNfTest` 添加入口按钮 + `finally` 断电保护
4. **尽快**: 拆分 `Form1.cs`，加 `lock` 保护 `_allResults`
5. **尽快**: 补全 `ISpectrumAnalyzer` 缺失方法
6. **尽快**: 实现 `SettingsDialog` 实际内容 + 绑定 `btnStop`/`btnSaveCfg`
7. **计划**: 配置类型系统重构、Logger 改进

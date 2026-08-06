# R&S SMU200A — 项目 SCPI 速查手册

> 所有命令按功能分类。SMU200A 使用类似 SCPI 的指令集，多数命令可省略冒号前缀。
> 带 `[W]` 为只写命令，带 `[Q]` 为只读查询。

---

## 一、系统控制

| 命令      | 方向  | 说明               |
| ------- |:---:| ---------------- |
| `*CLS`  | W   | 清空状态寄存器和错误队列     |
| `*IDN?` | Q   | 查询仪器标识字符串        |
| `*OPC?` | Q   | 等待当前操作完成后返回 `+1` |

---

## 二、CW 模式（点频输出）

信号源最常用模式——输出单一频率的连续波。

| 命令                | 方向  | 说明                         |
| ----------------- |:---:| -------------------------- |
| `FREQ <value>MHz` | W   | 设置输出频率（支持 MHz / GHz / KHz） |
| `POW <value>dBm`  | W   | 设置输出功率                     |
| `:FREQ:MODE CW`   | W   | 切换到 CW（连续波）模式              |
| `OUTP ON`         | W   | 开启 RF 输出                   |
| `OUTP OFF`        | W   | 关闭 RF 输出                   |

---

## 三、扫频模式

用于平坦度测试等需要信号源自动扫频的场景。

| 命令                     | 方向  | 说明         |
| ---------------------- |:---:| ---------- |
| `FREQ:STAR <value>GHz` | W   | 扫频起始频率     |
| `FREQ:STOP <value>GHz` | W   | 扫频终止频率     |
| `SWE:STEP <value>KHz`  | W   | 频率步进       |
| `SWE:DWEL <value>ms`   | W   | 每步驻留时间     |
| `SWE:SPAC LIN`         | W   | 线性扫频       |
| `SWE:MODE AUTO`        | W   | 自动扫频模式     |
| `:FREQ:MODE SWE`       | W   | 切换到扫频模式    |
| `POW <value>dBm`       | W   | 扫频时的固定输出功率 |

> **典型流程**：
> 
> ```
> FREQ:STAR 0.95GHz  →  FREQ:STOP 1.55GHz
> SWE:STEP 1000KHz  →  SWE:DWEL 20ms
> POW -14dBm  →  SWE:SPAC LIN  →  SWE:MODE AUTO
> :FREQ:MODE SWE
> ```

---

## 四、RF 输出控制

| 命令         | 方向  | 说明       |
| ---------- |:---:| -------- |
| `OUTP ON`  | W   | 开启 RF 输出 |
| `OUTP OFF` | W   | 关闭 RF 输出 |

> 注意：`OUTP` 不带冒号。

---

## 五、调制控制

SMU200A 配有基带单元，可输出调制信号。ACPR 测试需要开启调制。

| 命令                     | 方向  | 说明         |
| ---------------------- |:---:| ---------- |
| `:MOD:STAT ON`         | W   | 开启所有调制     |
| `:MOD:STAT OFF`        | W   | 关闭所有调制     |
| `:SOUR:BB:DM:STAT ON`  | W   | 开启基带数字调制输出 |
| `:SOUR:BB:DM:STAT OFF` | W   | 关闭基带数字调制输出 |

> **ACPR 测试注意事项**：
> 
> 1. ACPR 测量需要调制信号 → 先开 BB 输出、再开调制
> 2. 饱和功率（Psat）测量需要纯 CW → 先关调制、再关 BB 输出
> 
> ```
> ACPR:   :SOUR:BB:DM:STAT ON  →  :MOD:STAT ON
> Psat:   :MOD:STAT OFF  →  :SOUR:BB:DM:STAT OFF
> ```

---

## 六、C# 接口对应关系

| 方法                         | SCPI 命令                                                                 |
| -------------------------- | ----------------------------------------------------------------------- |
| `SetCw(freqMhz, powerDbm)` | `FREQ <f>MHz` + `POW <p>dBm` + `:FREQ:MODE CW`                          |
| `RfOn()`                   | `OUTP ON`                                                               |
| `RfOff()`                  | `OUTP OFF`                                                              |
| `ModOff()`                 | `:MOD:STAT OFF`                                                         |
| `SetCwMode()`              | `:FREQ:MODE CW`                                                         |
| `ConfigureSweep(...)`      | `POW` + `FREQ:STAR/STOP` + `SWE:STEP/DWEL/SPAC/MODE` + `:FREQ:MODE SWE` |

### 当前缺失

| 方法                       | SCPI 命令                   | 用途           |
| ------------------------ | ------------------------- | ------------ |
| `ModOn()`                | `:MOD:STAT ON`            | ACPR 测试需要开调制 |
| `BbDmOn()` / `BbDmOff()` | `:SOUR:BB:DM:STAT ON/OFF` | 基带输出控制       |

---

## 七、常用流程速记

```
CW 输出 (TX Gain测试):
  FREQ 1.200GHz  →  POW -14dBm
  :FREQ:MODE CW  →  OUTP ON

扫频输出 (TX Flatness测试):
  POW -14dBm
  FREQ:STAR 0.95GHz  →  FREQ:STOP 1.55GHz
  SWE:STEP 1000KHz  →  SWE:DWEL 20ms
  SWE:SPAC LIN  →  SWE:MODE AUTO
  :FREQ:MODE SWE

ACPR 测试 (调制信号):
  FREQ 0.950GHz  →  POW -20dBm
  :FREQ:MODE CW
  :SOUR:BB:DM:STAT ON  →  :MOD:STAT ON
  OUTP ON

饱和功率测试 (纯CW):
  :MOD:STAT OFF  →  :SOUR:BB:DM:STAT OFF
  FREQ 0.950GHz  →  POW -5dBm
  OUTP ON

关闭:
  OUTP OFF  →  :MOD:STAT OFF  →  :FREQ:MODE CW
```

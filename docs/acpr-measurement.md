# 频谱仪 ACPR 测量与接口扩展参考

> 依据：`D:\生产调试文件\UP45A\UBUC16_CTRL_202605\UPautoTestV0_4.m`（UP45A BUC 上变频器产测脚本）
> 关联仪器：Keysight N9020A 频谱仪（SCPI over TCP）

---

## 一、ACPR 测量流程（m 文件第 91-206 行）

```
① 切换 SA 模式          INST:SEL SA
② 加载 ACPR 模板        :MMEM:LOAD:STAT "State_ACPR.state"
③ 设置线损补偿          :DISPlay:WIND1:TRACe:Y:RLEVel:OFFSet <loss_dB>
④ 设置中心频率          FREQuency:CENTer <freq>GHz
⑤ 连续测量模式          :INIT:CONT ON
⑥ 触发测量              :INIT:IMM  → pause(2s)
⑦ 读取 ACPR 结果        read:acp?  → 返回 "acp_m,acp_l,acp_u"（逗号分隔）
⑧ 解析：
     acp_m = 主信道功率 (dBm)  → 增益 = acp_m - VSG_Pwr
     acp_l = 下邻道功率 (dBc)
     acp_u = 上邻道功率 (dBc)
     ACPR  = max(acp_l, acp_u)   ← 取较差的一个作为判定值
⑨ 饱和功率（Psat）：
     加载模板            :MMEM:LOAD:STAT "State_Psat.state"
     峰值搜索            CALC:MARK1:STAT ON → CALC:MARK1:MAXimum → CALC:MARK1:Y?
```

### 判定逻辑（m 文件第 156 行）

```
循环推高输入功率 Pin_act（步进自适应）：
  if acp_m > Pout_target (41dBm) 或 ACPR > -25.2 dBc → 完成测量，记录 Pout/ACPR/Pin
```

---

## 二、SCPI 指令 ↔ C# 接口覆盖对照

| SCPI 指令 | 用途 | 现状 |
|-----------|------|:--:|
| `INST:SEL SA` | 切 SA 模式 | ✅ `SetModeSa()` |
| `:MMEM:LOAD:STAT "xxx.state"` | 加载模板 | ✅ `LoadState()` |
| `:SYST:ERR?` | 查错误 | ✅ `CheckError()` |
| `:DISPlay:WIND1:TRACe:Y:RLEVel:OFFSet` | 线损补偿 | ✅ `SaSetOffset()` |
| `:INIT:CONT ON` / `:INIT:IMM` | 连续测量+触发 | ✅ `SaConfigureMhz()` 内 |
| `read:acp?` | 读 ACPR | ✅ `ReadAcp()` |
| `CALC:MARK1:STAT ON / MAXimum / Y?` | 峰值搜索 | ✅ `SaMarkerPeak()` |
| `*OPC?` | 等待操作完成 | ✅ `WaitForComplete()` |
| `:MMEM:STOR:SCR` + `:MMEM:DATA?` | 截图 | ✅ `Screenshot()` |

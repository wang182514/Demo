// Keysight N9020A MXA 频谱分析仪（TCP SCPI 端口 5025，SA/NF/PN 三模式）
using Demo.Instruments.Abstractions;

namespace Demo.Instruments;

public class KeysightN9020A : ScpiInstrument, ISpectrumAnalyzer
{
    public KeysightN9020A(string ip) : base(ip, 5025) { }

    // 连接后清空状态寄存器
    protected override void OnConnected() => Write("*CLS");

    // ============================================================
    // 模式切换
    // ============================================================

    public void SetModeSa() => Write(":INST SA");
    public void SetModeNf() => Write(":INST:SEL NFIGURE");
    public void SetModePn() => Write(":INST PNOISE");

    // ============================================================
    // 通用
    // ============================================================

    public void LoadState(string n) { Write("*CLS"); Write($":MMEM:LOAD:STAT \"{n}\""); Query("*OPC?"); }
    public string CheckError() => Query(":SYST:ERR?");
    public void ClearMarkers() { try { Write(":CALC:MARK:AOFF"); } catch { } }

    /// <summary>阻塞等待仪器完成所有待处理操作</summary>
    public void WaitForComplete() => Query("*OPC?");

    /// <summary>截取频谱仪屏幕保存为本地 PNG 文件</summary>
    public void Screenshot(string savePath)
    {
        const string tmpName = "tmp_screenshot.png";
        Write($":MMEM:STOR:SCR \"{tmpName}\"");
        Query("*OPC?");                         // 等待文件写入完成
        Thread.Sleep(500);

        var data = ReadRaw($":MMEM:DATA? \"{tmpName}\"");
        if (data.Length > 0)
            File.WriteAllBytes(savePath, data);
    }

    // ============================================================
    // SA 模式 — 频谱分析
    // ============================================================

    /// <summary>
    /// 配置 SA 模式扫频（MHz 单位）。
    /// traceType: WRIT(清屏重写,默认) / MAXHold(最大值保持) / AVERage(平均)
    /// </summary>
    public void SaConfigureMhz(double s, double p, double r, double v, double l, string traceType = "WRIT")
    {
        Write($":SENS:FREQ:STAR {s:F3}MHz");
        Write($":SENS:FREQ:STOP {p:F3}MHz");
        Write($":SENS:BAND:RES {r:F0}KHz");
        Write($":SENS:BAND:VID {v:F0}KHz");
        Write($":DISP:WIND:TRAC:Y:RLEV {l:F0}dBm");
        Write($":TRAC1:TYPE {traceType}");
        Write(":INIT:CONT ON");
    }

    /// <summary>设置参考电平偏移 (dB)，用于射频线缆损耗补偿</summary>
    public void SaSetOffset(double offsetDb)
        => Write($":DISP:WIND1:TRAC:Y:RLEV:OFFS {offsetDb:F2}");

    /// <summary>峰值搜索：返回 (频率 Hz, 幅度 dBm)</summary>
    public (double freqHz, double ampDbm) SaMarkerPeak()
    {
        Write(":CALC:MARK1:STAT ON");
        Write(":CALC:MARK1:MAX");
        Thread.Sleep(100);
        return (double.Parse(Query("CALC:MARK1:X?")), double.Parse(Query("CALC:MARK1:Y?")));
    }

    /// <summary>峰峰值标记：返回当前 trace 最大-最小差值 (dB)</summary>
    public double SaMarkerPtP()
    {
        Write(":CALC:MARK1:PTP");
        return double.Parse(Query(":CALC:MARK1:Y?"));
    }

    /// <summary>
    /// 噪底标记：在指定频率点开启噪声功能，返回功率密度 (dBm/Hz)。
    /// 噪声标记需要计算时间，默认等待 3 秒。
    /// </summary>
    public double SaMarkerNoise(double freqMhz, double waitSec = 3.0)
    {
        Write(":CALC:MARK:AOFF");                    // 先清所有标记
        Write(":CALC:MARK1:STAT ON");
        Write($":CALC:MARK1:X {freqMhz:F0}MHz");
        Write(":CALC:MARK1:FUNC NOIS");              // 开启噪声标记功能
        Thread.Sleep((int)(waitSec * 1000));         // 等仪器计算
        return double.Parse(Query(":CALC:MARK1:Y?"));
    }

    /// <summary>读取当前迹线 Y 轴数据（点数取决于当前 span/RBW 设置）</summary>
    public double[] ReadTrace()
    {
        var raw = ReadRaw(":TRAC:DATA? TRACE1");
        if (raw.Length == 0) return Array.Empty<double>();
        var text = System.Text.Encoding.ASCII.GetString(raw).Trim();
        return text.Split(',').Select(s => double.Parse(s)).ToArray();
    }

    // ============================================================
    // ACPR 测量
    // ============================================================

    /// <summary>
    /// 读取 ACPR 结果。
    /// 返回：mainDbm(主信道功率), lowerDbc(下邻道), upperDbc(上邻道)
    /// </summary>
    public (double mainDbm, double lowerDbc, double upperDbc) ReadAcp()
    {
        var resp = Query("read:acp?");
        var parts = resp.Split(',');
        if (parts.Length < 3)
            return (double.NaN, double.NaN, double.NaN);
        return (double.Parse(parts[0]), double.Parse(parts[1]), double.Parse(parts[2]));
    }

    // ============================================================
    // NF 模式 — 噪声系数
    // ============================================================

    /// <summary>启动噪声系数校准</summary>
    public void NfInitCal() { Write(":NFIG:CAL:INIT"); Query("*OPC?"); }

    /// <summary>查询噪声系数校准是否完成（1=已校准, 0=未校准）</summary>
    public bool NfIsCalibrated() => Query(":NFIG:CAL:STAT?") == "1";

    public void NfInitMeasurement() { Write(":INIT:CONT ON"); Write(":INIT:IMM"); Query("*OPC?"); }
    public void NfPrepareMarkers() { Write(":CALC:NFIG:MARK:COUP OFF"); Write(":CALC:NFIG:MARK:AOFF"); }

    public double NfSetMarker(int m, int t, double f)
    {
        Write($":CALC:NFIG:MARK{m}:STAT ON");
        Write($":CALC:NFIG:MARK{m}:TRAC TRAC{t}");
        Write($":CALC:NFIG:MARK{m}:X {f:F2}GHz");
        Thread.Sleep(50);
        return double.Parse(Query($":CALC:NFIG:MARK{m}:Y?"));
    }

    // ============================================================
    // PN 模式 — 相位噪声
    // ============================================================

    public void PnSetCenterFreq(double g) => Write($":FREQ:CENT {g:F3}GHz");
    public void PnInitMeasurement() { Write(":INIT:CONT OFF"); Write(":INIT:IMM"); Query("*OPC?"); }

    public (double freqHz, double noiseDbc) PnReadSpot(int m)
        => (double.Parse(Query($":CALC:LPLot:MARK{m}:X?")), double.Parse(Query($":CALC:LPLot:MARK{m}:Y?")));
}

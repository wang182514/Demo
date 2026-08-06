// ============================================================
// ISpectrumAnalyzer — 频谱分析仪功能接口
//
// 覆盖 SA (频谱分析)、NF (噪声系数)、PN (相位噪声)、ACPR 四大模式。
// 方法按模式分组，命名前缀指示所属模式。
// ============================================================

namespace Demo.Instruments.Abstractions;

public interface ISpectrumAnalyzer : IInstrument
{
    // ============================================================
    // 模式切换
    // ============================================================
    void SetModeSa();
    void SetModeNf();
    void SetModePn();

    // ============================================================
    // 通用
    // ============================================================
    void LoadState(string templateName);
    string CheckError();
    void ClearMarkers();

    /// <summary>阻塞等待仪器所有待处理操作完成 (*OPC?)</summary>
    void WaitForComplete();

    /// <summary>截图保存为 PNG 文件</summary>
    void Screenshot(string savePath);

    // ============================================================
    // SA 模式 — 频谱分析
    // ============================================================

    /// <summary>
    /// 配置 SA 模式扫频参数。
    /// <paramref name="traceType"/>: WRIT(清屏重写,默认) / MAXHold(最大值保持) / AVERage(平均)
    /// </summary>
    void SaConfigureMhz(double start, double stop, double rbw, double vbw, double refLevel, string traceType = "WRIT");
    
    /// <summary>设置参考电平偏移 (dB)，用于射频线缆损耗补偿</summary>
    void SaSetOffset(double offsetDb);

    /// <summary>峰值搜索：返回 (频率 Hz, 幅度 dBm)</summary>
    (double freqHz, double ampDbm) SaMarkerPeak();

    /// <summary>峰峰值标记：返回当前 trace 的最大-最小差值 (dB)</summary>
    double SaMarkerPtP();

    /// <summary>
    /// 噪底标记：在指定频率点开启噪声标记功能，返回噪底功率密度 (dBm/Hz)。
    /// <paramref name="freqMhz"/>: 标记频率 (MHz)
    /// <paramref name="waitSec"/>: 噪声标记需要计算时间，默认等待 3 秒
    /// </summary>
    double SaMarkerNoise(double freqMhz, double waitSec = 3.0);

    /// <summary>读取 SA 模式当前迹线数据（Y 轴数组）</summary>
    double[] ReadTrace();

    // ============================================================
    // ACPR 测量（SA 模式下进行）
    // ============================================================

    /// <summary>
    /// 读取 ACPR 测量结果。
    /// 返回：mainDbm = 主信道功率 (dBm), lowerDbc = 下邻道功率 (dBc), upperDbc = 上邻道功率 (dBc)
    /// </summary>
    (double mainDbm, double lowerDbc, double upperDbc) ReadAcp();

    // ============================================================
    // NF 模式 — 噪声系数
    // ============================================================
    void NfInitCal();
    bool NfIsCalibrated();
    void NfInitMeasurement();
    void NfPrepareMarkers();

    /// <summary>
    /// 在指定频率点设 NF 标记并读取噪声系数。
    /// <paramref name="marker"/>: 标记号 1-4
    /// <paramref name="trace"/>: 迹线号 1-4
    /// <paramref name="freqGhz"/>: 频率 (GHz)
    /// </summary>
    double NfSetMarker(int marker, int trace, double freqGhz);

    // ============================================================
    // PN 模式 — 相位噪声
    // ============================================================
    void PnSetCenterFreq(double ghz);
    void PnInitMeasurement();

    /// <summary>读取指定标记点的相位噪声 (频率 Hz, 噪声 dBc/Hz)</summary>
    (double freqHz, double noiseDbc) PnReadSpot(int marker);
}

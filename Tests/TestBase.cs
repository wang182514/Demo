using Demo.Instruments;
using Demo.Instruments.Abstractions;
using Demo.Models;

namespace Demo.Tests;

/// <summary>测试基类 — 封装仪器引用、日志、开关、进度</summary>
public class TestBase
{
    public IPowerSupply RxPwr { get; }
    public IPowerSupply TxPwr { get; }
    public ISignalGenerator Vsg { get; }
    public ISpectrumAnalyzer Sa { get; }
    public ISwitchMatrix Switch { get; }
    public ConfigNode Cfg { get; }
    public Action<string> Log { get; set; } = _ => { };
    public Func<bool> StopRequested { get; set; } = () => false;
    public Action<int, int>? ProgressCallback { get; set; }

    public TestBase(IPowerSupply rxPwr, IPowerSupply txPwr, ISignalGenerator vsg, ISpectrumAnalyzer sa, ISwitchMatrix sw, ConfigNode cfg)
    {
        RxPwr = rxPwr; TxPwr = txPwr; Vsg = vsg; Sa = sa; Switch = sw; Cfg = cfg;
    }

    public void SetSwitches(params int[] config)
    {
        if (config.Length >= 4) Switch.SetUdcSwitches(config[0], config[1], config[2], config[3]);
        Thread.Sleep(100);
    }

    public void ReportProgress(int cur, int total) => ProgressCallback?.Invoke(cur, total);

    public void SafeShutdown()
    {
        try { Vsg?.RfOff(); Vsg?.SetCwMode(); } catch { }
        try { RxPwr?.SetOutput(false); } catch { }
        try { TxPwr?.SetOutput(false); } catch { }
    }
}

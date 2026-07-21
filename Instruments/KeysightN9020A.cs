// Keysight N9020A MXA 频谱分析仪（TCP SCPI 端口 5025，SA/NF/PN 三模式）
using Demo.Instruments.Abstractions;

namespace Demo.Instruments;

public class KeysightN9020A : ScpiInstrument, ISpectrumAnalyzer
{
    public KeysightN9020A(string ip) : base(ip, 5025) { }

    // 连接后清空状态寄存器
    protected override void OnConnected() => Write("*CLS");

    // ============================================================
    // ISpectrumAnalyzer
    // ============================================================

    public void SetModeSa() => Write(":INST SA");
    public void SetModeNf() => Write(":INST:SEL NFIGURE");
    public void SetModePn() => Write(":INST PNOISE");
    public void LoadState(string n) { Write("*CLS"); Write($":MMEM:LOAD:STAT \"{n}\""); Query("*OPC?"); }
    public string CheckError() => Query(":SYST:ERR?");
    public void ClearMarkers() { try { Write(":CALC:MARK:AOFF"); } catch { } }

    public void SaConfigureMhz(double s, double p, double r, double v, double l)
    {
        Write($":SENS:FREQ:STAR {s:F3}MHz");
        Write($":SENS:FREQ:STOP {p:F3}MHz");
        Write($":SENS:BAND:RES {r:F0}KHz");
        Write($":SENS:BAND:VID {v:F0}KHz");
        Write($":DISP:WIND:TRAC:Y:RLEV {l:F0}dBm");
        Write(":TRAC1:TYPE WRIT");
        Write(":INIT:CONT ON");
    }

    public (double freqHz, double ampDbm) SaMarkerPeak()
    {
        Write(":CALC:MARK1:STAT ON");
        Write(":CALC:MARK1:MAX");
        Thread.Sleep(100);
        return (double.Parse(Query("CALC:MARK1:X?")), double.Parse(Query("CALC:MARK1:Y?")));
    }

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

    public void PnSetCenterFreq(double g) => Write($":FREQ:CENT {g:F3}GHz");
    public void PnInitMeasurement() { Write(":INIT:CONT OFF"); Write(":INIT:IMM"); Query("*OPC?"); }

    public (double freqHz, double noiseDbc) PnReadSpot(int m)
        => (double.Parse(Query($":CALC:LPLot:MARK{m}:X?")), double.Parse(Query($":CALC:LPLot:MARK{m}:Y?")));
}

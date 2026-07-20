// R&S SMU200A 矢量信号源（TCP SCPI 端口 5025）
using Demo.Instruments.Abstractions;

namespace Demo.Instruments;

public class RsSmu200A : TcpScpiInstrument, ISignalGenerator
{
    public RsSmu200A(string ip) : base(ip, 5025) { }

    // 连接后清空状态寄存器
    protected override void OnConnected() => Write("*CLS");

    // ============================================================
    // ISignalGenerator
    // ============================================================

    public void SetCw(double f, double p) { Write($"FREQ {f:F3}MHz"); Write($"POW {p:F2}dBm"); Write(":FREQ:MODE CW"); }
    public void RfOn() => Write("OUTP ON");
    public void RfOff() => Write("OUTP OFF");
    public void ModOff() => Write(":MOD:STAT OFF");
    public void SetCwMode() => Write(":FREQ:MODE CW");

    public void ConfigureSweep(double sg, double sp, double sk, double dm, double pd)
    {
        Write($"POW {pd:F2}dBm");
        Write($"FREQ:STAR {sg:F3}GHz");
        Write($"FREQ:STOP {sp:F3}GHz");
        Write($"SWE:STEP {sk:F0}KHz");
        Write($"SWE:DWEL {dm:F0}ms");
        Write("SWE:SPAC LIN");
        Write("SWE:MODE AUTO");
        Write("FREQ:MODE SWE");
    }
}

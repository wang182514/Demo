// ============================================================
// GWINSTEK PSW20-27E 可编程直流电源（TCP SCPI 端口 2268）
// 继承 TcpScpiInstrument 基类，仅保留电源特有 SCPI 命令
// ============================================================

using Demo.Instruments.Abstractions;

namespace Demo.Instruments;

public class GwInstekPsw : ScpiInstrument, IPowerSupply
{
    // 电源响应较慢，Write 后不加额外等待，Query 读前等待 50ms
    protected override int WriteDelayMs => 0;
    protected override int ReadDelayMs => 50;

    /// <param name="ip">仪器 IP 地址</param>
    /// <param name="port">TCP 端口号，电源默认 2268</param>
    /// <param name="timeoutSec">超时秒数</param>
    public GwInstekPsw(string ip, int port = 2268, double timeoutSec = 1.0)
        : base(ip, port, (int)(timeoutSec * 1000))
    {
    }

    // ============================================================
    // 电源特有操作 (IPowerSupply)
    // ============================================================

    /// <summary>开启/关闭电源输出。发送后等 200ms 让仪器执行。</summary>
    public void SetOutput(bool on)
    {
        Write($"OUTP {(on ? "1" : "0")}");
        Thread.Sleep(200);
    }

    /// <summary>测量当前实际电压（伏特）</summary>
    public double MeasureVoltage()
    {
        var resp = Query("MEAS:VOLT?");
        return double.TryParse(resp, out var v) ? v : double.NaN;
    }

    /// <summary>测量当前实际电流（安培）</summary>
    public double MeasureCurrent()
    {
        var resp = Query("MEAS:CURR?");
        return double.TryParse(resp, out var v) ? v : double.NaN;
    }

    /// <summary>设置目标电压（伏特）</summary>
    public void SetVoltage(double volts) => Write($"SOUR:VOLT {volts:F3}");

    /// <summary>设置电流上限（安培）</summary>
    public void SetCurrent(double amps) => Write($"SOUR:CURR {amps:F3}");
}

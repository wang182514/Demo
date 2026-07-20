// Keysight N9020A MXA 频谱分析仪（TCP SCPI，SA/NF/PN 三模式）
using System.Net.Sockets;
using System.Text;
using Demo.Instruments.Abstractions;

namespace Demo.Instruments;

public class KeysightN9020A : ISpectrumAnalyzer
{
    private readonly string _ip;
    private TcpClient? _tcp;
    private NetworkStream? _stream;
    private string _idn = "", _lastError = "";

    public KeysightN9020A(string ip) => _ip = ip;
    public string Idn => _idn; public bool IsConnected => _tcp?.Connected ?? false; public string LastError => _lastError;

    public string Connect() { _tcp = new TcpClient(); _tcp.Connect(_ip, 5025); _stream = _tcp.GetStream(); Write("*CLS"); _idn = Query("*IDN?"); return _idn; }
    public void Disconnect() { _stream?.Close(); _tcp?.Close(); }
    public void Dispose() => Disconnect();

    public void SetModeSa() => Write(":INST SA");
    public void SetModeNf() => Write(":INST:SEL NFIGURE");
    public void SetModePn() => Write(":INST PNOISE");
    public void LoadState(string n) { Write("*CLS"); Write($":MMEM:LOAD:STAT \"{n}\""); Query("*OPC?"); }
    public string CheckError() => Query(":SYST:ERR?");
    public void ClearMarkers() { try { Write(":CALC:MARK:AOFF"); } catch { } }
    public void SaConfigureMhz(double s, double p, double r, double v, double l)
    { Write($":SENS:FREQ:STAR {s:F3}MHz"); Write($":SENS:FREQ:STOP {p:F3}MHz"); Write($":SENS:BAND:RES {r:F0}KHz"); Write($":SENS:BAND:VID {v:F0}KHz"); Write($":DISP:WIND:TRAC:Y:RLEV {l:F0}dBm"); Write(":TRAC1:TYPE WRIT"); Write(":INIT:CONT ON"); }
    public (double freqHz, double ampDbm) SaMarkerPeak() { Write(":CALC:MARK1:STAT ON"); Write(":CALC:MARK1:MAX"); Thread.Sleep(100); return (double.Parse(Query("CALC:MARK1:X?")), double.Parse(Query("CALC:MARK1:Y?"))); }
    public void NfInitMeasurement() { Write(":INIT:CONT ON"); Write(":INIT:IMM"); Query("*OPC?"); }
    public void NfPrepareMarkers() { Write(":CALC:NFIG:MARK:COUP OFF"); Write(":CALC:NFIG:MARK:AOFF"); }
    public double NfSetMarker(int m, int t, double f) { Write($":CALC:NFIG:MARK{m}:STAT ON"); Write($":CALC:NFIG:MARK{m}:TRAC TRAC{t}"); Write($":CALC:NFIG:MARK{m}:X {f:F2}GHz"); Thread.Sleep(50); return double.Parse(Query($":CALC:NFIG:MARK{m}:Y?")); }
    public void PnSetCenterFreq(double g) => Write($":FREQ:CENT {g:F3}GHz");
    public void PnInitMeasurement() { Write(":INIT:CONT OFF"); Write(":INIT:IMM"); Query("*OPC?"); }
    public (double freqHz, double noiseDbc) PnReadSpot(int m) => (double.Parse(Query($":CALC:LPLot:MARK{m}:X?")), double.Parse(Query($":CALC:LPLot:MARK{m}:Y?")));

    private void Write(string c) { if (!c.EndsWith('\n')) c += '\n'; _stream!.Write(Encoding.ASCII.GetBytes(c)); Thread.Sleep(30); }
    private string Query(string c)
    {
        Write(c);
        var sb = new StringBuilder();
        var buf = new byte[4096];
        try
        {
            int n;
            do
            {
                n = _stream!.Read(buf, 0, buf.Length);
                if (n > 0) sb.Append(Encoding.ASCII.GetChars(buf, 0, n));
            } while (n > 0 && !sb.ToString().Contains('\n'));
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
        }
        return sb.ToString().Trim();
    }
}

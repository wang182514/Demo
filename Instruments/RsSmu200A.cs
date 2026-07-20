// R&S SMU200A 矢量信号源（TCP SCPI 端口 5025）
using System.Net.Sockets;
using System.Text;
using Demo.Instruments.Abstractions;

namespace Demo.Instruments;

public class RsSmu200A : ISignalGenerator
{
    private readonly string _ip;
    private TcpClient? _tcp;
    private NetworkStream? _stream;
    private string _idn = "", _lastError = "";

    public RsSmu200A(string ip) => _ip = ip;
    public string Idn => _idn;
    public bool IsConnected => _tcp?.Connected ?? false;
    public string LastError => _lastError;

    public string Connect() { _tcp = new TcpClient(); _tcp.Connect(_ip, 5025); _stream = _tcp.GetStream(); Write("*CLS"); _idn = Query("*IDN?"); return _idn; }
    public void Disconnect() { _stream?.Close(); _tcp?.Close(); }
    public void Dispose() => Disconnect();

    public void SetCw(double f, double p) { Write($"FREQ {f:F3}MHz"); Write($"POW {p:F2}dBm"); Write(":FREQ:MODE CW"); }
    public void RfOn() => Write("OUTP ON");
    public void RfOff() => Write("OUTP OFF");
    public void ModOff() => Write(":MOD:STAT OFF");
    public void SetCwMode() => Write(":FREQ:MODE CW");
    public void ConfigureSweep(double sg, double sp, double sk, double dm, double pd)
    { Write($"POW {pd:F2}dBm"); Write($"FREQ:STAR {sg:F3}GHz"); Write($"FREQ:STOP {sp:F3}GHz"); Write($"SWE:STEP {sk:F0}KHz"); Write($"SWE:DWEL {dm:F0}ms"); Write("SWE:SPAC LIN"); Write("SWE:MODE AUTO"); Write("FREQ:MODE SWE"); }

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

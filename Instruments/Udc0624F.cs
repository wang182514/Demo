using System.IO.Ports;
using Demo.Instruments.Abstractions;

namespace Demo.Instruments;

/// <summary>UDC-0624F 射频开关矩阵（UART 串口）</summary>
public class Udc0624F : ISwitchMatrix
{
    private readonly string _comPort;
    private readonly int _baudRate;
    private SerialPort? _sp;
    private string _idn = "";

    public Udc0624F(string comPort, int baudRate = 115200)
    {
        _comPort = comPort; _baudRate = baudRate;
    }

    public string Idn => _idn;
    public bool IsConnected => _sp?.IsOpen ?? false;
    public string LastError => "";

    public string Connect()
    {
        _sp = new SerialPort(_comPort, _baudRate, Parity.None, 8, StopBits.One);
        _sp.Open();
        _idn = $"UDC-0624F @ {_comPort}";
        return _idn;
    }

    public void Disconnect() { _sp?.Close(); }

    public void SetUdcSwitches(int sw1, int sw2, int sw3, int sw4)
    {
        // Build UDC control packet (5 bytes + checksum)
        byte swVal = (byte)((sw4 << 3) | (sw3 << 2) | (sw2 << 1) | sw1);
        byte[] payload = { 0, 0, 1, swVal, 2 }; // opMode=2, lo_en=1
        byte[] header = { 85, 68, 67 };
        byte checksum = (byte)((header.Sum(b => b) + payload.Sum(b => b)) % 256);
        _sp!.Write(header, 0, 3);
        _sp.Write(payload, 0, 5);
        _sp.Write(new[] { checksum }, 0, 1);
        Thread.Sleep(50);
        try { _sp.Read(new byte[8], 0, 8); } catch { }
    }

    public void Dispose() => Disconnect();
}

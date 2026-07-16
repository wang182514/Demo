namespace Demo.Instruments.Abstractions;

/// <summary>所有仪器的基接口</summary>
public interface IInstrument : IDisposable
{
    string Connect();
    void Disconnect();
    bool IsConnected { get; }
    string Idn { get; }
    string LastError { get; }
}

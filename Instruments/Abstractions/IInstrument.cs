namespace Demo.Instruments.Abstractions;

/// <summary>所有仪器的基接口</summary>
public interface IInstrument : IDisposable
{
    string Connect();
    void Disconnect();
    string Idn { get; }
    string LastError { get; }
}

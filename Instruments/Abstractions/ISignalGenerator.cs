namespace Demo.Instruments.Abstractions;

public interface ISignalGenerator : IInstrument
{
    void SetCw(double freqMhz, double powerDbm);
    void RfOn();
    void RfOff();
    void ModOff();
    void SetCwMode();
    void ConfigureSweep(double startGhz, double stopGhz, double stepKhz, double dwellMs, double powerDbm);
}

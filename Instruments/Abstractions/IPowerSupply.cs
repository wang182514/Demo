namespace Demo.Instruments.Abstractions;

public interface IPowerSupply : IInstrument
{
    void SetOutput(bool on);
    double MeasureVoltage();
    double MeasureCurrent();
    void SetVoltage(double volts);
    void SetCurrent(double amps);
}

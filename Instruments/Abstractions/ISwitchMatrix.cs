namespace Demo.Instruments.Abstractions;

public interface ISwitchMatrix : IInstrument
{
    void SetUdcSwitches(int sw1, int sw2, int sw3, int sw4);
}

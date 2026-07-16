namespace Demo.Instruments.Abstractions;

public interface ISpectrumAnalyzer : IInstrument
{
    void SetModeSa();
    void SetModeNf();
    void SetModePn();
    void LoadState(string templateName);
    string CheckError();
    void ClearMarkers();
    void SaConfigureMhz(double start, double stop, double rbw, double vbw, double refLevel);
    (double freqHz, double ampDbm) SaMarkerPeak();
    void NfInitMeasurement();
    void NfPrepareMarkers();
    double NfSetMarker(int marker, int trace, double freqGhz);
    void PnSetCenterFreq(double ghz);
    void PnInitMeasurement();
    (double freqHz, double noiseDbc) PnReadSpot(int marker);
}

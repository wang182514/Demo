using Demo.Models;

namespace Demo.Tests;

/// <summary>
/// RX Noise Figure, Gain, and Flatness — identical logic to rx_nf.py.
/// </summary>
public class RxNfTest
{
    public static TestResult Run(TestBase b)
    {
        var r = new TestResult { TestName = "RX NF & Gain" };
        var cfg = b.Cfg["test_rx_nf"];
        try
        {
            b.Log("加载噪声系数模板...");
            b.Sa.SetModeNf();
            Thread.Sleep(1000);
            b.Sa.LoadState(cfg["template_name"]);
            var err = b.Sa.CheckError();
            b.Log(err.Contains("+0") ? "  模板已调用" : $"  模板错误: {err}");

            b.SetSwitches((int[])cfg["switch_config"].ToIntArray());
            b.RxPwr.SetOutput(true);
            Thread.Sleep(500);

            b.Log("正在启动单次测量...");
            b.Sa.NfInitMeasurement();
            b.Log("  测量完成");
            b.Sa.NfPrepareMarkers();

            var freqs = cfg["nf_freq_list_ghz"].ToDoubleArray();
            var nfList = new List<double>();
            var gainList = new List<double>();
            foreach (var f in freqs)
            {
                nfList.Add(b.Sa.NfSetMarker(1, 1, f));
                gainList.Add(b.Sa.NfSetMarker(3, 2, f));
                b.Log($"  频率:{f:F2}GHz, NF:{nfList[^1]:F3} dB, Gain:{gainList[^1]:F3} dB");
            }
            r.Data["nf_list"] = nfList.ToArray();
            r.Data["gain_list"] = gainList.ToArray();
            r.Data["nf_freqs"] = freqs;

            b.RxPwr.SetOutput(false);

            var limits = cfg["limits"];
            double nfMax = nfList.Max(), nfMean = nfList.Average(),
                   gainMean = gainList.Average();
            var gainTrimmed = gainList.Skip(1).Take(gainList.Count - 2).ToArray();
            double gainDiff = gainTrimmed.Length > 1
                ? gainTrimmed.Zip(gainTrimmed.Skip(1), (a, b) => Math.Abs(b - a)).Max()
                : 0;

            r.Data["nf_max_db"] = nfMax;
            r.Data["nf_mean_db"] = nfMean;
            r.Data["gain_mean_db"] = gainMean;
            r.Data["gain_flatness_db"] = gainDiff;
            r.Data["limits"] = new Dictionary<string, object> {
                {"nf_max_db", (double)limits["nf_max_db"]},
                {"nf_mean_db", (double)limits["nf_mean_db"]},
                {"gain_mean_db", (double)limits["gain_mean_db"]},
                {"gain_flatness_db", (double)limits["gain_flatness_db"]},
            };

            string Ok(bool ok) => ok ? "PASS" : "FAIL";
            r.Passed = true;
            r.Messages.Add($"NF最大值: {nfMax:F2} dB (限 {limits["nf_max_db"]}) {Ok(nfMax <= (double)limits["nf_max_db"])}");
            r.Messages.Add($"NF平均值: {nfMean:F2} dB (限 {limits["nf_mean_db"]}) {Ok(nfMean < (double)limits["nf_mean_db"])}");
            r.Messages.Add($"增益平均值: {gainMean:F2} dB (限 {limits["gain_mean_db"]}) {Ok(gainMean > (double)limits["gain_mean_db"])}");
            r.Messages.Add($"增益平坦度: {gainDiff:F2} dB (限 {limits["gain_flatness_db"]}) {Ok(gainDiff < (double)limits["gain_flatness_db"])}");
            r.Passed = nfMax <= (double)limits["nf_max_db"] && nfMean < (double)limits["nf_mean_db"]
                       && gainMean > (double)limits["gain_mean_db"] && gainDiff < (double)limits["gain_flatness_db"];
        }
        catch (Exception e)
        {
            r.Passed = false;
            r.Messages.Add($"测试异常: {e.Message}");
        }
        finally { b.Sa.ClearMarkers(); }
        return r;
    }
}

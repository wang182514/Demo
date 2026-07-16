// ============================================================
// Form1.cs — 主窗口逻辑
// 所有按钮事件、仪器控制、测试调度都在这里
// ============================================================

using Demo.Models;
using Demo.Instruments;
using Demo.Tests;

namespace Demo;

public partial class Form1 : Form
{
    // 配置管理器
    private ConfigManager _cfg = new ConfigManager();

    // 仪器对象（连接前为 null）
    private GwInstekPsw? _rxPwr;
    private GwInstekPsw? _txPwr;
    private RsSmu200A? _vsg;
    private KeysightN9020A? _sa;
    private Udc0624F? _sw;

    // 累积的测试结果（用于生成报告）
    private List<Dictionary<string, object>> _allResults = new List<Dictionary<string, object>>();

    // ============================================================
    // 构造函数 — 加载配置、绑定事件
    // ============================================================
    public Form1()
    {
        // 先初始化界面控件（Designer.cs 自动生成）
        InitializeComponent();

        // 绑定按钮事件
        HookEvents();

        // 加载配置文件
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _cfg.LoadDefaults(Path.Combine(baseDir, "config", "default_settings.json"));
        string userPath = Path.Combine(baseDir, "config", "user_settings.json");
        if (File.Exists(userPath))
            _cfg.LoadUser(userPath);

        // 把配置显示到界面上
        LoadConfigToUi();
    }

    // ============================================================
    // 事件绑定 — 把所有按钮的 Click 事件连接到这里
    // ============================================================
    private void HookEvents()
    {
        btnConnect.Click += BtnConnect_Click;
        btnDisconnect.Click += BtnDisconnect_Click;
        btnRunAll.Click += BtnRunAll_Click;
        btnRxPn.Click += BtnRxPn_Click;
        btnTxGain.Click += BtnTxGain_Click;
        btnTxFlat.Click += BtnTxFlat_Click;
        btnTxRx.Click += BtnTxRx_Click;
        btnReport.Click += BtnReport_Click;
        btnSettings.Click += BtnSettings_Click;
        btnClearResults.Click += BtnClearResults_Click;
    }

    // ============================================================
    // 配置 ↔ 界面
    // ============================================================

    /// <summary>把配置中的序列号等显示到界面上</summary>
    private void LoadConfigToUi()
    {
        txtSn.Text = _cfg.Get("serial_number", "");
        chkScreenshot.Checked = _cfg.Get("screenshot.enabled") == "true";
    }

    /// <summary>把界面上的序列号等保存到配置中</summary>
    private void SaveUiToConfig()
    {
        _cfg.Set("serial_number", txtSn.Text);
        _cfg.Set("screenshot.enabled", chkScreenshot.Checked ? "true" : "false");
    }

    // ============================================================
    // 连接 / 断开仪表
    // ============================================================

    /// <summary>逐个连接 5 台仪表，更新状态指示灯</summary>
    private void BtnConnect_Click(object? sender, EventArgs e)
    {
        SaveUiToConfig();
        ConfigNode inst = _cfg.Root["instruments"];

        // 接收电源
        TryConnect(() =>
        {
            _rxPwr = new GwInstekPsw(inst["rx_power_supply"]["ip"], inst["rx_power_supply"]["port"]);
            _rxPwr.Connect();
            lblRxPwr.Text = "接收电源: ✓ " + _rxPwr.Idn;
            pnlRxPwr.BackColor = Color.LimeGreen;
        }, ex => { lblRxPwr.Text = "接收电源: ✗ " + ex.Message; pnlRxPwr.BackColor = Color.Red; });

        // 发射电源
        TryConnect(() =>
        {
            _txPwr = new GwInstekPsw(inst["tx_power_supply"]["ip"], inst["tx_power_supply"]["port"]);
            _txPwr.Connect();
            lblTxPwr.Text = "发射电源: ✓ " + _txPwr.Idn;
            pnlTxPwr.BackColor = Color.LimeGreen;
        }, ex => { lblTxPwr.Text = "发射电源: ✗ " + ex.Message; pnlTxPwr.BackColor = Color.Red; });

        // 信号源
        TryConnect(() =>
        {
            _vsg = new RsSmu200A(inst["signal_generator"]["ip"]);
            _vsg.Connect();
            lblVsg.Text = "信号源: ✓ 已连接";
            pnlVsg.BackColor = Color.LimeGreen;
        }, ex => { lblVsg.Text = "信号源: ✗ " + ex.Message; pnlVsg.BackColor = Color.Red; });

        // 频谱仪
        TryConnect(() =>
        {
            _sa = new KeysightN9020A(inst["spectrum_analyzer"]["ip"]);
            _sa.Connect();
            lblSa.Text = "频谱仪: ✓ 已连接";
            pnlSa.BackColor = Color.LimeGreen;
        }, ex => { lblSa.Text = "频谱仪: ✗ " + ex.Message; pnlSa.BackColor = Color.Red; });

        // 开关矩阵
        TryConnect(() =>
        {
            _sw = new Udc0624F(inst["switch_matrix"]["com_port"], inst["switch_matrix"]["baud_rate"]);
            _sw.Connect();
            lblSwitch.Text = "开关矩阵: ✓ " + (string)inst["switch_matrix"]["com_port"];
            pnlSwitch.BackColor = Color.LimeGreen;
        }, ex => { lblSwitch.Text = "开关矩阵: ✗ " + ex.Message; pnlSwitch.BackColor = Color.Red; });
    }

    /// <summary>执行 try 块，catch 块处理异常，避免一个仪表失败影响其他</summary>
    private static void TryConnect(Action tryAction, Action<Exception> catchAction)
    {
        try { tryAction(); }
        catch (Exception ex) { catchAction(ex); }
    }

    /// <summary>断开所有仪表，重置状态灯</summary>
    private void BtnDisconnect_Click(object? sender, EventArgs e)
    {
        _rxPwr?.Disconnect();
        _txPwr?.Disconnect();
        _vsg?.Disconnect();
        _sa?.Disconnect();
        _sw?.Disconnect();

        // 重置界面文字和颜色
        lblRxPwr.Text = "接收电源: 未连接"; pnlRxPwr.BackColor = Color.Gray;
        lblTxPwr.Text = "发射电源: 未连接"; pnlTxPwr.BackColor = Color.Gray;
        lblVsg.Text = "信号源: 未连接"; pnlVsg.BackColor = Color.Gray;
        lblSa.Text = "频谱仪: 未连接"; pnlSa.BackColor = Color.Gray;
        lblSwitch.Text = "开关矩阵: 未连接"; pnlSwitch.BackColor = Color.Gray;

        Log("=== 已断开全部仪表 ===");
    }

    // ============================================================
    // 运行测试
    // ============================================================

    /// <summary>运行全部测试（在后台线程中执行，不卡界面）</summary>
    private async void BtnRunAll_Click(object? sender, EventArgs e)
    {
        if (!InstrumentsReady()) return;
        SaveUiToConfig();
        txtLog.Clear();
        // Task.Run 把耗时操作放到后台线程
        await Task.Run(() =>
        {
            RunTests(new[] { "rx_pn", "tx_gain", "tx_flatness_pn", "tx_rx_influence" });
        });
    }

    private async void BtnRxPn_Click(object? sender, EventArgs e)
    {
        if (!InstrumentsReady()) return;
        await Task.Run(() => RunTests(new[] { "rx_pn" }));
    }

    private async void BtnTxGain_Click(object? sender, EventArgs e)
    {
        if (!InstrumentsReady()) return;
        await Task.Run(() => RunTests(new[] { "tx_gain" }));
    }

    private async void BtnTxFlat_Click(object? sender, EventArgs e)
    {
        if (!InstrumentsReady()) return;
        await Task.Run(() => RunTests(new[] { "tx_flatness_pn" }));
    }

    private async void BtnTxRx_Click(object? sender, EventArgs e)
    {
        if (!InstrumentsReady()) return;
        await Task.Run(() => RunTests(new[] { "tx_rx_influence" }));
    }

    /// <summary>检查所有仪器是否已连接</summary>
    private bool InstrumentsReady()
    {
        if (_rxPwr == null || _txPwr == null || _vsg == null || _sa == null || _sw == null)
        {
            MessageBox.Show("请先连接全部仪表");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 依次运行指定的测试项（在后台线程中调用）。
    /// testIds: 如 ["rx_pn", "tx_gain"]
    /// </summary>
    private void RunTests(string[] testIds)
    {
        // 测试注册表 — ID → (显示名称, 执行函数)
        var testMap = new Dictionary<string, (string name, Func<TestBase, TestResult> runner)>
        {
            ["rx_pn"] = ("RX 相位噪声", RxPnTest.Run),
            ["tx_gain"] = ("TX 增益 + 输出功率", TxGainTest.Run),
            ["tx_flatness_pn"] = ("TX 平坦度 + 相位噪声", TxFlatnessTest.Run),
            ["tx_rx_influence"] = ("收发干扰", TxRxInfluenceTest.Run),
        };

        // 创建测试上下文
        TestBase b = new TestBase(_rxPwr!, _txPwr!, _vsg!, _sa!, _sw!, _cfg.Root);
        b.Log = msg => Invoke(() => Log(msg));  // 跨线程写日志

        // 显示进度条
        Invoke(() =>
        {
            progressBar.Visible = true;
            progressBar.Maximum = testIds.Length;
        });

        // 逐个运行
        for (int i = 0; i < testIds.Length; i++)
        {
            if (!testMap.TryGetValue(testIds[i], out var entry))
                continue;

            string sep = new string('=', 50);
            Invoke(() => Log($"\n{sep}\n开始: {entry.name}\n{sep}"));

            // 执行测试
            TestResult result = entry.runner(b);

            // 保存结果
            _allResults.Add(new Dictionary<string, object>
            {
                { "name", entry.name },
                { "passed", result.Passed },
                { "messages", result.Messages },
                { "data", result.Data }
            });

            // 显示结果
            bool passed = result.Passed;
            Invoke(() =>
            {
                Log(passed ? $"✓ {entry.name} PASS" : $"✗ {entry.name} FAIL");
                foreach (string m in result.Messages)
                    Log("  " + m);
                progressBar.Value = i + 1;
            });
        }

        // 隐藏进度条，启用报告按钮
        Invoke(() =>
        {
            progressBar.Visible = false;
            btnReport.Enabled = _allResults.Count > 0;
        });
    }

    // ============================================================
    // 报告 / 设置 / 清空
    // ============================================================

    private void BtnReport_Click(object? sender, EventArgs e)
    {
        MessageBox.Show("报告生成功能 — 待实现", "TODO");
    }

    private void BtnSettings_Click(object? sender, EventArgs e)
    {
        SaveUiToConfig();
        using (SettingsDialog dlg = new SettingsDialog(_cfg))
        {
            if (dlg.ShowDialog() == DialogResult.OK)
                LoadConfigToUi();
        }
    }

    private void BtnClearResults_Click(object? sender, EventArgs e)
    {
        _allResults.Clear();
        btnReport.Enabled = false;
        Log("=== 结果已清空 ===");
    }

    // ============================================================
    // 日志工具
    // ============================================================

    /// <summary>在日志窗口追加一行（带时间戳）</summary>
    private void Log(string msg)
    {
        string ts = DateTime.Now.ToString("HH:mm:ss");
        txtLog.AppendText("[" + ts + "] " + msg + "\r\n");
    }

    // ============================================================
    // 窗口关闭时断开仪表
    // ============================================================
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        BtnDisconnect_Click(this, EventArgs.Empty);
        base.OnFormClosing(e);
    }
}

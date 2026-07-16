// ============================================================
// Form1.Designer.cs — 界面布局
// 注意：为了让 VS 设计器正常渲染，所有控件创建都内联在这里
// 不调用任何自定义方法
// ============================================================

namespace Demo;

partial class Form1
{
    // ── 控件声明 ──
    private Panel pnlRxPwr, pnlTxPwr, pnlVsg, pnlSa, pnlSwitch;
    private Label lblRxPwr, lblTxPwr, lblVsg, lblSa, lblSwitch;
    private Button btnConnect, btnDisconnect;
    private Button btnSettings, btnSaveCfg;
    private Button btnRunAll, btnRxPn, btnTxGain, btnTxFlat, btnTxRx, btnStop;
    private Button btnReport, btnClearResults;
    private TextBox txtSn, txtLog;
    private CheckBox chkScreenshot;
    private ProgressBar progressBar;
    private RichTextBox rtbDetail;
    private SplitContainer splitMain, splitRight;

    // ── 每行一个控件的创建方法（设计器兼容）──
    private void InitializeComponent()
    {
        this.splitMain = new SplitContainer();
        this.splitRight = new SplitContainer();
        this.txtLog = new TextBox();
        this.txtSn = new TextBox();
        this.chkScreenshot = new CheckBox();
        this.rtbDetail = new RichTextBox();
        this.progressBar = new ProgressBar();
        this.splitMain.Panel1.SuspendLayout();
        this.splitMain.Panel2.SuspendLayout();
        this.splitMain.SuspendLayout();
        this.splitRight.Panel1.SuspendLayout();
        this.splitRight.Panel2.SuspendLayout();
        this.splitRight.SuspendLayout();
        this.SuspendLayout();

        // ── 主分栏（左右）──
        this.splitMain.Dock = DockStyle.Fill;
        this.splitMain.Orientation = Orientation.Vertical;
        this.splitMain.SplitterWidth = 5;

        // ── 左侧面板 ──
        Panel leftPanel = new Panel();
        leftPanel.Dock = DockStyle.Fill;
        leftPanel.AutoScroll = true;
        this.splitMain.Panel1.Controls.Add(leftPanel);

        // ═══ 仪器状态 ═══
        GroupBox grpInst = new GroupBox();
        grpInst.Text = "仪器状态";
        grpInst.Location = new Point(8, 8);
        grpInst.Size = new Size(275, 200);

        this.pnlRxPwr = new Panel();
        this.pnlRxPwr.Location = new Point(10, 22);
        this.pnlRxPwr.Size = new Size(14, 14);
        this.pnlRxPwr.BackColor = Color.Gray;
        this.lblRxPwr = new Label();
        this.lblRxPwr.Text = "接收电源: 未连接";
        this.lblRxPwr.Location = new Point(30, 20);
        this.lblRxPwr.AutoSize = true;

        this.pnlTxPwr = new Panel();
        this.pnlTxPwr.Location = new Point(10, 50);
        this.pnlTxPwr.Size = new Size(14, 14);
        this.pnlTxPwr.BackColor = Color.Gray;
        this.lblTxPwr = new Label();
        this.lblTxPwr.Text = "发射电源: 未连接";
        this.lblTxPwr.Location = new Point(30, 48);
        this.lblTxPwr.AutoSize = true;

        this.pnlVsg = new Panel();
        this.pnlVsg.Location = new Point(10, 78);
        this.pnlVsg.Size = new Size(14, 14);
        this.pnlVsg.BackColor = Color.Gray;
        this.lblVsg = new Label();
        this.lblVsg.Text = "信号源: 未连接";
        this.lblVsg.Location = new Point(30, 76);
        this.lblVsg.AutoSize = true;

        this.pnlSa = new Panel();
        this.pnlSa.Location = new Point(10, 106);
        this.pnlSa.Size = new Size(14, 14);
        this.pnlSa.BackColor = Color.Gray;
        this.lblSa = new Label();
        this.lblSa.Text = "频谱仪: 未连接";
        this.lblSa.Location = new Point(30, 104);
        this.lblSa.AutoSize = true;

        this.pnlSwitch = new Panel();
        this.pnlSwitch.Location = new Point(10, 134);
        this.pnlSwitch.Size = new Size(14, 14);
        this.pnlSwitch.BackColor = Color.Gray;
        this.lblSwitch = new Label();
        this.lblSwitch.Text = "开关矩阵: 未连接";
        this.lblSwitch.Location = new Point(30, 132);
        this.lblSwitch.AutoSize = true;

        this.btnConnect = new Button();
        this.btnConnect.Text = "连接全部仪表";
        this.btnConnect.Location = new Point(12, 160);
        this.btnConnect.Size = new Size(120, 28);

        this.btnDisconnect = new Button();
        this.btnDisconnect.Text = "断开全部仪表";
        this.btnDisconnect.Location = new Point(140, 160);
        this.btnDisconnect.Size = new Size(120, 28);

        grpInst.Controls.Add(this.pnlRxPwr);
        grpInst.Controls.Add(this.lblRxPwr);
        grpInst.Controls.Add(this.pnlTxPwr);
        grpInst.Controls.Add(this.lblTxPwr);
        grpInst.Controls.Add(this.pnlVsg);
        grpInst.Controls.Add(this.lblVsg);
        grpInst.Controls.Add(this.pnlSa);
        grpInst.Controls.Add(this.lblSa);
        grpInst.Controls.Add(this.pnlSwitch);
        grpInst.Controls.Add(this.lblSwitch);
        grpInst.Controls.Add(this.btnConnect);
        grpInst.Controls.Add(this.btnDisconnect);
        leftPanel.Controls.Add(grpInst);

        // ═══ 快速设置 ═══
        GroupBox grpParams = new GroupBox();
        grpParams.Text = "快速设置";
        grpParams.Location = new Point(8, 218);
        grpParams.Size = new Size(275, 130);

        Label lblSn = new Label();
        lblSn.Text = "序列号:";
        lblSn.Location = new Point(10, 22);
        lblSn.AutoSize = true;

        this.txtSn.Location = new Point(60, 20);
        this.txtSn.Size = new Size(200, 23);

        this.chkScreenshot.Text = "启用截图";
        this.chkScreenshot.Location = new Point(10, 50);

        this.btnSettings = new Button();
        this.btnSettings.Text = "详细设置...";
        this.btnSettings.Location = new Point(10, 80);
        this.btnSettings.Size = new Size(120, 28);

        this.btnSaveCfg = new Button();
        this.btnSaveCfg.Text = "保存配置";
        this.btnSaveCfg.Location = new Point(140, 80);
        this.btnSaveCfg.Size = new Size(120, 28);

        grpParams.Controls.Add(lblSn);
        grpParams.Controls.Add(this.txtSn);
        grpParams.Controls.Add(this.chkScreenshot);
        grpParams.Controls.Add(this.btnSettings);
        grpParams.Controls.Add(this.btnSaveCfg);
        leftPanel.Controls.Add(grpParams);

        // ═══ 测试控制 ═══
        GroupBox grpTest = new GroupBox();
        grpTest.Text = "测试控制";
        grpTest.Location = new Point(8, 358);
        grpTest.Size = new Size(275, 340);

        this.btnRunAll = new Button();
        this.btnRunAll.Text = "运行全部测试";
        this.btnRunAll.Location = new Point(10, 20);
        this.btnRunAll.Size = new Size(255, 30);

        this.btnRxPn = new Button();
        this.btnRxPn.Text = "RX 相位噪声";
        this.btnRxPn.Location = new Point(10, 56);
        this.btnRxPn.Size = new Size(255, 30);

        this.btnTxGain = new Button();
        this.btnTxGain.Text = "TX 增益 + 输出功率";
        this.btnTxGain.Location = new Point(10, 92);
        this.btnTxGain.Size = new Size(255, 30);

        this.btnTxFlat = new Button();
        this.btnTxFlat.Text = "TX 平坦度 + 相位噪声";
        this.btnTxFlat.Location = new Point(10, 128);
        this.btnTxFlat.Size = new Size(255, 30);

        this.btnTxRx = new Button();
        this.btnTxRx.Text = "收发干扰";
        this.btnTxRx.Location = new Point(10, 164);
        this.btnTxRx.Size = new Size(255, 30);

        this.btnStop = new Button();
        this.btnStop.Text = "停止";
        this.btnStop.Location = new Point(10, 200);
        this.btnStop.Size = new Size(255, 30);

        this.progressBar.Location = new Point(10, 236);
        this.progressBar.Size = new Size(255, 20);
        this.progressBar.Visible = false;

        this.btnReport = new Button();
        this.btnReport.Text = "写入报告";
        this.btnReport.Location = new Point(10, 264);
        this.btnReport.Size = new Size(255, 30);
        this.btnReport.Enabled = false;

        grpTest.Controls.Add(this.btnRunAll);
        grpTest.Controls.Add(this.btnRxPn);
        grpTest.Controls.Add(this.btnTxGain);
        grpTest.Controls.Add(this.btnTxFlat);
        grpTest.Controls.Add(this.btnTxRx);
        grpTest.Controls.Add(this.btnStop);
        grpTest.Controls.Add(this.progressBar);
        grpTest.Controls.Add(this.btnReport);
        leftPanel.Controls.Add(grpTest);

        // ═══ 右侧面板 ═══
        this.splitRight.Dock = DockStyle.Fill;
        this.splitRight.Orientation = Orientation.Horizontal;
        this.splitRight.SplitterWidth = 4;

        // 详情
        this.rtbDetail.Dock = DockStyle.Fill;
        this.rtbDetail.ReadOnly = true;
        this.rtbDetail.Text = "测试结果将在运行后自动显示。";

        // 日志
        GroupBox grpLog = new GroupBox();
        grpLog.Text = "日志";
        grpLog.Dock = DockStyle.Fill;

        this.txtLog.Multiline = true;
        this.txtLog.ReadOnly = true;
        this.txtLog.ScrollBars = ScrollBars.Vertical;
        this.txtLog.Dock = DockStyle.Fill;

        this.btnClearResults = new Button();
        this.btnClearResults.Text = "清空日志";
        this.btnClearResults.Location = new Point(530, 0);
        this.btnClearResults.Size = new Size(80, 24);

        grpLog.Controls.Add(this.btnClearResults);
        grpLog.Controls.Add(this.txtLog);

        this.splitRight.Panel1.Controls.Add(this.rtbDetail);
        this.splitRight.Panel2.Controls.Add(grpLog);
        this.splitMain.Panel2.Controls.Add(this.splitRight);

        // ── 窗口 ──
        this.Controls.Add(this.splitMain);
        this.Text = "C波段射频模块自动化测试系统";
        this.Size = new Size(1200, 800);
        this.splitMain.SplitterDistance = 310;
        this.splitRight.SplitterDistance = 300;

        this.splitMain.Panel1.ResumeLayout(false);
        this.splitMain.Panel2.ResumeLayout(false);
        this.splitMain.ResumeLayout(false);
        this.splitRight.Panel1.ResumeLayout(false);
        this.splitRight.Panel2.ResumeLayout(false);
        this.splitRight.ResumeLayout(false);
        this.ResumeLayout(false);
    }
}

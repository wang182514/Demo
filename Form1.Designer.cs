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
        splitMain = new SplitContainer();
        leftPanel = new Panel();
        grpInst = new GroupBox();
        pnlRxPwr = new Panel();
        lblRxPwr = new Label();
        pnlTxPwr = new Panel();
        lblTxPwr = new Label();
        pnlVsg = new Panel();
        lblVsg = new Label();
        pnlSa = new Panel();
        lblSa = new Label();
        pnlSwitch = new Panel();
        lblSwitch = new Label();
        btnConnect = new Button();
        btnDisconnect = new Button();
        grpParams = new GroupBox();
        lblSn = new Label();
        txtSn = new TextBox();
        chkScreenshot = new CheckBox();
        btnSettings = new Button();
        btnSaveCfg = new Button();
        grpTest = new GroupBox();
        btnRunAll = new Button();
        btnRxPn = new Button();
        btnTxGain = new Button();
        btnTxFlat = new Button();
        btnTxRx = new Button();
        btnStop = new Button();
        progressBar = new ProgressBar();
        btnReport = new Button();
        splitRight = new SplitContainer();
        rtbDetail = new RichTextBox();
        grpLog = new GroupBox();
        btnClearResults = new Button();
        txtLog = new TextBox();
        ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
        splitMain.Panel1.SuspendLayout();
        splitMain.Panel2.SuspendLayout();
        splitMain.SuspendLayout();
        leftPanel.SuspendLayout();
        grpInst.SuspendLayout();
        grpParams.SuspendLayout();
        grpTest.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitRight).BeginInit();
        splitRight.Panel1.SuspendLayout();
        splitRight.Panel2.SuspendLayout();
        splitRight.SuspendLayout();
        grpLog.SuspendLayout();
        SuspendLayout();
        // 
        // splitMain
        // 
        splitMain.Dock = DockStyle.Fill;
        splitMain.Location = new Point(0, 0);
        splitMain.Name = "splitMain";
        // 
        // splitMain.Panel1
        // 
        splitMain.Panel1.Controls.Add(leftPanel);
        // 
        // splitMain.Panel2
        // 
        splitMain.Panel2.Controls.Add(splitRight);
        splitMain.Size = new Size(1184, 761);
        splitMain.SplitterDistance = 947;
        splitMain.SplitterWidth = 5;
        splitMain.TabIndex = 0;
        // 
        // leftPanel
        // 
        leftPanel.AutoScroll = true;
        leftPanel.Controls.Add(grpInst);
        leftPanel.Controls.Add(grpParams);
        leftPanel.Controls.Add(grpTest);
        leftPanel.Dock = DockStyle.Fill;
        leftPanel.Location = new Point(0, 0);
        leftPanel.Name = "leftPanel";
        leftPanel.Size = new Size(947, 761);
        leftPanel.TabIndex = 0;
        // 
        // grpInst
        // 
        grpInst.Controls.Add(pnlRxPwr);
        grpInst.Controls.Add(lblRxPwr);
        grpInst.Controls.Add(pnlTxPwr);
        grpInst.Controls.Add(btnDisconnect);
        grpInst.Controls.Add(lblTxPwr);
        grpInst.Controls.Add(pnlVsg);
        grpInst.Controls.Add(lblVsg);
        grpInst.Controls.Add(pnlSa);
        grpInst.Controls.Add(lblSa);
        grpInst.Controls.Add(pnlSwitch);
        grpInst.Controls.Add(lblSwitch);
        grpInst.Controls.Add(btnConnect);
        grpInst.Location = new Point(8, 8);
        grpInst.Name = "grpInst";
        grpInst.Size = new Size(275, 200);
        grpInst.TabIndex = 0;
        grpInst.TabStop = false;
        grpInst.Text = "仪器状态";
        // 
        // pnlRxPwr
        // 
        pnlRxPwr.BackColor = Color.Gray;
        pnlRxPwr.Location = new Point(10, 22);
        pnlRxPwr.Name = "pnlRxPwr";
        pnlRxPwr.Size = new Size(14, 14);
        pnlRxPwr.TabIndex = 0;
        // 
        // lblRxPwr
        // 
        lblRxPwr.AutoSize = true;
        lblRxPwr.Location = new Point(30, 20);
        lblRxPwr.Name = "lblRxPwr";
        lblRxPwr.Size = new Size(99, 17);
        lblRxPwr.TabIndex = 1;
        lblRxPwr.Text = "接收电源: 未连接";
        // 
        // pnlTxPwr
        // 
        pnlTxPwr.BackColor = Color.Gray;
        pnlTxPwr.Location = new Point(10, 50);
        pnlTxPwr.Name = "pnlTxPwr";
        pnlTxPwr.Size = new Size(14, 14);
        pnlTxPwr.TabIndex = 2;
        // 
        // lblTxPwr
        // 
        lblTxPwr.AutoSize = true;
        lblTxPwr.Location = new Point(30, 48);
        lblTxPwr.Name = "lblTxPwr";
        lblTxPwr.Size = new Size(99, 17);
        lblTxPwr.TabIndex = 3;
        lblTxPwr.Text = "发射电源: 未连接";
        // 
        // pnlVsg
        // 
        pnlVsg.BackColor = Color.Gray;
        pnlVsg.Location = new Point(10, 78);
        pnlVsg.Name = "pnlVsg";
        pnlVsg.Size = new Size(14, 14);
        pnlVsg.TabIndex = 4;
        // 
        // lblVsg
        // 
        lblVsg.AutoSize = true;
        lblVsg.Location = new Point(30, 76);
        lblVsg.Name = "lblVsg";
        lblVsg.Size = new Size(87, 17);
        lblVsg.TabIndex = 5;
        lblVsg.Text = "信号源: 未连接";
        // 
        // pnlSa
        // 
        pnlSa.BackColor = Color.Gray;
        pnlSa.Location = new Point(10, 106);
        pnlSa.Name = "pnlSa";
        pnlSa.Size = new Size(14, 14);
        pnlSa.TabIndex = 6;
        // 
        // lblSa
        // 
        lblSa.AutoSize = true;
        lblSa.Location = new Point(30, 104);
        lblSa.Name = "lblSa";
        lblSa.Size = new Size(87, 17);
        lblSa.TabIndex = 7;
        lblSa.Text = "频谱仪: 未连接";
        // 
        // pnlSwitch
        // 
        pnlSwitch.BackColor = Color.Gray;
        pnlSwitch.Location = new Point(10, 134);
        pnlSwitch.Name = "pnlSwitch";
        pnlSwitch.Size = new Size(14, 14);
        pnlSwitch.TabIndex = 8;
        // 
        // lblSwitch
        // 
        lblSwitch.AutoSize = true;
        lblSwitch.Location = new Point(30, 132);
        lblSwitch.Name = "lblSwitch";
        lblSwitch.Size = new Size(99, 17);
        lblSwitch.TabIndex = 9;
        lblSwitch.Text = "开关矩阵: 未连接";
        // 
        // btnConnect
        // 
        btnConnect.Location = new Point(12, 160);
        btnConnect.Name = "btnConnect";
        btnConnect.Size = new Size(120, 28);
        btnConnect.TabIndex = 10;
        btnConnect.Text = "连接全部仪表";
        // 
        // btnDisconnect
        // 
        btnDisconnect.Location = new Point(149, 160);
        btnDisconnect.Name = "btnDisconnect";
        btnDisconnect.Size = new Size(120, 28);
        btnDisconnect.TabIndex = 11;
        btnDisconnect.Text = "断开全部仪表";
        btnDisconnect.Click += btnDisconnect_Click_1;
        // 
        // grpParams
        // 
        grpParams.Controls.Add(lblSn);
        grpParams.Controls.Add(txtSn);
        grpParams.Controls.Add(chkScreenshot);
        grpParams.Controls.Add(btnSettings);
        grpParams.Controls.Add(btnSaveCfg);
        grpParams.Location = new Point(8, 218);
        grpParams.Name = "grpParams";
        grpParams.Size = new Size(275, 130);
        grpParams.TabIndex = 1;
        grpParams.TabStop = false;
        grpParams.Text = "快速设置";
        // 
        // lblSn
        // 
        lblSn.AutoSize = true;
        lblSn.Location = new Point(10, 22);
        lblSn.Name = "lblSn";
        lblSn.Size = new Size(47, 17);
        lblSn.TabIndex = 0;
        lblSn.Text = "序列号:";
        // 
        // txtSn
        // 
        txtSn.Location = new Point(60, 20);
        txtSn.Name = "txtSn";
        txtSn.Size = new Size(200, 23);
        txtSn.TabIndex = 1;
        // 
        // chkScreenshot
        // 
        chkScreenshot.Location = new Point(10, 50);
        chkScreenshot.Name = "chkScreenshot";
        chkScreenshot.Size = new Size(104, 24);
        chkScreenshot.TabIndex = 2;
        chkScreenshot.Text = "启用截图";
        // 
        // btnSettings
        // 
        btnSettings.Location = new Point(10, 80);
        btnSettings.Name = "btnSettings";
        btnSettings.Size = new Size(120, 28);
        btnSettings.TabIndex = 3;
        btnSettings.Text = "详细设置...";
        // 
        // btnSaveCfg
        // 
        btnSaveCfg.Location = new Point(140, 80);
        btnSaveCfg.Name = "btnSaveCfg";
        btnSaveCfg.Size = new Size(120, 28);
        btnSaveCfg.TabIndex = 4;
        btnSaveCfg.Text = "保存配置";
        // 
        // grpTest
        // 
        grpTest.Controls.Add(btnRunAll);
        grpTest.Controls.Add(btnRxPn);
        grpTest.Controls.Add(btnTxGain);
        grpTest.Controls.Add(btnTxFlat);
        grpTest.Controls.Add(btnTxRx);
        grpTest.Controls.Add(btnStop);
        grpTest.Controls.Add(progressBar);
        grpTest.Controls.Add(btnReport);
        grpTest.Location = new Point(8, 358);
        grpTest.Name = "grpTest";
        grpTest.Size = new Size(275, 340);
        grpTest.TabIndex = 2;
        grpTest.TabStop = false;
        grpTest.Text = "测试控制";
        // 
        // btnRunAll
        // 
        btnRunAll.Location = new Point(10, 20);
        btnRunAll.Name = "btnRunAll";
        btnRunAll.Size = new Size(255, 30);
        btnRunAll.TabIndex = 0;
        btnRunAll.Text = "运行全部测试";
        // 
        // btnRxPn
        // 
        btnRxPn.Location = new Point(10, 56);
        btnRxPn.Name = "btnRxPn";
        btnRxPn.Size = new Size(255, 30);
        btnRxPn.TabIndex = 1;
        btnRxPn.Text = "RX 相位噪声";
        // 
        // btnTxGain
        // 
        btnTxGain.Location = new Point(10, 92);
        btnTxGain.Name = "btnTxGain";
        btnTxGain.Size = new Size(255, 30);
        btnTxGain.TabIndex = 2;
        btnTxGain.Text = "TX 增益 + 输出功率";
        // 
        // btnTxFlat
        // 
        btnTxFlat.Location = new Point(10, 128);
        btnTxFlat.Name = "btnTxFlat";
        btnTxFlat.Size = new Size(255, 30);
        btnTxFlat.TabIndex = 3;
        btnTxFlat.Text = "TX 平坦度 + 相位噪声";
        // 
        // btnTxRx
        // 
        btnTxRx.Location = new Point(10, 164);
        btnTxRx.Name = "btnTxRx";
        btnTxRx.Size = new Size(255, 30);
        btnTxRx.TabIndex = 4;
        btnTxRx.Text = "收发干扰";
        // 
        // btnStop
        // 
        btnStop.Location = new Point(10, 200);
        btnStop.Name = "btnStop";
        btnStop.Size = new Size(255, 30);
        btnStop.TabIndex = 5;
        btnStop.Text = "停止";
        // 
        // progressBar
        // 
        progressBar.Location = new Point(10, 236);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(255, 20);
        progressBar.TabIndex = 6;
        progressBar.Visible = false;
        // 
        // btnReport
        // 
        btnReport.Enabled = false;
        btnReport.Location = new Point(10, 264);
        btnReport.Name = "btnReport";
        btnReport.Size = new Size(255, 30);
        btnReport.TabIndex = 7;
        btnReport.Text = "写入报告";
        // 
        // splitRight
        // 
        splitRight.Dock = DockStyle.Fill;
        splitRight.Location = new Point(0, 0);
        splitRight.Name = "splitRight";
        splitRight.Orientation = Orientation.Horizontal;
        // 
        // splitRight.Panel1
        // 
        splitRight.Panel1.Controls.Add(rtbDetail);
        // 
        // splitRight.Panel2
        // 
        splitRight.Panel2.Controls.Add(grpLog);
        splitRight.Size = new Size(232, 761);
        splitRight.SplitterDistance = 540;
        splitRight.TabIndex = 0;
        // 
        // rtbDetail
        // 
        rtbDetail.Dock = DockStyle.Fill;
        rtbDetail.Location = new Point(0, 0);
        rtbDetail.Name = "rtbDetail";
        rtbDetail.ReadOnly = true;
        rtbDetail.Size = new Size(232, 540);
        rtbDetail.TabIndex = 0;
        rtbDetail.Text = "测试结果将在运行后自动显示。";
        // 
        // grpLog
        // 
        grpLog.Controls.Add(btnClearResults);
        grpLog.Controls.Add(txtLog);
        grpLog.Dock = DockStyle.Fill;
        grpLog.Location = new Point(0, 0);
        grpLog.Name = "grpLog";
        grpLog.Size = new Size(232, 217);
        grpLog.TabIndex = 0;
        grpLog.TabStop = false;
        grpLog.Text = "日志";
        // 
        // btnClearResults
        // 
        btnClearResults.Location = new Point(530, 0);
        btnClearResults.Name = "btnClearResults";
        btnClearResults.Size = new Size(80, 24);
        btnClearResults.TabIndex = 0;
        btnClearResults.Text = "清空日志";
        // 
        // txtLog
        // 
        txtLog.Dock = DockStyle.Fill;
        txtLog.Location = new Point(3, 19);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Vertical;
        txtLog.Size = new Size(226, 195);
        txtLog.TabIndex = 1;
        // 
        // Form1
        // 
        ClientSize = new Size(1184, 761);
        Controls.Add(splitMain);
        Name = "Form1";
        Text = "C波段射频模块自动化测试系统";
        splitMain.Panel1.ResumeLayout(false);
        splitMain.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
        splitMain.ResumeLayout(false);
        leftPanel.ResumeLayout(false);
        grpInst.ResumeLayout(false);
        grpInst.PerformLayout();
        grpParams.ResumeLayout(false);
        grpParams.PerformLayout();
        grpTest.ResumeLayout(false);
        splitRight.Panel1.ResumeLayout(false);
        splitRight.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitRight).EndInit();
        splitRight.ResumeLayout(false);
        grpLog.ResumeLayout(false);
        grpLog.PerformLayout();
        ResumeLayout(false);
    }
    private Panel leftPanel;
    private GroupBox grpInst;
    private GroupBox grpParams;
    private Label lblSn;
    private GroupBox grpTest;
    private GroupBox grpLog;
}

using Demo.Models;

namespace Demo;

public partial class SettingsDialog : Form
{
    private readonly ConfigManager _cfg;

    public SettingsDialog(ConfigManager cfg)
    {
        _cfg = cfg;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "详细设置";
        Size = new Size(700, 550);
        var tabs = new TabControl { Dock = DockStyle.Fill };
        Controls.Add(tabs);

        var tabInst = new TabPage("仪器连接");
        tabs.TabPages.Add(tabInst);

        var tabProd = new TabPage("产品信息");
        tabs.TabPages.Add(tabProd);

        var btnOk = new Button { Text = "确定", DialogResult = DialogResult.OK, Left = 500, Top = 470 };
        var btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel, Left = 580, Top = 470 };
        Controls.Add(btnOk);
        Controls.Add(btnCancel);
    }
}

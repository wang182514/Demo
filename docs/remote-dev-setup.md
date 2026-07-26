# 远程编译开发环境搭建指南

## 适用场景

| 项目 | 说明 |
|---|---|
| 你的台式机（家） | R7‑5700X / 32G / 主力编译机 |
| 公司小主机 | 低配，只写代码 |
| 网络 | 同城，两边都是民用 WiFi |
| 目标 | 在家机编译调试，公司机只负责编辑和看结果 |

---

## 整体方案

```
                   Tailscale 虚拟局域网
公司机（低配） ───────────────────── 家机（5700X）
  VS Code                           dotnet SDK 8.0
  Remote - SSH 插件                  OpenSSH Server
                                    Visual Studio 2022（可选）
```

**工作流**：公司机 VS Code 通过 Tailscale SSH 连到家机，代码写在家机上，编译调试全过程在家机跑，公司机只渲染 VS Code 界面。

---

## 第一步：家机安装 Tailscale（两台都要装，先装家机）

Tailscale 是一个超简单的组网工具，装好后两台电脑像在同一个局域网里。

### 1.1 下载安装

打开浏览器访问：https://tailscale.com/download

点击 **Windows** 下载安装包，双击安装，一路下一步。

### 1.2 登录

安装完后系统托盘会出现 Tailscale 图标（一只小狐狸），点击它 → **Sign in** → 浏览器会打开登录页面。

用你的 **Google / Microsoft / GitHub 账号** 随便一个登录就行（个人用免费版已经够了）。

### 1.3 记下家机的 Tailscale IP

登录后 Tailscale 窗口会显示一个 IP 地址，格式类似 `100.x.x.x`。**把这个地址记下来**，后面 SSH 连接要用。

```
例：家机 Tailscale IP = 100.88.22.11
```

> 这个 IP 是 Tailscale 分配的虚拟 IP，只有装了 Tailscale 的设备才能访问，安全。

---

## 第二步：家机开启 OpenSSH Server

Win10/Win11 自带的，不需要装额外软件。

### 2.1 检查是否已安装

右键 **开始菜单** → **设置** → **应用** → **可选功能**

在列表里找 **"OpenSSH 服务器"**。如果已经有了，跳到 2.3。

### 2.2 安装 OpenSSH 服务器

点击 **添加功能** → 搜索 **OpenSSH 服务器** → 勾选 → **安装**。

安装完成后可能需要重启电脑。

### 2.3 启动 SSH 服务

以**管理员身份**打开 PowerShell（右键开始菜单 → Windows PowerShell(管理员)/终端(管理员)）：

```powershell
# 启动 SSH 服务
Start-Service sshd

# 设置开机自动启动（重要！否则家机重启后就连不上了）
Set-Service -Name sshd -StartupType 'Automatic'

# 确认防火墙规则已放行（通常安装时自动配好了）
Get-NetFirewallRule -Name *ssh*
```

### 2.4 验证 SSH 是否开启

在**家机自己**上测试：

```powershell
ssh 用户名@localhost
```

> `用户名` 是你的 Windows 登录用户名。可以在 PowerShell 里输入 `whoami` 查看，显示的是 `电脑名\用户名`，取反斜杠后面的部分。

如果提示输入密码并登录成功，说明 SSH 已就绪。

---

## 第三步：公司机安装 Tailscale 和 VS Code

### 3.1 公司机装 Tailscale

和第一步一样，下载安装、登录（用同一个账号）。

装好后公司机也会有 Tailscale IP，两台机器会出现在同一个虚拟网络里。

### 3.2 在公司机上验证能连到家机

公司机打开 **PowerShell**，执行：

```powershell
# ping 家机的 Tailscale IP
ping 100.88.22.11
```

如果能 ping 通（有回复），说明组网成功。

### 3.3 公司机装 VS Code

如果公司机还没有 VS Code，去 https://code.visualstudio.com 下载安装。

### 3.4 安装 Remote - SSH 插件

打开 VS Code → 左侧点 **扩展**（四个方块的图标）→ 搜索 **Remote - SSH** → 安装（发布者是 Microsoft）。

---

## 第四步：家机准备项目环境

### 4.1 安装 .NET SDK 8.0

家机打开浏览器 → https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0 → 下载 **SDK 8.0.x** 的 Windows x64 安装包 → 安装。

装完后验证：

```powershell
dotnet --version
```

应该显示 `8.0.xxx`。

### 4.2 把项目代码放到家机

**方式 A（推荐）**：在家机上直接 git clone

```powershell
cd D:\
git clone https://github.com/wang182514/Demo.git
```

> 以后在家机提交代码也方便。

**方式 B**：从公司机复制过来（用 U 盘或局域网共享）

### 4.3 在家机验证能编译

```powershell
cd D:\Demo
dotnet build
```

应该编译通过，没有错误。

---

## 第五步：公司机 VS Code 连家机

### 5.1 配置 SSH 连接

公司机 VS Code 打开后 → 左下角有一个绿色按钮 **`><`**（Remote Window）→ 点击它 → 选择 **"Connect to Host…"** → 选 **"Add New SSH Host…"**

在弹出的框里输入：

```
ssh 用户名@100.88.22.11
```

> `用户名` 是**家机**的 Windows 登录用户名。
> `100.88.22.11` 替换成你家机的实际 Tailscale IP。

回车 → 选择 SSH 配置文件位置（默认就行，回车）→ 提示添加成功 → 点 **Connect**。

### 5.2 首次连接

- 会弹出一个新窗口，提示输入密码 → 输家机的 Windows 登录密码
- 首次连接会提示"继续连接？"（host key 确认）→ 选 **Continue**
- 连接成功后，左下角绿色按钮变成 **SSH: 100.88.22.11**

### 5.3 打开项目

在新窗口里：**文件** → **打开文件夹** → 输入 `D:\Demo`（或者在浏览里找到 D 盘的 Demo 文件夹）→ **确定**

VSCode 会加载家机上的项目，等几秒钟加载 IntelliSense。

---

## 第六步：验证远程开发是否成功

在公司机的 VS Code 里：

1. 打开 `Form1.cs`
2. 按 `F5`（或点 **运行** → **开始调试**）

如果一切正常：
- VS Code 会通过 SSH 让家机执行 `dotnet run`
- 家机上的 WinForms 窗口会**在家机屏幕弹出**（因为 GUI 应用在远程服务器上跑，不会传回来）

> ⚠️ **WinForms 的显示问题**：WinForms 是桌面 GUI 程序，远程编译后程序界面会显示在家机上，不会传到公司机。
>
> **解决方案**：在公司机你主要做"编码 + 编译检查"，不需要看到 WinForms 窗口。日常验证编译是否通过按 `Ctrl+Shift+B`（运行 dotnet build）即可。

---

## 日常使用流程

```
到公司坐下：
  ① 开机 → 等 Tailscale 自动连上（系统托盘出现小狐狸图标）
  ② 打开 VS Code → 左下角绿色按钮 → "Connect to Last Host"（直接重连上次的）
  ③ 输密码 → 等待加载 → 开始写代码

写代码时：
  按 Ctrl+Shift+B  → 远程编译
  按 F5            → 远程运行（窗口在家的显示器上弹出）
  VS Code 的终端  → 天然就是家机的 PowerShell

下班前：
  直接关 VS Code → 会自动断开 SSH 连接
  家机不用关机，下次能秒连
```

---

## 排查指南

### 连不上 —— ping 不通

```powershell
# 两台机器上都检查 Tailscale 是否在运行
# 系统托盘有没有小狐狸图标？右键 → 查看状态
# 确认两台都登录了同一个账号
```

### SSH 连接被拒绝

```powershell
# 在家机上检查 SSH 服务是否在跑
Get-Service sshd

# 如果没跑
Start-Service sshd

# 检查是否设了自动启动
Set-Service -Name sshd -StartupType 'Automatic'
```

### 忘记家机的 Tailscale IP

在家机系统托盘右键 Tailscale → Admin Console → 网页上能看到所有设备的 IP。

或者在 PowerShell 里：

```powershell
ipconfig | findstr "100."
```

### 编译报错说 SDK 版本不对

在家机上检查：

```powershell
dotnet --version
dotnet --list-sdks
```

确保安装了 .NET 8.0 SDK。

---

## 进阶技巧（熟手后再看）

### 免密码登录（SSH Key）

每次输密码嫌麻烦的话，可以配置 SSH 公钥登录：

**公司机 PowerShell**：

```powershell
# 生成密钥对（如果还没生成过）
ssh-keygen -t ed25519 -f "$env:USERPROFILE\.ssh\id_ed25519" -N ""

# 把公钥传到家里电脑
type "$env:USERPROFILE\.ssh\id_ed25519.pub" | ssh 用户名@100.88.22.11 "mkdir -p .ssh && cat >> .ssh/authorized_keys"
```

之后连接就**不需要密码了**。

### 在公司机也能看 WinForms 界面

如果确实需要看到 WinForms 界面，家机装 **VNC 服务器**（如 TigerVNC），公司机用 VNC Viewer 连过去看家机的桌面。但这需要额外配置，且比较吃带宽。**大多数情况下不需要**——只做编译验证的话，`dotnet build` 就够。

---

## 所需软件清单

| 软件 | 装在哪 | 下载地址 |
|---|---|---|
| Tailscale | **两台都装** | https://tailscale.com/download |
| VS Code | **公司机** | https://code.visualstudio.com |
| Remote - SSH 插件 | **公司机** | VS Code 扩展商店搜 |
| .NET SDK 8.0 | **家机** | https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0 |
| Git（可选） | **家机** | https://git-scm.com |

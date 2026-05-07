# WinCal —— Windows 菜单栏日历

> 一款轻量、优雅的 Windows 11 任务栏日历工具，灵感来自 macOS 平台的 Itsycal。

---

## 目录

1. [产品概述](#1-产品概述)
2. [功能规划](#2-功能规划)
3. [技术选型](#3-技术选型)
4. [项目结构](#4-项目结构)
5. [核心模块设计](#5-核心模块设计)
6. [UI 设计规范](#6-ui-设计规范)
7. [开发阶段规划](#7-开发阶段规划)
8. [发布方案](#8-发布方案)
9. [代码量与工作量估算](#9-代码量与工作量估算)
10. [依赖与环境](#10-依赖与环境)

---

## 1. 产品概述

### 1.1 产品定位

WinCal 是一款运行在 Windows 11 系统托盘区域的日历小工具。用户点击任务栏右侧的时间/日期区域，即可弹出一个轻量日历面板，同时展示近期日程事件，目标是填补 Windows 原生日历体验的空白。

### 1.2 核心价值

- **零打扰**：常驻托盘，不占用任务栏空间，不影响其他窗口
- **一键直达**：单击即弹出日历，再次点击或失焦自动收起
- **事件可见**：直接在日历面板展示近期事件，无需打开完整日历应用
- **轻量原生**：Self-contained 单文件发布，25~40 MB，无需安装运行时

### 1.3 目标用户

- 需要频繁查看日期和日程的知识工作者
- 不满意 Windows 原生日历体验的用户
- 使用 Outlook / Windows 日历的用户

---

## 2. 功能规划

### 2.1 MVP 功能（第一版必须实现）

| 功能 | 描述 |
|------|------|
| 系统托盘图标 | 显示当前日期时间星期作为托盘图标 |
| 日历弹出面板 | 点击托盘图标弹出月历视图 |
| 月历导航 | 支持上一月 / 下一月切换 |
| 今日高亮 | 当天日期特殊标注 |
| 事件点标记 | 有事件的日期底部显示小圆点 |
| 近期事件列表 | 面板下方列出未来 N 天的事件，可设置 |
| 面板自动定位 | 弹出窗口贴近任务栏右下角，不超出屏幕边界 |
| 失焦自动关闭 | 点击面板外区域自动收起 |
| 开机自启动 | 写入注册表实现开机启动，可设置 |

### 2.2 进阶功能

| 功能 | 优先级 |
|------|--------|
| 接入 Windows 日历 API | 高 |
| 接入 Outlook 日历 | 高 |
| 深色 / 浅色主题自动跟随系统 | 高 |
| 事件点击跳转对应日历应用 | 高 |
| 自定义周起始日（周日/周一） | 高 |
| 自适应系统缩放DPI | 高 |
| 农历显示 | 高 |

暂时不做
| 多显示器支持 | 低 |

---

## 3. 技术选型

### 3.1 核心技术栈

| 层次 | 技术 | 选择理由 |
|------|------|---------|
| 语言 | C# 12 | 类型安全，Windows API 支持最完整 |
| UI 框架 | WPF (.NET 8) | 矢量渲染，支持自定义控件，动画能力强 |
| 系统托盘 | Hardcodet.NotifyIcon.Wpf | WPF 原生 NotifyIcon 功能受限，此库弥补缺口 |
| 日历数据 | Windows.ApplicationModel.Appointments (via CsWinRT) | 读取系统日历，支持 Outlook / iCloud 等账户；CsWinRT 比 WindowsAppSDK 轻量 10+ MB |
| 农历计算 | System.Globalization.ChineseLunisolarCalendar | .NET 内置，无需额外依赖 |
| 发布方式 | Self-contained + SingleFile + Trim | 单文件，无需安装运行时，体积 25~40 MB |
| IDE | Visual Studio 2022 / VS Code + C# Dev Kit | 均可，VS2022 调试体验更佳 |

### 3.2 关键 NuGet 包

```xml
<PackageReference Include="Hardcodet.NotifyIcon.Wpf" Version="2.0.1" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.0" />
<PackageReference Include="Microsoft.Windows.CsWinRT" Version="2.1.1" />
```

> **为什么用 CsWinRT 而非 WindowsAppSDK？**
> - WindowsAppSDK 包含大量运行时组件（WinUI、Bootstrap、Deployment 等），打包后增加 **10~20 MB**
> - CsWinRT 仅提供 WinRT 类型投影层，本应用只需要 `AppointmentManager` API，用 CsWinRT 足够
> - CsWinRT 与 Trim 兼容性更好，经过 `TrimMode=partial` 裁剪后仅增加 **1~3 MB**

### 3.3 发布配置（.csproj）

```xml
<PropertyGroup>
  <TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>
  <UseWPF>true</UseWPF>
  <UseWinRT>true</UseWinRT>
  <WindowsPackageType>None</WindowsPackageType>  <!-- 非打包 Win32 应用 -->
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <PublishTrimmed>true</PublishTrimmed>
  <TrimMode>partial</TrimMode>  <!-- WPF 不支持完整 Trim，用 partial -->
  <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
</PropertyGroup>

<ItemGroup>
  <!-- 保留 WinRT 运行时类型，防止 Trim 误裁 -->
  <TrimmerRootAssembly Include="WinRT.Runtime" />
</ItemGroup>
```

> **关键说明**：
> - `TargetFramework` 使用 `net8.0-windows10.0.19041.0`（Windows 10 1903 对应的 SDK 版本），这样 CsWinRT 可以直接投影 WinRT API
> - `WindowsPackageType=None` 标识为非打包 Win32 应用，避免 MSIX 相关依赖
> - `UseWinRT=true` 启用 WinRT 类型投影
> - `TrimmerRootAssembly` 防止 Trim 误裁 WinRT 运行时类型
> - 预期打包体积 **25~40 MB**（比 WindowsAppSDK 方案减少 10~20 MB）

---

## 4. 项目结构

```
WinCal/
├── WinCal.csproj
├── App.xaml                        # 应用入口，注册托盘图标资源
├── App.xaml.cs
│
├── Core/                           # 业务逻辑层
│   ├── Models/
│   │   ├── CalendarEvent.cs        # 事件数据模型
│   │   └── CalendarDay.cs          # 单天数据模型（日期 + 是否有事件）
│   ├── Services/
│   │   ├── ICalendarService.cs     # 日历服务接口
│   │   ├── WindowsCalendarService.cs  # 接入 Windows 日历 API
│   │   └── MockCalendarService.cs  # 开发阶段用的模拟数据
│   └── Helpers/
│       ├── WindowPositionHelper.cs # 计算弹出窗口位置（DPI 感知）
│       ├── StartupHelper.cs        # 注册表开机自启
│       ├── TrayIconGenerator.cs    # 动态生成带日期数字的托盘图标
│       └── LunarCalendarHelper.cs  # 农历计算
│
├── ViewModels/                     # MVVM ViewModel 层
│   ├── CalendarViewModel.cs        # 主日历逻辑（月份、选中日期）
│   └── EventListViewModel.cs       # 事件列表逻辑
│
├── Views/                          # UI 层
│   ├── PopupWindow.xaml            # 弹出日历窗口
│   ├── PopupWindow.xaml.cs
│   ├── Controls/
│   │   ├── MonthCalendar.xaml      # 月历控件
│   │   ├── MonthCalendar.xaml.cs
│   │   ├── DayCell.xaml            # 单天格子控件
│   │   ├── DayCell.xaml.cs
│   │   ├── EventItem.xaml          # 单条事件控件
│   │   ├── EventItem.xaml.cs
│   │   ├── EventDetailPopup.xaml   # 事件悬停详情浮层
│   │   └── EventDetailPopup.xaml.cs
│   └── Themes/
│       ├── Light.xaml              # 浅色主题资源
│       └── Dark.xaml               # 深色主题资源
│
├── Assets/
│   └── Fonts/                      # 可选：自定义字体
│
├── Tests/                          # 单元测试
│   └── WinCal.Tests.csproj
│       ├── LunarCalendarHelperTests.cs
│       ├── CalendarViewModelTests.cs
│       └── WindowPositionHelperTests.cs
│
└── publish.bat                     # 一键发布脚本
```

---

## 5. 核心模块设计

### 5.1 系统托盘与弹窗触发

```csharp
// App.xaml.cs - 托盘图标初始化
public partial class App : Application
{
    private TaskbarIcon _trayIcon;
    private PopupWindow _popup;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
        _trayIcon.LeftClickCommand = new RelayCommand(TogglePopup);

        // 动态生成带今日日期数字的图标（支持 DPI 缩放）
        _trayIcon.Icon = TrayIconGenerator.Generate(DateTime.Today.Day);
    }

    private void TogglePopup()
    {
        if (_popup == null || !_popup.IsVisible)
        {
            _popup = new PopupWindow();
            WindowPositionHelper.PositionNearTaskbar(_popup);
            _popup.Show();
            _popup.Activate();
        }
        else
        {
            _popup.Hide();
        }
    }
}
```

### 5.2 弹出窗口定位（贴近任务栏，DPI 感知）

```csharp
// Helpers/WindowPositionHelper.cs
public static class WindowPositionHelper
{
    public static void PositionNearTaskbar(Window window)
    {
        // WPF 的 SystemParameters.WorkArea 已自动处理 DPI 缩放
        // 返回的是 WPF 逻辑像素单位，无需手动转换
        var screen = SystemParameters.WorkArea;

        // 检测任务栏位置（通过比较 WorkArea 和屏幕尺寸推断）
        bool taskbarOnTop = screen.Top > 0;
        bool taskbarOnRight = screen.Right < SystemParameters.PrimaryScreenWidth;
        bool taskbarOnLeft = screen.Left > 0;
        bool taskbarOnBottom = !taskbarOnTop && !taskbarOnRight && !taskbarOnLeft;

        const int margin = 8;

        if (taskbarOnBottom || taskbarOnTop)
        {
            window.Left = screen.Right - window.Width - margin;
            window.Top = taskbarOnBottom
                ? screen.Bottom - window.Height - margin
                : screen.Top + margin;
        }
        else
        {
            window.Top = screen.Bottom - window.Height - margin;
            window.Left = taskbarOnRight
                ? screen.Right - window.Width - margin
                : screen.Left + margin;
        }

        // 确保窗口不超出屏幕边界
        window.Left = Math.Max(screen.Left, Math.Min(window.Left, screen.Right - window.Width));
        window.Top = Math.Max(screen.Top, Math.Min(window.Top, screen.Bottom - window.Height));
    }
}
```

> **DPI 处理说明**：
> - WPF 应用默认是 DPI 感知的（Per-Monitor DPI Aware V2）
> - `SystemParameters.WorkArea` 返回的是当前显示器的工作区（逻辑像素），已自动缩放
> - 托盘图标所在显示器由系统决定，大多数情况为主显示器
> - 如需支持多显示器精确定位，可后续通过 `Screen` API 扩展

### 5.3 日历数据模型

```csharp
// Models/CalendarEvent.cs
public record CalendarEvent(
    string Title,
    DateTime StartTime,
    DateTime EndTime,
    bool IsAllDay,
    string CalendarName,
    string Color,          // 事件来源日历的颜色，用于左侧色条
    string? Location,      // 事件地点
    string? Description    // 事件详细描述
);

// Models/CalendarDay.cs
public record CalendarDay(
    DateTime Date,
    bool IsCurrentMonth,
    bool IsToday,
    bool HasEvents,
    string LunarDate,              // 农历显示文本，如 "初五"、"春节"
    List<CalendarEvent> Events
);
```

### 5.4 Windows 日历服务（含错误处理）

```csharp
// Services/WindowsCalendarService.cs
public class WindowsCalendarService : ICalendarService
{
    public async Task<List<CalendarEvent>> GetEventsAsync(DateTime start, DateTime end)
    {
        try
        {
            var store = await AppointmentManager.RequestStoreAsync(
                AppointmentStoreAccessType.AllCalendarsReadOnly);

            if (store == null)
            {
                // 用户拒绝权限或系统不支持
                return new List<CalendarEvent>();
            }

            var options = new FindAppointmentsOptions
            {
                MaxCount = 100,
                CalendarIds = { }  // 空表示所有日历
            };

            var appointments = await store.FindAppointmentsAsync(
                start, end - start, options);

            return appointments.Select(a => new CalendarEvent(
                Title: a.Subject,
                StartTime: a.StartTime.LocalDateTime,
                EndTime: a.StartTime.LocalDateTime + a.Duration,
                IsAllDay: a.AllDay,
                CalendarName: a.CalendarId,
                Color: "#0078D4",
                Location: a.Location ?? string.Empty,
                Description: a.Details ?? string.Empty
            )).ToList();
        }
        catch (UnauthorizedAccessException)
        {
            // 用户拒绝日历访问权限
            System.Diagnostics.Debug.WriteLine("Calendar permission denied.");
            return new List<CalendarEvent>();
        }
        catch (Exception ex)
        {
            // API 调用失败（服务不可用、账户问题等），不阻塞 UI
            System.Diagnostics.Debug.WriteLine($"Calendar API error: {ex.Message}");
            return new List<CalendarEvent>();
        }
    }
}
```

> **非打包 Win32 应用注意事项**：
> - 首次调用 `AppointmentManager.RequestStoreAsync` 时，系统会弹出权限请求对话框
> - 用户授权后，后续调用不再弹窗；如用户拒绝，`store` 返回 `null`
> - 权限可通过 Windows 设置 → 隐私 → 日历 重新开启
> - 在极少数旧版本 Windows 10 上，此 API 可能不可用，需在调用前检查系统版本

### 5.5 失焦自动关闭

```csharp
// PopupWindow.xaml.cs
protected override void OnDeactivated(EventArgs e)
{
    base.OnDeactivated(e);
    Hide(); // 失去焦点立即隐藏
}
```

### 5.6 开机自启动

```csharp
// Helpers/StartupHelper.cs
public static class StartupHelper
{
    private const string AppName = "WinCal";

    public static void Enable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", writable: true);
        key?.SetValue(AppName, Environment.ProcessPath!);
    }

    public static void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", writable: true);
        key?.DeleteValue(AppName, throwOnMissingValue: false);
    }

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", writable: false);
        return key?.GetValue(AppName) is not null;
    }
}
```

### 5.7 农历支持

使用 .NET 内置的 `System.Globalization.ChineseLunisolarCalendar`，无需外部依赖：

```csharp
// Helpers/LunarCalendarHelper.cs
public static class LunarCalendarHelper
{
    private static readonly ChineseLunisolarCalendar _calendar = new();

    private static readonly string[] LunarMonthNames =
        { "正月", "二月", "三月", "四月", "五月", "六月",
          "七月", "八月", "九月", "十月", "冬月", "腊月" };

    private static readonly string[] LunarDayNames =
        { "初一", "初二", "初三", "初四", "初五", "初六", "初七", "初八", "初九", "初十",
          "十一", "十二", "十三", "十四", "十五", "十六", "十七", "十八", "十九", "二十",
          "廿一", "廿二", "廿三", "廿四", "廿五", "廿六", "廿七", "廿八", "廿九", "三十" };

    /// <summary>
    /// 获取指定日期的农历显示文本。
    /// 优先显示重要农历节日名，其次显示"初一"时返回月名，其余返回日名。
    /// </summary>
    public static string GetLunarDateText(DateTime date)
    {
        try
        {
            int year = _calendar.GetYear(date);
            int month = _calendar.GetMonth(date);
            int day = _calendar.GetDayOfMonth(date);
            int leapMonth = _calendar.GetLeapMonth(year);

            // 处理闰月：闰月后面的月份数字需减 1
            bool isLeapMonth = false;
            if (leapMonth > 0)
            {
                if (month == leapMonth) isLeapMonth = true;
                if (month >= leapMonth) month--;
            }

            // 初一显示月名（如 "正月"、"闰六月"）
            if (day == 1)
                return (isLeapMonth ? "闰" : "") + LunarMonthNames[month - 1];

            // 其他日期显示日名（如 "十五"、"廿三"）
            return LunarDayNames[day - 1];
        }
        catch
        {
            return string.Empty;
        }
    }
}
```

> **农历显示策略**：
> - 日历格子中在公历日期下方用小字显示农历日（如 "初五"）
> - 每月初一显示月名（如 "正月"），便于快速识别农历月份
> - 重要农历节日（春节、元宵、端午、中秋等）可额外标注，通过内置节日映射表实现
> - 农历文本使用 `TextSecondaryColor`，字号 9px，不干扰主要信息

### 5.8 错误处理策略

| 场景 | 处理方式 | 用户感知 |
|------|---------|---------|
| 日历权限被拒绝 | 返回空事件列表，面板正常显示但无事件 | 事件区域显示"点击此处授权日历访问"，点击打开系统设置 |
| 日历 API 不可用（旧系统） | `AppointmentManager` 调用失败，catch 后降级 | 面板正常显示日历，无事件数据 |
| 日历服务异常 | catch 异常，返回空列表 | 事件区域显示"暂无事件" |
| 网络相关（Exchange 账户同步） | 不影响本地缓存数据的展示 | 无额外提示，事件列表可能不完整 |
| 系统版本不支持 | 启动时检查 `Environment.OSVersion` | 弹窗提示最低系统要求 |

```csharp
// Services/ICalendarService.cs
public interface ICalendarService
{
    Task<List<CalendarEvent>> GetEventsAsync(DateTime start, DateTime end);
    Task<bool> IsAvailableAsync();  // 检查服务是否可用
}
```

---

## 6. UI 设计规范

### 6.1 弹出面板尺寸

| 元素 | 规格 |
|------|------|
| 面板总宽度 | 320 px（逻辑像素） |
| 面板最大高度 | 480 px（超出后事件列表滚动） |
| 圆角半径 | 12 px（与 Win11 系统风格一致） |
| 面板阴影 | DropShadow, BlurRadius=20, Opacity=0.3 |
| 日历格子高度 | 38 px（含公历日期 + 农历小字） |
| 农历文字 | 9 px，使用 TextSecondaryColor |

> **DPI 缩放说明**：
> - 以上尺寸均为逻辑像素，WPF 会自动按系统 DPI 缩放
> - 在 150% 缩放下，面板物理宽度为 480px，视觉效果与 100% 下的 320px 一致
> - 托盘图标需生成多分辨率版本（16×16 @100%、24×24 @150%、32×32 @200%），使用 `RenderTargetBitmap` 动态渲染时自动处理

### 6.2 颜色规范（跟随系统主题）

```xml
<!-- Light.xaml -->
<Color x:Key="BackgroundColor">#FFFFFFFF</Color>
<Color x:Key="TodayAccentColor">#FF0078D4</Color>   <!-- Win11 蓝 -->
<Color x:Key="TextPrimaryColor">#FF1A1A1A</Color>
<Color x:Key="TextSecondaryColor">#FF767676</Color>
<Color x:Key="TextTertiaryColor">#FF999999</Color>  <!-- 农历等辅助文本 -->
<Color x:Key="SeparatorColor">#FFE5E5E5</Color>
<Color x:Key="HoverColor">#FFF3F3F3</Color>

<!-- Dark.xaml -->
<Color x:Key="BackgroundColor">#FF2D2D2D</Color>
<Color x:Key="TodayAccentColor">#FF4CC2FF</Color>
<Color x:Key="TextPrimaryColor">#FFFFFFFF</Color>
<Color x:Key="TextSecondaryColor">#FF9D9D9D</Color>
<Color x:Key="TextTertiaryColor">#FF6D6D6D</Color>
<Color x:Key="SeparatorColor">#FF404040</Color>
<Color x:Key="HoverColor">#FF383838</Color>
```

### 6.3 日历格子布局（XAML 示意）

```
┌─────────────────────────────────┐
│  < 2025年5月 >              [×] │  ← 月份导航栏，高度 40px
├─────────────────────────────────┤
│  日  一  二  三  四  五  六      │  ← 星期头，高度 28px
├─────────────────────────────────┤
│  28  29  30   1   2   3   4    │  ← 公历日期，居中
│  初二 初三 初四 初五 初六 初七 初八│  ← 农历，9px 小字，灰色
│   5   6   7  [8]  9  10  11    │  ← [8] = 今日，蓝色圆形高亮
│  初九 初十 十一 十二 十三 十四 十五│
│  12  13  14  15  16  17  18    │
│  十六 十七 十八 十九 二十 廿一 廿二│
│  19  20  21  22  23  24  25    │
│  廿三 廿四 廿五 廿六 廿七 廿八 廿九│
│  26  27  28  29  30  31   1    │
│  三十 正月 初二 初三 初四 初五 初六│  ← "正月" = 月初显示月名
│       ●               ●        │  ← 事件点，有事件的日期底部
├─────────────────────────────────┤
│  近期事件                        │  ← 分隔线
│  ▎ 今天  团队周会          10:00 │
│  ▎ 明天  产品评审          14:00 │
│  ▎ 周五  季度总结          全天  │
└─────────────────────────────────┘
```

### 6.4 事件悬停详情浮层

鼠标 hover 某条事件时，在该事件条目左侧弹出详情浮层：

```
┌──────────────────────┐
│  团队周会              │  ← 标题，14px，加粗
│  📅 今天 10:00-11:00  │  ← 时间
│  📍 3楼会议室A         │  ← 地点（无地点时隐藏此行）
│  ─────────────────── │
│  讨论本周项目进展，      │  ← 描述（最多3行，超出截断）
│  同步各组阻塞问题。      │
└──────────────────────┘
```

| 元素 | 规格 |
|------|------|
| 浮层宽度 | 200 px |
| 圆角半径 | 8 px |
| 出现延迟 | 200 ms（避免快速划过时闪烁） |
| 消失延迟 | 0 ms（鼠标移出立即消失） |
| 阴影 | 同面板阴影样式 |
| 背景 | 同面板背景色 |
| 最大高度 | 180 px（超出截断） |

---

## 7. 开发阶段规划

### 第一阶段：框架搭建（1~2 天）

- [ ] 创建 WPF 项目，配置 .csproj 发布参数（含 CsWinRT）
- [ ] 集成 Hardcodet.NotifyIcon.Wpf，实现托盘图标显示
- [ ] 实现弹出窗口基础框架（无内容，验证弹出/收起/定位逻辑）
- [ ] 实现失焦自动关闭
- [ ] 实现动态托盘图标（显示今日日期数字，多 DPI 分辨率支持）

### 第二阶段：日历 UI（2~3 天）

- [ ] 实现月历控件（纯 UI，使用硬编码数据）
- [ ] 实现月份切换（上一月 / 下一月）
- [ ] 今日高亮样式
- [ ] 其他月日期灰显样式
- [ ] 农历日期显示（集成 LunarCalendarHelper）
- [ ] 接入 CalendarViewModel，数据驱动 UI

### 第三阶段：事件接入（1~2 天）

- [ ] 实现 MockCalendarService（模拟数据，便于 UI 调试）
- [ ] 实现 WindowsCalendarService（接入真实 Windows 日历 API）
- [ ] 实现错误处理与降级（权限拒绝、API 不可用等）
- [ ] 事件点标记渲染（有事件的日期底部圆点）
- [ ] 近期事件列表渲染（事件名、时间、颜色条）
- [ ] 实现事件悬停详情浮层（EventDetailPopup）

### 第四阶段：体验完善（1~2 天）

- [ ] 深色 / 浅色主题自动跟随系统
- [ ] 开机自启动开关（右键托盘菜单）
- [ ] 多显示器任务栏定位兼容
- [ ] 弹出动画（FadeIn，100ms）
- [ ] 右键托盘菜单（关于、退出）

### 第五阶段：发布准备（0.5~1 天）

- [ ] 配置 Self-contained + Trim 发布
- [ ] 测试单文件运行（无 .NET 环境的干净虚拟机）
- [ ] 验证 CsWinRT + Trim 兼容性（确保 WinRT API 正常工作）
- [ ] 编写 README
- [ ] 打包为 ZIP

---

## 8. 发布方案

### 8.1 发布命令

```bat
:: publish.bat
dotnet publish WinCal.csproj ^
  -c Release ^
  -r win-x64 ^
  --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:PublishTrimmed=true ^
  -p:TrimMode=partial ^
  -p:EnableCompressionInSingleFile=true ^
  -o ./dist
```

### 8.2 预期产物

```
dist/
├── WinCal.exe          ← 单文件，约 25~40 MB
└── WinCal.pdb          ← 调试符号，发布时可删除
```

> **体积对比**：
> | 依赖方案 | 预期体积 |
> |---------|---------|
> | WindowsAppSDK（原方案） | 35~55 MB |
> | CsWinRT（当前方案） | 25~40 MB |
> | 减少 | **约 10~15 MB** |

### 8.3 分发方式对比

| 方式 | 优点 | 缺点 | 适用场景 |
|------|------|------|---------|
| 直接分发 .exe | 最简单，解压即用 | 部分杀软误报 | 自用、小范围分享 |
| ZIP 压缩包 | 体积减小约 40% | 需手动解压 | GitHub Release |
| MSIX 安装包 | 微软签名，无误报，支持自动更新 | 需开发者证书（约 ¥800/年） | 正式发布、上架商店 |

> **推荐**：使用 ZIP 分发，成熟后申请 MSIX 证书正式发布。

---

## 9. 代码量与工作量估算

### 9.1 代码量

| 模块 | 文件类型 | 估算行数 |
|------|---------|---------|
| 托盘 + 弹窗触发逻辑 | C# | 120 行 |
| 弹出窗口定位与动画 | C# | 80 行 |
| CalendarViewModel | C# | 150 行 |
| EventListViewModel | C# | 80 行 |
| WindowsCalendarService | C# | 120 行 |
| LunarCalendarHelper | C# | 80 行 |
| StartupHelper | C# | 50 行 |
| TrayIconGenerator | C# | 70 行 |
| PopupWindow UI | XAML | 80 行 |
| MonthCalendar 控件 | XAML | 130 行 |
| DayCell 控件（含农历显示） | XAML | 80 行 |
| EventItem 控件 | XAML | 50 行 |
| EventDetailPopup 控件 | XAML | 60 行 |
| 主题资源字典 | XAML | 90 行 |
| 单元测试 | C# | 150 行 |
| **合计** | | **约 1,300 行** |

### 9.2 工作量（个人独立开发）

| 阶段 | 工作天数 |
|------|---------|
| 框架搭建 | 1~2 天 |
| 日历 UI（含农历） | 2~3 天 |
| 事件接入（含错误处理） | 1~2 天 |
| 体验完善 | 1~2 天 |
| 发布准备 | 0.5~1 天 |
| **合计** | **6~10 天** |

---

## 10. 依赖与环境

### 10.1 开发环境

| 工具 | 版本要求 |
|------|---------|
| .NET SDK | 8.0 或以上 |
| Visual Studio | 2022（推荐）或 VS Code + C# Dev Kit |
| Windows | 11（目标平台，建议同平台开发） |

### 10.2 运行环境（用户侧）

| 项目 | 要求 |
|------|------|
| 操作系统 | Windows 10 1903+ / Windows 11 |
| .NET 运行时 | 无需安装（Self-contained 已内置） |
| 权限 | 普通用户权限即可（注册表写入用当前用户 Hive） |
| 磁盘空间 | 约 50 MB |

### 10.3 访问 Windows 日历 API

本应用使用 CsWinRT 投影层访问 `Windows.ApplicationModel.Appointments` 命名空间：

**非打包 Win32 应用的特殊处理**：

1. 无需 `Package.appxmanifest` 文件，权限通过代码运行时申请
2. 首次调用 `AppointmentManager.RequestStoreAsync` 时系统自动弹出权限对话框
3. 用户授权后权限持久化，后续调用无需再次授权
4. 权限可通过 **Windows 设置 → 隐私和安全 → 日历** 管理

```csharp
// 启动时检查日历可用性（可选，友好提示）
public static bool IsCalendarApiSupported()
{
    // Windows 10 1903 (build 18362) 及以上支持
    return Environment.OSVersion.Version >= new Version(10, 0, 18362);
}
```

---

*文档版本：v1.1 | 最后更新：2026 年 5 月*

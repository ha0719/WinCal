namespace WinCal.Core.Models;

/// <summary>
/// 日历账户信息（用于设置界面展示）
/// </summary>
public record CalendarAccountInfo(
    string AccountName,
    string CalendarName,
    string CalendarId,
    string ColorHex,
    bool IsEnabled
);

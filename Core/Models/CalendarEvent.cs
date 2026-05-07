namespace WinCal.Core.Models;

/// <summary>
/// 日历事件数据模型
/// </summary>
public record CalendarEvent(
    string Title,
    DateTime StartTime,
    DateTime EndTime,
    bool IsAllDay,
    string CalendarName,
    string Color,
    string? Location = null,
    string? Description = null
);

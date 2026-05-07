namespace WinCal.Core.Models;

/// <summary>
/// 单天数据模型（日期 + 事件 + 农历）
/// </summary>
public record CalendarDay(
    DateTime Date,
    bool IsCurrentMonth,
    bool IsToday,
    bool HasEvents,
    string LunarDate,
    List<CalendarEvent> Events
);

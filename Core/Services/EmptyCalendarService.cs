using WinCal.Core.Models;

namespace WinCal.Core.Services;

/// <summary>
/// 空日历服务（ICS URL 未配置时的占位服务）
/// </summary>
public class EmptyCalendarService : ICalendarService
{
    public Task<List<CalendarEvent>> GetEventsAsync(DateTime start, DateTime end)
        => Task.FromResult(new List<CalendarEvent>());

    public Task<bool> IsAvailableAsync()
        => Task.FromResult(false);

    public Task<List<CalendarAccountInfo>> GetCalendarAccountsAsync()
        => Task.FromResult(new List<CalendarAccountInfo>());

    public Task ForceRefreshAsync()
        => Task.CompletedTask;

    public void OpenSystemCalendarApp() { }
}

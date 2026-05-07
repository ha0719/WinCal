using WinCal.Core.Models;

namespace WinCal.Core.Services;

/// <summary>
/// 聚合日历服务 —— 合并多个数据源的事件
/// </summary>
public class AggregateCalendarService : ICalendarService
{
    private readonly List<ICalendarService> _services;

    public AggregateCalendarService(params ICalendarService[] services)
    {
        _services = services.ToList();
    }

    public async Task<List<CalendarEvent>> GetEventsAsync(DateTime start, DateTime end)
    {
        var allEvents = new List<CalendarEvent>();

        foreach (var service in _services)
        {
            try
            {
                var events = await service.GetEventsAsync(start, end);
                allEvents.AddRange(events);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WinCal: Service {service.GetType().Name} failed: {ex.Message}");
            }
        }

        return allEvents.OrderBy(e => e.StartTime).ToList();
    }

    public async Task<bool> IsAvailableAsync()
    {
        foreach (var service in _services)
        {
            try
            {
                if (await service.IsAvailableAsync())
                    return true;
            }
            catch { }
        }
        return false;
    }

    public async Task<List<CalendarAccountInfo>> GetCalendarAccountsAsync()
    {
        var accounts = new List<CalendarAccountInfo>();
        foreach (var service in _services)
        {
            try
            {
                accounts.AddRange(await service.GetCalendarAccountsAsync());
            }
            catch { }
        }
        return accounts;
    }

    public async Task ForceRefreshAsync()
    {
        foreach (var service in _services)
        {
            try
            {
                await service.ForceRefreshAsync();
            }
            catch { }
        }
    }

    public void OpenSystemCalendarApp()
    {
        // 尝试打开第一个可用服务的日历应用
        foreach (var service in _services)
        {
            try
            {
                service.OpenSystemCalendarApp();
                return;
            }
            catch { }
        }
    }
}

using WinCal.Core.Models;

namespace WinCal.Core.Services;

/// <summary>
/// 日历服务接口
/// </summary>
public interface ICalendarService
{
    /// <summary>
    /// 获取指定时间范围内的事件列表
    /// </summary>
    Task<List<CalendarEvent>> GetEventsAsync(DateTime start, DateTime end);

    /// <summary>
    /// 检查日历服务是否可用
    /// </summary>
    Task<bool> IsAvailableAsync();

    /// <summary>
    /// 获取系统中已配置的日历账户列表
    /// </summary>
    Task<List<CalendarAccountInfo>> GetCalendarAccountsAsync();

    /// <summary>
    /// 强制刷新缓存（忽略缓存，重新从系统拉取）
    /// </summary>
    Task ForceRefreshAsync();

    /// <summary>
    /// 打开系统日历应用
    /// </summary>
    void OpenSystemCalendarApp();
}

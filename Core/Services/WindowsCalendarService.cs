using System.Diagnostics;
using Windows.ApplicationModel.Appointments;
using WinCal.Core.Models;

namespace WinCal.Core.Services;

/// <summary>
/// Windows 系统日历服务实现（带 10 分钟缓存）
/// </summary>
public class WindowsCalendarService : ICalendarService
{
    private List<CalendarEvent>? _cachedEvents;
    private DateTime _cacheTimestamp;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);
    private (DateTime start, DateTime end) _cacheRange;

    public async Task<List<CalendarEvent>> GetEventsAsync(DateTime start, DateTime end)
    {
        // 检查缓存是否有效（相同范围 + 未过期）
        if (_cachedEvents != null &&
            _cacheRange.start == start && _cacheRange.end == end &&
            DateTime.Now - _cacheTimestamp < _cacheDuration)
        {
            return _cachedEvents;
        }

        try
        {
            var store = await AppointmentManager.RequestStoreAsync(
                AppointmentStoreAccessType.AllCalendarsReadOnly);

            if (store == null)
            {
                Debug.WriteLine("WinCal: Calendar store is null (permission denied or unsupported).");
                return new List<CalendarEvent>();
            }

            // 先获取所有日历，建立 ID→颜色映射
            var calendarColors = new Dictionary<string, string>();
            try
            {
                var calendars = await store.FindAppointmentCalendarsAsync(
                    FindAppointmentCalendarsOptions.IncludeHidden);
                foreach (var cal in calendars)
                {
                    var color = cal.DisplayName.Contains("Outlook", StringComparison.OrdinalIgnoreCase)
                        ? "#0078D4"
                        : cal.DisplayName.Contains("Google", StringComparison.OrdinalIgnoreCase)
                            ? "#4285F4"
                            : cal.DisplayName.Contains("iCloud", StringComparison.OrdinalIgnoreCase)
                                ? "#FC3D39"
                                : "#0078D4";
                    calendarColors[cal.Id] = color;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WinCal: Failed to get calendar colors: {ex.Message}");
            }

            var options = new FindAppointmentsOptions
            {
                MaxCount = 200
            };

            var appointments = await store.FindAppointmentsAsync(start, end - start, options);

            var events = appointments.Select(a =>
            {
                // 尝试匹配日历颜色
                var color = calendarColors.TryGetValue(a.CalendarId ?? "", out var c) ? c : "#0078D4";
                return new CalendarEvent(
                    Title: a.Subject,
                    StartTime: a.StartTime.LocalDateTime,
                    EndTime: a.StartTime.LocalDateTime + a.Duration,
                    IsAllDay: a.AllDay,
                    CalendarName: a.CalendarId ?? string.Empty,
                    Color: color,
                    Location: a.Location ?? string.Empty,
                    Description: a.Details ?? string.Empty
                );
            }).ToList();

            // 更新缓存
            _cachedEvents = events;
            _cacheRange = (start, end);
            _cacheTimestamp = DateTime.Now;

            return events;
        }
        catch (UnauthorizedAccessException)
        {
            Debug.WriteLine("WinCal: Calendar permission denied.");
            return new List<CalendarEvent>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"WinCal: Calendar API error: {ex.Message}");
            return new List<CalendarEvent>();
        }
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            if (Environment.OSVersion.Version < new Version(10, 0, 18362))
                return false;

            var store = await AppointmentManager.RequestStoreAsync(
                AppointmentStoreAccessType.AllCalendarsReadOnly);
            return store != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<CalendarAccountInfo>> GetCalendarAccountsAsync()
    {
        try
        {
            var store = await AppointmentManager.RequestStoreAsync(
                AppointmentStoreAccessType.AllCalendarsReadOnly);
            if (store == null) return new List<CalendarAccountInfo>();

            var calendars = await store.FindAppointmentCalendarsAsync(
                FindAppointmentCalendarsOptions.IncludeHidden);

            return calendars
                .Where(c => !c.Hidden)
                .Select(c => new CalendarAccountInfo(
                    AccountName: ExtractAccountName(c.DisplayName),
                    CalendarName: c.DisplayName,
                    CalendarId: c.Id,
                    ColorHex: "#0078D4",
                    IsEnabled: true
                )).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"WinCal: GetCalendarAccounts error: {ex.Message}");
            return new List<CalendarAccountInfo>();
        }
    }

    public Task ForceRefreshAsync()
    {
        _cachedEvents = null;
        _cacheTimestamp = DateTime.MinValue;
        return Task.CompletedTask;
    }

    public void OpenSystemCalendarApp()
    {
        try
        {
            // 尝试打开 Windows 日历应用
            Process.Start(new ProcessStartInfo("outlookcal:") { UseShellExecute = true });
        }
        catch
        {
            try
            {
                // 备用：打开 Windows 设置中的日历
                Process.Start(new ProcessStartInfo("ms-settings:calendar") { UseShellExecute = true });
            }
            catch
            {
                Debug.WriteLine("WinCal: Cannot open system calendar app.");
            }
        }
    }

    private static string ExtractAccountName(string calendarDisplayName)
    {
        // 尝试从显示名称中提取账户名
        // 例如 "Calendar (user@outlook.com)" → "user@outlook.com"
        var match = System.Text.RegularExpressions.Regex.Match(
            calendarDisplayName, @"\(([^)]+)\)");
        return match.Success ? match.Groups[1].Value : calendarDisplayName;
    }
}

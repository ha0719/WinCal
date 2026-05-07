using System.Globalization;

namespace WinCal.Core.Helpers;

/// <summary>
/// 农历计算工具：基于 ChineseLunisolarCalendar
/// </summary>
public static class LunarCalendarHelper
{
    private static readonly ChineseLunisolarCalendar _calendar = new();

    // 农历月份名称
    private static readonly string[] _monthNames =
    {
        "", "正月", "二月", "三月", "四月", "五月", "六月",
        "七月", "八月", "九月", "十月", "冬月", "腊月"
    };

    // 农历日期名称
    private static readonly string[] _dayNames =
    {
        "", "初一", "初二", "初三", "初四", "初五", "初六", "初七", "初八", "初九", "初十",
        "十一", "十二", "十三", "十四", "十五", "十六", "十七", "十八", "十九", "二十",
        "廿一", "廿二", "廿三", "廿四", "廿五", "廿六", "廿七", "廿八", "廿九", "三十"
    };

    // 重要农历节日（月*100+日 -> 名称）
    private static readonly Dictionary<int, string> _festivals = new()
    {
        { 101, "春节" }, { 115, "元宵节" }, { 505, "端午节" },
        { 707, "七夕" }, { 815, "中秋节" }, { 909, "重阳节" },
        { 1230, "除夕" }
    };

    /// <summary>
    /// 获取指定日期的农历显示文本
    /// </summary>
    public static string GetLunarDateText(DateTime date)
    {
        try
        {
            // 检查日期是否在农历支持范围内
            if (date < _calendar.MinSupportedDateTime || date > _calendar.MaxSupportedDateTime)
                return string.Empty;

            int month = _calendar.GetMonth(date);
            int day = _calendar.GetDayOfMonth(date);

            // 检查是否为闰月
            int leapMonth = _calendar.GetLeapMonth(_calendar.GetYear(date));
            bool isLeapMonth = leapMonth > 0 && month == leapMonth;

            // 检查是否是节日
            int normalMonth = leapMonth > 0 && month >= leapMonth ? month - 1 : month;
            int key = normalMonth * 100 + day;

            if (_festivals.TryGetValue(key, out var festival))
                return festival;

            // 初一显示月份名，其他显示日期名
            if (day == 1)
            {
                string prefix = isLeapMonth ? "闰" : "";
                return prefix + _monthNames[normalMonth];
            }

            return _dayNames[day];
        }
        catch
        {
            return string.Empty;
        }
    }
}

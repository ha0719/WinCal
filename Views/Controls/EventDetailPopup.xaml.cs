using System.Windows;
using System.Windows.Controls;
using WinCal.Core.Models;
using WinCal.ViewModels;

namespace WinCal.Views.Controls;

public partial class EventDetailPopup : Border
{
    public EventDetailPopup()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 显示指定事件的详情
    /// </summary>
    public void ShowEvent(CalendarEvent evt)
    {
        DetailTitle.Text = evt.Title;
        DetailTime.Text = FormatDateAndTime(evt);

        // 地点
        if (!string.IsNullOrEmpty(evt.Location))
        {
            LocationPanel.Visibility = Visibility.Visible;
            DetailLocation.Text = evt.Location;
        }
        else
        {
            LocationPanel.Visibility = Visibility.Collapsed;
        }

        // 日历来源
        if (!string.IsNullOrEmpty(evt.CalendarName))
        {
            CalendarPanel.Visibility = Visibility.Visible;
            DetailCalendar.Text = evt.CalendarName;
        }
        else
        {
            CalendarPanel.Visibility = Visibility.Collapsed;
        }

        // 描述
        if (!string.IsNullOrEmpty(evt.Description))
        {
            DetailDescription.Visibility = Visibility.Visible;
            DetailDescription.Text = evt.Description;
        }
        else
        {
            DetailDescription.Visibility = Visibility.Collapsed;
        }

        Visibility = Visibility.Visible;
    }

    /// <summary>
    /// 格式化事件的日期和时间显示
    /// </summary>
    private static string FormatDateAndTime(CalendarEvent evt)
    {
        var dateStr = evt.StartTime.ToString("M月d日");

        if (evt.IsAllDay)
            return $"{dateStr}  全天";

        var timeStr = EventListViewModel.FormatEventTime(evt);
        return $"{dateStr}  {timeStr}";
    }

    /// <summary>
    /// 隐藏详情
    /// </summary>
    public void Hide()
    {
        Visibility = Visibility.Collapsed;
    }
}

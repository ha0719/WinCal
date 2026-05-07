using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WinCal.Core.Models;

namespace WinCal.Views.Controls;

public partial class DayCell : UserControl
{
    public DayCell()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 绑定的日历日数据
    /// </summary>
    public static readonly DependencyProperty DayDataProperty =
        DependencyProperty.Register(
            nameof(DayData), typeof(CalendarDay), typeof(DayCell),
            new PropertyMetadata(null, OnDayDataChanged));

    public CalendarDay? DayData
    {
        get => (CalendarDay?)GetValue(DayDataProperty);
        set => SetValue(DayDataProperty, value);
    }

    /// <summary>
    /// 是否被选中
    /// </summary>
    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected), typeof(bool), typeof(DayCell),
            new PropertyMetadata(false, OnIsSelectedChanged));

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// 日期被点击时触发的事件
    /// </summary>
    public event RoutedEventHandler? DayClicked;

    private static void OnDayDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var cell = (DayCell)d;
        if (e.NewValue is not CalendarDay day) return;

        // 日期数字
        cell.DayText.Text = day.Date.Day.ToString();

        // 今日高亮
        cell.TodayCircle.Visibility = day.IsToday ? Visibility.Visible : Visibility.Collapsed;
        cell.DayText.Foreground = day.IsToday
            ? FindResource(cell, "TextOnAccentBrush") as Brush
            : day.IsCurrentMonth
                ? FindResource(cell, "TextPrimaryBrush") as Brush
                : FindResource(cell, "TextDisabledBrush") as Brush;

        cell.DayText.FontWeight = day.IsToday ? FontWeights.Bold : FontWeights.Normal;

        // 农历
        cell.LunarText.Text = day.LunarDate;
        cell.LunarText.Visibility = string.IsNullOrEmpty(day.LunarDate)
            ? Visibility.Collapsed
            : Visibility.Visible;

        // 事件圆点 - 最多3个，按日历去重显示不同颜色
        if (day.HasEvents && day.Events.Count > 0)
        {
            // 按颜色去重，最多3种颜色
            var distinctColors = day.Events
                .Select(e => e.Color)
                .Distinct()
                .Take(3)
                .ToList();

            cell.EventDots.Visibility = Visibility.Visible;
            var dots = new[] { cell.Dot1, cell.Dot2, cell.Dot3 };
            for (int i = 0; i < dots.Length; i++)
            {
                if (i < distinctColors.Count)
                {
                    dots[i].Visibility = Visibility.Visible;
                    try
                    {
                        var brush = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString(distinctColors[i]));
                        brush.Freeze();
                        dots[i].Fill = brush;
                    }
                    catch
                    {
                        dots[i].Fill = FindResource(cell, "EventDotBrush") as Brush;
                    }
                }
                else
                {
                    dots[i].Visibility = Visibility.Collapsed;
                }
            }
        }
        else
        {
            cell.EventDots.Visibility = Visibility.Collapsed;
            cell.Dot1.Visibility = Visibility.Collapsed;
            cell.Dot2.Visibility = Visibility.Collapsed;
            cell.Dot3.Visibility = Visibility.Collapsed;
        }
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var cell = (DayCell)d;
        bool isSelected = (bool)e.NewValue;

        // 选中状态：显示蓝色边框圆圈（仅非今日时显示，今日有自己的填充圆）
        if (cell.DayData is { IsToday: false })
        {
            cell.SelectedCircle.Visibility = isSelected ? Visibility.Visible : Visibility.Collapsed;
            cell.DayText.Foreground = isSelected
                ? FindResource(cell, "TodayAccentBrush") as Brush
                : cell.DayData is { IsCurrentMonth: true }
                    ? FindResource(cell, "TextPrimaryBrush") as Brush
                    : FindResource(cell, "TextDisabledBrush") as Brush;
        }
    }

    private void OnDayClick(object sender, MouseButtonEventArgs e)
    {
        DayClicked?.Invoke(this, new RoutedEventArgs { Source = this });
    }

    private static object? FindResource(DependencyObject obj, string key)
    {
        return Application.Current.TryFindResource(key);
    }
}

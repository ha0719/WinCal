using System.Windows;
using Microsoft.Win32;
using WinCal.Core.Services;

namespace WinCal.Core.Helpers;

/// <summary>
/// 主题切换帮助类
/// </summary>
public static class ThemeHelper
{
    private static readonly string LightThemeUri = "Views/Themes/Light.xaml";
    private static readonly string DarkThemeUri = "Views/Themes/Dark.xaml";

    /// <summary>
    /// 根据设置应用主题
    /// </summary>
    public static void ApplyTheme(ThemeMode mode)
    {
        var isDark = mode switch
        {
            ThemeMode.Dark => true,
            ThemeMode.Light => false,
            _ => IsSystemDarkMode() // FollowSystem
        };

        SetTheme(isDark);
    }

    /// <summary>
    /// 检测 Windows 系统是否为深色模式
    /// </summary>
    public static bool IsSystemDarkMode()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            return value is int intValue && intValue == 0;
        }
        catch
        {
            return false; // 默认浅色
        }
    }

    /// <summary>
    /// 切换应用资源字典
    /// </summary>
    private static void SetTheme(bool isDark)
    {
        var app = Application.Current;
        if (app == null) return;

        var themeUri = isDark ? DarkThemeUri : LightThemeUri;

        var dicts = app.Resources.MergedDictionaries;

        // 移除旧主题（Source 会变成 pack:// URI，所以用 Contains 匹配）
        for (var i = dicts.Count - 1; i >= 0; i--)
        {
            var source = dicts[i].Source?.OriginalString;
            if (source != null &&
                (source.Contains("Light.xaml") || source.Contains("Dark.xaml")))
            {
                dicts.RemoveAt(i);
            }
        }

        // 添加新主题
        dicts.Add(new ResourceDictionary
        {
            Source = new Uri(themeUri, UriKind.Relative)
        });
    }
}

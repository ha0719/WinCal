using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WinCal.Core.Helpers;

/// <summary>
/// 弹出窗口定位工具：使用 Win32 API 获取正确的显示器工作区，处理 DPI 缩放
/// </summary>
public static class WindowPositionHelper
{
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    /// <summary>
    /// 将窗口定位到任务栏右下角上方，同时限制最大高度不超出屏幕。
    /// 必须在窗口 Show() 之后调用（需要窗口句柄和 DPI 信息）。
    /// </summary>
    public static void PositionNearTaskbar(Window window)
    {
        var source = PresentationSource.FromVisual(window);
        if (source?.CompositionTarget == null) return;

        // DPI 缩放因子：物理像素 → DIPs
        double dpiScaleX = source.CompositionTarget.TransformFromDevice.M11;
        double dpiScaleY = source.CompositionTarget.TransformFromDevice.M22;

        // 获取窗口所在显示器的句柄
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero) return;

        var hMonitor = MonitorFromWindow(hwnd, 2 /* MONITOR_DEFAULTTONEAREST */);
        var monitorInfo = new MONITORINFO { cbSize = Marshal.SizeOf(typeof(MONITORINFO)) };

        double workRight, workBottom, workTop;

        if (GetMonitorInfo(hMonitor, ref monitorInfo))
        {
            // Win32 返回的是物理像素，转为 DIPs
            workRight = monitorInfo.rcWork.Right * dpiScaleX;
            workBottom = monitorInfo.rcWork.Bottom * dpiScaleY;
            workTop = monitorInfo.rcWork.Top * dpiScaleY;
        }
        else
        {
            // 回退
            var workArea = SystemParameters.WorkArea;
            workRight = workArea.Right;
            workBottom = workArea.Bottom;
            workTop = workArea.Top;
        }

        // 可用高度（工作区顶部到底部）
        double availableHeight = workBottom - workTop;
        double margin = 8; // 底部留白

        // 限制窗口最大高度不超过可用高度
        double maxHeight = availableHeight - margin;
        if (window.MaxHeight > 0 && window.MaxHeight < maxHeight)
        {
            // XAML 中设置的 MaxHeight 更小则尊重它
            maxHeight = window.MaxHeight;
        }
        window.MaxHeight = maxHeight;

        // 使用窗口实际渲染尺寸
        double windowWidth = window.ActualWidth;
        double windowHeight = window.ActualHeight;

        if (windowWidth <= 0 || windowHeight <= 0)
        {
            window.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            windowWidth = window.DesiredSize.Width;
            windowHeight = window.DesiredSize.Height;
        }

        // 确保不会超出最大高度
        windowHeight = Math.Min(windowHeight, maxHeight);

        double left = workRight - windowWidth - 12;
        double top = workBottom - windowHeight - margin;

        if (left < 0) left = 4;
        if (top < workTop) top = workTop;

        window.Left = left;
        window.Top = top;
    }
}

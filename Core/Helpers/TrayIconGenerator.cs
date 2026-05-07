using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows;

namespace WinCal.Core.Helpers;

/// <summary>
/// 动态生成带日期数字的系统托盘图标
/// </summary>
public static class TrayIconGenerator
{
    /// <summary>
    /// 生成一个带日期数字的图标
    /// </summary>
    /// <param name="day">日期数字（1~31）</param>
    /// <returns>System.Drawing.Icon 用于托盘显示</returns>
    public static Icon Generate(int day)
    {
        int size = 32; // 托盘图标标准尺寸（会自动缩放到 16x16）

        using var bitmap = new Bitmap(size, size);
        using var graphics = Graphics.FromImage(bitmap);

        // 高质量渲染
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

        // 透明背景
        graphics.Clear(Color.Transparent);

        // 绘制圆形背景
        using var bgBrush = new SolidBrush(Color.FromArgb(37, 37, 38)); // 深色背景
        graphics.FillEllipse(bgBrush, 1, 1, size - 2, size - 2);

        // 绘制边框
        using var borderPen = new Pen(Color.FromArgb(100, 100, 100), 1);
        graphics.DrawEllipse(borderPen, 1, 1, size - 3, size - 3);

        // 绘制日期数字
        string text = day.ToString();
        using var font = new Font("Segoe UI", text.Length > 1 ? 11f : 14f, System.Drawing.FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.White);

        var textSize = graphics.MeasureString(text, font);
        float x = (size - textSize.Width) / 2;
        float y = (size - textSize.Height) / 2;

        graphics.DrawString(text, font, textBrush, x, y);

        // 转换为 Icon
        var handle = bitmap.GetHicon();
        return Icon.FromHandle(handle);
    }
}

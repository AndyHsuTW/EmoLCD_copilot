using System;
using EmoLcd.Rendering.Primitives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace EmoLcd.Tests;

/// <summary>
/// FaceCanvas 幾何繪製功能測試，驗證各種圖形 primitive 的正確性。
/// </summary>
/// <remarks>
/// 測試範圍包含：
/// <list type="bullet">
///   <item>DrawCircle：圓形繪製（用於眼睛）</item>
///   <item>DrawLine：直線繪製（用於眉毛、中性嘴巴）</item>
///   <item>DrawQuadratic：貝茲曲線繪製（用於彎曲嘴巴）</item>
/// </list>
/// </remarks>
public class FaceCanvasTests
{
    private static readonly Rgba32 Stroke = new(0, 0, 0, 255);
    private static readonly Rgba32 Background = new(255, 255, 255, 255);

    /// <summary>
    /// 驗證 DrawCircle 繪製的圓形不超出指定半徑範圍。
    /// </summary>
    /// <remarks>
    /// 驗證項目：
    /// <list type="number">
    ///   <item>繪製後有前景像素存在</item>
    ///   <item>前景像素的 X/Y 範圍符合 [center - radius, center + radius]</item>
    /// </list>
    /// </remarks>
    [Fact]
    public void DrawCircle_RespectsRadiusBounds()
    {
        using var image = new Image<Rgba32>(80, 80);
        var canvas = new FaceCanvas(image, Stroke);
        canvas.Clear(Background);

        const int radius = 12;
        canvas.DrawCircle((40, 40), radius, radius);

        var pixels = ForegroundPixels(image).ToList();
        Assert.NotEmpty(pixels);
        Assert.Equal(40 - radius, pixels.Min(p => p.Item1));
        Assert.Equal(40 + radius, pixels.Max(p => p.Item1));
        Assert.Equal(40 - radius, pixels.Min(p => p.Item2));
        Assert.Equal(40 + radius, pixels.Max(p => p.Item2));
    }

    /// <summary>
    /// 驗證 DrawLine 繪製的直線能覆蓋起始點與結束點。
    /// </summary>
    /// <remarks>
    /// 使用 Bresenham 演算法繪製，驗證線段兩端點附近都有像素被填色。
    /// </remarks>
    [Fact]
    public void DrawLine_CoversBothEndpoints()
    {
        using var image = new Image<Rgba32>(64, 64);
        var canvas = new FaceCanvas(image, Stroke);
        canvas.Clear(Background);

        var start = (X: 10, Y: 10);
        var end = (X: 50, Y: 30);
        canvas.DrawLine(start, end, thickness: 3);

        var pixels = ForegroundPixels(image).ToList();
        Assert.NotEmpty(pixels);
        Assert.Contains(pixels, p => Math.Abs(p.Item1 - start.X) <= 1 && Math.Abs(p.Item2 - start.Y) <= 1);
        Assert.Contains(pixels, p => Math.Abs(p.Item1 - end.X) <= 1 && Math.Abs(p.Item2 - end.Y) <= 1);
    }

    /// <summary>
    /// 驗證 DrawQuadratic 繪製的貝茲曲線能形成弧形且覆蓋兩端點。
    /// </summary>
    /// <remarks>
    /// 驗證項目：
    /// <list type="number">
    ///   <item>曲線上有像素位於控制點方向（弧形彎曲）</item>
    ///   <item>起始點與結束點附近都有像素</item>
    /// </list>
    /// </remarks>
    [Fact]
    public void DrawQuadratic_ProducesArcAboveBaseline()
    {
        using var image = new Image<Rgba32>(80, 80);
        var canvas = new FaceCanvas(image, Stroke);
        canvas.Clear(Background);

        var start = (X: 15, Y: 50);
        var control = (X: 40, Y: 20);
        var end = (X: 65, Y: 50);
        canvas.DrawQuadratic(start, control, end, thickness: 4, samples: 24);

        var pixels = ForegroundPixels(image).ToList();
        Assert.NotEmpty(pixels);
        Assert.Contains(pixels, p => p.Item2 < 40);
        Assert.Contains(pixels, p => Math.Abs(p.Item1 - start.X) <= 1 && Math.Abs(p.Item2 - start.Y) <= 1);
        Assert.Contains(pixels, p => Math.Abs(p.Item1 - end.X) <= 1 && Math.Abs(p.Item2 - end.Y) <= 1);
    }

    /// <summary>
    /// 列舉圖像中所有前景（筆劃色）像素的座標。
    /// </summary>
    /// <param name="image">要掃描的圖像。</param>
    /// <returns>前景像素座標的集合。</returns>
    private static IEnumerable<(int X, int Y)> ForegroundPixels(Image<Rgba32> image)
    {
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                if (image[x, y].Equals(Stroke))
                {
                    yield return (X: x, Y: y);
                }
            }
        }
    }
}

using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EmoLcd.Rendering.Primitives;

/// <summary>
/// 提供基礎幾何 primitives，讓表情繪製時能以一致邏輯操作像素。
/// </summary>
/// <remarks>
/// 此類別封裝了繪製表情所需的基本圖形操作：
/// <list type="bullet">
///   <item>圓形（眼睛）</item>
///   <item>直線（眉毛、中性嘴巴）</item>
///   <item>二次貝茲曲線（彎曲嘴巴）</item>
/// </list>
/// 所有座標皆以像素為單位，原點在畫布左上角。
/// </remarks>
public sealed class FaceCanvas
{
    private readonly Image<Rgba32> _surface;
    private readonly int _width;
    private readonly int _height;
    private readonly Rgba32 _strokeColor;

    /// <summary>
    /// 初始化 <see cref="FaceCanvas"/> 的新執行個體。
    /// </summary>
    /// <param name="surface">要繪製的目標圖像，不可為 null。</param>
    /// <param name="strokeColor">繪製線條的顏色（通常為黑色）。</param>
    /// <exception cref="ArgumentNullException"><paramref name="surface"/> 為 null。</exception>
    public FaceCanvas(Image<Rgba32> surface, Rgba32 strokeColor)
    {
        _surface = surface ?? throw new ArgumentNullException(nameof(surface));
        _width = surface.Width;
        _height = surface.Height;
        _strokeColor = strokeColor;
    }

    /// <summary>
    /// 以指定顏色填滿整個畫布。
    /// </summary>
    /// <param name="color">填滿顏色（通常為白色背景）。</param>
    public void Clear(Rgba32 color)
    {
        _surface.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < _height; y++)
            {
                var row = accessor.GetRowSpan(y);
                row.Fill(color);
            }
        });
    }

    /// <summary>
    /// 繪製空心圓形（用於眼睛）。
    /// </summary>
    /// <param name="center">圓心座標。</param>
    /// <param name="radius">外圓半徑（像素）。</param>
    /// <param name="strokeThickness">圓環線條粗細（像素）。若等於 radius 則為實心圓。</param>
    /// <remarks>
    /// 使用距離平方判斷法繪製圓環，避免浮點運算提升效能。
    /// 像素位於 [inner, outer] 半徑範圍內時會被填色。
    /// </remarks>
    public void DrawCircle((int X, int Y) center, int radius, int strokeThickness)
    {
        if (radius <= 0 || strokeThickness <= 0)
        {
            return;
        }

        int outer = Math.Max(1, radius);
        int inner = Math.Max(0, outer - strokeThickness);
        int outerSq = outer * outer;
        int innerSq = inner * inner;

        for (int y = center.Y - outer; y <= center.Y + outer; y++)
        {
            int dy = y - center.Y;
            for (int x = center.X - outer; x <= center.X + outer; x++)
            {
                int dx = x - center.X;
                int distSq = dx * dx + dy * dy;
                if (distSq <= outerSq && distSq >= innerSq)
                {
                    SetPixel(x, y);
                }
            }
        }
    }

    /// <summary>
    /// 繪製直線（用於眉毛或中性嘴巴）。
    /// </summary>
    /// <param name="start">線段起始點座標。</param>
    /// <param name="end">線段結束點座標。</param>
    /// <param name="thickness">線條粗細（像素）。</param>
    /// <remarks>
    /// 使用 Bresenham 直線演算法沿線段路徑繪製，
    /// 每個路徑點以 <paramref name="thickness"/> 為直徑的方形填色。
    /// </remarks>
    public void DrawLine((int X, int Y) start, (int X, int Y) end, int thickness)
    {
        if (thickness <= 0)
        {
            return;
        }

        int x0 = start.X;
        int y0 = start.Y;
        int x1 = end.X;
        int y1 = end.Y;

        int dx = Math.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        while (true)
        {
            DrawThickPoint(x0, y0, thickness);
            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            int e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x0 += sx;
            }
            if (e2 <= dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    /// <summary>
    /// 繪製二次貝茲曲線（用於彎曲嘴巴）。
    /// </summary>
    /// <param name="p0">曲線起始點。</param>
    /// <param name="p1">控制點，決定曲線彎曲方向與幅度。</param>
    /// <param name="p2">曲線結束點。</param>
    /// <param name="thickness">線條粗細（像素）。</param>
    /// <param name="samples">取樣點數，數值越大曲線越平滑。</param>
    /// <remarks>
    /// 使用二次貝茲曲線公式：B(t) = (1-t)²P₀ + 2(1-t)tP₁ + t²P₂，
    /// 其中 t 從 0 到 1 均勻取樣 <paramref name="samples"/> 個點。
    /// </remarks>
    public void DrawQuadratic((int X, int Y) p0, (int X, int Y) p1, (int X, int Y) p2, int thickness, int samples)
    {
        if (thickness <= 0 || samples <= 0)
        {
            return;
        }

        for (int i = 0; i <= samples; i++)
        {
            double t = (double)i / samples;
            double mt = 1 - t;
            int x = (int)(mt * mt * p0.X + 2 * mt * t * p1.X + t * t * p2.X);
            int y = (int)(mt * mt * p0.Y + 2 * mt * t * p1.Y + t * t * p2.Y);
            DrawThickPoint(x, y, thickness);
        }
    }

    /// <summary>
    /// 以指定點為中心繪製粗點（方形填色區域）。
    /// </summary>
    /// <param name="cx">中心點 X 座標。</param>
    /// <param name="cy">中心點 Y 座標。</param>
    /// <param name="thickness">填色區域的邊長（像素）。</param>
    private void DrawThickPoint(int cx, int cy, int thickness)
    {
        int radius = Math.Max(1, thickness) / 2;
        for (int y = cy - radius; y <= cy + radius; y++)
        {
            for (int x = cx - radius; x <= cx + radius; x++)
            {
                SetPixel(x, y);
            }
        }
    }

    /// <summary>
    /// 設定單一像素的顏色，超出畫布邊界時自動忽略。
    /// </summary>
    /// <param name="x">像素 X 座標。</param>
    /// <param name="y">像素 Y 座標。</param>
    private void SetPixel(int x, int y)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
        {
            return;
        }

        _surface[x, y] = _strokeColor;
    }
}

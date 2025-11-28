using EmoLcd.Domain.Models;
using EmoLcd.Rendering.Display;
using EmoLcd.Rendering.Expressions;
using EmoLcd.Rendering.Primitives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EmoLcd.Rendering;

/// <summary>
/// 渲染管線，負責協調表情繪製與輸出目標之間的流程。
/// </summary>
/// <remarks>
/// 此類別為渲染流程的核心，職責包括：
/// <list type="bullet">
///   <item>根據指定表情在記憶體中繪製 RGBA32 圖像</item>
///   <item>將像素資料傳遞給 <see cref="IDisplayTarget"/> 進行輸出</item>
/// </list>
/// </remarks>
public class RenderPipeline
{
    /// <summary>
    /// 執行表情渲染，並透過指定的輸出目標完成最終輸出。
    /// </summary>
    /// <param name="request">渲染請求參數，包含表情類型、輸出目標與尺寸設定。</param>
    /// <param name="displayTarget">輸出目標實作，決定像素資料如何被寫出（LCD 或 PNG）。</param>
    /// <returns>渲染結果，包含耗時與輸出位置等資訊。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="displayTarget"/> 為 null。</exception>
    /// <exception cref="ArgumentOutOfRangeException">指定的表情類型不在 <see cref="ExpressionCatalog"/> 中。</exception>
    public RenderResult Render(RenderRequest request, IDisplayTarget displayTarget)
    {
        ArgumentNullException.ThrowIfNull(displayTarget);

        using var image = DrawExpression(request.Emotion, request.Width, request.Height);
        var pixels = new Rgba32[request.Width * request.Height];
        image.CopyPixelDataTo(pixels);
        return displayTarget.Render(pixels, request);
    }

    /// <summary>
    /// 根據表情類型在記憶體中繪製圖像。
    /// </summary>
    /// <param name="emotion">要繪製的表情類型。</param>
    /// <param name="width">畫布寬度（像素）。</param>
    /// <param name="height">畫布高度（像素）。</param>
    /// <returns>繪製完成的 RGBA32 圖像，呼叫端負責 Dispose。</returns>
    /// <exception cref="ArgumentOutOfRangeException">表情類型不在目錄中。</exception>
    private static Image<Rgba32> DrawExpression(Emotion emotion, int width, int height)
    {
        if (!ExpressionCatalog.TryGet(emotion, out var shape))
        {
            throw new ArgumentOutOfRangeException(nameof(emotion), "不支援的表情");
        }

        var image = new Image<Rgba32>(width, height);
        var white = new Rgba32(255, 255, 255, 255);
        var black = new Rgba32(0, 0, 0, 255);
        var canvas = new FaceCanvas(image, black);

        canvas.Clear(white);
        canvas.DrawCircle(shape.LeftEye, shape.EyeRadius, shape.EyeStroke);
        canvas.DrawCircle(shape.RightEye, shape.EyeRadius, shape.EyeStroke);

        var mouthStart = shape.MouthStart;
        var mouthEnd = shape.MouthEnd;
        if (shape.MouthCurveOffset != 0)
        {
            var control = (
                (mouthStart.X + mouthEnd.X) / 2,
                (mouthStart.Y + mouthEnd.Y) / 2 + shape.MouthCurveOffset);
            canvas.DrawQuadratic(mouthStart, control, mouthEnd, shape.MouthThickness, shape.MouthSamples);
        }
        else
        {
            canvas.DrawLine(mouthStart, mouthEnd, shape.MouthThickness);
        }

        if (shape.LeftBrow is { } leftBrow)
        {
            canvas.DrawLine(leftBrow.Start, leftBrow.End, shape.BrowThickness);
        }

        if (shape.RightBrow is { } rightBrow)
        {
            canvas.DrawLine(rightBrow.Start, rightBrow.End, shape.BrowThickness);
        }

        return image;
    }
}

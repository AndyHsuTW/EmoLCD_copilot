using System.Diagnostics;
using EmoLcd.Domain.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EmoLcd.Rendering.Display;

/// <summary>
/// Dry-run 輸出目標，將表情圖像儲存為 PNG 檔案。
/// </summary>
/// <remarks>
/// 此實作用於以下場景：
/// <list type="bullet">
///   <item>無 LCD 硬體的開發環境測試</item>
///   <item>產生離線預覽圖片進行視覺驗證</item>
///   <item>當 framebuffer 寫入失敗時的 fallback 輸出</item>
/// </list>
/// 輸出格式為標準 PNG，像素格式保持 RGBA32。
/// </remarks>
public sealed class DryRunDisplayTarget : IDisplayTarget
{
    /// <inheritdoc />
    public RenderTarget Target => RenderTarget.DryRun;

    /// <summary>
    /// 將像素資料儲存為 PNG 檔案。
    /// </summary>
    /// <param name="pixels">RGBA32 格式的像素陣列。</param>
    /// <param name="request">渲染請求，其中 <see cref="RenderRequest.OutputPath"/> 指定輸出檔案路徑。</param>
    /// <returns>渲染結果，<see cref="RenderResult.OutputPath"/> 為實際儲存的檔案路徑。</returns>
    public RenderResult Render(ReadOnlyMemory<Rgba32> pixels, RenderRequest request)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(request.OutputPath) ?? ".");
        var stopwatch = Stopwatch.StartNew();

        using (var image = Image.LoadPixelData(pixels.Span, request.Width, request.Height))
        {
            image.SaveAsPng(request.OutputPath);
        }

        stopwatch.Stop();

        return new RenderResult
        {
            Emotion = request.Emotion,
            Target = RenderTarget.DryRun,
            DurationMs = stopwatch.ElapsedMilliseconds,
            OutputPath = request.OutputPath
        };
    }
}

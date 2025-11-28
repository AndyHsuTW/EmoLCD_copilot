using System.Diagnostics;
using EmoLcd.Domain.Models;
using EmoLcd.Rendering.Framebuffer;
using EmoLcd.Rendering.Pixels;
using SixLabors.ImageSharp.PixelFormats;

namespace EmoLcd.Rendering.Display;

/// <summary>
/// Framebuffer 輸出目標，將表情圖像直接寫入 Linux framebuffer 裝置。
/// </summary>
/// <remarks>
/// 此實作專為 Raspberry Pi + Waveshare 3.5" LCD 設計：
/// <list type="bullet">
///   <item>將 RGBA32 像素轉換為 RGB565 格式（5-6-5 bit packing, Little Endian）</item>
///   <item>透過 memory-mapped I/O 寫入 /dev/fb0</item>
///   <item>寫入前會驗證 framebuffer 幾何契約（480×320, 16bpp）</item>
/// </list>
/// </remarks>
public sealed class FramebufferDisplayTarget : IDisplayTarget
{
    private readonly FramebufferWriter _writer;

    /// <summary>
    /// 初始化 <see cref="FramebufferDisplayTarget"/> 的新執行個體。
    /// </summary>
    public FramebufferDisplayTarget()
    {
        _writer = new FramebufferWriter();
    }

    /// <inheritdoc />
    public RenderTarget Target => RenderTarget.Lcd;

    /// <summary>
    /// 將像素資料轉換為 RGB565 並寫入 framebuffer。
    /// </summary>
    /// <param name="pixels">RGBA32 格式的像素陣列。</param>
    /// <param name="request">渲染請求，其中 <see cref="RenderRequest.FramebufferPath"/> 指定裝置路徑。</param>
    /// <returns>渲染結果，<see cref="RenderResult.OutputPath"/> 為 framebuffer 裝置路徑。</returns>
    /// <exception cref="FramebufferContractException">framebuffer 幾何或像素格式不符合契約。</exception>
    /// <exception cref="FileNotFoundException">找不到 framebuffer 裝置。</exception>
    public RenderResult Render(ReadOnlyMemory<Rgba32> pixels, RenderRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var buffer = Rgb565Converter.ToRgb565(pixels.Span, request.Width, request.Height);
        _writer.Write(buffer, request.FramebufferPath, request.Width, request.Height);
        stopwatch.Stop();

        return new RenderResult
        {
            Emotion = request.Emotion,
            Target = RenderTarget.Lcd,
            DurationMs = stopwatch.ElapsedMilliseconds,
            OutputPath = request.FramebufferPath
        };
    }
}

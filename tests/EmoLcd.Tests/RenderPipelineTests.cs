using EmoLcd.Domain.Models;
using EmoLcd.Rendering;
using EmoLcd.Rendering.Display;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace EmoLcd.Tests;

/// <summary>
/// RenderPipeline 單元測試，驗證渲染管線的資料傳遞與協調正確性。
/// </summary>
/// <remarks>
/// 測試重點：
/// <list type="bullet">
///   <item>像素資料能正確傳遞給 IDisplayTarget</item>
///   <item>RenderRequest 的元資料能正確傳遞</item>
///   <item>RenderResult 能正確反映執行結果</item>
/// </list>
/// </remarks>
public class RenderPipelineTests
{
    /// <summary>
    /// 驗證 Render() 能正確傳遞像素資料與元資料給 IDisplayTarget。
    /// </summary>
    /// <remarks>
    /// 使用假的 IDisplayTarget 實作捕捉傳入的參數，驗證：
    /// <list type="number">
    ///   <item>像素數量符合 Width × Height</item>
    ///   <item>RenderRequest 被完整傳遞</item>
    ///   <item>RenderResult 反映正確的表情與輸出路徑</item>
    /// </list>
    /// </remarks>
    [Fact]
    public void Render_Forwards_Pixels_And_Metadata()
    {
        var pipeline = new RenderPipeline();
        var fakeTarget = new FakeDisplayTarget();
        var request = new RenderRequest
        {
            Emotion = Emotion.Smile,
            Target = RenderTarget.DryRun,
            Width = 64,
            Height = 64,
            OutputPath = Path.Combine(Path.GetTempPath(), $"render_{Guid.NewGuid():N}.png")
        };

        var result = pipeline.Render(request, fakeTarget);

        Assert.Same(request, fakeTarget.CapturedRequest);
        Assert.Equal(request.Width * request.Height, fakeTarget.CapturedPixelCount);
        Assert.Equal(fakeTarget.Target, result.Target);
        Assert.Equal(request.Emotion, result.Emotion);
        Assert.Equal(request.OutputPath, result.OutputPath);
    }

    /// <summary>
    /// 假的 IDisplayTarget 實作，用於捕捉傳入的參數以便驗證。
    /// </summary>
    private sealed class FakeDisplayTarget : IDisplayTarget
    {
        /// <summary>捕捉的 RenderRequest。</summary>
        public RenderRequest? CapturedRequest { get; private set; }

        /// <summary>捕捉的像素數量。</summary>
        public int CapturedPixelCount { get; private set; }

        /// <inheritdoc />
        public RenderTarget Target => RenderTarget.DryRun;

        /// <inheritdoc />
        public RenderResult Render(ReadOnlyMemory<Rgba32> pixels, RenderRequest request)
        {
            CapturedRequest = request;
            CapturedPixelCount = pixels.Length;
            return new RenderResult
            {
                Emotion = request.Emotion,
                Target = Target,
                DurationMs = 1,
                OutputPath = request.OutputPath
            };
        }
    }
}

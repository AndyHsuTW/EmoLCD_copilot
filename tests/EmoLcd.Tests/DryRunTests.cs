using EmoLcd.Domain.Models;
using EmoLcd.Rendering;
using EmoLcd.Rendering.Display;
using Xunit;

namespace EmoLcd.Tests;

/// <summary>
/// Dry-run 輸出模式測試，驗證 PNG 檔案產生與效能門檻。
/// </summary>
/// <remarks>
/// 對應 User Story 2：產生離線預覽圖片。
/// 驗證重點：
/// <list type="bullet">
///   <item>所有支援的表情都能成功產生 PNG 檔案</item>
///   <item>渲染耗時在 3 秒內完成</item>
/// </list>
/// </remarks>
public class DryRunTests
{
    /// <summary>
    /// 驗證各種表情的 dry-run 輸出能成功產生 PNG 並在時限內完成。
    /// </summary>
    /// <param name="emotion">要測試的表情類型。</param>
    /// <remarks>
    /// 驗證項目：
    /// <list type="number">
    ///   <item>RenderResult.Target 為 DryRun</item>
    ///   <item>輸出的 PNG 檔案確實存在</item>
    ///   <item>渲染耗時不超過 3000ms</item>
    /// </list>
    /// </remarks>
    [Theory]
    [InlineData(Emotion.Neutral)]
    [InlineData(Emotion.Smile)]
    [InlineData(Emotion.Angry)]
    public void DryRun_Generates_Png_And_Within_Time(Emotion emotion)
    {
        var pipeline = new RenderPipeline();
        var tmp = Path.Combine(Path.GetTempPath(), $"emotion_{emotion}_{Guid.NewGuid():N}.png");

        try
        {
            var result = pipeline.Render(new RenderRequest
            {
                Emotion = emotion,
                Target = RenderTarget.DryRun,
                OutputPath = tmp
            }, new DryRunDisplayTarget());

            Assert.Equal(RenderTarget.DryRun, result.Target);
            Assert.True(File.Exists(tmp));
            Assert.True(result.DurationMs <= 3000, $"dry-run 耗時超過 3 秒：{result.DurationMs}ms");
        }
        finally
        {
            if (File.Exists(tmp))
            {
                File.Delete(tmp);
            }
        }
    }
}

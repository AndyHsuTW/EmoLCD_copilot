using System.Security.Cryptography;
using EmoLcd.Domain.Models;
using EmoLcd.Rendering;
using EmoLcd.Rendering.Display;
using Xunit;

namespace EmoLcd.Tests;

/// <summary>
/// LCD 表情切換整合測試，驗證不同表情產生不同輸出。
/// </summary>
/// <remarks>
/// 對應 User Story 1 的無殘影驗證：
/// 確保切換表情時，前後兩幀的輸出內容完全不同。
/// </remarks>
public class LcdSwitchIntegrationTests
{
    /// <summary>
    /// 驗證不同表情產生不同的輸出內容（以 SHA256 比對）。
    /// </summary>
    /// <remarks>
    /// 此測試確保：
    /// <list type="bullet">
    ///   <item>Smile 與 Angry 表情的輸出檔案都存在</item>
    ///   <item>兩個檔案的 SHA256 雜湊值不同</item>
    /// </list>
    /// </remarks>
    [Fact]
    public void Different_Emotions_Produce_Different_Output()
    {
        var pipeline = new RenderPipeline();
        var tmp1 = Path.Combine(Path.GetTempPath(), $"emotion_{Guid.NewGuid():N}_1.png");
        var tmp2 = Path.Combine(Path.GetTempPath(), $"emotion_{Guid.NewGuid():N}_2.png");

        try
        {
            var r1 = pipeline.Render(new RenderRequest
            {
                Emotion = Emotion.Smile,
                Target = RenderTarget.DryRun,
                OutputPath = tmp1
            }, new DryRunDisplayTarget());

            var r2 = pipeline.Render(new RenderRequest
            {
                Emotion = Emotion.Angry,
                Target = RenderTarget.DryRun,
                OutputPath = tmp2
            }, new DryRunDisplayTarget());

            Assert.Equal(RenderTarget.DryRun, r1.Target);
            Assert.Equal(RenderTarget.DryRun, r2.Target);
            Assert.True(File.Exists(tmp1));
            Assert.True(File.Exists(tmp2));

            var hash1 = Sha256(tmp1);
            var hash2 = Sha256(tmp2);
            Assert.NotEqual(hash1, hash2); // 表情不同應有不同輸出
        }
        finally
        {
            if (File.Exists(tmp1)) File.Delete(tmp1);
            if (File.Exists(tmp2)) File.Delete(tmp2);
        }
    }

    /// <summary>
    /// 計算檔案的 SHA256 雜湊值。
    /// </summary>
    /// <param name="path">檔案路徑。</param>
    /// <returns>十六進位的雜湊字串。</returns>
    private static string Sha256(string path)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(path);
        var hash = sha.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }
}

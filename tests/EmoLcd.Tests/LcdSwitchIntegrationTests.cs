using System.Security.Cryptography;
using EmoLcd.Domain.Models;
using EmoLcd.Rendering;
using Xunit;

namespace EmoLcd.Tests;

public class LcdSwitchIntegrationTests
{
    [Fact]
    public void Different_Emotions_Produce_Different_Output()
    {
        var pipeline = new RenderPipeline();
        var tmp1 = Path.Combine(Path.GetTempPath(), $"emotion_{Guid.NewGuid():N}_1.png");
        var tmp2 = Path.Combine(Path.GetTempPath(), $"emotion_{Guid.NewGuid():N}_2.png");

        try
        {
            var r1 = pipeline.RenderToFile(new RenderRequest
            {
                Emotion = Emotion.Smile,
                Target = RenderTarget.DryRun,
                OutputPath = tmp1
            });

            var r2 = pipeline.RenderToFile(new RenderRequest
            {
                Emotion = Emotion.Angry,
                Target = RenderTarget.DryRun,
                OutputPath = tmp2
            });

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

    private static string Sha256(string path)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(path);
        var hash = sha.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }
}

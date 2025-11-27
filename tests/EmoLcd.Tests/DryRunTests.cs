using EmoLcd.Domain.Models;
using EmoLcd.Rendering;
using Xunit;

namespace EmoLcd.Tests;

public class DryRunTests
{
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
            var result = pipeline.RenderToFile(new RenderRequest
            {
                Emotion = emotion,
                Target = RenderTarget.DryRun,
                OutputPath = tmp
            });

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

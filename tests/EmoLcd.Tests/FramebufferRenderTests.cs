using EmoLcd.Domain.Models;
using EmoLcd.Rendering;
using Xunit;

namespace EmoLcd.Tests;

public class FramebufferRenderTests
{
    [Fact]
    public void Writes_Buffer_With_Correct_Length_And_Content()
    {
        var pipeline = new RenderPipeline();
        var width = 480;
        var height = 320;
        var bufferSize = width * height * 2;

        var tempPath = Path.Combine(Path.GetTempPath(), $"fb_{Guid.NewGuid():N}.bin");
        File.WriteAllBytes(tempPath, new byte[bufferSize]);

        try
        {
            var request = new RenderRequest
            {
                Emotion = Emotion.Smile,
                Target = RenderTarget.Lcd,
                FramebufferPath = tempPath,
                Width = width,
                Height = height
            };

            var result = pipeline.RenderToFramebuffer(request);

            Assert.Equal(RenderTarget.Lcd, result.Target);
            Assert.Equal(bufferSize, new FileInfo(tempPath).Length);

            var bytes = File.ReadAllBytes(tempPath);
            Assert.Contains(bytes, b => b != 0); // 應有畫素資料
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}

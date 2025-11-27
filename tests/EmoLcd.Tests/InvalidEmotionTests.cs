using EmoLcd.App;
using Xunit;

namespace EmoLcd.Tests;

public class InvalidEmotionTests
{
    [Fact]
    public void Invalid_Emotion_Returns_Error_And_No_Output()
    {
        var tmp = Path.Combine(Path.GetTempPath(), $"invalid_{Guid.NewGuid():N}.png");
        try
        {
            var code = Program.Main(new[]
            {
                "--emotion", "Unknown",
                "--target", "DryRun",
                "--output", tmp
            });

            Assert.Equal(1, code);
            Assert.False(File.Exists(tmp));
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

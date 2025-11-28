using EmoLcd.App;
using Xunit;

namespace EmoLcd.Tests;

/// <summary>
/// 無效輸入防護測試，驗證無效表情不會產生副作用。
/// </summary>
/// <remarks>
/// 對應 User Story 3：防範無效請求。
/// 驗證重點：
/// <list type="bullet">
///   <item>無效表情回傳非零退出碼</item>
///   <item>無效表情不會產生輸出檔案</item>
/// </list>
/// </remarks>
public class InvalidEmotionTests
{
    /// <summary>
    /// 驗證無效表情會回傳錯誤碼且不產生輸出。
    /// </summary>
    /// <remarks>
    /// 驗證項目：
    /// <list type="number">
    ///   <item>退出碼為 1（表示錯誤）</item>
    ///   <item>指定的輸出檔案不存在（無副作用）</item>
    /// </list>
    /// </remarks>
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

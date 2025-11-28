using System;
using System.IO;
using EmoLcd.App;
using EmoLcd.Domain.Models;
using Xunit;

namespace EmoLcd.Tests;

/// <summary>
/// CLI 參數解析測試，驗證 Program.ParseArgs() 的各種輸入情境。
/// </summary>
/// <remarks>
/// 測試範圍包含：
/// <list type="bullet">
///   <item>必要參數缺失時的錯誤處理</item>
///   <item>無效 framebuffer/輸出路徑的驗證</item>
///   <item>大小寫不敏感的列舉解析</item>
///   <item>有效輸入的正確解析</item>
/// </list>
/// </remarks>
public class ProgramCliTests
{
    /// <summary>
    /// 驗證缺少 --emotion 參數時回傳失敗。
    /// </summary>
    [Fact]
    public void MissingEmotion_ReturnsFailure()
    {
        var args = new[]
        {
            "--target", "DryRun",
            "--output", TempOutputPath()
        };

        var result = Program.ParseArgs(args);

        Assert.False(result.Ok);
        Assert.Contains("--emotion", result.ErrorMessage);
        Assert.True(result.ShowAllowed);
    }

    /// <summary>
    /// 驗證缺少 --target 參數時回傳失敗。
    /// </summary>
    [Fact]
    public void MissingTarget_ReturnsFailure()
    {
        var args = new[]
        {
            "--emotion", Emotion.Smile.ToString(),
            "--output", TempOutputPath()
        };

        var result = Program.ParseArgs(args);

        Assert.False(result.Ok);
        Assert.Contains("--target", result.ErrorMessage);
    }

    /// <summary>
    /// 驗證當 framebuffer 路徑不存在時回傳失敗。
    /// </summary>
    [Fact]
    public void InvalidFramebufferPath_ReturnsFailure()
    {
        var invalidPath = Path.Combine(Path.GetTempPath(), $"fb_missing_{Guid.NewGuid():N}");
        var args = new[]
        {
            "--emotion", Emotion.Angry.ToString(),
            "--target", RenderTarget.Lcd.ToString(),
            "--framebuffer", invalidPath
        };

        var result = Program.ParseArgs(args);

        Assert.False(result.Ok);
        Assert.Contains("framebuffer", result.ErrorMessage);
    }

    /// <summary>
    /// 驗證當輸出目錄不存在時回傳失敗。
    /// </summary>
    [Fact]
    public void InvalidOutputDirectory_ReturnsFailure()
    {
        var invalidOutput = Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}", "emotion.png");
        var args = new[]
        {
            "--emotion", Emotion.Neutral.ToString(),
            "--target", RenderTarget.DryRun.ToString(),
            "--output", invalidOutput
        };

        var result = Program.ParseArgs(args);

        Assert.False(result.Ok);
        Assert.Contains("輸出資料夾不存在", result.ErrorMessage);
    }

    /// <summary>
    /// 驗證列舉參數能以大小寫不敏感的方式解析。
    /// </summary>
    [Fact]
    public void MixedCaseEnums_AreAccepted()
    {
        var args = new[]
        {
            "--emotion", "sMiLe",
            "--target", "dryRun",
            "--output", TempOutputPath()
        };

        var result = Program.ParseArgs(args);

        Assert.True(result.Ok);
        Assert.Equal(Emotion.Smile, result.Emotion);
        Assert.Equal(RenderTarget.DryRun, result.Target);
    }

    /// <summary>
    /// 驗證有效的 framebuffer 路徑能通過驗證。
    /// </summary>
    [Fact]
    public void ValidFramebuffer_PassesValidation()
    {
        var framebuffer = Path.GetTempFileName();
        try
        {
            var args = new[]
            {
                "--emotion", Emotion.Neutral.ToString(),
                "--target", RenderTarget.Lcd.ToString(),
                "--framebuffer", framebuffer
            };

            var result = Program.ParseArgs(args);

            Assert.True(result.Ok);
            Assert.Equal(framebuffer, result.Framebuffer);
        }
        finally
        {
            if (File.Exists(framebuffer))
            {
                File.Delete(framebuffer);
            }
        }
    }

    /// <summary>
    /// 產生暫時輸出檔案路徑。
    /// </summary>
    /// <returns>唯一的暫時檔案路徑。</returns>
    private static string TempOutputPath() =>
        Path.Combine(Path.GetTempPath(), $"emotion_{Guid.NewGuid():N}.png");
}

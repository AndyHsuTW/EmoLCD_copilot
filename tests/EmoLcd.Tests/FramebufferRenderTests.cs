using EmoLcd.Domain.Models;
using EmoLcd.Rendering;
using EmoLcd.Rendering.Display;
using Xunit;

namespace EmoLcd.Tests;

/// <summary>
/// Framebuffer 渲染整合測試，驗證完整的 LCD 輸出流程。
/// </summary>
/// <remarks>
/// 對應 User Story 1：在 LCD 顯示指定表情。
/// 驗證重點：
/// <list type="bullet">
///   <item>完整渲染管線能寫入正確長度的 RGB565 資料</item>
///   <item>輸出檔案含有非零像素（表示有繪製內容）</item>
/// </list>
/// </remarks>
[Collection("FramebufferTests")]
public class FramebufferRenderTests
{
    /// <summary>
    /// 驗證完整渲染流程寫入正確長度與內容的 RGB565 資料。
    /// </summary>
    /// <remarks>
    /// 驗證項目：
    /// <list type="number">
    ///   <item>RenderResult.Target 為 Lcd</item>
    ///   <item>輸出檔案大小符合契約（width × height × 2 bytes）</item>
    ///   <item>檔案內容含有非零位元組</item>
    /// </list>
    /// </remarks>
    [Fact]
    public void Writes_Buffer_With_Correct_Length_And_Content()
    {
        var pipeline = new RenderPipeline();
        var width = 480;
        var height = 320;

        using var fake = FakeFramebufferEnvironment.Create(width, height, 16);
        var tempPath = fake.FramebufferPath;

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

            var result = pipeline.Render(request, new FramebufferDisplayTarget());

            Assert.Equal(RenderTarget.Lcd, result.Target);
            Assert.Equal(fake.ExpectedBytes, new FileInfo(tempPath).Length);

            var bytes = File.ReadAllBytes(tempPath);
            Assert.Contains(bytes, b => b != 0); // 應有畫素資料
        }
        finally
        {
            // fake 環境由 FakeFramebufferEnvironment.Dispose() 清理
        }
    }
}

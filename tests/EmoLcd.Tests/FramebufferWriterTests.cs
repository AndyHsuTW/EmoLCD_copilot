using EmoLcd.Rendering.Framebuffer;
using Xunit;

namespace EmoLcd.Tests;

/// <summary>
/// FramebufferWriter 寫入功能測試，驗證像素資料寫入 framebuffer 的正確性。
/// </summary>
/// <remarks>
/// 測試重點：
/// <list type="bullet">
///   <item>當資料長度符合契約時能正確寫入</item>
///   <item>當資料長度不符時拋出契約例外</item>
/// </list>
/// </remarks>
[Collection("FramebufferTests")]
public class FramebufferWriterTests
{
    /// <summary>
    /// 驗證當資料長度符合契約時，能完整寫入 framebuffer。
    /// </summary>
    [Fact]
    public void Write_Persists_Buffer_When_Matching_Contract()
    {
        using var fake = FakeFramebufferEnvironment.Create(32, 32, 16);
        var writer = new FramebufferWriter();
        Span<byte> data = stackalloc byte[fake.ExpectedBytes];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)(i % 255);
        }

        writer.Write(data, fake.FramebufferPath, fake.Width, fake.Height);

        var bytes = File.ReadAllBytes(fake.FramebufferPath);
        Assert.Equal(data.ToArray(), bytes);
    }

    /// <summary>
    /// 驗證當資料長度不符合契約時，會拋出 FramebufferContractException。
    /// </summary>
    [Fact]
    public void Write_Throws_When_Length_Mismatch()
    {
        using var fake = FakeFramebufferEnvironment.Create(16, 16, 16);
        var writer = new FramebufferWriter();
        var shortBuffer = new byte[10];

        Assert.Throws<FramebufferContractException>(() =>
            writer.Write(shortBuffer, fake.FramebufferPath, fake.Width, fake.Height));
    }
}

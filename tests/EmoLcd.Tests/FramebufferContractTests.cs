using EmoLcd.Rendering.Framebuffer;
using Xunit;

namespace EmoLcd.Tests;

/// <summary>
/// FramebufferContract 契約讀取測試，驗證從 sysfs 讀取 framebuffer 幾何資訊的正確性。
/// </summary>
/// <remarks>
/// 測試重點：
/// <list type="bullet">
///   <item>能正確解析 virtual_size 與 bits_per_pixel</item>
///   <item>sysfs 不存在時拋出適當例外</item>
/// </list>
/// </remarks>
[Collection("FramebufferTests")]
public class FramebufferContractTests
{
    /// <summary>
    /// 驗證 Load() 能從假 sysfs 正確讀取幾何資訊。
    /// </summary>
    [Fact]
    public void Load_ReadsGeometry_FromSysfs()
    {
        using var fake = FakeFramebufferEnvironment.Create(480, 320, 16);

        var contract = FramebufferContract.Load(fake.FramebufferPath);

        Assert.Equal(fake.Width, contract.Width);
        Assert.Equal(fake.Height, contract.Height);
        Assert.Equal(fake.BitsPerPixel, contract.BitsPerPixel);
        Assert.Equal(fake.ExpectedBytes, contract.ExpectedBytes);
    }

    /// <summary>
    /// 驗證當 sysfs 路徑不存在時會拋出 FramebufferContractException。
    /// </summary>
    [Fact]
    public void Load_Throws_WhenSysfsMissing()
    {
        using var fake = FakeFramebufferEnvironment.Create(64, 64, 16);
        var unknownPath = Path.Combine(Path.GetTempPath(), $"fb-missing_{Guid.NewGuid():N}");

        Assert.Throws<FramebufferContractException>(() => FramebufferContract.Load(unknownPath));
    }
}

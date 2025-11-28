using EmoLcd.Rendering.Framebuffer;

namespace EmoLcd.Tests;

/// <summary>
/// 假 framebuffer 環境，用於在無實際 LCD 硬體的情況下測試 framebuffer 相關功能。
/// </summary>
/// <remarks>
/// 此類別會在測試期間：
/// <list type="bullet">
///   <item>建立暫時目錄模擬 sysfs 結構（virtual_size, bits_per_pixel）</item>
///   <item>建立空白檔案模擬 framebuffer 裝置</item>
///   <item>設定 FRAMEBUFFER_SYS_PATH 環境變數指向假 sysfs</item>
///   <item>Dispose 時自動清理並還原環境變數</item>
/// </list>
/// 適用於 FramebufferContract、FramebufferWriter 等測試。
/// </remarks>
internal sealed class FakeFramebufferEnvironment : IDisposable
{
    private readonly string _root;
    private readonly string? _originalSysPath;

    private FakeFramebufferEnvironment(string root, string sysPath, string framebufferPath, int width, int height, int bitsPerPixel)
    {
        _root = root;
        SysPath = sysPath;
        FramebufferPath = framebufferPath;
        Width = width;
        Height = height;
        BitsPerPixel = bitsPerPixel;
        ExpectedBytes = checked(width * height * bitsPerPixel / 8);
        _originalSysPath = Environment.GetEnvironmentVariable("FRAMEBUFFER_SYS_PATH");
        Environment.SetEnvironmentVariable("FRAMEBUFFER_SYS_PATH", sysPath);
    }

    /// <summary>假 sysfs 路徑。</summary>
    public string SysPath { get; }

    /// <summary>假 framebuffer 檔案路徑。</summary>
    public string FramebufferPath { get; }

    /// <summary>模擬的畫布寬度。</summary>
    public int Width { get; }

    /// <summary>模擬的畫布高度。</summary>
    public int Height { get; }

    /// <summary>模擬的位元深度。</summary>
    public int BitsPerPixel { get; }

    /// <summary>預期的 framebuffer 位元組數。</summary>
    public int ExpectedBytes { get; }

    /// <summary>
    /// 建立指定幾何的假 framebuffer 環境。
    /// </summary>
    /// <param name="width">模擬寬度。</param>
    /// <param name="height">模擬高度。</param>
    /// <param name="bitsPerPixel">模擬位元深度。</param>
    /// <returns>已初始化的假環境執行個體。</returns>
    public static FakeFramebufferEnvironment Create(int width, int height, int bitsPerPixel)
    {
        var root = Path.Combine(Path.GetTempPath(), $"fbtest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        var sysPath = Path.Combine(root, "sys");
        Directory.CreateDirectory(sysPath);

        var fbName = $"fbtest_{Guid.NewGuid():N}";
        var fbSysPath = Path.Combine(sysPath, fbName);
        Directory.CreateDirectory(fbSysPath);

        File.WriteAllText(Path.Combine(fbSysPath, "virtual_size"), $"{width},{height}");
        File.WriteAllText(Path.Combine(fbSysPath, "bits_per_pixel"), bitsPerPixel.ToString());

        var framebufferPath = Path.Combine(root, fbName);
        File.WriteAllBytes(framebufferPath, new byte[checked(width * height * bitsPerPixel / 8)]);

        return new FakeFramebufferEnvironment(root, sysPath, framebufferPath, width, height, bitsPerPixel);
    }

    /// <summary>
    /// 清理假環境：還原環境變數並刪除暫時目錄。
    /// </summary>
    public void Dispose()
    {
        Environment.SetEnvironmentVariable("FRAMEBUFFER_SYS_PATH", _originalSysPath);
        try
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
        catch
        {
            // best-effort cleanup
        }
    }
}

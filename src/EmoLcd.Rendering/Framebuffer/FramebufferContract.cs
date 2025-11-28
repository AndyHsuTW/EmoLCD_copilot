using System.Globalization;

namespace EmoLcd.Rendering.Framebuffer;

/// <summary>
/// 封裝 Linux framebuffer 裝置的幾何契約資訊。
/// </summary>
/// <remarks>
/// 此類別透過讀取 /sys/class/graphics/{fbName}/ 下的系統檔案，
/// 取得 framebuffer 的解析度與每像素位元數，用於驗證寫入資料的正確性。
/// </remarks>
public sealed class FramebufferContract
{
    /// <summary>
    /// 預設的 sysfs 路徑，用於讀取 framebuffer 幾何資訊。
    /// </summary>
    private const string DefaultSysPath = "/sys/class/graphics";

    /// <summary>
    /// 初始化 <see cref="FramebufferContract"/> 實例。
    /// </summary>
    /// <param name="framebufferPath">framebuffer 裝置路徑，例如 /dev/fb0。</param>
    /// <param name="width">螢幕寬度（像素）。</param>
    /// <param name="height">螢幕高度（像素）。</param>
    /// <param name="bitsPerPixel">每像素位元數。</param>
    private FramebufferContract(string framebufferPath, int width, int height, int bitsPerPixel)
    {
        FramebufferPath = framebufferPath;
        Width = width;
        Height = height;
        BitsPerPixel = bitsPerPixel;
    }

    /// <summary>
    /// 取得 framebuffer 裝置路徑。
    /// </summary>
    public string FramebufferPath { get; }

    /// <summary>
    /// 取得螢幕寬度（像素）。
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// 取得螢幕高度（像素）。
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// 取得每像素位元數，例如 RGB565 為 16。
    /// </summary>
    public int BitsPerPixel { get; }

    /// <summary>
    /// 取得預期的 framebuffer 資料大小（位元組）。
    /// </summary>
    /// <remarks>
    /// 計算公式：Width × Height × (BitsPerPixel / 8)。
    /// </remarks>
    public int ExpectedBytes => checked(Width * Height * BitsPerPixel / 8);

    /// <summary>
    /// 從系統檔案載入指定 framebuffer 的幾何契約資訊。
    /// </summary>
    /// <param name="framebufferPath">framebuffer 裝置路徑，例如 /dev/fb0。</param>
    /// <returns>已載入幾何資訊的 <see cref="FramebufferContract"/> 實例。</returns>
    /// <exception cref="FramebufferContractException">
    /// 當路徑為空、找不到 sysfs 幾何檔案、或格式無法解析時拋出。
    /// </exception>
    /// <remarks>
    /// 此方法會讀取 /sys/class/graphics/{fbName}/virtual_size 與 bits_per_pixel 檔案。
    /// 可透過環境變數 FRAMEBUFFER_SYS_PATH 覆寫預設的 sysfs 路徑（主要供測試使用）。
    /// </remarks>
    public static FramebufferContract Load(string framebufferPath)
    {
        if (string.IsNullOrWhiteSpace(framebufferPath))
        {
            throw new FramebufferContractException("framebuffer 路徑不可為空");
        }

        var fbName = Path.GetFileName(framebufferPath);
        if (string.IsNullOrWhiteSpace(fbName))
        {
            throw new FramebufferContractException($"無法解析 framebuffer 名稱：{framebufferPath}");
        }

        var sysPath = Environment.GetEnvironmentVariable("FRAMEBUFFER_SYS_PATH") ?? DefaultSysPath;
        var fbSysPath = Path.Combine(sysPath, fbName);
        var virtualSizeFile = Path.Combine(fbSysPath, "virtual_size");
        var bitsPerPixelFile = Path.Combine(fbSysPath, "bits_per_pixel");

        if (!File.Exists(virtualSizeFile) || !File.Exists(bitsPerPixelFile))
        {
            throw new FramebufferContractException($"找不到 framebuffer 幾何資料：{fbSysPath}");
        }

        var sizeText = File.ReadAllText(virtualSizeFile).Trim();
        var sizeParts = sizeText
            .Split(new[] { ',', 'x', 'X' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (sizeParts.Length < 2)
        {
            throw new FramebufferContractException($"virtual_size 格式錯誤：{sizeText}");
        }

        if (!int.TryParse(sizeParts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var width) ||
            !int.TryParse(sizeParts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var height))
        {
            throw new FramebufferContractException($"virtual_size 無法解析：{sizeText}");
        }

        var bppText = File.ReadAllText(bitsPerPixelFile).Trim();
        if (!int.TryParse(bppText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var bitsPerPixel))
        {
            throw new FramebufferContractException($"bits_per_pixel 無法解析：{bppText}");
        }

        if (bitsPerPixel % 8 != 0)
        {
            throw new FramebufferContractException($"bits_per_pixel 需為 8 的倍數，實際 {bitsPerPixel}");
        }

        return new FramebufferContract(framebufferPath, width, height, bitsPerPixel);
    }
}

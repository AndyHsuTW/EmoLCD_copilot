using System.IO.MemoryMappedFiles;

namespace EmoLcd.Rendering.Framebuffer;

/// <summary>
/// 負責將像素資料寫入 Linux framebuffer 裝置。
/// </summary>
/// <remarks>
/// 此類別使用 Memory-Mapped File 技術，將 RGB565 格式的像素資料
/// 直接寫入 framebuffer，以達到高效能的螢幕更新。
/// </remarks>
public class FramebufferWriter
{
    /// <summary>
    /// 將像素資料寫入指定的 framebuffer 裝置。
    /// </summary>
    /// <param name="buffer">RGB565 格式的像素資料緩衝區。</param>
    /// <param name="framebufferPath">framebuffer 裝置路徑，例如 /dev/fb0。</param>
    /// <param name="width">預期的螢幕寬度（像素）。</param>
    /// <param name="height">預期的螢幕高度（像素）。</param>
    /// <exception cref="FileNotFoundException">當 framebuffer 裝置不存在時拋出。</exception>
    /// <exception cref="FramebufferContractException">
    /// 當幾何尺寸不符或資料長度與契約不一致時拋出。
    /// </exception>
    /// <remarks>
    /// 寫入流程：
    /// 1. 驗證 framebuffer 裝置存在
    /// 2. 載入 framebuffer 契約並驗證幾何資訊
    /// 3. 驗證緩衝區大小與契約一致
    /// 4. 透過 Memory-Mapped File 寫入像素資料
    /// </remarks>
    public void Write(ReadOnlySpan<byte> buffer, string framebufferPath, int width, int height)
    {
        if (!File.Exists(framebufferPath))
        {
            throw new FileNotFoundException("找不到 framebuffer 裝置或檔案", framebufferPath);
        }

        var contract = FramebufferContract.Load(framebufferPath);
        if (contract.Width != width || contract.Height != height)
        {
            throw new FramebufferContractException(
                $"framebuffer 幾何不符：契約 {contract.Width}x{contract.Height}，實際 {width}x{height}");
        }

        if (buffer.Length != contract.ExpectedBytes)
        {
            throw new FramebufferContractException(
                $"像素資料長度不符：契約 {contract.ExpectedBytes} bytes，實際 {buffer.Length} bytes");
        }

        using var mmf = MemoryMappedFile.CreateFromFile(
            framebufferPath,
            FileMode.Open,
            null,
            contract.ExpectedBytes,
            MemoryMappedFileAccess.ReadWrite);

        using var stream = mmf.CreateViewStream(0, contract.ExpectedBytes, MemoryMappedFileAccess.Write);
        stream.Write(buffer);
    }
}

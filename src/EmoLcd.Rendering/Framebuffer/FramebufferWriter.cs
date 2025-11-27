using System.IO.MemoryMappedFiles;

namespace EmoLcd.Rendering.Framebuffer;

public class FramebufferWriter
{
    public void Write(ReadOnlySpan<byte> buffer, string framebufferPath)
    {
        if (!File.Exists(framebufferPath))
        {
            throw new FileNotFoundException("找不到 framebuffer 裝置或檔案", framebufferPath);
        }

        using var mmf = MemoryMappedFile.CreateFromFile(
            framebufferPath,
            FileMode.Open,
            null,
            buffer.Length,
            MemoryMappedFileAccess.ReadWrite);

        using var accessor = mmf.CreateViewAccessor(0, buffer.Length, MemoryMappedFileAccess.Write);
        accessor.WriteArray(0, buffer.ToArray(), 0, buffer.Length);
    }
}

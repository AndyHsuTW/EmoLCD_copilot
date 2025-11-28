using SixLabors.ImageSharp.PixelFormats;

namespace EmoLcd.Rendering.Pixels;

/// <summary>
/// RGB565 像素格式轉換器，將 RGBA32 像素資料轉換為 LCD framebuffer 所需的 RGB565 格式。
/// </summary>
/// <remarks>
/// RGB565 格式說明：
/// <list type="bullet">
///   <item>每像素 16 bits（2 bytes）</item>
///   <item>Red: 5 bits（bit 11-15）</item>
///   <item>Green: 6 bits（bit 5-10）</item>
///   <item>Blue: 5 bits（bit 0-4）</item>
///   <item>位元組序：Little Endian（低位元組在前）</item>
/// </list>
/// 此格式對應 <c>fbset</c> 輸出：<c>rgba 5/11,6/5,5/0,0/0</c>。
/// </remarks>
public static class Rgb565Converter
{
    /// <summary>
    /// 將 RGBA32 像素陣列轉換為 RGB565 位元組陣列。
    /// </summary>
    /// <param name="pixels">RGBA32 格式的像素資料。</param>
    /// <param name="width">圖像寬度（像素）。</param>
    /// <param name="height">圖像高度（像素）。</param>
    /// <returns>RGB565 格式的位元組陣列，長度為 width × height × 2。</returns>
    /// <exception cref="ArgumentException">像素數量與 width × height 不符。</exception>
    /// <remarks>
    /// 轉換公式：
    /// <code>
    /// R5 = R8 >> 3  // 取高 5 位
    /// G6 = G8 >> 2  // 取高 6 位
    /// B5 = B8 >> 3  // 取高 5 位
    /// packed = (R5 << 11) | (G6 << 5) | B5
    /// </code>
    /// </remarks>
    public static byte[] ToRgb565(ReadOnlySpan<Rgba32> pixels, int width, int height)
    {
        var expected = width * height;
        if (pixels.Length != expected)
        {
            throw new ArgumentException($"像素數量不符，預期 {expected}，實際 {pixels.Length}");
        }

        var output = new byte[expected * 2];
        int o = 0;
        foreach (var p in pixels)
        {
            // 5-6-5 packing，Little Endian
            var r = p.R >> 3;
            var g = p.G >> 2;
            var b = p.B >> 3;
            ushort packed = (ushort)((r << 11) | (g << 5) | b);
            output[o++] = (byte)(packed & 0xFF);
            output[o++] = (byte)(packed >> 8);
        }
        return output;
    }
}

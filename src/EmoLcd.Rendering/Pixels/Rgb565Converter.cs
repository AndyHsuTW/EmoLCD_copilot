using SixLabors.ImageSharp.PixelFormats;

namespace EmoLcd.Rendering.Pixels;

public static class Rgb565Converter
{
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

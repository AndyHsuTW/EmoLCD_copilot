using System.Diagnostics;
using EmoLcd.Domain.Models;
using EmoLcd.Rendering.Expressions;
using EmoLcd.Rendering.Framebuffer;
using EmoLcd.Rendering.Pixels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EmoLcd.Rendering;

public class RenderPipeline
{
    private readonly FramebufferWriter _framebufferWriter = new();

    public RenderResult RenderToFramebuffer(RenderRequest request)
    {
        var emotion = request.Emotion;
        using var image = DrawExpression(emotion, request.Width, request.Height);
        var pixels = new Rgba32[request.Width * request.Height];
        image.CopyPixelDataTo(pixels);
        var buffer = Rgb565Converter.ToRgb565(pixels, request.Width, request.Height);

        var start = Stopwatch.GetTimestamp();
        _framebufferWriter.Write(buffer, request.FramebufferPath);
        var duration = ElapsedMs(start);

        return new RenderResult
        {
            Emotion = emotion,
            Target = RenderTarget.Lcd,
            DurationMs = duration,
            OutputPath = request.FramebufferPath
        };
    }

    public RenderResult RenderToFile(RenderRequest request)
    {
        var emotion = request.Emotion;
        using var image = DrawExpression(emotion, request.Width, request.Height);

        var start = Stopwatch.GetTimestamp();
        image.SaveAsPng(request.OutputPath);
        var duration = ElapsedMs(start);

        return new RenderResult
        {
            Emotion = emotion,
            Target = RenderTarget.DryRun,
            DurationMs = duration,
            OutputPath = request.OutputPath
        };
    }

    private static long ElapsedMs(long start)
    {
        var elapsed = Stopwatch.GetTimestamp() - start;
        return elapsed * 1000 / Stopwatch.Frequency;
    }

    private static Image<Rgba32> DrawExpression(Emotion emotion, int width, int height)
    {
        if (!ExpressionCatalog.TryGet(emotion, out var shape))
        {
            throw new ArgumentOutOfRangeException(nameof(emotion), "不支援的表情");
        }

        var image = new Image<Rgba32>(width, height);
        var white = new Rgba32(255, 255, 255, 255);
        var black = new Rgba32(0, 0, 0, 255);

        // 填滿背景
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < width; x++)
                {
                    row[x] = white;
                }
            }
        });

        void DrawCircle((int X, int Y) center, int radius)
        {
            int r2 = radius * radius;
            for (int y = center.Y - radius; y <= center.Y + radius; y++)
            {
                if (y < 0 || y >= height) continue;
                int dy = y - center.Y;
                for (int x = center.X - radius; x <= center.X + radius; x++)
                {
                    if (x < 0 || x >= width) continue;
                    int dx = x - center.X;
                    if (dx * dx + dy * dy <= r2)
                    {
                        image[x, y] = black;
                    }
                }
            }
        }

        void DrawLine((int X, int Y) start, (int X, int Y) end, int thickness = 3)
        {
            int dx = Math.Abs(end.X - start.X), sx = start.X < end.X ? 1 : -1;
            int dy = -Math.Abs(end.Y - start.Y), sy = start.Y < end.Y ? 1 : -1;
            int err = dx + dy;
            int x = start.X, y = start.Y;
            while (true)
            {
                DrawThickPoint(x, y, thickness);
                if (x == end.X && y == end.Y) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x += sx; }
                if (e2 <= dx) { err += dx; y += sy; }
            }
        }

        void DrawThickPoint(int cx, int cy, int thickness)
        {
            int r = thickness / 2;
            for (int y = cy - r; y <= cy + r; y++)
            {
                if (y < 0 || y >= height) continue;
                for (int x = cx - r; x <= cx + r; x++)
                {
                    if (x < 0 || x >= width) continue;
                    image[x, y] = black;
                }
            }
        }

        void DrawQuadratic((int X, int Y) p0, (int X, int Y) p1, (int X, int Y) p2, int thickness, int samples)
        {
            for (int i = 0; i <= samples; i++)
            {
                double t = (double)i / samples;
                double mt = 1 - t;
                var x = (int)(mt * mt * p0.X + 2 * mt * t * p1.X + t * t * p2.X);
                var y = (int)(mt * mt * p0.Y + 2 * mt * t * p1.Y + t * t * p2.Y);
                DrawThickPoint(x, y, thickness);
            }
        }

        // 畫眼睛
        DrawCircle(shape.LeftEye, 12);
        DrawCircle(shape.RightEye, 12);

        // 畫嘴巴
        var mouthStart = shape.MouthStart;
        var mouthEnd = shape.MouthEnd;
        if (shape.MouthCurveOffset != 0)
        {
            var mid = (
                (mouthStart.X + mouthEnd.X) / 2,
                (mouthStart.Y + mouthEnd.Y) / 2 + shape.MouthCurveOffset);
            DrawQuadratic(mouthStart, mid, mouthEnd, 4, samples: 40);
        }
        else
        {
            DrawLine(mouthStart, mouthEnd, 4);
        }

        // 眉毛（僅怒或需要時繪製）
        if (shape.LeftBrow is { } lb)
        {
            DrawLine(lb.Start, lb.End, 3);
        }
        if (shape.RightBrow is { } rb)
        {
            DrawLine(rb.Start, rb.End, 3);
        }

        return image;
    }
}

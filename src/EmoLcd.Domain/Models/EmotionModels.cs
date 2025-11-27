namespace EmoLcd.Domain.Models;

public enum Emotion
{
    Neutral,
    Smile,
    Angry
}

public enum RenderTarget
{
    Lcd,
    DryRun
}

public sealed class RenderRequest
{
    public Emotion Emotion { get; init; } = Emotion.Neutral;
    public RenderTarget Target { get; init; } = RenderTarget.Lcd;
    public string FramebufferPath { get; init; } = "/dev/fb0";
    public string OutputPath { get; init; } = "/tmp/emotion.png";
    public int Width { get; init; } = 480;
    public int Height { get; init; } = 320;
}

public sealed class RenderResult
{
    public Emotion Emotion { get; init; }
    public RenderTarget Target { get; init; }
    public long DurationMs { get; init; }
    public string OutputPath { get; init; } = string.Empty;
}

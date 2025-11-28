namespace EmoLcd.Domain.Models;

/// <summary>
/// 表情類型列舉，定義 LCD 可顯示的臉部表情。
/// </summary>
public enum Emotion
{
    /// <summary>
    /// 中性表情：水平直線嘴巴，無眉毛。
    /// </summary>
    Neutral,

    /// <summary>
    /// 微笑表情：向上彎曲的嘴巴，無眉毛。
    /// </summary>
    Smile,

    /// <summary>
    /// 憤怒表情：向下彎曲的嘴巴，搭配向內下斜的眉毛。
    /// </summary>
    Angry
}

/// <summary>
/// 渲染輸出目標列舉，決定表情圖像的輸出方式。
/// </summary>
public enum RenderTarget
{
    /// <summary>
    /// LCD 模式：將 RGB565 像素資料直接寫入 Linux framebuffer（/dev/fb0）。
    /// </summary>
    Lcd,

    /// <summary>
    /// Dry-run 模式：將圖像儲存為 PNG 檔案，用於測試或無硬體環境預覽。
    /// </summary>
    DryRun
}

/// <summary>
/// 渲染請求參數，封裝單次表情渲染所需的所有輸入。
/// </summary>
public sealed class RenderRequest
{
    /// <summary>
    /// 要渲染的表情類型。預設為 <see cref="Emotion.Neutral"/>。
    /// </summary>
    public Emotion Emotion { get; init; } = Emotion.Neutral;

    /// <summary>
    /// 輸出目標。預設為 <see cref="RenderTarget.Lcd"/>。
    /// </summary>
    public RenderTarget Target { get; init; } = RenderTarget.Lcd;

    /// <summary>
    /// Framebuffer 裝置路徑，僅在 <see cref="RenderTarget.Lcd"/> 模式下使用。
    /// </summary>
    public string FramebufferPath { get; init; } = "/dev/fb0";

    /// <summary>
    /// PNG 輸出檔案路徑，僅在 <see cref="RenderTarget.DryRun"/> 模式下使用。
    /// </summary>
    public string OutputPath { get; init; } = "/tmp/emotion.png";

    /// <summary>
    /// 畫布寬度（像素）。預設為 480，對應 Waveshare 3.5" LCD。
    /// </summary>
    public int Width { get; init; } = 480;

    /// <summary>
    /// 畫布高度（像素）。預設為 320，對應 Waveshare 3.5" LCD。
    /// </summary>
    public int Height { get; init; } = 320;
}

/// <summary>
/// 渲染結果，封裝單次表情渲染的輸出資訊。
/// </summary>
public sealed class RenderResult
{
    /// <summary>
    /// 實際渲染的表情類型。
    /// </summary>
    public Emotion Emotion { get; init; }

    /// <summary>
    /// 實際使用的輸出目標。
    /// </summary>
    public RenderTarget Target { get; init; }

    /// <summary>
    /// 渲染耗時（毫秒）。
    /// </summary>
    public long DurationMs { get; init; }

    /// <summary>
    /// 輸出位置：Lcd 模式為 framebuffer 路徑，DryRun 模式為 PNG 檔案路徑。
    /// </summary>
    public string OutputPath { get; init; } = string.Empty;
}

using EmoLcd.Domain.Models;
using SixLabors.ImageSharp.PixelFormats;

namespace EmoLcd.Rendering.Display;

/// <summary>
/// 定義渲染輸出目標的契約介面。
/// </summary>
/// <remarks>
/// 實作此介面的類別負責將像素資料輸出至特定目標：
/// <list type="bullet">
///   <item><see cref="FramebufferDisplayTarget"/>：寫入 Linux framebuffer（/dev/fb0）</item>
///   <item><see cref="DryRunDisplayTarget"/>：儲存為 PNG 檔案</item>
/// </list>
/// 此設計符合 DIP（依賴反轉原則），讓 <see cref="RenderPipeline"/> 不直接依賴具體實作。
/// </remarks>
public interface IDisplayTarget
{
    /// <summary>
    /// 取得此輸出目標對應的 <see cref="RenderTarget"/> 列舉值。
    /// </summary>
    RenderTarget Target { get; }

    /// <summary>
    /// 將像素資料渲染至目標輸出。
    /// </summary>
    /// <param name="pixels">RGBA32 格式的像素陣列，長度應為 Width × Height。</param>
    /// <param name="request">渲染請求參數，包含輸出路徑與尺寸資訊。</param>
    /// <returns>渲染結果，包含耗時與實際輸出位置。</returns>
    RenderResult Render(ReadOnlyMemory<Rgba32> pixels, RenderRequest request);
}

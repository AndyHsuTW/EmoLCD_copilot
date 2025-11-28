using Xunit;

namespace EmoLcd.Tests;

/// <summary>
/// 為何這個檔案存在？
/// </summary>
/// <remarks>
/// 此文件不是測試本身，而是為一組會共享系統資源的測試提供運行規則。
///
/// 主要目的：
/// - 防止 Framebuffer 相關測試（會建立/設定假 sysfs 與環境變數、或直接寫入暫存 framebuffer 檔案）
///   在 xUnit 平行執行時互相干擾，導致 race condition 或非確定性錯誤。
/// - 讓這些測試以隔離且可預測的方式串列執行，確保環境變數與暫存檔案的建立/清理行為不會互相衝突，提升測試穩定性。
///
/// 為何要保留此檔案：
/// - 它明確地在測試層級宣告了執行規則（collection 與禁用平行化），屬於測試運行機制的一部分；
///   移除或修改它會改變測試的執行行為，可能導致難以重現的失敗。
///
/// 注意：此檔案刻意保持精簡（僅宣告 collection），用途在於定義測試執行策略，而非包含測試邏輯。
/// </remarks>
[CollectionDefinition("FramebufferTests", DisableParallelization = true)]
public sealed class FramebufferTestCollection
{
}

# EmoLCD Copilot 說明

本檔案說明本專案目前的目的、硬體環境與預期程式架構，協助 AI 代理在尚未完全成形的程式碼庫中，依照既有規劃往正確方向擴充。

## 專案概觀
- 目標：在 Raspberry Pi 5 上，使用 C#/.NET 透過 Linux framebuffer `/dev/fb0` 在 Waveshare 3.5" SPI LCD 顯示簡單表情（Smile、Angry、Neutral 等）。
- 目前狀態：只有規劃與環境說明（`README.md`），尚未建立 `src/` 下的 .NET 專案與實作程式碼。
- 硬體：Raspberry Pi 5 + Waveshare 3.5" LCD (B) v2，解析度固定 480×320，像素格式 RGB565，裝置節點為 `/dev/fb0`。

## 語言
- 所有 AI 產出的文件與回覆一律使用繁體中文（zh-TW），維持專業且精簡的語氣。

## 未來檔案與資料夾結構（建議 AI 採用）
- `src/EmoLcdDemo/`：主要 .NET 8 console 專案
  - `Program.cs`：解析參數（表情種類），呼叫後續服務
  - `Expressions/ExpressionType.cs`：`enum ExpressionType { Smile, Angry, Neutral, ... }`
  - `Rendering/EmotionRenderer.cs`：依據 `ExpressionType` 在 offscreen 影像上繪製表情
  - `Display/FramebufferDisplay.cs`：負責開啟 `/dev/fb0`，處理像素格式轉換並寫入 framebuffer
- 若需要額外工具或測試，可放在 `tools/` 或 `tests/`，但目前沒有既有慣例，請保持簡單。

## 繪圖與像素格式約定
- 請以 480×320 固定解析度設計所有繪圖邏輯，避免寫死與特定表情綁死的座標，適度抽象成「臉部區域」、「眼睛區域」等相對位置。
- 建議流程：
  1. 使用 ImageSharp 或 SkiaSharp 在記憶體中建立 32-bit 圖像（例如 RGBA8888）。
  2. 以幾何圖形（圓形、線段、弧線）畫出表情元素（眼睛、眉毛、嘴巴），線條顏色以黑/白為主即可。
  3. 將 32-bit 像素資料逐像素轉為 RGB565（`R:5bit, G:6bit, B:5bit`），低位對齊，對應 `fbset` 輸出：`rgba 5/11,6/5,5/0,0/0`。
  4. 以 `FileStream` 或 `SafeFileHandle` 開啟 `/dev/fb0`（位元組長度 = 寬×高×2 bytes），一次寫入整個 frame。

## 執行與工作流程
- 預期的執行指令（AI 建立專案後需確保能運作）：
  - `dotnet run --project src/EmoLcdDemo -- Smile`
  - `dotnet run --project src/EmoLcdDemo -- Angry`
- 開發時可以先在非 Raspberry Pi 環境上，輸出到檔案（例如 `.bmp` 或 `.png`）模擬 LCD 畫面，再在實機上改為寫入 `/dev/fb0`：
  - 建議在 `FramebufferDisplay` 中加入「檔案輸出模式」，透過建構子或設定值切換，避免在其他類別中散落 `#if DEBUG`。

## 例外處理與相依條件
- 請確認 `/dev/fb0` 存在且幾何資訊如 `fbset` 所示（480×320, 16bpp），若不符合可回報錯誤訊息並中止。
- 若 ImageSharp / SkiaSharp 無法在目標環境方便安裝，可改用純手寫像素 buffer + 簡單幾何演算法，但仍需維持 RGB565 輸出規格。
- 目前專案尚未定義 logging / DI framework，請避免引入大量基礎設施；必要時可使用最小的 `Console.WriteLine` 訊息。

## 設計與程式風格原則
- **KISS (Keep It Simple, Stupid)**：
  - 優先採用直覺、易讀的實作；避免過早抽象或過度泛型。
  - 對於僅此一處使用的邏輯，允許使用簡單方法函式，不必一開始就設計複雜架構。
- **DRY (Don't Repeat Yourself)**：
  - 若同樣的像素轉換、繪圖步驟在多處出現，集中到共用 helper（例如 `Rendering` 底下的小型靜態類別或方法）。
  - 避免在多個表情實作中重複硬編座標計算，可抽出「臉部/眼睛/嘴巴」相對位置的共用計算。
- **YAGNI (You Ain't Gonna Need It)**：
  - 尚未明確需要的功能（多語系、複雜設定檔、插件系統等）先不要加入。
  - 以「現在為了在 LCD 上畫出幾種表情」為優先目標，等需求明確再擴充。
- **SOLID 原則（在小而清楚的類別上適度運用）**：
  - **SRP**：每個類別負責單一清楚職責，例如：
    - `EmotionRenderer` 只關心「如何在 buffer 上畫表情」，不處理 `/dev/fb0` I/O。
    - `FramebufferDisplay` 只負責像素格式轉換與寫入 framebuffer。
  - **OCP**：新增表情時，盡量透過擴充 `ExpressionType` 與在 `EmotionRenderer` 增加對應分支或策略類別，而不是大改既有程式架構。
  - **LSP**：若之後引入介面／繼承層次（例如不同 renderer 實作），子類別應可在不驚喜破壞呼叫端的情況下替換使用。
  - **ISP**：若有介面（例如 `IDisplayTarget`），讓介面維持精簡，只暴露實際需要的方法（如 `WriteFrame(ReadOnlySpan<byte> frame)`），避免超大「上帝介面」。
  - **DIP**：若未來需要抽換顯示目標（檔案輸出 vs. framebuffer），可以讓高階模組依賴抽象（例如簡單介面），由 `Program.cs` 或組態決定具體實作；在目前小型範圍內，保持實作輕量即可，避免引入完整 DI 容器。

## AI 變更範圍建議
- 可以主動：
  - 建立 `src/EmoLcdDemo` 專案與基本繪圖 / framebuffer 寫入骨架。
  - 在 `README.md` 增補「如何建置與執行 .NET 專案」一節，對齊上方約定的命令。
- 請避免：
  - 修改現有 `README.md` 中的硬體說明與 `fbset` 結論（這些是現場實測資料）。
  - 引入與 LCD 顯示無直接關係的大型架構（web API、資料庫、DI 容器等）。

## 與人類協作建議
- 若需新增表情種類，請：
  - 先在 `ExpressionType` 中擴充列舉。
  - 在 `EmotionRenderer` 中集中實作各表情繪製邏輯，避免在 `Program.cs` 內分支太多。
- 對於畫面結果，人類開發者會在實機上 fine tune 比例與位置，AI 不需要追求完美美術，只要結構分明、易於微調即可。

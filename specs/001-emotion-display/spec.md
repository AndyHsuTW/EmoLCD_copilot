# 功能規格：參數驅動的表情顯示

**功能分支**：`001-emotion-display`  
**建立日期**：2025-11-25  
**狀態**：Draft  
**輸入**：使用者描述：「讓 pi5 的 LCD 可以根據輸入不同的參數而顯示不同的表情。呈現上使用白底黑色像素的五官即可。」  
**輸入約束**：僅接受表情文字指令，不接受任何圖片／檔案／影像作為輸入。

## 澄清紀錄

### 2025-11-26

- 問：基礎支援的表情集合為何？→ 答：支援 3 種表情：Neutral、Smile、Angry。
- 問：輸入與目標切換的介面為何？→ 答：單一 CLI 指令（使用旗標切換 dry-run 與 framebuffer，必填 emotion 參數）。

## 硬體與平台假設（裝置必填）

- Raspberry Pi 型號與作業系統：Raspberry Pi 5，Raspberry Pi OS（trixie 64-bit）。
- 顯示器：Waveshare 3.5" LCD (B) v2，解析度 480×320，RGB565，目標裝置 `/dev/fb0`，驅動由 LCD-show 腳本安裝。
- SPI／framebuffer 是否需修改：不需額外修改；若變更開機 overlay，必須附上對應卸載腳本的回滾步驟。
- 無硬體時的 dry-run 目標：針對每個表情產出 PNG（例：`/tmp/emotion.png`），維持與裝置相同比例與排版。

## 設計守則（KISS／DRY／YAGNI／SOLID）

- **KISS**：繪圖與寫入流程保持直覺、易讀，避免多層抽象或額外框架。
- **DRY**：色彩轉換、畫布尺寸、路徑驗證等共用邏輯抽為單一路徑，dry-run 與 framebuffer 重用同一套程序。
- **YAGNI**：只實作 Neutral／Smile／Angry 及現行 CLI 旗標；未確認的表情或輸出模式不預先開發。
- **SOLID**
  - SRP：渲染、輸出寫入、CLI 參數解析分開職責。
  - OCP：允許新增輸出策略（framebuffer／檔案）時透過擴充，不需修改既有核心。
  - LSP：不同輸出策略保持一致的介面與成功／失敗語意，避免替換時破壞呼叫端。
  - ISP：介面僅暴露必要方法，不強迫依賴不需要的檢查或設定。
  - DIP：上層依賴抽象輸出介面，具體實作（framebuffer、dry-run）透過注入切換。

## 使用情境與測試（必填）

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### 使用者故事 1－在 LCD 顯示指定表情（優先度：P1）

操作人員輸入表情名稱，Pi 5 的 LCD 立即顯示白底黑線條的對應表情。

**為何此優先度**：核心價值是能隨選在目標硬體上看到指定表情。

**可獨立驗證方式**：以 "Smile" 呼叫指令並確認 LCD 顯示微笑表情，再以 "Angry" 呼叫確認能切換。

**驗收情境**：

1. **Given** 裝置已上電且 LCD 驅動正常，**When** 操作人員請求 "Smile"，**Then** LCD 在 2 秒內顯示白底黑線條的微笑表情。
2. **Given** 先前已顯示某表情，**When** 操作人員請求另一個受支援的表情，**Then** LCD 會完全替換為新表情，不留殘影。

---

### 使用者故事 2－產生離線預覽圖片（優先度：P2）

無法接觸 LCD 的開發者可透過相同文字輸入取得 dry-run 輸出，產生 PNG 以驗證表情版面，不涉及影像上傳。

**為何此優先度**：在無硬體情況下仍能開發與審查，並確保視覺一致。

**可獨立驗證方式**：對 "Neutral" 使用 dry-run 旗標執行指令，確認產出 PNG 並符合預期構圖與比例。

**驗收情境**：

1. **Given** framebuffer 不可用或提供 dry-run 旗標，**When** 操作人員請求 "Neutral"，**Then** 會儲存一張白底黑線條的中性表情 PNG。

---

### 使用者故事 3－防範無效請求（優先度：P3）

操作人員輸入不支援的表情或遺漏參數時，系統需清楚回應並避免破壞 LCD 輸出。

**為何此優先度**：避免對 framebuffer 進行混亂或不安全的寫入，並明確告知可用選項。

**可獨立驗證方式**：以未知表情呼叫指令，確認系統拒絕繪製、回傳允許清單，且不改變螢幕內容。

**驗收情境**：

1. **Given** 無效表情輸入，**When** 操作人員執行指令，**Then** 系統回覆包含有效表情清單的錯誤訊息，且不改變螢幕或輸出檔。

---

[如有需要可繼續新增使用者故事並標註優先度]

### 邊界情境

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right edge cases.
-->

- 當 framebuffer 路徑 `/dev/fb0` 不存在或無寫入權限時會怎麼處理？
- 系統如何處理不支援的表情名稱或缺少參數？
- 若短時間內連續收到多個表情請求，最後一個是否可預期地生效？
- 若 framebuffer 回報意外的解析度或旋轉，如何維持正確方向與比例？
- 當 dry-run 輸出路徑不可寫時會回傳什麼結果？

## 需求（必填）

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right functional requirements.
-->

### 功能性需求

- **FR-001**：系統必須接受表情參數（初始支援 Neutral、Smile、Angry），並對應到預先定義的表情構圖。
- **FR-001a**：輸入僅限表情文字參數；系統不得要求或接受圖片上傳，並必須對非文字／影像負載回傳明確錯誤。
- **FR-002**：系統必須在 Pi 5 LCD 上以白底黑線條繪製所選表情。
- **FR-003**：系統必須提供 dry-run 模式，在無硬體或選用時產生與裝置版面一致的圖片檔。
- **FR-004**：系統必須驗證輸入，若缺少或不支援則回傳含有效表情清單的明確錯誤，且不改變現有輸出。
- **FR-006**：系統必須提供單一 CLI 入口，透過旗標選擇輸出目標（LCD framebuffer 或 dry-run 檔案），並要求明確的 emotion 參數。
- **FR-007**：若無法確認 framebuffer 契約（裝置路徑、解析度、像素格式），系統必須安全失敗，避免部分寫入。

### 關鍵實體（若涉及資料）

- **EmotionExpression**：受支援的表情集合（名稱、五官描述、顯示順序），初期限於 Neutral、Smile、Angry。
- **RenderRequest**：包含表情輸入、輸出目標（LCD 或 dry-run 圖片）與預覽輸出位置的請求。

### 繪製與可觀察性需求（裝置特定）

- **RR-001**：繪製必須遵守 480×320 RGB565 契約，並明確宣告輸出目標為 `/dev/fb0` 或 dry-run 圖片。
- **RR-002**：每個受支援表情的繪製流程需提供可重現的離線驗證（golden image／像素比對）。
- **RR-003**：需記錄繪製參數（請求表情、解析度、耗時、目標路徑），並可控詳細度以避免淹沒裝置。
- **RR-004**：若加入單一繪圖函式庫之外的依賴，必須以簡單與安全為理由加以說明。

## 成功標準（必填）

<!--
  ACTION REQUIRED: Define measurable success criteria.
  These must be technology-agnostic and measurable.
-->

### 可量測成果

- **SC-001**：受支援的表情在請求後 2 秒內於 LCD 顯示，並乾淨替換先前表情。
- **SC-002**：dry-run 圖片在 3 秒內產出，且維持 480×320 比例與白底黑線條。
- **SC-003**：每種表情的 dry-run 輸出與基準圖的像素差異低於 1%。
- **SC-004**：100% 無效或缺少的表情輸入都回傳清楚的允許清單訊息，且不改變 LCD 或輸出檔。

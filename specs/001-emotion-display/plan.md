# 實作計畫：參數驅動的表情顯示

**分支**：`001-emotion-display`｜**日期**：2025-11-26｜**對應規格**：/home/hamowe/Projects/EmoLCD_copilot/specs/001-emotion-display/spec.md  
**輸入**：`/specs/001-emotion-display/spec.md` 的功能規格  
⚠️ 產出時全檔使用正體中文；僅 class／function／檔名等專有名詞保留英文。

**說明**：依修改後憲章，以 C#/.NET 8 為主要語言；繪圖優先採用 ImageSharp，並維持直接寫入 `/dev/fb0` 的無 GUI 路線。

## 摘要

Pi 5 上的表情顯示工具，支援 Neutral／Smile／Angry 三種表情，以白底黑線條呈現；透過單一 CLI 入口可選擇輸出至 Waveshare 3.5" LCD (B) v2 的 `/dev/fb0`，或在 dry-run 模式生成等比例 PNG。LCD 繪製須在 2 秒內完成、dry-run 3 秒內；輸入需驗證並提供允許清單；契約不明時安全失敗。

## 技術脈絡

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**語言／版本**：C#/.NET 8（console）  
**主要依賴**：ImageSharp（繪圖與像素處理）；mmap 或等效檔案流直寫 framebuffer  
**儲存**：N/A  
**測試**：xUnit 搭配像素摘要／差異檢查  
**目標平台**：Raspberry Pi OS（trixie 64-bit）上的 `/dev/fb0`（Waveshare 3.5" LCD B v2）  
**專案型態**：單一 CLI 專案  
**效能目標**：LCD 繪製 ≤2s；dry-run PNG ≤3s；像素差異 <1%  
**限制**：16bpp RGB565；不得引入 GUI/視窗堆疊；契約未知時不得部分寫入  
**規模／範圍**：單機顯示，三種表情，無網路

## 憲章檢核

＊關卡：Phase 0 研究前必須通過；Phase 1 設計後再次覆核。

- 硬體鎖定：Raspberry Pi 5＋Waveshare 3.5" B v2、480×320、RGB565、`/dev/fb0`。偏離需列變更理由與回滾。
- 繪圖管線：預設 C#/.NET 8＋ImageSharp，零拷貝或等效方式直寫 framebuffer；禁止 GUI/視窗框架。
- 色彩驗證：規劃 RGB888→RGB565 轉換檢查（位元序、對齊、緩衝區長度），提供自動或半自動比對。
- 測試策略：列出單元測試與至少一個實機煙囪測試（預製圖樣或像素摘要）；檢驗耗時門檻。
- 文件語言：所有產出（plan/spec/tasks/implement）使用正體中文，專有名詞保留英文。
- 風險與回滾：涉及裝置節點或 sudo 的步驟需寫明最小權限與回退方式；未備妥不得進入實作。

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
# [REMOVE IF UNUSED] Option 1: Single project (DEFAULT)
src/
├── EmoLcd.App/           # CLI 入口（Program.cs、參數解析）
├── EmoLcd.Rendering/     # 繪圖、RGB565 轉換、framebuffer 寫入
└── EmoLcd.Domain/        # 表情模型與共用契約（可視需要）

tests/
├── EmoLcd.Tests/         # xUnit 測試（單元、整合、像素摘要）
└── golden/               # 基準影像與摘要（避免手動修改）

# [REMOVE IF UNUSED] Option 2: Web application (when "frontend" + "backend" detected)
backend/
├── src/
│   ├── models/
│   ├── services/
│   └── api/
└── tests/

frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/

# [REMOVE IF UNUSED] Option 3: Mobile + API (when "iOS/Android" detected)
api/
└── [same as backend above]

ios/ or android/
└── [platform-specific structure: feature modules, UI flows, platform tests]
```

**Structure Decision**: 採單一解決方案下多專案（App/Rendering/Domain），測試集中於 EmoLcd.Tests，golden 資產獨立目錄；無前後端分離。

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |

# 實作計畫：[FEATURE]

**分支**：`[###-feature-name]`｜**日期**：[DATE]｜**對應規格**：[link]  
**輸入**：`/specs/[###-feature-name]/spec.md` 的功能規格  
⚠️ 產出時全檔使用正體中文；僅 class／function／檔名等專有名詞保留英文。

**說明**：本模板由 `/speckit.plan` 指令填入；流程說明請見 `.specify/templates/commands/plan.md`。

## 摘要

[擷取規格重點：主要需求＋研究後的技術路線]

## 技術脈絡

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**語言／版本**：[例如 C#/.NET 8（預設）、或需澄清]  
**主要依賴**：[例如 ImageSharp／SkiaSharp；若改用其他套件需註記理由]  
**儲存**：[若適用，例如檔案或 N/A]  
**測試**：[例如 pytest、XCTest、cargo test 或待澄清]  
**目標平台**：[例如 Raspberry Pi OS 上的 `/dev/fb0`]  
**專案型態**：[單一專案／前後端分離等]  
**效能目標**：[領域特定，例如 480×320 全畫面更新在 16ms 以內]  
**限制**：[領域特定，例如 16bpp RGB565、不得新增 GUI 堆疊]  
**規模／範圍**：[例如 單機顯示、未涉網路]

## 憲章檢核

＊關卡：Phase 0 研究前必須通過；Phase 1 設計後再次覆核。

- 硬體鎖定：Raspberry Pi 5＋Waveshare 3.5" B v2、480×320、RGB565、`/dev/fb0`。任何偏離需先列出變更理由與回滾。
- 繪圖管線：預設 Python 3.11＋Pillow＋mmap。若改用其他語言／套件，需證明不增加 GUI 堆疊且符合色彩格式。
- 色彩驗證：規劃 RGB888→RGB565 轉換檢查（位元序、對齊、緩衝區長度），提供自動或半自動比對方法。
- 測試策略：列出單元測試與至少一個實機煙囪測試（預製圖樣或像素摘要）。
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
├── models/
├── services/
├── cli/
└── lib/

tests/
├── contract/
├── integration/
└── unit/

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

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |

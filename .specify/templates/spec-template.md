# 功能規格：[FEATURE NAME]

**功能分支**：`[###-feature-name]`  
**建立日期**：[DATE]  
**狀態**：Draft  
**輸入**：使用者描述："$ARGUMENTS"  
⚠️ 產出時全檔使用正體中文；僅 class／function／檔名等專有名詞保留英文。

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

### 使用者故事 1－[簡要標題]（優先度：P1）

[以白話描述此旅程]

**為何此優先度**： [說明價值與排序理由]

**可獨立驗證方式**： [描述如何單獨驗證，例如「透過 XXX 指令即可完整驗收且產出 YY 價值」]

**驗收情境**：

1. **Given** [初始狀態]，**When** [行為]，**Then** [預期結果]
2. **Given** [初始狀態]，**When** [行為]，**Then** [預期結果]

---

### 使用者故事 2－[簡要標題]（優先度：P2）

[以白話描述此旅程]

**Why this priority**: [Explain the value and why it has this priority level]

**Independent Test**: [Describe how this can be tested independently]

**Acceptance Scenarios**:

1. **Given** [initial state], **When** [action], **Then** [expected outcome]

---

### 使用者故事 3－[簡要標題]（優先度：P3）

[以白話描述此旅程]

**Why this priority**: [Explain the value and why it has this priority level]

**Independent Test**: [Describe how this can be tested independently]

**Acceptance Scenarios**:

1. **Given** [initial state], **When** [action], **Then** [expected outcome]

---

[Add more user stories as needed, each with an assigned priority]

### 邊界情境

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right edge cases.
-->

- [邊界條件] 會發生什麼事？
- 系統如何處理 [錯誤情境]？

## 需求（必填）

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right functional requirements.
-->

### 功能性需求

- **FR-001**：系統必須 [具體能力，例如「可在 480×320、RGB565 上繪製指定表情」]
- **FR-002**：系統必須 [具體能力，例如「色彩轉換需符合 RGB565 位元序與對齊」]  
- **FR-003**：系統必須 [互動或流程，例如「支援選擇表情輸出與重繪」]
- **FR-004**：系統必須 [資料要求，例如「對於 framebuffer 寫入需一次性寫滿緩衝區」]
- **FR-005**：系統必須 [行為要求，例如「記錄或輸出實機驗證摘要」]

*Example of marking unclear requirements:*

- **FR-006**：系統必須 [NEEDS CLARIFICATION：需求未明，例如「是否需支援多種硬體型號」]
- **FR-007**：系統必須 [NEEDS CLARIFICATION：需求未明，例如「保持畫面多久不更新」]

### 關鍵實體（若涉及資料）

- **[實體 1]**：[代表意義、主要屬性（不寫實作）]
- **[實體 2]**：[代表意義、與其他實體的關係]

## 成功標準（必填）

<!--
  ACTION REQUIRED: Define measurable success criteria.
  These must be technology-agnostic and measurable.
-->

### 可量測成果

- **SC-001**：[量測指標，例如「輸出預設表情時，整張畫面更新在 16ms 內完成」]
- **SC-002**：[量測指標，例如「色彩轉換後的像素摘要與基準檔比對誤差為零」]
- **SC-003**：[使用者滿意度或可用性指標，例如「第一次執行即可看見完整表情的成功率達 90%」]
- **SC-004**：[維運或品質指標，例如「更換表情的程式執行不產生核心轉儲或錯誤日誌」]

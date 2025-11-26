# 實作計畫：參數驅動的表情顯示

**分支**：`001-emotion-display`｜**日期**：2025-11-26｜**對應規格**：specs/001-emotion-display/spec.md  
**輸入**：`/specs/001-emotion-display/spec.md` 的功能規格

## 摘要

在 Raspberry Pi 5 上，以白底黑線條繪製 Neutral／Smile／Angry 三種表情，可輸出至 Waveshare 480×320 RGB565 framebuffer，或在 dry-run 模式輸出與裝置版面一致的 PNG。交付為單一 CLI 入口，透過旗標選擇目標；LCD 繪製需在 2 秒內完成，dry-run 在 3 秒內；若無法確認 framebuffer 契約則安全失敗，並以清楚的允許清單驗證輸入，預設表情為 Neutral。

## 技術脈絡

**語言／版本**：Python 3.11  
**主要依賴**：Pillow 繪圖；mmap 寫入 RGB565 framebuffer，不疊加 GUI 框架  
**儲存**：N/A  
**測試**：pytest，採 golden image 像素差異檢查  
**目標平台**：Raspberry Pi OS（trixie 64-bit）於 Raspberry Pi 5，上螢幕 `/dev/fb0`  
**專案型態**：單一 CLI 工具  
**效能目標**：LCD 繪製 ≤2s；dry-run PNG ≤3s；與基準圖像素差異 <1%  
**限制**：必須遵守 480×320 RGB565 契約；契約未知時避免部分寫入；缺少 emotion 時預設 Neutral；拒絕非文字負載  
**規模／範圍**：單裝置工具；三個基礎表情；支援離線預覽

## 憲章檢核

- 硬體鎖定：Pi 5＋Waveshare 3.5" B v2、480×320、RGB565、`/dev/fb0`。偏離需提案與回滾步驟（LCD-show 卸載腳本、改寫前先 dry-run）。
- 繪圖管線：預設 Python 3.11＋Pillow＋mmap，禁止額外 GUI 堆疊；若改用其他工具，需證明不影響 RGB565 並保留零拷貝寫入。
- 測試防線：保留 golden image 像素比對；新增實機煙囪驗證（固定表情寫入 `/dev/fb0`，擷取像素摘要或 hash 比對，失敗時自動切換 dry-run）。
- 文件語言：所有輸出文件維持正體中文與全形標點，專有名詞保留英文。
- 版本與回滾：涉及裝置或 sudo 的變更需列回滾指令；未備妥回滾不得合併。
- KISS：設計保持直覺易讀，繪圖與寫入流程僅保留必要步驟，避免多層抽象或過度框架化。
- DRY：共用的色彩轉換、畫布尺寸、路徑驗證抽為單一函式／模組，避免在 dry-run 與 framebuffer 路徑重複。
- YAGNI：僅實作 Neutral／Smile／Angry 與當前 CLI 旗標；未確定的表情或輸出模式不預先開發。
- SOLID：SRP（渲染、寫入、CLI 參數解析分職責）；OCP（支援新增情境，如檔案輸出／framebuffer 切換時可透過策略註冊）；LSP（不同輸出策略對外應保有相同介面與成功／失敗語意）；ISP（介面僅暴露必要方法，避免強迫依賴不需要的檢查）；DIP（上層依賴抽象的輸出介面，具體實作由依賴注入，便於切換 dry-run 與 framebuffer）。

## 專案結構

```text
specs/001-emotion-display/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
└── tasks.md（未來由 /speckit.tasks 產生）

src/
├── cli/
│   └── emotion_cli.py
├── rendering/
│   ├── expressions.py
│   ├── framebuffer.py
│   └── draw.py
├── outputs/（dry-run PNG 輸出）
└── __init__.py

tests/
├── unit/
├── integration/
└── contract/
```

**結構決策**：單一專案佈局，CLI、rendering 模組與 golden 資產／測試共置；不需前後端分離。

## 複雜度追蹤

| 違規項目 | 必要原因 | 拒絕較簡方案的理由 |
|---------|---------|------------------|
| 無 | N/A | N/A |

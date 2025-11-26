# 資料模型

## EmotionExpression
- **欄位**
  - `name`（enum: Neutral｜Smile｜Angry）— 必填；採用標準大小寫
  - `features`（struct）— 眼睛／嘴巴的座標與筆畫定義；必填
  - `description`（string）— 選填的人類可讀說明
- **驗證規則**
  - `name` 必須是允許的列舉值之一
  - `features` 必須符合 480×320 畫布座標並使用白底黑線條色盤
- **關聯**
  - 由 `RenderRequest.emotion` 參照

## RenderRequest
- **欄位**
  - `emotion`（enum: Neutral｜Smile｜Angry）— 必填；缺省時預設為 Neutral
  - `target`（enum: lcd｜dry-run）— 必填；由 CLI 旗標選擇
  - `framebufferPath`（string，預設 `/dev/fb0`）— `target=lcd` 時必填
  - `resolution`（struct: width=480, height=320）— 必填；必須符合裝置
  - `pixelFormat`（enum: RGB565）— 必填；必須符合裝置
  - `dryRunPath`（string，預設 `/tmp/emotion.png`）— `target=dry-run` 時必填
- **驗證規則**
  - 拒絕列舉外的 `emotion` 並回傳允許清單
  - 若 framebuffer 路徑不存在／不可寫或契約不符，需安全失敗
  - 繪製前須確認 `dryRunPath` 可寫
- **關聯**
  - 使用 `EmotionExpression` 取得繪製所需的特徵

## RenderResult
- **欄位**
  - `emotion`（enum）— 已繪製的表情
  - `target`（enum）— lcd 或 dry-run
  - `durationMs`（integer）— 繪製延遲
  - `outputPath`（string）— framebuffer 路徑或 PNG 路徑
- **驗證規則**
  - 必須記錄 `durationMs` 以檢查效能（lcd ≤2000ms，dry-run ≤3000ms）
  - `outputPath` 必須對應所選目標
- **關聯**
  - 由 `RenderRequest` 衍生

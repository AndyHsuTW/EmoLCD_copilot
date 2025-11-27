# Phase 0 研究

## 決策：以 C#/.NET 8 搭配 ImageSharp 繪製
- **理由**：符合專案主要技術棧，ImageSharp 提供跨平台像素處理，便於 RGB565 轉換與 PNG 產出，部署在 Raspberry Pi OS 無需額外 GUI。
- **替代方案**：Python＋Pillow（與既有 .NET 路線不符）、C++／SDL（建置鏈較重）、Node／canvas（執行時負擔較大）、純 C framebuffer（迭代效率低）。

## 決策：使用 MemoryMappedFile／檔案流直寫 `/dev/fb0`（RGB565）
- **理由**：直接存取 framebuffer 依賴最小、延遲低，可維持 480×320、RGB565 的契約；支援長度檢查與位元序校驗。
- **替代方案**：`fbi`／`fbv` 子行程（控制性低、額外依賴）、DRM/KMS（對靜態表情過度）、自訂驅動經 SPI 重繪（複雜度過高）。

## 決策：單一 CLI 入口並以旗標選擇輸出目標
- **理由**：符合使用者故事，部署簡單，便於在同一程式內切換 framebuffer 與 dry-run。
- **替代方案**：HTTP 服務（需常駐與管理）、常駐 daemon＋socket（營運負擔高）、僅提供程式庫 API（操作人員不直覺）。

## 決策：採用 golden image 像素比對驗證
- **理由**：ImageSharp 可生成與比對 PNG，確保 framebuffer 與 dry-run 一致；1% 偏差門檻滿足規格並便於 xUnit 整合。
- **替代方案**：人工目視（不可重複）、checksum（對細微變化過於脆弱）、感知雜湊（難以界定 1% 門檻）。

## 決策：當 framebuffer 契約缺失時改用 dry-run
- **理由**：安全失敗避免部分寫入，同時給予 PNG 回饋；符合治理中回滾與最小風險要求。
- **替代方案**：僅硬性失敗（無預覽）、盡力寫入（可能破壞顯示）、未確認契約就寫入（安全風險）。

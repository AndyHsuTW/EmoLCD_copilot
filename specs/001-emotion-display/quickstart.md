# 快速開始

## 先決條件
- Raspberry Pi OS（trixie 64-bit）上的 .NET 8 SDK
- 已透過 LCD-show 設定好的 Waveshare 3.5" LCD (B) v2，framebuffer 位於 `/dev/fb0`
- NuGet 相依：ImageSharp（繪圖與像素處理），xUnit（測試）

## 還原套件
```bash
dotnet restore
```

## 執行（LCD）
```bash
dotnet run --project src/EmoLcd.App -- --emotion Smile --target lcd --framebuffer /dev/fb0
```

## 執行（dry-run PNG）
```bash
dotnet run --project src/EmoLcd.App -- --emotion Neutral --target dry-run --output /tmp/emotion.png
```

## 驗證
- golden image 像素比對與耗時測試：
```bash
dotnet test
```
- 確認繪製時間：LCD ≤2s、dry-run ≤3s；無效輸入時，錯誤訊息需列出允許的表情且不寫入 framebuffer／檔案。
- 設計守則檢查：維持 KISS（流程簡潔）、DRY（共用轉換與驗證函式）、YAGNI（僅支援既定表情與旗標）、SOLID（輸出策略可替換且介面一致）。
- 產出 dry-run golden 範例：`specs/001-emotion-display/test_output/{neutral,smile,angry}.png`（未納入版控，Smile 已修正為上揚曲線）

## 實機煙囪驗證（範例流程，請在 Pi5 + LCD 上執行並紀錄）
- 前置：`fbset -fb /dev/fb0` 確認 480×320、RGB565；確認 `/dev/fb0` 可寫。
- 命令：
  ```bash
  for e in Neutral Smile Angry; do
    ts=$(date +%s%3N)
    dotnet run --project src/EmoLcd.App -- --emotion $e --target Lcd --framebuffer /dev/fb0
    te=$(date +%s%3N); echo "$e cost: $((te-ts)) ms"
    dd if=/dev/fb0 bs=1 count=$((480*320*2)) status=none | sha256sum | awk '{print "'$e' framebuffer sha256="$1}'
  done
  ```
- 預期：退出碼 0；每次耗時 ≤ 2000ms；不同表情的 sha256 不同且畫面無殘影。
- 回滾：若異常，可重跑 dry-run 覆寫或使用 LCD-show 恢復腳本還原顯示設定。

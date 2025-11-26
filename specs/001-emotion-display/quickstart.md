# 快速開始

## 先決條件
- Raspberry Pi OS（trixie 64-bit）上的 Python 3.11
- 已透過 LCD-show 設定好的 Waveshare 3.5" LCD (B) v2，framebuffer 位於 `/dev/fb0`
- 安裝套件：`pip install pillow pytest`

## 執行（LCD）
```bash
python -m src.cli.emotion_cli --emotion Smile --target lcd --framebuffer /dev/fb0
```

## 執行（dry-run PNG）
```bash
python -m src.cli.emotion_cli --emotion Neutral --target dry-run --output /tmp/emotion.png
```

## 驗證
- golden image 像素比對測試：
```bash
pytest tests
```
- 確認繪製時間：LCD ≤2s、dry-run ≤3s；無效輸入時，錯誤訊息需列出允許的表情。
- 設計守則檢查：維持 KISS（流程簡潔）、DRY（共用轉換與驗證函式）、YAGNI（僅支援既定表情與旗標）、SOLID（輸出策略可替換且介面一致）。

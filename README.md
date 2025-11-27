# EmoLCD 專案說明

在 Raspberry Pi 5 上，使用 C#/.NET 8 在 Waveshare 3.5" SPI LCD 上顯示表情圖案的實驗與開發紀錄。

## 硬體資訊

- **主板**：Raspberry Pi 5
- **作業系統**：Raspberry Pi OS (trixie 64bit)
- **LCD**：Waveshare 3.5" LCD (B) v2
- **解析度**：480 × 320
- **介面**：SPI
- **官方驅動 / 範例**：https://github.com/waveshare/LCD-show.git
  - 實際使用的腳本：`LCD35B-show`（或新版名稱）

### GPIO 腳位（來源：Waveshare 文件）

> 一般使用 HAT 方式直接插在 40-pin 上即可，不需另外跳線，以下資訊用於日後除錯或自訂驅動。

- 電源
  - PIN 1, 17：3.3V
  - PIN 2, 4：5V
  - PIN 6, 9, 14, 20, 25：GND
- LCD SPI
  - PIN 18：`LCD_RS`（Data/Command 選擇）
  - PIN 19：`LCD_SI` / `TP_SI`（SPI MOSI）
  - PIN 22：`RST`（重置）
  - PIN 23：`LCD_SCK` / `TP_SCK`（SPI SCLK）
  - PIN 24：`LCD_CS`（LCD Chip Select）
- 觸控面板
  - PIN 11：`TP_IRQ`（觸控中斷，低電位表示有觸控）
  - PIN 21：`TP_SO`（SPI MISO）
  - PIN 26：`TP_CS`（觸控 Chip Select）

目前專案只需要顯示表情，不使用觸控功能。

## 驅動與 framebuffer 設定

1. 啟用 SPI 介面
   - 透過 `raspi-config` → `Interface Options` → `SPI` → Enable。

2. 安裝 Waveshare 官方驅動
   ```bash
   git clone https://github.com/waveshare/LCD-show.git
   cd LCD-show
   sudo ./LCD35B-show   # 對應 LCD 3.5" B v2 型號
   ```
   - 腳本會幫忙設定 `config.txt` 的 `dtoverlay`，並將系統預設顯示輸出切換到 LCD。

3. 確認 framebuffer 裝置
   - 開機後以 `fbset` 檢查：
     ```bash
     fbset -fb /dev/fb0
     ```
   - 目前實際輸出：
     ```
     mode "480x320"
         geometry 480 320 480 320 16
         timings 0 0 0 0 0 0 0
         nonstd 1
         rgba 5/11,6/5,5/0,0/0
     endmode
     ```
   - 結論：
     - 顯示解析度：480 × 320
     - 色深：16 bits per pixel
     - 像素格式：RGB565（R:5bits, G:6bits, B:5bits），低階對齊。

## 執行與驗證（現況）

- 還原與建置
  ```bash
  dotnet restore
  dotnet build
  ```

- LCD 輸出（/dev/fb0）
  ```bash
  dotnet run --project src/EmoLcd.App -- --emotion Smile --target Lcd --framebuffer /dev/fb0
  ```

- dry-run PNG
  ```bash
  dotnet run --project src/EmoLcd.App -- --emotion Neutral --target DryRun --output /tmp/emotion.png
  ```

- 測試（含 framebuffer 假裝置、dry-run 與無效輸入）
  ```bash
  dotnet test
  ```

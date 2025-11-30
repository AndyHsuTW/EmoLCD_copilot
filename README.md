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
  dotnet build
  ```

- LCD 輸出（/dev/fb0）
  ```bash
  dotnet run --project src/EmoLcd.App -- --emotion Smile --target Lcd --framebuffer /dev/fb0
  ```

  - 必填旗標：`--emotion`, `--target`
  - `--framebuffer` 須為可寫入的裝置檔案，否則 CLI 會提前失敗並提示「找不到」或「權限不足」。
- dry-run PNG
  ```bash
  dotnet run --project src/EmoLcd.App -- --emotion Neutral --target DryRun --output /tmp/emotion.png
  ```
  - `--output` 的父資料夾必須存在，否則 CLI 會提示「輸出資料夾不存在」。

- 測試（含 framebuffer 假裝置、dry-run 與無效輸入）
  ```bash
  dotnet test
  ```
 `--emotion` 與 `--target` 為必填；若缺少，CLI 會顯示允許值並直接結束。
 `--target Lcd` 時必須提供存在且可寫入的 `--framebuffer` 路徑；工具會從 `/sys/class/graphics/fbX/virtual_size` 與 `bits_per_pixel` 建立契約，若像素資料大小或寬高不符就拒絕寫入。
 `--target DryRun` 時需指定 `--output` 檔案，程式會先檢查父資料夾是否存在。
 若 Lcd 輸出因契約不符而失敗，程式會自動 fallback 到 dry-run PNG，並顯示實際輸出路徑以便人工檢查畫面（詳見下方實機驗證流程）。

### 常見錯誤訊息

- `缺少必要參數 --emotion`：啟動指令少帶 `--emotion`。
- `缺少必要參數 --target`：啟動指令少帶 `--target`。
- `不支援的表情：XXX`：表情超出 `Smile/Angry/Neutral` 等允許清單。
- `輸出資料夾不存在：/path/...`：DryRun 模式的輸出路徑位於不存在的資料夾。
- `framebuffer 無法寫入：權限不足 - /dev/fb0`：LCD 模式未以具寫權限的使用者執行。
## 測試腳位狀態(GPIO5的腳位)

sudo gpioget -c gpiochip0 5
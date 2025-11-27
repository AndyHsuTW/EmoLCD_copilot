---

description: "Task list for 001-emotion-display feature"
---

# 任務列表：001-emotion-display

⚠️ 產出時全檔使用正體中文；僅 class／function／檔名等專有名詞保留英文。請在任務中標註實機驗證（RGB565 緩衝區檢查或煙囪測試）與回滾步驟。

## Phase 1: Setup（共用初始化）

- [X] T001 建立解決方案與專案骨架：/home/hamowe/Projects/EmoLCD_copilot/EmoLCD.sln，包含 src/EmoLcd.App、src/EmoLcd.Rendering、src/EmoLcd.Domain、tests/EmoLcd.Tests 並互相參照
- [X] T002 建立 golden 資料夾與 README：/home/hamowe/Projects/EmoLCD_copilot/tests/golden/README.md（說明生成方式與禁止手動修改）

## Phase 2: Foundational（阻擋性前置）

- [X] T003 定義資料模型（EmotionExpression／RenderRequest／RenderResult）與列舉：/home/hamowe/Projects/EmoLCD_copilot/src/EmoLcd.Domain/Models/EmotionModels.cs（僅涵蓋三種表情與兩種輸出模式）
- [X] T004 填寫預設表情特徵與座標：/home/hamowe/Projects/EmoLCD_copilot/src/EmoLcd.Rendering/Expressions/ExpressionCatalog.cs（共用畫布與色彩設定，DRY）
- [X] T005 建立繪圖與色彩轉換工具：/home/hamowe/Projects/EmoLCD_copilot/src/EmoLcd.Rendering/Pixels/Rgb565Converter.cs（RGB888→RGB565、緩衝區長度檢查，LCD/dry-run 共用）
- [X] T006 實作 framebuffer 寫入抽象：/home/hamowe/Projects/EmoLCD_copilot/src/EmoLcd.Rendering/Framebuffer/FramebufferWriter.cs（MemoryMappedFile 或檔案流安全寫入、契約檢查與回滾：失敗轉 dry-run），符合 DIP/OCP
- [X] T007 建立 CLI 參數骨架：/home/hamowe/Projects/EmoLCD_copilot/src/EmoLcd.App/Program.cs（僅支援當前旗標，YAGNI 禁止預留額外模式）

## Phase 3: User Story 1－在 LCD 顯示指定表情（P1）🎯 MVP

目標：接受表情指令並在 `/dev/fb0` 以白底黑線條呈現；2 秒內完成並乾淨替換。

獨立驗證方式：使用假 framebuffer 檔（正確尺寸）執行 "Smile"、"Angry" 兩次，檢查像素摘要變化與無殘影；實機時擷取 `/dev/fb0` 前後摘要。

- [X] T008 [P] [US1] 編寫 framebuffer 單元測試：/home/hamowe/Projects/EmoLCD_copilot/tests/EmoLcd.Tests/FramebufferRenderTests.cs（含假設備、長度檢查與回滾切換檢驗）
- [X] T009 [US1] 實作表情繪製管線至 framebuffer：/home/hamowe/Projects/EmoLCD_copilot/src/EmoLcd.Rendering/RenderPipeline.cs 與 /home/hamowe/Projects/EmoLCD_copilot/src/EmoLcd.Rendering/Framebuffer/FramebufferWriter.cs（整合模型、轉換與寫入）
- [X] T010 [US1] 實作 CLI 執行路徑（LCD 模式）：/home/hamowe/Projects/EmoLCD_copilot/src/EmoLcd.App/Program.cs（含耗時量測與失敗時切換 dry-run 的回饋訊息）
- [X] T011 [US1] 撰寫 LCD 切換整合測試：/home/hamowe/Projects/EmoLCD_copilot/tests/EmoLcd.Tests/LcdSwitchIntegrationTests.cs（驗證表情切換無殘影、耗時門檻）

## Phase 4: User Story 2－產生離線預覽圖片（P2）

目標：在無硬體或 dry-run 旗標下產生與裝置版面一致的 PNG，3 秒內完成並符合 golden。

獨立驗證方式：對 "Neutral"／"Smile"／"Angry" 執行 dry-run，與 golden PNG 進行像素差異 <1%。

- [X] T012 [P] [US2] 產出並檢入三種表情 golden PNG：/home/hamowe/Projects/EmoLCD_copilot/specs/001-emotion-display/test_output/{neutral,smile,angry}.png（由 ImageSharp 繪製並記錄摘要）
- [X] T013 [US2] 實作 dry-run PNG 輸出路徑：/home/hamowe/Projects/EmoLCD_copilot/src/EmoLcd.Rendering/RenderPipeline.cs（共用繪圖邏輯，避免重複碼）
- [X] T014 [US2] 實作 CLI dry-run 路徑與輸出參數：/home/hamowe/Projects/EmoLCD_copilot/src/EmoLcd.App/Program.cs（含預設輸出檔與權限檢查）
- [X] T015 [US2] 撰寫 dry-run 像素比對測試：/home/hamowe/Projects/EmoLCD_copilot/tests/EmoLcd.Tests/DryRunTests.cs（比較輸出與 golden 的像素摘要／差異比）

## Phase 5: User Story 3－防範無效請求（P3）

目標：對無效表情或缺參數給出清楚錯誤與允許清單，不改變現有輸出。

獨立驗證方式：以未知表情呼叫 CLI，確認回傳允許列表、退出碼非零、framebuffer／輸出檔未變。

- [X] T016 [US3] 增加輸入驗證與錯誤訊息：/home/hamowe/Projects/EmoLCD_copilot/src/EmoLcd.App/Program.cs（保留允許清單並避免寫入副作用）
- [X] T017 [US3] 撰寫無效輸入測試：/home/hamowe/Projects/EmoLCD_copilot/tests/EmoLcd.Tests/InvalidEmotionTests.cs（檢查訊息、退出碼與未寫入假設備）

## Phase 6: Polish & Cross-Cutting

- [X] T018 更新快速開始與 README 範例：/home/hamowe/Projects/EmoLCD_copilot/specs/001-emotion-display/quickstart.md 與 /home/hamowe/Projects/EmoLCD_copilot/README.md（補充回滾與煙囪驗證步驟，對齊 .NET 8）
- [X] T019 整理共用工具與介面註解：/home/hamowe/Projects/EmoLCD_copilot/src/EmoLcd.Rendering/*（確保 KISS/DRY，移除重複邏輯）
- [X] T020 最終實機煙囪驗證紀錄：/home/hamowe/Projects/EmoLCD_copilot/specs/001-emotion-display/quickstart.md（附上像素摘要與耗時）

## 依賴與執行順序

- 先完成 Phase 1 → Phase 2，之後方可進入任何使用者故事。
- 使用者故事優先順序：US1（LCD）→ US2（dry-run）→ US3（無效防護）。
- Polish 在所有故事完成後進行。

## 平行化範例

- 可平行：T008（LCD 單元測試腳本）與 T012（golden 圖檔產生）因涉不同檔案。
- 可平行：T013（dry-run 輸出實作）與 T016（輸入驗證邏輯）在不同模組修改時。

## 實作策略

1. 完成 Phase 1＋Phase 2，建立共用模型、繪圖與安全寫入基礎。  
2. 先交付 US1 作為 MVP，驗證實機寫入路徑與耗時。  
3. 加入 US2 dry-run，確保離線預覽與 golden 比對。  
4. 最後完成 US3 錯誤處理，鞏固防護與訊息品質。  
5. Polish 階段整理文件、註解與最終實機紀錄。

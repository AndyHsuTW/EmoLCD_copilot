# EmoLCD_copilot Development Guidelines

Auto-generated from all feature plans. Last updated: 2025-11-26

## Active Technologies
- C#/.NET 8 + ImageSharp for drawing; direct RGB565 framebuffer write or PNG dry-run output (001-emotion-display)

- (001-emotion-display)

## Project Structure

```text
backend/
frontend/
tests/
```

## Code Style

: Follow standard conventions

## Software Design Principles

- **KISS (Keep It Simple, Stupid)**
	- 優先採用直覺、易讀、容易維護的實作方式。
	- 避免過度設計與複雜抽象，只為真實需求增加結構。

- **DRY (Don't Repeat Yourself)**
	- 共用的邏輯（演算法、像素轉換、參數計算等）應抽成可重用的函式或模組。
	- 避免在多處複製貼上同一段程式碼或常數設定。

- **YAGNI (You Ain't Gonna Need It)**
	- 不為尚未出現、也未明確規劃的需求預先實作功能。
	- 先完成「現在一定需要」的最小可用版本，再視實際需求演進。

- **SOLID 原則**
	- **SRP — Single Responsibility Principle**
		- 每個模組、類別或函式應專注於單一、明確的職責。
	- **OCP — Open Closed Principle**
		- 對擴充開放、對修改封閉；以擴充行為（新增類別、策略）取代大幅改動既有核心流程。
	- **LSP — Liskov Substitution Principle**
		- 子型別應可在不破壞正確性的前提下替換父型別被使用。
	- **ISP — Interface Segregation Principle**
		- 介面應精簡且聚焦，避免肥大、用途混雜的「上帝介面」。
	- **DIP — Dependency Inversion Principle**
		- 高階模組依賴抽象而非具體實作，方便在不同執行環境（例如 framebuffer、檔案輸出）間切換。

## Recent Changes
- 001-emotion-display: Switched to C#/.NET 8 + ImageSharp for drawing; direct RGB565 framebuffer writes or PNG dry-run output

- 001-emotion-display: Added

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->

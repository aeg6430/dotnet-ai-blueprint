# dotnet-ai-blueprint — Starter Pack 導入手冊（Phase A–E）

> 本檔位於 **repository 根目錄**。中路徑若無特別說明，皆相對於 repo root。部署前請先閱：[docs/starter-pack/README.md](docs/starter-pack/README.md)。

## 這個 repo 是什麼

本 repo 是一套 **可複製的分層 .NET 後端 starter pack**。它提供規範、樣板、架構測試模板、以及 AI 入口文件，幫助你把新專案或既有專案整理到較一致的工程做法。

它**不是 runtime library**，也不建議直接把整份原始碼樹併入你的業務專案。比較合適的用法是：把它當成來源樣板與規範庫，用來建立工作目錄，然後再整理到你的目標專案。

## 這個 repo 能做什麼

- 提供分層架構、交易邊界、韌性、外部整合等規則文件。
- 提供可複製的 `templates/` 與 `docs/starter-pack/shadow-examples/` 寫法範例。
- 提供可落地到 CI 的架構測試模板與防火牆規則。
- 提供 Cursor / Copilot 的入口文件，讓 AI 先讀規則、再做設定或實作。

## 你要做什麼

使用這個 repo 時，通常要先完成下面四件事：

1. 準備一個**工作目錄**，讓 AI 或你自己先完成名稱、路徑與基本設定調整。
2. 準備一個**目標專案**，用來承接最後要保留的內容，不論它是既有專案或新專案。
3. 準備需求文件位置：
   - 原始需求文件放在目標專案的 `docs/requirements/raw/`
   - 可參考 `docs/requirements/raw/warehouse-onboarding-notes.md` 看偏白話的 raw requirement 範例
   - AI 可執行規格放在 `docs/specs/`
   - 可從 `docs/specs/feature-spec-template.md` 開始整理白話需求
   - 可參考 `docs/specs/example-warehouse-create.md` 看完整填寫範例
4. 若多人協作且工具不一致，先補上 repo 內的 `CONTRIBUTING.md`、`.github/pull_request_template.md` 與必要的本機驗證命令。
   - 若使用 GitHub issues，可搭配 `.github/ISSUE_TEMPLATE/bug_report.md` 與 `.github/ISSUE_TEMPLATE/feature_request.md`
   - 若有 production incident / 緊急修復情境，可搭配 `.github/ISSUE_TEMPLATE/incident_hotfix.md` 與 `.github/ISSUE_TEMPLATE/config.yml`
   - 若需要暫時阻擋已知惡意 path / query，參考 `docs/rules/request-screening.md`；此控制預設停用，只有 `RequestScreening:Enabled = true` 時才啟動

若 `docs/specs/` 已定義 feature spec，後續實作時應以 spec 優先。

## 如何上手

最短路徑如下：

1. 準備工作目錄。
2. 先讀 [`docs/START_HERE.md`](docs/START_HERE.md)。
3. 若你在做專案設定、命名調整或 namespace 轉換，讀 [`docs/starter-pack/project-setup-protocol.md`](docs/starter-pack/project-setup-protocol.md)。
4. 若你在做日常功能修改或 defect fix，讀 [`docs/starter-pack/core/daily-work-quickstart.md`](docs/starter-pack/core/daily-work-quickstart.md)。
5. 完成工作目錄整理後，再將需要保留的內容帶到目標專案。

若 `docs/specs/` 或目標專案已有既定命名，應以那些定義為優先，而非使用預設名稱。

## 建議工作方式：三個目錄

建議將工作分成三個目錄來處理：

1. **來源目錄**：本 repository，存放 `docs/rules/`、`.cursor/rules/`、`templates/` 與 starter-pack 指南。
2. **工作目錄**：可直接交給 AI 執行專案設定與命名調整的工作資料夾。
3. **目標專案**：你的實際產品 repo，不論是既有專案還是新專案。將工作目錄中的內容整理到這裡，同時保留目標專案原本的命名、風格與邊界。

## IDE：Cursor、Copilot

不論你使用哪個工具，先讀 [`docs/START_HERE.md`](docs/START_HERE.md)。

若需要工具專屬入口，再看：

- Cursor: [`.cursor/rules/README.md`](.cursor/rules/README.md)
- GitHub Copilot: [`.github/copilot-instructions.md`](.github/copilot-instructions.md)
- 短版 Copilot 起手提示: [`COPILOT_PROMPT.md`](COPILOT_PROMPT.md)

## 主要入口

依工作類型選擇最接近的入口：

- 一般任務導覽: [`docs/START_HERE.md`](docs/START_HERE.md)
- 日常工作快速起手: [`docs/starter-pack/core/daily-work-quickstart.md`](docs/starter-pack/core/daily-work-quickstart.md)
- 既有專案的小功能 / defect fix: [`docs/starter-pack/core/legacy-bugfix-feature-sop.md`](docs/starter-pack/core/legacy-bugfix-feature-sop.md)
- 新專案或早期專案接手: [`docs/starter-pack/core/new-project-day0-collaboration-checklist.md`](docs/starter-pack/core/new-project-day0-collaboration-checklist.md)
- 專案設定 / namespace / 路徑調整: [`docs/starter-pack/project-setup-protocol.md`](docs/starter-pack/project-setup-protocol.md)
- starter pack 全覽: [`docs/starter-pack/README.md`](docs/starter-pack/README.md)
- 協作與驗證慣例: [`CONTRIBUTING.md`](CONTRIBUTING.md)

## 入口角色對照

| 檔案 | 主要用途 | 建議放什麼 |
|---|---|---|
| [`README.md`](README.md) | repo 入口 | repo 是什麼、最短如何開始、主要文件入口 |
| [`docs/START_HERE.md`](docs/START_HERE.md) | 任務入口 | 依任務類型決定先看哪些文件 |
| [`docs/starter-pack/README.md`](docs/starter-pack/README.md) | starter pack 概覽 | pack 內容、目錄用途、核心與可選模組 |
| [`docs/starter-pack/core/daily-work-quickstart.md`](docs/starter-pack/core/daily-work-quickstart.md) | 日常工作 quickstart | 小功能、defect fix、新專案接手、外部整合的短清單 |
| [`.github/copilot-instructions.md`](.github/copilot-instructions.md) | Copilot 專屬入口 | Copilot 的必讀順序、最少工作規則 |
| [`.cursor/rules/README.md`](.cursor/rules/README.md) | Cursor 專屬入口 | Cursor 規則索引、常駐規則與任務型規則 |

若內容同時適用於多種工具或一般人工協作，優先放回 `docs/` 或 `README.md`，不要只放在工具專屬入口。

## 相關規則

依主題補讀對應規則：

- 分層與責任邊界: [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md), [`docs/rules/architecture-protocol.md`](docs/rules/architecture-protocol.md)
- 交易與寫入邊界: [`docs/rules/transactions.md`](docs/rules/transactions.md)
- 外部整合與韌性: [`docs/rules/resilience.md`](docs/rules/resilience.md), [`docs/rules/external-integration-firewall.md`](docs/rules/external-integration-firewall.md), [`docs/rules/anti-corruption-layer.md`](docs/rules/anti-corruption-layer.md)
- 稽核與 API 邊界紀錄: [`docs/rules/audit-log.md`](docs/rules/audit-log.md)
- 臨時請求防護: [`docs/rules/request-screening.md`](docs/rules/request-screening.md)
- 端點防護與例外外洩檢查: [`docs/rules/endpoint-protection.md`](docs/rules/endpoint-protection.md)

## 深入閱讀

如果你要完整了解導入方式、可用模板、可選模組與延伸文件，從這裡繼續：

- [`docs/starter-pack/README.md`](docs/starter-pack/README.md)
- [`docs/specs/feature-spec-template.md`](docs/specs/feature-spec-template.md)
- [`docs/specs/example-warehouse-create.md`](docs/specs/example-warehouse-create.md)
- [`docs/adr/README.md`](docs/adr/README.md)

## 結語

這份 starter pack 的定位是可複製的規則、模板與協作入口集合，不是可直接部署的產品專案。

若你只是要開始工作，先從 [`docs/START_HERE.md`](docs/START_HERE.md) 或 [`docs/starter-pack/core/daily-work-quickstart.md`](docs/starter-pack/core/daily-work-quickstart.md) 開始即可。

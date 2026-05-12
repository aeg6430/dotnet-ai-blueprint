# dotnet-ai-blueprint

`dotnet-ai-blueprint` 是一套可複製的分層 .NET 後端 starter pack。它提供規則文件、可複製樣板、架構測試範本，以及可供多種工具與協作方式使用的入口文件。

它不是 runtime library，也不建議直接把整份 repository 併入業務專案。比較合適的用法，是把它當成來源樣板與規範庫，先整理出工作目錄，再把需要保留的內容帶到真正的目標專案。

## 這個 repo 提供什麼

- 分層架構、交易邊界、韌性與外部整合的規則文件
- 可複製的 `templates/` 與 starter-pack 寫法範例
- 可落地到 CI 的架構測試模板與防火牆規則
- 給 Cursor、Copilot 與其他協作流程使用的 repo-local 入口文件

## 使用方式

1. 準備一個工作目錄，先完成名稱、路徑與基本設定調整。
2. 將名稱、namespace、目錄與 setup 相關參考對齊到實際專案。
3. 把需要保留的內容整理到目標專案，而不是整份 blueprint 原封不動搬入。
4. 在目標專案中補上實際需求規格、協作流程與驗證命令。

若 `docs/specs/` 已定義 feature spec，後續實作應以 spec 優先。

## 驗證與 setup

可直接依文件操作，並執行對應的原生指令，例如 `dotnet build` 與 `dotnet test`。

Repo 也提供 GitHub Actions workflow，會以 `.NET 8` 與 `.NET 9` matrix 驗證 `skeleton/StarterPack.Skeleton.sln` 的 restore / build / test。

## 建議工作方式：三個目錄

建議將工作分成三個目錄來處理：

1. **來源目錄**：本 repository，存放規則、模板與 starter-pack 文件。
2. **工作目錄**：用來進行命名調整、placeholder 清理與 setup 對齊的工作資料夾。
3. **目標專案**：真正承接最後成果的產品 repo，可為新專案或既有專案。

這種分法的重點，是讓 setup 與實際產品演進分離，避免把 blueprint 專屬內容直接殘留在目標專案中。

## 工具說明

不論你使用哪個工具，repo-local 文件都應是 source of truth。需要任務導引時，先從 [`docs/START_HERE.md`](docs/START_HERE.md) 開始；若需要工具專屬入口，再看：

- Cursor: [`.cursor/rules/README.md`](.cursor/rules/README.md)
- GitHub Copilot: [`.github/copilot-instructions.md`](.github/copilot-instructions.md)
- 短版 Copilot 起手提示: [`COPILOT_PROMPT.md`](COPILOT_PROMPT.md)

## 下一步

如果你要繼續往下看，從下面幾個入口擇一即可：

- [`docs/START_HERE.md`](docs/START_HERE.md)：依任務類型找最短路徑
- [`docs/starter-pack/README.md`](docs/starter-pack/README.md)：看 starter pack 的整體內容與目錄用途
- [`CONTRIBUTING.md`](CONTRIBUTING.md)：看協作、驗證與 review 慣例

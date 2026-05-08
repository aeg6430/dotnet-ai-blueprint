---
inclusion: manual
---

# File Upload & Untrusted Asset Ingress (rules)

本文件定義「檔案上傳／非信任資源進入系統」的**強制性行為邊界**。適用於 Excel、圖片、PDF、壓縮檔、附件等所有檔案型輸入。

## Adoption profile（legacy-safe vs strict）

- **Legacy-safe（default）**：先把規範當作 code review 與新功能的準則；僅對新增/變更的上傳路徑導入隔離與狀態閘門，不要求一次性改寫既有檔案儲存流程。
- **Strict（new project）**：從 day 0 以同一套 ingress guard 落地（同一組 ports/adapters + 狀態機 + 稽核），並將高訊號的檢查門檻納入 CI。

## Core principles（non-negotiable）

### 1) Input hygiene：不信任副檔名與 Content-Type

- 必須做 **content-based** 檢查（例如 Magic Bytes / signature），而非依賴副檔名或 `Content-Type`。
- 必須限制資源消耗：檔案大小（request body）、解碼後大小（例如圖片像素上限）、展開後大小（例如 zip/OOXML 解壓炸彈）。

### 2) Boundary enforcement：Core 不得接觸 ASP.NET 與 IO primitives

- API 層可接收 `IFormFile`（或等價 host primitive），但**不得將 `IFormFile` 傳入 Core**。
- Core service 的簽章必須是可攜的資料形態，例如：
  - `Stream`（具名參數，代表「上傳內容串流」）
  - `byte[]`（具名參數，代表「已受資源限制的 buffer」）
- Core **不得**直接使用下列 primitive 進行檔案讀寫或解析：
  - `Microsoft.AspNetCore.*`（包含 `IFormFile`）
  - `File.*` / `Directory.*` / `Path.*`（直接路徑存取）
  - 任意 parser/driver 的例外型別或錯誤訊息作為 API 對外輸出

> 目的：以 ports/adapters（防腐層）把「host 依賴、解析器依賴、儲存依賴」隔離在 Infrastructure，Core 只看乾淨的抽象與 domain error。

### 3) Error masking：解析器例外必須在 adapter 轉譯

- 任何 parser/library 噴出的 `InvalidFormatException` / `ZipException` / `IOException` / library-specific exceptions，不得直接穿透到 API。
- Infrastructure adapter 必須將其轉譯為 **穩定的 domain error**（例：`FileCorruptionError`、`FileTooLargeError`、`FileSignatureMismatchError`），再由外層的 exception boundary 統一塑形 HTTP 回應。
- 禁止把 stack trace、內部路徑、driver 型別、連線字串片段等敏感資訊回傳給客戶端（見 `architecture-tests/ExceptionLeakTests.cs.txt`）。

## Compliance-ready Dual-Track（raw bytes 必須保留的場景）

某些高合規環境要求「數位證據保存」或「原始文件不可竄改性」，導致 raw bytes **必須落地**。此時不得採用「清洗後丟棄」的單一路徑，而必須採用 **Dual-Track / Quarantine**。

### Three-zone buffer model（三層緩衝）

- **Vault/Staging（Raw bytes）**：保存原始二進位檔（證據/稽核用）。\n  - 存取權限：僅限安全服務/管理者（security/admin）。\n  - 存放：不可位於 web root；不可執行；不可被當作靜態網站直出。\n  - 命名：不得使用原始檔名；必須以隨機化識別（例如 UUID）保存。
- **Processing（Stream-in-flight）**：adapter 的暫存處理區（串流掃描、解析、清洗、重編碼）。\n  - 存取權限：僅限 adapter。\n  - 禁止 Core 直接存取此層的暫存資料。
- **Domain Assets（Sanitized copy）**：清洗後的產物（系統業務真正使用）。\n  - 例：圖片 re-encode 後的安全鏡像、縮圖；OOXML 的 canonicalized intermediate；PDF 的安全副本（如需）。

### State gate（Dirty Bit / 狀態閘門）

- 資料庫記錄檔案時，預設 `IsSanitized = false`（或等價狀態機）。\n  - **只有當 `IsSanitized = true`** 時，adapter 才能將該檔案內容流入一般 Core 用例。\n  - 若需要「下載原檔審查」，必須走**獨立且受控**的 read path（重新觸發授權、稽核、必要時重新掃描）。

### Storage access isolation（強制）

- Core 不得直接存取實體路徑；只能透過 port，例如：
  - `IBlobStorage.GetAsync(fileId)` / `PutAsync(...)`
  - `IAssetStore.OpenReadAsync(assetId)`（語意更貼近「業務資產」）
- Infrastructure 擁有 zone 的落地決策（raw vs sanitized），但**不得繞過狀態閘門**。

## Reference ports/adapters（docs-only）

以下為示意，用於描述責任邊界（非強制 API 形狀）：

- **Core ports**\n  - `IFileSanitizer` / `IImageSanitizer` / `IWorkbookParser`\n  - `IBlobStorage` / `IAssetStore`
- **Infrastructure adapters**\n  - `ExcelOoxmlWorkbookParser`（OOXML signature + scan + parse + sanitize）\n  - `ImageSanitizer`（decode + re-encode + strip metadata + dimension bounds）\n  - `BlobStorageAdapter`（隔離區/領域區的具體儲存）

## Practical checklist（最低要求）

- **Request bounds**：限制 request body 大小（server 設定 + endpoint 細項）。\n- **Signature validation**：Magic Bytes / content-based 檢查。\n- **Sanitization**：對高風險格式（至少 Excel/OOXML、Image）提供清洗/重建策略。\n- **Exception boundary**：對外輸出固定錯誤碼與固定訊息；敏感細節只留在結構化 log。\n- **Audit**：至少在 ingest/拒絕時寫入一筆稽核紀錄（成功/失敗皆記錄）。\n- **Storage isolation**：raw bytes 隔離、不可執行、不可直出；sanitized copy 供業務使用。


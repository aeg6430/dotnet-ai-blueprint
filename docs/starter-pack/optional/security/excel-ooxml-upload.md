---
inclusion: manual
---

# Optional security profile: Excel / OOXML upload (defense-in-depth)

本文件是一個可複製的「Excel/OOXML 上傳」安全範本，用於補足 `docs/rules/file-upload.md` 的通用規範（Rules）。\n\nExcel/OOXML（`.xlsx` / `.xlsm`）複雜度高，常見風險包含：解壓炸彈、外部關聯（SSRF）、巨集/嵌入物、解析器例外洩漏與資源耗盡。

## Adoption profile（legacy-safe vs strict）

- **Legacy-safe（default）**：先在新上傳端點導入 G-pre0；既有端點先以 observation/report-only 蒐集違規報告，再逐步補齊。
- **Strict（new project）**：將 G-pre0 視為必要檢查門檻；所有上傳路徑一致遵守。

## Profile boundary（ports/adapters）

- **Core**：只看 ports（例如 `IWorkbookPreprocessService` / `IWorkbookParser` / `IBlobStorage`）。\n  - Core 的輸入是 `Stream`/`byte[]`（具名），而非 `IFormFile`。\n- **Infrastructure adapter**：負責 G-pre0 與格式處理（掃描、解析、清洗、狀態閘門）。

## G-pre0 checklist（upload guards）

以下清單可作為「可稽核的實作規範」，並可對應到測試/掃描的檢查門檻（先 report-only，再逐步阻斷）。

### 1) Receive（串流與大小限制）

- 僅使用串流讀取（避免落地原檔到磁碟暫存區）。\n- 設定 request size limits：\n  - endpoint 層級（例如 `RequestSizeLimit`）\n  - server/multipart 層級（例如 `MultipartBodyLengthLimit`、`MaxRequestBodySize`）\n- 拒絕空檔與未知大小行為（依 repo 風格處理）。

### 2) Signature（Magic Bytes / content-based）

- 以內容簽章確認為 ZIP/OOXML（拒絕「改副檔名」的 payload）。\n- 不信任副檔名與 `Content-Type`。

### 3) Malware scan（AV）

- 在解析之前進行 AV 掃描。\n- 預設 **fail-closed**：掃描器不可用時，預設拒絕（如需例外，必須有明確記錄與安全審查）。
- Air-gapped：需提供離線更新/內部鏡像策略（見 `docs/rules/security-sast.md` 的 air-gapped 原則）。

### 4) Decompression bomb guard（解壓炸彈）

- 對解壓後總量與壓縮比設上限（inflate ceiling + ratio ceiling）。\n- 一旦觸發，立即中止並回傳穩定的 domain error（禁止外洩 parser 細節）。

### 5) External relationships block（SSRF / 外部連結）

- 封鎖任何 External relationship（例如 `TargetMode=External` 或 URL/UNC path）。\n- 不解析/不追蹤外部資源；不啟用任何可能觸發外部存取的功能。

### 6) Exception sanitization（對外固定錯誤碼）

- 解析失敗、格式錯誤、zip 壞檔等狀況，必須在 adapter 轉譯為穩定 domain error（例如 `FileCorruptionError`）。\n- API 回應不得包含 stack trace、解析器型別名稱、內部路徑、工作目錄等。

### 7) Audit（成功與失敗都要記錄）

- 每一次 preprocess 嘗試（成功/拒絕）都要寫入稽核紀錄。\n- 至少包含：時間、actor、檔案大小、雜湊（如 sha256）、簽章結果、AV 結果、拒絕原因 code、Trace/Correlation ID。\n- 若使用 Dual-Track（raw bytes 必須保留），稽核紀錄需能串接 raw zone 的 fileId 與 sanitized assetId。

## Dual-Track storage recommendation（raw + sanitized）

若合規要求保留原檔：\n\n- raw bytes 進 **Vault/Staging**（隔離、不可執行、不可直出、隨機命名）\n- 解析/清洗在 **Processing** 進行\n- 產生可供業務使用的 **Sanitized copy** 進 **Domain Assets**\n- 業務預設只使用 sanitized copy；raw download 必須走特權、可稽核的路徑

## Suggested acceptance check thresholds（optional）

- 例外不外洩：沿用 `architecture-tests/ExceptionLeakTests.cs.txt` 的精神，覆蓋「上傳/解析」錯誤回應。\n- 觀察者模式：可用 `STARTER_PACK_OBSERVE=1` 先蒐集報告，再逐步轉為阻斷。


---
inclusion: manual
---

# Optional security profile: Image upload sanitization (defense-in-depth)

本文件是一個可複製的「圖片上傳」安全範本，用於補足 `docs/rules/file-upload.md` 的通用規範（Rules）。\n\n圖片屬於「非結構化資料」，但同樣是高風險入口：EXIF/metadata 洩漏、解碼資源耗盡（pixel flood / decompression bomb）、polyglot 檔案、以及影像處理庫的歷史漏洞（例如 ImageTragick 類型問題）。

## Adoption profile（legacy-safe vs strict）

- **Legacy-safe（default）**：先導入「簽章 + 尺寸/像素上限 + metadata 剝離 + re-encode」的 sanitizer；raw bytes 如需保留先放隔離區並關閉直出。\n- **Strict（new project）**：所有圖片上傳一律走 sanitizer；對外只提供 sanitized mirror；raw download 僅限特權審查流程。

## Threat model highlights（盲點提醒）

- **Metadata leak（EXIF/GPS）**：可能外洩地理位置、裝置資訊、拍攝時間。\n- **Pixel flood / decompression bomb**：小檔案解碼後佔用巨大記憶體/CPU，造成 DoS。\n- **Polyglot files**：表面上是 JPG/PNG，尾端夾帶其他 payload；若被錯誤配置為可執行或被當作靜態站直出，可能升級為 RCE。\n- **Parser/library vulnerabilities**：影像處理庫或解碼器的漏洞面不可忽視；策略上應降低解析表面並採用重建式防禦。

## Profile boundary（ports/adapters）

- **Core**：不得接觸 `IFormFile` 或影像處理庫；只接收「已驗證與清洗」後的內容（例如 sanitized mirror 的 `Stream`）。\n- **Infrastructure adapter（sanitizer）**：負責 decode + re-encode + metadata 剝離 + 資源上限。

## Defense-in-depth checklist（建議必做）

### 1) Signature allow-list（Magic Bytes）

- 僅允許白名單格式（例如 JPEG/PNG/GIF/WebP），以 Magic Bytes 做 content-based 驗證。\n- 不信任副檔名與 `Content-Type`。

### 2) Decode bounds（資源上限）

- 限制最大 decoded 像素數（`maxWidth * maxHeight`）與最大寬/高。\n- 限制最大檔案大小（request size + storage size）。\n- 對可動態展開的格式（例如 GIF）要額外留意幀數與展開成本（以庫支援度為準）。

### 3) Re-encoding（重建式清洗，首選）

- **decode → re-encode** 產生「安全鏡像」（sanitized mirror）。\n- 目的：移除 polyglot 尾段、降低解析差異、並把後續業務操作固定在單一安全格式/參數組合。

### 4) Strip metadata（預設剝離 EXIF）

- 預設移除所有 metadata（EXIF、XMP、ICC profile 等視需求決定）。\n- 若業務需要保留某些欄位，必須改為白名單保留，並有明確理由與稽核。

### 5) Storage policy（Dual-Track / Quarantine）

合規要求保留原檔時，採用三區模型：\n\n- **Vault/Staging（raw bytes）**：保存原始檔供稽核/存證。\n  - 隨機命名（UUID）；不可在 web root；不可執行；不可直接靜態直出。\n  - raw access 僅限特權審查路徑（授權 + 稽核 + 必要時重新掃描）。\n- **Domain Assets（sanitized mirror）**：系統業務使用一律以此為主（前端展示、匯出、浮水印等）。\n- **Processing**：adapter 內部串流處理區，Core 不可見。

## Suggested acceptance check thresholds（optional）

- 確保 API 回應不外洩 parser/庫型別與訊息（沿用 Exception leak 的策略）。\n- 若採用 observation mode，先產出報告再逐步轉為阻斷。


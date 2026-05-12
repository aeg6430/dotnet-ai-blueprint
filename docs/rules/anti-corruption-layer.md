---
inclusion: manual
---

# Anti-Corruption Layer

An anti-corruption layer (ACL) is **not** a transport mirror or a DTO ferry. It is the semantic boundary that prevents foreign systems from polluting Core/Application language.

Use this rule when an external dependency is structurally or semantically dirty enough that a thin mapping layer is no longer sufficient.

## 30-second triage

Use this quick table first. It is intentionally fast and conservative: if you are unsure, classify upward.

| External system traits | Pollution level | Recommended template shape |
|---|---|---|
| Docs are coherent, types are clear, same-team ownership, mostly naming drift | **Light** | `BaseHttpAdapter` + `Mapperly` / thin helper |
| SDK/vendor client exists, naming is strange, response meaning depends on 1-2 fields, error model is non-standard | **Moderate** | `Gateway` + `Translator` + `ResiliencePolicies` |
| Contract is contradictory, `200` may still mean failure, payloads are dirty, multiple signals decide success/failure, no trustworthy test environment | **Heavy** | Full ACL: `Gateway` + `WireModels` + `Sanitizer` + `Translator` + `ExceptionTranslator` + `EvidenceLogger` |

If success/failure must be inferred from multiple fields, message text, HTTP status, or missing-field heuristics, treat the integration as **heavy**.

## Adoption profile (legacy-safe vs strict)

- **Legacy-safe (default)**: add ACL behavior around the unstable integrations you touch. Do not rewrite every existing adapter just for style.
- **Strict (new project)**: decide integration severity early and require the appropriate ACL shape from day 0.

## Three no principles

### 1. Do not let dirty state into Core

- External status strings, code tables, transport flags, and inconsistent result envelopes must be translated before Core sees them.
- Core should never branch on foreign values such as vendor status codes, ad hoc flags, or raw protocol payload markers.

### 2. Do not let dirty exceptions define Core semantics

- Raw SDK exceptions, protocol parser failures, foreign `SqlException`s, odd transport statuses, or vendor-specific error wrappers must be normalized at the boundary.
- Expected business-ish outcomes may become stable results/rejections.
- Unexpected dependency or protocol faults must stay visible as dependency-aware failures and flow to the global exception boundary.

### 3. Do not let dirty naming pollute domain language

- Foreign field names such as legacy abbreviations, codebook column names, and payload aliases stop at the wire-model/translator boundary.
- Core receives meaningful names only.

## What an ACL owns

An ACL may own:

- wire DTO parsing
- semantic normalization
- data sanitization/canonicalization
- exception translation
- partial-success or contradictory-envelope handling
- environment-specific protocol quirks
- evidence-capture hooks for dispute-heavy integrations

An ACL must **not** become:

- the business rules engine
- the transaction owner for the use case
- a place to silently “fix” meaning-changing data problems with fake defaults
- a dumping ground for unrelated orchestration

## Internal split

Use these roles to keep the ACL from becoming a giant `if/else` file:

- **Port** (`Core`) — what the use case needs
- **Adapter** (`Infrastructure`) — how we call the foreign system
- **Wire models** (`Infrastructure`) — the raw external payload shape
- **Sanitizer** (`Infrastructure`) — low-level canonicalization for text, dates, and dirty field formats
- **Translator / normalizer** (`Infrastructure`) — semantic cleanup and canonicalization
- **Exception translator** (`Infrastructure`) — foreign/runtime/protocol faults to stable internal faults
- **Evidence logger** (`Infrastructure`) — redacted dispute trail when required

For simple field-to-field mapping, prefer [`mapping.md`](mapping.md) and `Mapperly`. Once semantics become unstable, move beyond pure mapping and introduce a real ACL.

## Integration decision table

| Pollution level | Typical scenario | Recommended ACL shape | Acceptable approach | Unacceptable approach |
|---|---|---|---|---|
| **Light** | Same-company modern system, correct semantics, clean models, mostly naming differences | Thin mapping + boundary translation | `Mapperly`, thin helper, wire DTO separated from Core DTO | Reusing the foreign DTO directly as a Core DTO |
| **Moderate** | Third-party API/SDK, strange names, unique error model, outcome depends on 1-2 fields | Core port + Infrastructure adapter + translator + exception normalization | SDK wrapper, explicit error mapping, correlation propagation, wire-model isolation | Letting Core reference SDK types or raw `HttpClient`/SDK results |
| **Heavy** | Legacy/shared service, `200` with semantic failure, `dynamic`/`JObject`, dirty data, success/failure inferred from multiple signals | Full ACL: adapter + wire models + translator + sanitization + exception translator + evidence logging | Multi-step normalization, contradictory-signal handling, dispute-ready side-channel logging | Letting magic strings, code tables, `dynamic`, fake-success semantics, or vendor naming leak into Core |

### Escalation rules

- If success/failure requires combining multiple fields, message text, HTTP status, or missing-field heuristics, treat the integration as **heavy**.
- If the external system emits untrusted semantics, thin mapping alone is insufficient.
- If filling a missing value would affect business behavior, reject or mark `unknown` instead of inventing a safe-looking default.

## Sanitization rules

- Sanitize text, dates, numbers, and enum-like values before they enter Core DTOs.
- It is acceptable to normalize:
  - whitespace and encoding variants
  - alternate calendar formats
  - punctuation/casing conventions
  - vendor-specific “blank means null” patterns
- It is **not** acceptable to silently synthesize values that change business meaning.

## Transaction and runtime boundaries still belong elsewhere

The ACL supports safe integration, but it does not replace:

- [`transactions.md`](transactions.md) for transaction ownership and remote-IO ordering
- [`resilience.md`](resilience.md) for timeout/retry/breaker policy
- [`audit-log.md`](audit-log.md) and [`external-integration-firewall.md`](external-integration-firewall.md) for evidence/logging strategy

The preferred use-case shape remains:

1. call adapter / ACL
2. validate normalized result
3. open short local transaction
4. perform local write
5. use outbox for post-commit side effects when needed

## Review checklist

1. Are foreign names, codes, and payload shapes confined to wire models and translators?
2. Is the chosen ACL shape proportional to the integration severity?
3. Are sanitization and normalization explicit instead of scattered in use cases?
4. Are exceptions normalized without hiding real dependency faults?
5. Did we avoid silently inventing business-significant default values?
6. Does Core still speak only in clean DTOs, ports, and domain language?

## Related rules

- [`external-integration-firewall.md`](external-integration-firewall.md)
- [`cross-project-boundaries.md`](cross-project-boundaries.md)
- [`architecture-protocol.md`](architecture-protocol.md)
- [`mapping.md`](mapping.md)
- [`transactions.md`](transactions.md)
- [`resilience.md`](resilience.md)

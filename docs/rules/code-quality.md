---
inclusion: manual
---

# Code Quality & Review

This file is a **copyable** checklist. Prefer rules that can be enforced by analyzers/tests.

## Adoption profile (legacy-safe vs strict)

- **Legacy-safe (default)**: do-not-touch working legacy code. Apply rules to new/changed code and prefer incremental refactors.
- **Strict (new project)**: enforce the full checklist globally from day 0.

## Code Review Output Order
1. [Problems] - rule violations (SOLID, SQL ownership, async pitfalls, Fowler smells, etc.)
2. [Improvements] - works but could be cleaner
3. [What's good]

Read fully before commenting. Name the rule violated. Review only - no fixes unless asked.

## Fowler Smells
| Smell | Flag |
|---|---|
| Long Method (>30 lines) | [suggestion] |
| Long Parameter List (4+) | Suggest DTO |
| Duplicate Code | Extract |
| Primitive Obsession (int/string for status/type) | Use enum |
| Dead Code | Delete |
| Large Class, Divergent Change, Shotgun Surgery, Feature Envy, Data Clumps, Switch Statements, Speculative Generality, Middle Man, Message Chains, Temporary Field | [suggestion] |

## Null Safety
- Use `?.` and `??` over explicit null checks
- `string.IsNullOrWhiteSpace()` never `== null` or `== ""`
- Never return `null` from Service - throw or return empty

## Exceptions
- Never throw generic `Exception`
- `throw new KeyNotFoundException($"Warehouse {id} not found")`
- `throw new ArgumentException("...", nameof(param))`
- `throw new InvalidOperationException($"...")`

Bad (stop):

```csharp
throw new Exception("failed");
```

Good (follow):

```csharp
throw new ArgumentException("Invalid template id.", nameof(templateId));
```

## Security scanning (SAST)

- Treat SAST as a delivery **check threshold** (report-only for legacy onboarding; enforceable for new projects).
- Maintain auditable artifacts (report + exceptions + remediation tracking).
- See: [`security-sast.md`](security-sast.md)

## Return Types
- `IEnumerable<T>` not `List<T>` in Service/Repository
- Use `record` for immutable DTOs
- Use `IAsyncEnumerable<T>` for large result sets

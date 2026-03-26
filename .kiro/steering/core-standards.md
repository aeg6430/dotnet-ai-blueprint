---
inclusion: always
---

# Core Standards

## Modes
Read `.ai-modes` for current values of `LEARNING_MODE`, `CODE_REVIEW_MODE`, `BLIND_SPOT_MODE`.

**Learning Mode ON:** after every generation append 3-5 bullets pattern applied, bad pattern avoided, trade-off.
**Code Review Mode ON:** on trigger phrases ("review this / check this / is this good") output: [Problems] [Improvements] [What's good]. Review only never implement unless asked.
**Blind Spot STRICT:** missing spec detail that affects code stop and ask before writing anything.
**Blind Spot WARN:** flag as comment at top of generated code, proceed with best-guess defaults.

## Spec Override
`docs/specs/` always overrides defaults no exceptions.
Conflict with hard rule flag and ask. Ambiguous flag as blind spot, ask first.

## SOLID Non-Negotiable
Violates SOLID **refuse, explain, suggest correct approach.** Even if user says "just do it."

- **S**: One class, one job. One Service per domain. One Repository per feature context.
- **O**: New use case = new class or method. Never modify existing for unrelated behavior.
- **L**: Never implement interface methods as no-op. Can't fulfill contract split interface.
- **I**: No fat interfaces. Split by feature. Repository >5-7 methods warn and suggest split.
- **D**: Always inject interfaces, never concrete classes. Controllers, Services, Repositories.

## Never Delete Code
Can't fix something stop, explain, leave code untouched, use not-implemented pattern.
Never delete, comment out, or silent `// TODO`. Only delete when user **explicitly** instructs it.

## No Magic Numbers
- Use enums from `Project.Core/Enums/` never raw literals for state/type/status/category
- Enum missing create it first
- `StatusId`/`TypeId`/`CategoryId` typed as `int` flag as enum candidate

## Coding Style
- `catch (Exception e)` always `e`, never `ex`
- `readonly` on all injected fields
- Full names: `_warehouseRepository` not `_warehouseRepo`
- `IEnumerable<T>` not `List<T>` in return types
- `nameof()` not hardcoded strings
- No `async void` always `async Task` (except event handlers)
- No `.Result`/`.Wait()` always `await`
- No commented-out code
- No hardcoded config use `IConfiguration`
- No `Thread.Sleep` use `await Task.Delay`
- No `out`/`ref` params return tuple or result object
- No `static` except extension methods and pure utility helpers
- Max 2 nesting levels extract deeper logic to private method
- Max ~30 lines per method
- Guard clauses first, happy path last
- No nested ternary operators
- No `if (x == true)` | No `return condition ? true : false`
- Method name contains "And" split into two
- Prefer pattern matching `switch` over long `if/else`
- `string.IsNullOrWhiteSpace()` never `== null` or `== ""`
- Never return `null` from Service throw or return empty
- Never throw generic `Exception` specific type with context

## Detail Rules Read On Demand
| Working on | File |
|---|---|
| SQL, Dapper, repositories | `docs/rules/sql.md` |
| Mapperly, DTO API model | `docs/rules/mapping.md` |
| Code quality, smells, async, null safety | `docs/rules/code-quality.md` |
| Unit tests, NUnit, Moq | `docs/rules/testing.md` |
| Code review or learning mode | `docs/rules/review-learning.md` |

Apply Martin Fowler's smell catalogue silently during generation. Flag by name during review.

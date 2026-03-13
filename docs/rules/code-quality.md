# Code Quality Rules

> Read this file when generating or reviewing business logic, Services, or any non-SQL code.
> Apply Martin Fowler's "Refactoring: Improving the Design of Existing Code" as a continuous lens.

---

## Early Return & Guard Clauses

- Validate and return early at the top — never wrap the entire method in an `if` block
- Guard clauses first, happy path last
- If a method has more than one level of indentation caused by validation → refactor to early return

---

## Boolean Simplification

- `if (x == true)` → `if (x)`
- `return condition ? true : false` → `return condition`
- `bool hasStock = quantity > 0 ? true : false` → `bool hasStock = quantity > 0`

---

## Nesting & Complexity

- Max **2 levels of nesting** — extract deeper logic to a private method with a descriptive name 💡
- Max **~30 lines per method** — extract if longer 💡
- No nested ternary operators 🚫
- If a block needs a comment to explain what it does → that block should be its own method

---

## Method Design

- If method name contains "And" → split into two methods
- No `out` / `ref` parameters — return a tuple or result object instead
- No `static` classes except extension methods and pure utility helpers
- Prefer pattern matching `switch` expressions over long `if/else` chains

---

## Async / Await 🚫

- Never `async void` — always `async Task` (exception: event handlers only)
- Never `.Result` or `.Wait()` — always `await`
- Never `await` inside a `catch` block
- Never fire-and-forget without explicit error handling
- Never `Thread.Sleep` — use `await Task.Delay`

---

## Null Safety

- Use `?.` and `??` instead of explicit null checks where possible
- `string.IsNullOrWhiteSpace()` — never `== null` or `== ""`
- Never return `null` from a Service method — throw a meaningful exception or return empty result

---

## Exceptions

- Never throw generic `Exception` — always use a specific type with context:
  - `throw new KeyNotFoundException($"Warehouse {id} not found")`
  - `throw new ArgumentException("Quantity must be greater than zero", nameof(quantity))`
  - `throw new InvalidOperationException($"Transfer already in progress for SkuId {skuId}")`

---

## Return Types

- `IEnumerable<T>` not `List<T>` in Service and Repository return types
- Use `record` for immutable DTOs instead of `class` where applicable
- Use `IAsyncEnumerable<T>` for streaming large result sets — see `docs/rules/sql.md`

---

## General

- `readonly` on all constructor-injected fields
- `nameof()` not hardcoded strings in exceptions and logging
- No commented-out code — delete it (git history exists for a reason)
- No hardcoded config values — always use `IConfiguration`

---

## Fowler Code Smell Reference

Apply these as a lens during generation (avoid silently) and review (flag by name):

| Smell | What to look for | Tier |
|---|---|---|
| **Long Method** | Method exceeds ~30 lines | 💡 |
| **Large Class** | Repository or Service with too many methods | 💡 |
| **Long Parameter List** | Method with 4+ parameters | 💡 Suggest DTO |
| **Duplicate Code** | Same logic in two places | 🚫 Extract |
| **Divergent Change** | One class changed for many reasons | 💡 Split (SOLID-S) |
| **Shotgun Surgery** | One change requires edits in many classes | 💡 Consolidate |
| **Feature Envy** | Method uses another class's data more than its own | 💡 Move method |
| **Primitive Obsession** | `int`/`string` for status, type, category | 🚫 Use enum |
| **Data Clumps** | Same group of variables always together | 💡 Introduce DTO |
| **Switch Statements** | Long switch/if-else on type or status | 💡 Pattern matching or polymorphism |
| **Dead Code** | Unused methods, variables, commented-out code | 🚫 Delete |
| **Speculative Generality** | Abstractions built "just in case" | 💡 Remove until needed |
| **Middle Man** | Class that only delegates to another | 💡 Remove indirection |
| **Message Chains** | `a.b().c().d()` long call chains | 💡 Introduce method |
| **Temporary Field** | Field only set under certain conditions | 💡 Extract class or method |

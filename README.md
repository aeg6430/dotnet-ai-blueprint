# dotnet-ai-blueprint

A reusable seed for .NET Clean Architecture projects with built-in AI IDE rules.  
Works with **Kiro** (automatic via steering) and **Cursor / Windsurf** (automatic via `.cursorrules`) and **Copilot Chat in Visual Studio / VS Code** (manual paste).

---

## What's Inside

This seed gives every new project:
- A strict, token-efficient rule set that AI assistants follow automatically
- Enforced Clean Architecture boundaries — no guessing, no drift
- Ready-to-use code templates for the patterns you'll always need
- Toggleable AI behaviour modes via a single config file

---

## Repo Structure

```
dotnet-ai-blueprint/
├── .kiro/
│   └── steering/                 ← Kiro steering rules (auto-loaded by inclusion mode)
│       ├── core-standards.md     ← [always] SOLID, coding style, modes, spec override
│       ├── not-implemented.md    ← [always] not-implemented comment+throw pattern
│       ├── sql.md                ← [fileMatch: **/Repositories/**/*.cs]
│       ├── mapping.md            ← [fileMatch: **/Mappers/**/*.cs]
│       ├── testing.md            ← [fileMatch: **/Tests/**/*.cs]
│       └── code-quality.md       ← [manual] Fowler smells, review output format
├── .cursorrules                  ← Cursor / Windsurf rule entry point
├── .ai-modes                     ← Toggle AI behaviour modes
├── .gitignore
├── COPILOT_PROMPT.md             ← Paste into Copilot Chat at session start
│
├── docs/
│   ├── ARCHITECTURE.md           ← Folder structure, patterns, implementation rules
│   ├── rules/                    ← Detailed rules — read on demand by AI
│   │   ├── sql.md
│   │   ├── mapping.md
│   │   ├── code-quality.md
│   │   ├── testing.md
│   │   ├── review-learning.md
│   │   └── not-implemented-pattern.md
│   ├── specs/                    ← Feature specs (one .md per feature)
│   │   └── feature-spec-template.md
│   └── diagrams/                 ← ER diagrams, flow diagrams
│
└── templates/                    ← Canonical code patterns — AI always follows these
    ├── BaseRepository.cs
    ├── WarehouseRepository.cs
    ├── StockService.cs
    ├── WarehouseMapper.cs
    ├── GlobalExceptionHandler.cs
    ├── ServiceExtensions.cs
    ├── ApiResponse.cs
    ├── NotImplementedPattern.cs
    ├── PaginationTemplate.cs
    ├── Program.cs
    ├── Startup.cs
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── nlog.config
    └── Project.Api.http
```

> `src/` is not in the seed — that's your project-specific code.

---

## Quickstart

### 1. Clone the seed
```bash
git clone https://github.com/you/dotnet-ai-blueprint.git
cd dotnet-ai-blueprint
```

### 2. Create your .NET solution alongside it
```bash
dotnet new webapi -n MyProject.Api
dotnet new classlib -n MyProject.Core
dotnet new classlib -n MyProject.Infrastructure
dotnet new nunit -n MyProject.Tests
```

### 3. Copy templates into your project
Use the files in `templates/` as your starting point — rename namespaces from `Project.*` to your project name.

### 4. Configure AI modes
Edit `.ai-modes` at the repo root:

```env
LEARNING_MODE=false        # true = AI explains every generation
CODE_REVIEW_MODE=true      # true = AI auto-reviews on trigger phrases
BLIND_SPOT_MODE=STRICT     # STRICT = stop and ask | WARN = proceed with comment
```

### 5. Start your AI IDE

**Kiro** — open the project and Kiro reads `.kiro/steering/` automatically. `core-standards` and `not-implemented` load on every interaction. `sql`, `mapping`, and `testing` load only when you open a matching file. Call `#code-quality` manually when you want a code review.

**Cursor / Windsurf** — open the project and `.cursorrules` is loaded automatically.

---

## Using with Copilot Chat (Visual Studio / VS Code)

Copilot Chat does not read rule files automatically. Instead:

1. Open `COPILOT_PROMPT.md`
2. Paste the entire content as your **first message** in a new Copilot Chat session
3. Then ask your question normally

For mode toggles — paste the relevant lines from `.ai-modes` alongside the prompt.

---

## Code Review

Output order is always: Problems -> Improvements -> What's good. Review only — AI never implements fixes unless explicitly asked.

**Kiro** — `code-quality.md` is `manual` inclusion (Fowler smells + review format are only needed at review time, not during generation):
1. Type `#code-quality` to load the steering file
2. Say "review this" / "check this" / "is this good"

**Cursor / Windsurf** — `code-quality.md` is listed in the on-demand table inside `.cursorrules`. Mention the file or use a trigger phrase and the AI will apply it.

**Copilot Chat** — review format is already included in `COPILOT_PROMPT.md`. Just use a trigger phrase: "review this" / "check this" / "is this good".

---

## Stack

| Concern | Library |
|---|---|
| ORM | Dapper (EF Core forbidden) |
| Mapping | Riok.Mapperly |
| Logging | NLog |
| Testing | NUnit + Moq |
| Auth | JWT Bearer |
| Error Handling | `IExceptionHandler` (ASP.NET 8+) |

---

## Key Rules (summary)

- **SOLID** is non-negotiable — AI refuses and suggests the correct approach on any violation
- **No magic numbers** — always enums, never raw `int` literals for state or type
- **SQL ownership** — one method one purpose, never modify existing SQL for a new caller
- **No loops calling repositories** — use batch operations instead
- **Never delete code** to solve a problem — if stuck, explain and use `throw new NotImplementedException()`
- **Transactions** belong in Services only — repositories are never aware of transaction boundaries
- **All DI** lives in `ServiceExtensions.cs` — never in `Program.cs`

Full rules: see `docs/ARCHITECTURE.md` and `docs/rules/`.

---

## Adding a Feature

1. Create `docs/specs/{feature-name}.md` — describe the feature, endpoints, fields, business rules
2. Open your AI IDE and describe the task — the AI reads your spec first and flags any blind spots before writing code
3. Review generated code — Code Review Mode is on by default

---

## License

MIT

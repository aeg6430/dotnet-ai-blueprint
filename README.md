# dotnet-ai-blueprint

A reusable seed for .NET Clean Architecture projects with built-in AI IDE rules.  
Works with **Kiro, Cursor, Windsurf** (automatic) and **Copilot Chat in Visual Studio / VS Code** (manual paste).

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
├── .cursorrules                  ← AI IDE rule entry point (slim, ~5KB)
├── .ai-modes                     ← Toggle AI behaviour modes
├── .gitignore
│
├── docs/
│   ├── ARCHITECTURE.md           ← Folder structure, patterns, implementation rules
│   ├── rules/                    ← Detailed rules — read on demand by AI
│   │   ├── sql.md                ← SQL ownership, batch, parameters, optimization
│   │   ├── mapping.md            ← Mapperly patterns, DTO ↔ API model boundaries
│   │   ├── code-quality.md       ← Fowler smells, async rules, nesting, null safety
│   │   ├── testing.md            ← NUnit + Moq patterns, naming, coverage
│   │   └── review-learning.md    ← Code Review Mode, Learning Mode
│   ├── specs/                    ← Feature specs (one .md per feature)
│   │   └── feature-spec-template.md ← Copy this when starting a new feature
│   ├── NOT_IMPLEMENTED_PATTERN.md ← How to handle code the AI cannot implement
│   └── diagrams/                 ← ER diagrams, flow diagrams
│
├── templates/                    ← Canonical code patterns — AI always follows these
│   ├── BaseRepository.cs
│   ├── WarehouseRepository.cs
│   ├── StockService.cs
│   ├── WarehouseMapper.cs
│   ├── GlobalExceptionHandler.cs
│   ├── ServiceExtensions.cs
│   ├── ApiResponse.cs
│   ├── Program.cs
│   ├── Startup.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── nlog.config
│   └── Project.Api.http
│
└── COPILOT_PROMPT.md             ← Condensed rules for Copilot Chat (paste at session start)
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
Edit `.ai-modes` at the repo root to set your preferred behaviour:

```env
LEARNING_MODE=false        # true = AI explains every generation
CODE_REVIEW_MODE=true      # true = AI auto-reviews on trigger phrases
BLIND_SPOT_MODE=STRICT     # STRICT = stop and ask | WARN = proceed with comment
```

### 5. Start your AI IDE
Open the project in Kiro, Cursor, or Windsurf — the AI will bootstrap automatically from `.cursorrules` and `.ai-modes`.

---

## Using with Copilot Chat (Visual Studio / VS Code)

Copilot Chat does not read `.cursorrules` automatically. Instead:

1. Open `COPILOT_PROMPT.md`
2. Paste the entire content as your **first message** in a new Copilot Chat session
3. Then ask your question normally

For mode toggles — paste the relevant lines from `.ai-modes` alongside the prompt.

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

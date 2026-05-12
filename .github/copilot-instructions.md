# Copilot instructions

This file is the **entrypoint** for Copilot guidance. It is intentionally short; it points you to the authoritative rules and copyable examples.

For Cursor, the equivalent repository guidance lives under [`.cursor/rules/`](../.cursor/rules/), starting from [`00-entrypoint.mdc`](../.cursor/rules/00-entrypoint.mdc).

## Read order (required)

1. [`docs/ARCHITECTURE.md`](../docs/ARCHITECTURE.md)
2. Rules (binding):
   - [`docs/rules/architecture-protocol.md`](../docs/rules/architecture-protocol.md)
   - [`docs/rules/transactions.md`](../docs/rules/transactions.md)
   - [`docs/rules/resilience.md`](../docs/rules/resilience.md)
   - [`docs/rules/audit-log.md`](../docs/rules/audit-log.md)
   - [`docs/rules/external-integration-firewall.md`](../docs/rules/external-integration-firewall.md)
   - [`docs/rules/anti-corruption-layer.md`](../docs/rules/anti-corruption-layer.md)
   - [`docs/rules/sql.md`](../docs/rules/sql.md)
   - [`docs/rules/mapping.md`](../docs/rules/mapping.md)
   - [`docs/rules/code-quality.md`](../docs/rules/code-quality.md)
   - [`docs/rules/testing.md`](../docs/rules/testing.md)
3. Shadow examples (copy patterns):
   - [`docs/starter-pack/shadow-examples/`](../docs/starter-pack/shadow-examples/)
   - [`templates/`](../templates/)

For endpoint protection, CI slowness, Build Agent exclusions, or restricted cloud tooling, also read [`docs/rules/endpoint-protection.md`](../docs/rules/endpoint-protection.md).

## Recommended workspace model

When using this starter pack with a separate product repository, prefer a three-folder workflow:

- **Blueprint**: this repository; rules, templates, and source-of-truth docs live here.
- **Seed / Sandbox**: a clean exported starter-pack project used to prototype the ideal implementation.
- **Target**: the real product repository (legacy or new project) where final changes are integrated.

For project setup, namespace conversion, or folder renaming on a Seed folder, read [`docs/starter-pack/project-setup-protocol.md`](../docs/starter-pack/project-setup-protocol.md) before implementation work begins.

Default behavior:

1. Read the target repo's `docs/specs/` first, if present.
2. Use the target repo's `docs/requirements/raw/` as source material, not as the final coding contract.
3. If the spec is incomplete, fall back to this blueprint's rules and templates.
4. If the target codebase is noisy or legacy-heavy, prototype the clean version in Seed first.
5. Integrate the logic back into Target using Target naming, style, and boundaries rather than copying blueprint-specific abstractions verbatim.

## Plan-first workflow (VS Code & Visual Studio)

This repository’s rules live in markdown; **Plan mode** is how you get a Cursor-style “plan before edits” flow in Copilot: the plan agent uses read-only exploration, produces steps and open questions, and **does not apply code changes** until you approve and hand off to **Agent** mode (or you implement manually). Plan may be **preview** and can be disabled by org policy—check your IDE version and Copilot settings.

If Plan / Agent features are unavailable in your environment, use normal Copilot Chat plus manual edits and follow the same markdown entrypoints in smaller reviewable batches.

### Visual Studio Code

1. Open **Copilot Chat** (e.g. chat icon / `Ctrl+Alt+I` per your keybindings).
2. At the **bottom of the chat view**, open the **mode / agents** dropdown and choose **Plan** (or start the prompt with **`/plan`** and describe the task—type **`/`** in the box to see current slash commands for your version).
3. Iterate on the plan (clarify scope, answer questions).
4. When ready, start implementation via **Agent** mode or the **Start implementation** (or equivalent) control in the chat UI.

Docs: [Planning with agents in VS Code](https://code.visualstudio.com/docs/copilot/agents/planning) · [Chat in your IDE (VS Code)](https://docs.github.com/en/copilot/how-tos/chat-with-copilot/chat-in-ide?tool=vscode)

### Visual Studio (2022)

1. Open **GitHub Copilot Chat**: **View → GitHub Copilot Chat** (Copilot is built in from **17.10** onward; older minors may need the Copilot extensions—see Microsoft Learn).
2. At the **bottom of the chat panel**, open the **agents / mode** dropdown and select **Plan**.
3. Enter a prompt that describes the feature, refactor, or bugfix; review the generated plan and follow-ups.
4. When satisfied, use **Start Implementation** / switch to **Agent** as the UI offers so Copilot can apply edits.

Docs: [Chat in your IDE (Visual Studio) — Plan mode](https://docs.github.com/en/copilot/how-tos/chat-with-copilot/chat-in-ide?tool=visualstudio#plan-mode) · [Using GitHub Copilot Chat in Visual Studio](https://learn.microsoft.com/visualstudio/ide/visual-studio-github-copilot-chat?view=vs-2022#use-copilot-chat-in-visual-studio)

## Non-negotiables

- **Layering**: `{CoreNamespace}` must not depend on `{InfrastructureNamespace}` or `{ApiNamespace}` (replace tokens per [`docs/starter-pack/README.md`](../docs/starter-pack/README.md)).
- **Repositories**: SQL + Dapper only. No business rules, no JSON parsing, no `SELECT *`, no interpolated SQL.
- **Services / use cases**: Follow the explicit short-lived UoW rules in [`docs/rules/transactions.md`](../docs/rules/transactions.md): no transaction for read-only flows, no remote IO while the DB transaction is active, begin late, commit early, and prefer outbox for cross-system side effects.
- **API**: Controllers are thin; Infrastructure wiring belongs in DI composition.
- **Audit logging**: Capture security-relevant audit events at the API entry point / global exception boundary per [`docs/rules/audit-log.md`](../docs/rules/audit-log.md); include actor identity, action, target, result, and correlation metadata.
- **Logging**: Prefer `ILogger<T>`; do not leak secrets/PII.

## Cross-cutting (“AOP”) meaning

Use official ASP.NET Core mechanisms: middleware + MVC filters + (optionally) DI decorators. Do **not** introduce IL-weaving aspect frameworks.

## Output requirements

When generating code:

- For **multi-file** work or anything that touches **layering / transactions / security**, prefer **Plan mode first** (see above), then **Agent** mode or manual edits.
- For project setup requests on a Seed folder, use [`docs/starter-pack/project-setup-protocol.md`](../docs/starter-pack/project-setup-protocol.md) before feature implementation or refactoring.
- If a target repo provides `docs/specs/`, follow the feature spec before applying default blueprint assumptions.
- If the task touches API-edge monitoring, traceability, security review, or audit evidence, also follow [`docs/rules/audit-log.md`](../docs/rules/audit-log.md).
- If the task touches endpoint protection, CI slowness, Build Agent exclusions, restricted cloud tooling, or Apex One, also follow [`docs/rules/endpoint-protection.md`](../docs/rules/endpoint-protection.md).
- Prefer patterns from `docs/starter-pack/shadow-examples/` or `templates/`.
- Keep changes minimal and consistent with analyzers and architecture tests.
- If a rule is unclear, add an ADR or update the rule instead of inventing a new convention.


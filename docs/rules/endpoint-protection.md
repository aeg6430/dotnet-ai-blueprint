---
inclusion: manual
---

# Endpoint Protection (Apex One and similar environments)

This document defines the operational guidance for working in environments where endpoint protection, antivirus, or real-time scanning can materially affect developer workflows and CI performance.

Treat this as the source of truth when the team operates under tools such as **Trend Micro Apex One** or similar centrally managed endpoint-protection products.

## Why this matters

Real-time scanning can turn normal engineering activity into a performance bottleneck when the workflow creates or touches many files in a short period.

Common high-churn scenarios in this pack:

- AI-driven project setup / namespace conversion across a working directory
- `dotnet test` for Phase B/C architecture gates
- reflection-heavy checks that load many assemblies
- observation/report modes that write under `artifacts/`
- exported working directories such as `_starter-pack-seed/out/`

## Typical symptoms

### Developer workstation

- Cursor or VS Code becomes sluggish or appears frozen during AI-driven rename/update batches
- file save / rename latency spikes
- high I/O wait during project setup or large code-generation passes

### CI / Build Agent

- `dotnet test` time increases sharply without corresponding code complexity growth
- architecture tests fail by timeout while code remains unchanged
- assembly-loading or report-writing steps become the slowest part of the pipeline

## Reviewed exclusion candidates

Exclusions should be centrally reviewed and approved by the responsible security / platform team. Do **not** assume local developers should disable endpoint protection on their own.

Typical reviewed exclusion candidates:

- Build Agent workspace root
- `bin/`
- `obj/`
- `TestResults/`
- `artifacts/`
- `_starter-pack-seed/out/`

### Workstation vs CI

- **Developer workstation**: focus on high-churn working directories used for project setup, AI rename operations, and repeated local test output.
- **CI / Build Agent**: focus on the build workspace and generated output paths that are recreated every run.

If exclusions are not permitted, prefer smaller rename batches and avoid repeatedly regenerating or rescanning the same artifact folders during normal editing.

## Testing strategy under endpoint protection

Prefer the lightest mechanism that still gives stable enforcement:

- **Source-scan firewalls (regex heuristics)**: preferred for obvious folder-local anti-patterns; usually lower overhead than reflection-based checks.
- **ArchUnit / reflection-heavy checks**: keep for assembly/type boundary rules that genuinely need them, but expect higher sensitivity to scanner overhead.
- **Observation/report modes**: use intentionally and avoid rerunning them continuously during active editing.

The goal is **not** to remove strong architecture checks. The goal is to avoid paying reflection/assembly-load cost where a simpler source scan already gives the needed signal.

## AI tooling constraints

If Cursor cloud features, Copilot agents, or similar capabilities are restricted:

- use the shared [`../starter-pack/project-setup-protocol.md`](../starter-pack/project-setup-protocol.md)
- keep Copilot as a supported fallback entrypoint via [`.github/copilot-instructions.md`](../../.github/copilot-instructions.md)
- fall back to plain chat/manual editing when agent features are unavailable

Restricted cloud tooling must not block the baseline onboarding flow.

## Documentation links

- Setup flow: [`../starter-pack/project-setup-protocol.md`](../starter-pack/project-setup-protocol.md)
- Architecture enforcement map: [`architecture-protocol.md`](architecture-protocol.md)
- Automation coverage: [`../optional/automation/automation-coverage.md`](../optional/automation/automation-coverage.md)
- Automation decision matrix: [`../optional/automation/automation-decision-matrix.md`](../optional/automation/automation-decision-matrix.md)

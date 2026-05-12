# Dependency graph (optional)

Goal: produce a simple dependency graph artifact (e.g. DOT/SVG) so engineers and PMs can see layering at a glance.

## Recommended approach

- Generate a **Graphviz DOT** file from project/assembly references.
- Convert DOT to SVG/PNG using Graphviz (`dot`).

## Minimal DOT format

```dot
digraph deps {
  Core -> Infrastructure;
  Api -> Core;
}
```

## How to produce the artifact

1. Use a small console app, repo-local tool, or test to generate `artifacts/deps.dot`.
2. If your environment allows Graphviz, convert the DOT file into `artifacts/deps.svg`.
3. Use in draw.io (diagrams.net) (optional):
   - Import the generated `artifacts/deps.svg`, then edit annotations as needed.

## Repo-local tool

If this pack includes the repo-local tool under `tools/dependency-graph/`, use it through whatever local workflow is approved in your environment.

Default behavior:

- scans `*.csproj` files under the current root
- ignores `bin/` and `obj/`
- writes `artifacts/deps.dot`

## Pack-provided tool (optional)

This pack can include a small console tool that scans `*.csproj` project references and emits `deps.dot` without external dependencies.

- Source: `tools/dependency-graph/`
- Output: `artifacts/deps.dot`

Notes:

- Keep this optional; it is for visibility, not a gate.
- The authoritative gate remains architecture tests (layering + firewalls).
- If `artifacts/` is monitored by Apex One or similar endpoint protection, generated graph output may cause extra I/O; use the artifact path approved by your environment and align with [`../../rules/endpoint-protection.md`](../../rules/endpoint-protection.md).

## How to interpret (review checklist)

- **Horizontal dependencies**: check whether a Service references another Service's concrete implementation or internal adapters. Prefer ports (interfaces) and keep use-case ownership clear.
- **Cycles**: check for circular references (A -> B and B -> A, or longer cycles). Cycles increase refactor cost and test complexity.
- **Core as an island**: Core should not depend on Web/host or DB driver primitives. Dependency arrows should not point from Core to outer layers.


# Performance acceptance template (k6 / JMeter)

This is a **compliance-friendly**, tool-agnostic acceptance template. Copy it to your project and fill in the numbers.

## Workload definitions

Define three layers:

1. **API-level budgets** (p95/p99, RPS, error rate)
2. **Scenario SLA** (end-to-end user journeys)
3. **Soak** (2–12h stability, resource ceilings)

## Environment assumptions

- Environment: dev / staging / pre-prod
- DB: shared / dedicated
- External dependencies: stubbed / real
- Test data volume:
- Warm-up strategy:

## 1) API-level budgets (example skeleton)

| Endpoint class | p95 | p99 | RPS | Error rate | Notes |
|---------------|-----|-----|-----|------------|-------|
| Read (GET) | TBD | TBD | TBD | TBD | |
| Write (POST/PUT) | TBD | TBD | TBD | TBD | |
| Import/batch | TBD | TBD | TBD | TBD | |

## 2) Scenario SLA (example skeleton)

| Scenario | Success criteria | Latency | Throughput | Notes |
|---------|------------------|---------|------------|-------|
| Login + list | 100% 2xx | TBD | TBD | |
| Create + verify | 100% 2xx | TBD | TBD | |
| Batch import | partial failures handled | TBD | TBD | |

## 3) Soak / stability

- Duration: TBD hours
- Error rate ceiling: TBD
- CPU ceiling: TBD
- Memory ceiling: TBD
- DB connections ceiling: TBD
- GC: no runaway allocations (define metric source)

## Output artifacts (evidence)

- k6/JMeter results file path:
- Summary report:
- CI link:


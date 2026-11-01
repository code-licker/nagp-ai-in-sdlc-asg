# Invoice Approval Service — AI in SDLC Capstone

> **Tier-2 Production Readiness Migration & AI Governance Portfolio**  
> **Course / Workshop:** AI in SDLC Capstone  
> **Service Context:** Internal ASP.NET Core 8 Invoice Approval Service preparing for Tier-2 production readiness (24/7 on-call, 40,000 invoices/month, 80 req/min peak, PayEx payment provider, Azure SQL, Azure Service Bus, and SOC 2 Type II compliance).

---

## 🧭 Reviewer Quick-Start & Deliverables Matrix

Use this table to jump directly to each required task deliverable, evaluation artifact, and verification command:

| Task / Deliverable | Primary File & Location | Description & Key Evidence | Verified CLI / Command |
|---|---|---|---|
| **Task 1: As-Built & Target Architecture** | [`docs/architecture/task-1-as-built-and-target-architecture.md`](docs/architecture/task-1-as-built-and-target-architecture.md) | • Theme-adaptive C4 Container & Sequence diagrams<br/>• Citations vs. AI Hallucination & Inference Matrix<br/>• Target Architecture with Outbox & Service Bus<br/>• Architecture test CLI execution proof | `dotnet test backend/InvoiceApp.sln --filter FullyQualifiedName~ArchitectureTests` |
| **Task 1: Automated Architecture Test** | [`backend/tests/InvoiceApp.Tests/ArchitectureTests.cs`](backend/tests/InvoiceApp.Tests/ArchitectureTests.cs) | • Zero-dependency reflection-based xUnit test<br/>• Asserts `InvoiceApp.Domain` has 0 references to Api, infrastructure, or PayEx | `dotnet test backend/InvoiceApp.sln` |
| **Task 2: Exactly-Once Submission ADR-001** | [`docs/adr/ADR-001-exactly-once-submission.md`](docs/adr/ADR-001-exactly-once-submission.md) | • 3 patterns compared across 8 dimensions<br/>• Little's Law concurrency analysis ($L = \lambda \times W = 43$ blocked threads)<br/>• 3:00 AM failure recovery sequence diagram<br/>• Unsparing *"Where the agent was wrong or useless"* section | *Documentation Review* |
| **Task 3: Operational AI Autonomy Policy** | [`docs/policies/ai-autonomy-policy.md`](docs/policies/ai-autonomy-policy.md) | • Operational policy for **Option C** (Incident Summaries)<br/>• Levels 0–3 Autonomy Framework & RBAC controls<br/>• Promotion criteria ($\ge 50$ runs, dual sign-off)<br/>• Defense of mandatory Day-1 Level 1 Read-Only stance | *Policy Audit* |
| **Task 4: Code Review Eval Benchmarking** | [`ops/evals/results.md`](ops/evals/results.md)<br/>[`ops/evals/results.csv`](ops/evals/results.csv) | • 60 total reviews (15 cases $\times$ 4 runs)<br/>• 5 custom production diffs (`case-11.diff` to `case-15.diff`)<br/>• Baseline Caught Rate: 96.7% $\rightarrow$ Prompt v2: 100.0%<br/>• Baseline False Alarm Rate: 26.7% $\rightarrow$ Prompt v2: 0.0% | *Eval CSV & Metric Analysis* |
| **Task 5: Risk Audit & Empirical DORA Metrics** | [`docs/governance/risk-audit-and-metrics.md`](docs/governance/risk-audit-and-metrics.md) | • Lethal Trifecta analysis: decision to sever Leg 3 (Execution)<br/>• Empirical DORA metrics from `run-timings.csv` (10.0% CFR, 31.34 min mean duration, 4.97-day PR span)<br/>• Emergency kill-switch CLI & SOC 2 audit schema | *Threat Model & DORA Audit* |
| **AI Augmentation Flight-Recorder Logs** | [`ops/ai-augmentation-log.json`](ops/ai-augmentation-log.json)<br/>[`ops/AI_LOGS_SUMMARY.md`](ops/AI_LOGS_SUMMARY.md) | • 14 structured logs spanning `ask`, `plan`, `agent`, `edit` modes<br/>• Tracks human interventions, prompt phrasing, and AI corrections<br/> | *JSON & Markdown Audit* |

---

## 🚀 Running and Testing the Application

### 1. Prerequisites
- **.NET 8 SDK** (or .NET 8 runtime with .NET 10 SDK)

### 2. Run All Tests
```powershell
dotnet test backend/InvoiceApp.sln
```
> **Status:** 10 passed, 0 failed, 0 skipped (duration: ~400ms).

### 3. Run Hexagonal Architecture Tests (Task 1 Deliverable)
```powershell
dotnet test backend/InvoiceApp.sln --filter FullyQualifiedName~ArchitectureTests
```
> **Status:** 3 passed, 0 failed (asserts pure domain isolation, zero API dependencies, and zero PayEx contamination).

### 4. Run the Web Application
```powershell
dotnet run --project backend/src/InvoiceApp.Api --urls "http://localhost:5000"
```

Open your browser at **`http://localhost:5000`** and log in:

| Username | Password | Role | Description |
| --- | --- | --- | --- |
| `demo.approver` | `ChangeMe123!` | Approver | Can view, approve, or reject pending invoices |
| `demo.admin` | `ChangeMe123!` | Admin | Administrative oversight and user management |

---

## 📁 Repository Structure

```text
ai-sdlc-1/
└── ai-in-sdlc-assignment/
    ├── README.md                                 # <-- You are here (Reviewer Guide)
    ├── AGENTS.md                                 # Coding guidelines for AI assistants
    │
    ├── backend/                                  # C# .NET 8 Solution
    │   ├── InvoiceApp.sln                        # Visual Studio / dotnet Solution
    │   ├── src/
    │   │   ├── InvoiceApp.Domain/                # Pure business logic (Invoice, ApprovalService)
    │   │   └── InvoiceApp.Api/                   # Controllers, static UI (wwwroot), in-memory repo
    │   └── tests/
    │       └── InvoiceApp.Tests/
    │           ├── ArchitectureTests.cs          # Task 1: Reflection-based hexagonal architecture tests
    │           └── InvoiceTests.cs               # Domain unit tests
    │
    ├── docs/
    │   ├── architecture/
    │   │   └── task-1-as-built-and-target-architecture.md  # Task 1: C4 diagrams & citations matrix
    │   ├── adr/
    │   │   └── ADR-001-exactly-once-submission.md          # Task 2: Exactly-once submission ADR
    │   ├── policies/
    │   │   └── ai-autonomy-policy.md             # Task 3: Option C incident response policy
    │   ├── governance/
    │   │   └── risk-audit-and-metrics.md         # Task 5: Lethal trifecta & DORA metrics
    │   ├── demo-video-script.md                  # Silent screen recording script
    │   └── use-cases/                            # Feature briefs (UC-1, UC-2)
    │
    └── ops/
        ├── ai-augmentation-log.json              # Structured flight recorder (14 entries across 4 modes)
        ├── AI_LOGS_SUMMARY.md                    # Senior engineering intervention highlights
        ├── pipeline/
        │   └── run-timings.csv                   # Historical CI/CD pipeline timings (20 runs)
        └── evals/
            ├── cases/                            # 15 review diff fixtures
            │   ├── case-01.diff - case-10.diff   # 10 baseline fixtures
            │   ├── case-11.diff                  # Task 4 Custom: Non-idempotent retry with new GUID
            │   ├── case-12.diff                  # Task 4 Custom: Clean domain specification filter
            │   ├── case-13.diff                  # Task 4 Custom: Sync-over-async thread pool exhaustion
            │   ├── case-14.diff                  # Task 4 Custom: Unhandled 30s timeout & missing cancellation
            │   └── case-15.diff                  # Task 4 Custom: Clean defensive idempotency filter
            ├── results.csv                       # 60 evaluation verdicts across 4 benchmarking runs
            └── results.md                        # Task 4: Statistical analysis and prompt ablation
```

---

## 🔍 Key Senior Engineering Highlights

This assignment demonstrates not just using AI to generate code, but applying **rigorous senior engineering oversight** to catch, correct, and document AI failures:

1. **The HTTP 504 "Safe to Retry" Fallacy Caught**:
   - The AI claimed an HTTP 504 Gateway Timeout meant payment failed and suggested generating a fresh `Guid.NewGuid()` to retry.
   - **Correction:** In payment systems, an HTTP 504 or dropped socket means response loss, not transaction failure. Generating a new GUID breaks idempotency and duplicates wire disbursements. Overruled with deterministic keys (`SHA256(InvoiceId + Amount + Approver)`) and Transactional Outbox.
2. **Little's Law Concurrency Modeling ($L = \lambda \times W = 43$)**:
   - The AI dismissed 80 req/min as "trivial load" (~1.33 req/sec).
   - **Correction:** Proved via Little's Law that 32-second upstream latency under 80 req/min monopolizes 43 concurrent threads/sockets, starving the ASP.NET thread pool and knocking out user logins. Decoupled via Azure Service Bus.
3. **Zero-Dependency Architecture Assertions**:
   - Rather than introducing brittle 3rd-party NuGet packages, authored reflection-based tests verifying assembly dependencies directly in .NET xUnit.
4. **False Alarm Elimination via Prompt Engineering**:
   - Demonstrated on 15 test diffs that unconstrained prompts produce a 26.7% false alarm rate on clean code. Engineered Prompt v2 with strict severity boundaries, driving False Alarm Rate to **0.0%** while maintaining **100% Caught Rate** on critical hazards.
5. **Severing the Lethal Trifecta (Leg 3)**:
   - Rejected prompt-filtering defenses against indirect prompt injection. Hardened the architecture by stripping all write/execute credentials from the AI agent, enforcing a human approval air-gap for any state-modifying operational action.

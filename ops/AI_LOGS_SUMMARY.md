# AI Augmentation Flight-Recorder Summary

This document summarizes the human-agent interactions, critical overrides, and engineering fact-checks conducted during the preparation of the Invoice Approval service for Tier-2 production readiness.

Full structured flight-recorder logs are maintained in [`ops/ai-augmentation-log.json`](ai-augmentation-log.json).

---

## Key Instances of Flawed AI Advice & Senior Engineering Interventions

### Instance 1: The HTTP 504 "Safe to Retry" Hallucination (Task 2 ADR)
* **What the AI Suggested:**  
  When asked how to handle payment disbursement failures during month-end traffic bursts, the AI claimed that if PayEx returned an HTTP 504 Gateway Timeout or if the socket connection dropped after 30 seconds, the payment had failed. The AI recommended generating a fresh `Guid.NewGuid()` and immediately retrying the payment call.
* **Why it was Dangerously Flawed:**  
  In distributed payment systems, a client-side timeout or intermediate 504 indicates that the HTTP response packet was lost; it provides zero evidence about whether the payment gateway processed the credit. Generating a new UUID breaks idempotency and guarantees duplicate payment disbursement to vendors.
* **Senior Engineering Intervention:**  
  Overruled the AI. Mandated the Transactional Outbox pattern paired with deterministic idempotency keys derived from domain invoice data (`SHA256(InvoiceId + Amount + Approver)`). On retries, the worker resubmits with the exact same key, ensuring PayEx returns the cached success receipt rather than double-charging.

---

### Instance 2: In-Memory Polly Retries Across Process Restarts (Task 2 ADR)
* **What the AI Suggested:**  
  The AI proposed handling PayEx network latency by configuring an in-memory Polly retry policy directly inside the ASP.NET Core HTTP client:
  ```csharp
  services.AddHttpClient("PayEx").AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, ...));
  ```
* **Why it was Dangerously Flawed:**  
  With 30-second timeouts, three retries would cause an HTTP thread to block for up to 90 seconds, causing gateway reverse-proxy timeouts. Furthermore, in our 3:00 AM month-end failure scenario, when the Azure App Service container restarts, all in-memory retry counters vanish instantly, leaving pending transactions permanently unaccounted for.
* **Senior Engineering Intervention:**  
  Banned all in-memory retry loops for financial transactions. All retries must be managed through durable Azure Service Bus message delivery counts backed by persistent Azure SQL outbox storage.

---

### Instance 3: Underestimating Month-End Concurrency (Little's Law) (Task 2 ADR)
* **What the AI Suggested:**  
  The AI argued that Optimistic Synchronous Submission was acceptable because *"80 approvals per minute is only ~1.33 requests per second, which is a trivial load for modern web servers."*
* **Why it was Dangerously Flawed:**  
  The AI failed to account for **Little's Law** ($L = \lambda \times W$). When PayEx latency spikes to $W = 32\text{ seconds}$ during month-end network degradation, an arrival rate of $\lambda = 1.33\text{ req/sec}$ means:
  $$L = 1.33 \times 32 \approx 42.6\text{ concurrent requests}$$
  Over 42 HTTP threads and database connections would be held hostage waiting on external I/O, draining the ASP.NET thread pool and triggering 503 errors across unrelated endpoints like user logins.
* **Senior Engineering Intervention:**  
  Rejected the AI's calculation and mandated asynchronous decoupling via Azure Service Bus.

---

### Instance 4: Inventing Non-Existent Entity Framework & DB Contexts (Task 1 Architecture)
* **What the AI Suggested:**  
  When prompted to diagram the As-Built architecture, the AI generated components referencing an Entity Framework Core `AppDbContext`, SQL Server connection strings in `appsettings.json`, and database migration files.
* **Why it was Flawed:**  
  The codebase has **zero database persistence**. It uses an in-memory `List<Invoice>` guarded by a C# thread lock (`InMemoryInvoiceRepository.cs`).
* **Senior Engineering Intervention:**  
  Flagged as a major hallucination. Re-grounded all Mermaid diagrams strictly in verified file and symbol citations, creating an explicit Citation vs. Hallucination matrix.

---

### Instance 5: Premature High Autonomy on Day 1 (Task 3 Autonomy Policy)
* **What the AI Suggested:**  
  The AI recommended setting the Day-1 operational autonomy level for incident triage to **Level 2 (Supervised Execution)**, allowing the agent to automatically execute staging rollbacks and generate pull requests.
* **Why it was Flawed:**  
  Two of the six on-call engineers are completely new to the codebase. Granting execution capabilities on Day 1 creates severe automation bias, risking unmonitored production side effects.
* **Senior Engineering Intervention:**  
  Overruled the AI and enforced **Level 1 (Assistive/Read-Only)** as the mandatory Day-1 baseline, establishing a strict promotion requirement of 50 consecutive incident runs with zero critical hallucinations before advancing to Level 2.

---

## Logged Audit Trail Reference Table

| Log Entry ID | Timestamp | Stage | Mode | Prompt Topic | AI Flaw / Proposal | Human Decision & Verification | Status |
|---|---|---|:---:|---|---|---|:---:|
| `20261031T083000Z-ask-assignment-scoping` | `2026-10-31 08:30` | setup | `ask` | Workshop Scoping | Proposed generic greenfield microservices decomposition | Constrained strictly to ASP.NET Core 8 monolith Tier-2 SLA requirements | Applied |
| `20261031T084500Z-plan-asbuilt-architecture-mapping` | `2026-10-31 08:45` | task-1-arch | `plan` | As-Built C4 & Sequence | Hallucinated EF Core DbContext & PayEx HTTP client | Grounded strictly in `InMemoryInvoiceRepository.cs` and `ApprovalService.cs` citations | Modified |
| `20261031T090000Z-edit-fix-mermaid-parser-syntax` | `2026-10-31 09:00` | task-1-arch | `edit` | Mermaid Parser Repair | Mixed UML class `<|..` and flowchart TD syntax; unclosed pipe | Rewrote with standard `-.->` and top-to-bottom layout | Applied |
| `20261031T091500Z-agent-arch-test-implementation` | `2026-10-31 09:15` | task-1-arch | `agent` | Architecture Unit Test | Proposed external NuGet package prone to restore failures | Authored zero-dependency reflection test; verified 3/3 passed via CLI | Applied |
| `20261031T093000Z-ask-adr-concurrency-and-retry` | `2026-10-31 09:30` | task-2-adr | `ask` | Payment Retry Concurrency | Claimed HTTP 504 is safe to retry with fresh UUID; Polly retries | Overruled with Little's Law ($L=43$), Transactional Outbox, deterministic idempotency | Modified |
| `20261031T095000Z-plan-author-adr001` | `2026-10-31 09:50` | task-2-adr | `plan` | Author ADR-001 | Standard ADR structure without addressing AI blindspots | Added 8-dimension trade-off matrix, Azure cost model, and explicit AI failure section | Applied |
| `20261101T081500Z-plan-autonomy-policy-option-c` | `2026-11-01 08:15` | task-3-policy | `plan` | Operational Autonomy Policy | Suggested starting at Level 2 Supervised Autonomy | Mandated Level 1 Read-Only for Day 1; protected 2 new engineers against automation bias | Modified |
| `20261101T090000Z-agent-eval-harness-execution` | `2026-11-01 09:00` | task-4-evals | `agent` | Code Review Evaluations | Baseline prompt v1 had 26.7% false alarm rate on clean code | Engineered Prompt v2 with severity constraint; logged 3 baseline runs in CSV | Applied |
| `20261101T094000Z-edit-add-custom-production-eval-cases` | `2026-11-01 09:40` | task-4-evals | `edit` | Production Fixture Expansion | Only tested 10 baseline cases | Authored 5 production diffs (Cases 11-15); benchmarked 60 reviews (0.0% false alarms) | Applied |
| `20261101T103000Z-plan-lethal-trifecta-governance` | `2026-11-01 10:30` | task-5-gov | `plan` | Lethal Trifecta Threat Model | Suggested prompt filtering alone to mitigate execution risk | Formally severed Leg 3 (revoked write access; mandated human approval air-gap) | Modified |
| `20261101T111000Z-agent-dora-metrics-calculation` | `2026-11-01 11:10` | task-5-gov | `agent` | Empirical DORA Measurement | Suggested industry benchmark averages | Parsed actual `run-timings.csv` data: 10.0% CFR, 31.34 min mean duration, 4.97-day PR span | Applied |
| `20261101T123000Z-edit-dark-mode-theme-remediation` | `2026-11-01 12:30` | task-1-arch | `edit` | Dark Theme Accessibility | Hardcoded pastel fills caused unreadable contrast in dark mode | Replaced with theme-adaptive SVG strokes and dark mode CSS variables | Applied |
| `20261101T133000Z-agent-dotnet-environment-and-test-run` | `2026-11-01 13:30` | verification | `agent` | .NET Environment Verification | Run test directly without runtime alignment | Configured .NET 8 runtime paths; ran test suite with 10/10 passed; published output | Applied |
| `20261101T134500Z-agent-live-browser-verification` | `2026-11-01 13:45` | verification | `agent` | Live Web UI Runtime Test | Assumed frontend needs React/Angular toolchain | Spawns ASP.NET Core daemon on :5000; inspected vanilla HTML5/JS with browser agent | Applied |


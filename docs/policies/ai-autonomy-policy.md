# Operational AI Autonomy Policy: First-Response Incident Summaries

| Policy Metadata | Specification |
|---|---|
| **Policy Identifier** | POL-OPS-003-INCIDENT-AI |
| **Effective Date** | 2026-11-01 (Mandatory Day-1 Production Baseline) |
| **Workflow Scope** | **Option C: First-Response Incident Summaries & Telemetry Triage** |
| **Governed Service** | Invoice Approval Service (Tier-2 Production) |
| **Policy Owners** | Tech Lead, On-Call Rotation Lead, Security & Compliance Lead |
| **Review Cadence** | Quarterly, or upon any Sev-1 incident post-mortem |

---

## 1. Objective & Operational Scope

When production alerts fire (e.g., latency threshold breach, PayEx timeout spike, Service Bus dead-lettering), the on-call engineer needs immediate situational awareness. 

This policy defines the operational autonomy boundaries for an AI Incident Assistant tasked with ingesting telemetry (alerts, logs, metrics, deployment diffs) and generating first-response incident briefings for the 6-engineer on-call rotation.

---

## 2. Autonomy Levels Defined

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ Level 0: Human Only                                                                    │
│ No AI assistance. Humans manually inspect logs, query metrics, and write updates.      │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ Level 1: Assistive (Read-Only)  ◄── [MANDATORY DAY-1 BASELINE]                         │
│ Agent reads telemetry; drafts incident summary & ranked hypotheses. Human acts.       │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ Level 2: Supervised Execution                                                          │
│ Agent stages diagnostic scripts or sandbox rollbacks; human explicitly approves.      │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ Level 3: Autonomous with Guardrails                                                    │
│ Agent auto-executes low-risk diagnostics (thread dumps, log level bump) & alerts.      │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

| Autonomy Level | Agent Permissions & Capabilities | Human-in-the-Loop Requirement | Fallback / Failure Mode |
|---|---|---|---|
| **Level 0 (Human Only)** | None. Agent processes are completely disabled. | Human engineer performs all investigations, log greps, and status drafts manually. | Full reliance on standard on-call runbooks. |
| **Level 1 (Assistive) [Day-1 Baseline]** | **Read-only access** to Azure Monitor logs, metrics, GitHub release diffs, and incident alert payloads. Agent synthesizes draft incident summaries and ranks probable causes. | **Mandatory Human Verification.** Drafts are tagged `[AI Draft - Verify Before Action]`. Human engineer must review facts and execute all terminal commands. | Agent outage has zero operational impact; human continues triage manually. |
| **Level 2 (Supervised)** | Agent creates pre-formatted diagnostic commands, stages rollback PRs in GitHub, or deploys fixes to a staging/sandbox environment. | **Explicit Human Approval.** No production command or customer notification executes without an explicit click-to-approve from the active on-call engineer. | Reverts to Level 1 if an unapproved action attempt is detected. |
| **Level 3 (Autonomous with Guardrails)** | Agent autonomously executes safe, idempotent, non-destructive containment actions (e.g., captures thread dumps, temporarily increases log verbosity to DEBUG for 15 min, captures Service Bus DLQ sample headers). | **Human Oversight & Anomaly Alerting.** Agent notifies the incident channel of every automated action. Human retains emergency stop / kill-switch authority. | Circuit breaker trips; downgrades to Level 1 immediately upon any execution error. |

---

## 3. Mandatory Safety Controls & Guardrails

To prevent production disruption, data leakage, and unauthorized state changes:

1. **Strict Read-Only Identity (RBAC):**  
   The AI agent operates under a dedicated Azure Managed Identity assigned exclusively the **`Monitoring Reader`** and **`Reader`** roles. It possesses zero write permissions to Azure SQL, App Service configurations, or Service Bus queues.
2. **Credential & Secret Redaction (Blast-Radius Control):**  
   All log streams pass through an automated regex sanitization filter prior to LLM ingestion. Connection strings, bearer tokens, customer PII, and financial amounts are masked with `[REDACTED]`.
3. **No Direct Execution on Payment Paths:**  
   The agent is strictly prohibited from executing, retrying, or modifying payment requests to PayEx under any circumstance or autonomy level.
4. **Channel Isolation:**  
   The agent may only post to internal triage channels (`#incidents-internal`). It is strictly blocked from communicating with customer-facing channels, public status pages, or external vendor portals.
5. **Rate & Concurrency Throttling:**  
   Bounded to a maximum of 5 LLM inference requests per incident trigger to prevent API thrashing, run-away token costs, and context flooding during multi-alert storms.
6. **Emergency Kill-Switch:**  
   The on-call engineer can immediately revoke agent access with a single CLI command:
   ```powershell
   az ad app update --id $AGENT_CLIENT_ID --end-date "2000-01-01"
   ```

---

## 4. Promotion Criteria (Level 1 $\rightarrow$ Level 2)

An increase in autonomy from Level 1 to Level 2 requires demonstrating statistical reliability across real incidents:

| Evaluation Metric | Minimum Threshold Requirement | Measurement Method |
|---|---|---|
| **Triage Sample Size** | $\ge 50$ consecutive production or staged incident runs | Evaluated during weekly on-call incident handovers. |
| **Factual Accuracy** | **100% zero critical hallucinations** | No invented log lines, non-existent error codes, or fabricated HTTP status codes. |
| **Root-Cause Hypothesis Ranking** | $\ge 90\%$ Top-1 or Top-2 accuracy | The true root cause identified in the post-mortem was ranked in the agent's top 2 hypotheses. |
| **Dangerous Recommendations** | **0 instances** | Zero recommendations suggesting blind retries on timeouts, in-memory retry loops, or database drops. |
| **Time-to-Draft (MTTD)** | $\le 120$ seconds from alert trigger | Automated timestamp difference between alert webhook and summary post. |

---

## 5. Approval Authority

Advancing or downgrading autonomy levels requires formal sign-off:

* **Promotion (Level 1 $\rightarrow$ Level 2 or Level 2 $\rightarrow$ Level 3):**  
  Requires unanimous written approval from:
  1. **Tech Lead** (validates technical safety and accuracy metrics).
  2. **Security & Compliance Lead** (validates SOC 2 compliance and least-privilege boundaries).
  3. **On-Call Rotation Lead** (confirms operational readiness of all 6 team engineers).
* **Demotion / Revocation:**  
  Any on-call engineer has the unilateral authority to immediately demote the agent to **Level 0 (Disabled)** if flawed behavior, hallucinated evidence, or unexpected actions occur.

---

## 6. Day-One Baseline Declaration & Justification

### Mandatory Day-One Stance: **Level 1 (Assistive / Read-Only Only)**

### Engineering Justification:
1. **New Team Members (Knowledge Asymmetry):**  
   Two of the six on-call engineers are completely new to the Invoice Approval codebase. If the agent operates above Level 1, new engineers are at high risk of developing **automation bias**—uncritically accepting AI conclusions during high-stress 2:00 AM outages rather than verifying the underlying telemetry.
2. **Financial Stakes (Tier-2 Production & PayEx):**  
   Unlike the previous prototype, the Tier-2 service disburses real company funds. A hallucinated recommendation (such as resending a timed-out batch without checking PayEx's settlement ledger) carries severe financial duplication liability.
3. **Auditability for SOC 2 Type II:**  
   With the audit 6 months away, auditors require clear proof of human oversight over financial infrastructure. Starting at Level 1 ensures an unbroken, verifiable paper trail of human decision-making on every operational incident.


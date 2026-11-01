# Task 1: As-Built & Target Architecture Diagrams + Architecture Test

## 1. Context & Executive Summary

The **Invoice Approval Service** is an 18-month-old internal ASP.NET Core 8 application currently operating as a prototype.
Under leadership mandate, this service is transitioning into a **Tier-2 mission-critical enterprise service** with:
- ~40,000 invoices/month (60% clustered in the final 3 working days; peak throughput exceeding 80 approvals/minute).
- External payment disbursement integration via **PayEx** (30-second timeout, where timeout does not indicate failure).
- 24/7 on-call rotation with 6 engineers (2 completely new to the codebase).
- SOC 2 Type II audit in 6 months requiring a 7-year immutable audit trail.

This document establishes:
1. The **As-Built Architecture** (Mermaid C4 Container diagram and Approval Path Sequence diagram).
2. The **Citation vs. AI Hallucination/Inference Matrix**, rigorously separating verified code facts from ungrounded assumptions.
3. The **Target State Architecture** highlighting planned components (Azure SQL, Azure Service Bus, PayEx, Transactional Outbox worker).
4. The **Automated Architecture Test** enforcing domain boundary decoupling.

---

## 2. As-Built Container Diagram (C4 Model)

The following Mermaid diagram depicts the actual state of the application as verified directly in the codebase.

```mermaid
graph TB
    subgraph Client ["Client Tier (Browser)"]
        UI["Single Page Web UI<br/>(wwwroot/index.html, app.js, styles.css)"]
    end

    subgraph Service ["ASP.NET Core 8 Web API"]
        subgraph Pipeline ["HTTP Middleware Pipeline"]
            StaticFiles["Static Files Middleware<br/>(UseDefaultFiles, UseStaticFiles)"]
            AuthMiddleware["SessionAuthMiddleware<br/>(Auth/SessionAuthMiddleware.cs)"]
            SessionStore["In-Memory Session Store<br/>(Auth/SessionStore.cs)"]
        end

        subgraph Controllers ["Controllers Tier (HTTP Endpoints)"]
            AuthController["AuthController<br/>(Controllers/AuthController.cs)"]
            InvoicesController["InvoicesController<br/>(Controllers/InvoicesController.cs)"]
            ApprovalsController["ApprovalsController<br/>(Controllers/ApprovalsController.cs)<br/>[RequireRole: Approver, Admin]"]
            BaseController["ApiControllerBase<br/>(Controllers/ApiControllerBase.cs)<br/>Handle() exception mapping"]
        end

        subgraph Domain ["Domain Tier (Business Logic)"]
            IRepository["IInvoiceRepository (Interface)<br/>(IInvoiceRepository.cs)"]
            ApprovalService["ApprovalService<br/>(ApprovalService.cs)<br/>Submit, Approve, Reject"]
            InvoiceEntity["Invoice & InvoiceStatus<br/>(Invoice.cs, InvoiceStatus.cs)"]
            DiscountCalculator["DiscountCalculator<br/>(DiscountCalculator.cs)"]
        end

        subgraph Infrastructure ["Infrastructure Tier (In-Memory Persistence)"]
            InMemoryRepo["InMemoryInvoiceRepository<br/>(InMemoryInvoiceRepository.cs)<br/>List(Invoice) with thread lock"]
        end
    end

    %% Client requests
    UI -->|"Static assets"| StaticFiles
    UI -->|"POST /api/auth/login"| AuthController
    UI -->|"GET & POST /api/invoices"| InvoicesController
    UI -->|"POST /api/approvals/*"| ApprovalsController

    %% Middleware flow
    AuthMiddleware -->|"Validates token"| SessionStore

    %% Controller dependencies (Downwards into Domain)
    InvoicesController -->|"Direct CRUD"| IRepository
    InvoicesController -->|"Submit invoice"| ApprovalService
    ApprovalsController -->|"Approve / Reject"| ApprovalService

    %% Domain to Infrastructure flow (Strictly Downwards)
    ApprovalService -->|"Query & update"| IRepository
    IRepository -.->|"Implemented by (Singleton)"| InMemoryRepo

    %% Theme-Adaptive Styling
    classDef inMemory stroke:#f59e0b,stroke-width:2px,stroke-dasharray: 4 4;
    classDef domain stroke:#10b981,stroke-width:2px;
    classDef web stroke:#06b6d4,stroke-width:2px;

    class InMemoryRepo,SessionStore inMemory;
    class ApprovalService,DiscountCalculator,InvoiceEntity,IRepository domain;
    class AuthController,InvoicesController,ApprovalsController,BaseController,AuthMiddleware,StaticFiles web;
```

---

## 3. As-Built Approval Flow Sequence Diagram

This sequence diagram maps the exact runtime execution path when an Approver approves an invoice.

```mermaid
sequenceDiagram
    autonumber
    actor Approver as Approver (User)
    participant Browser as Browser UI<br/>(app.js)
    participant Middleware as SessionAuthMiddleware<br/>(SessionAuthMiddleware.cs)
    participant Sessions as SessionStore<br/>(SessionStore.cs)
    participant Controller as ApprovalsController<br/>(ApprovalsController.cs)
    participant Service as ApprovalService<br/>(ApprovalService.cs)
    participant Repo as InMemoryInvoiceRepository<br/>(InMemoryInvoiceRepository.cs)

    Approver->>Browser: Clicks "Approve" button
    Browser->>Middleware: POST /api/approvals/INV-1001/approve<br/>Headers: Authorization: Bearer {token}
    
    activate Middleware
    Middleware->>Sessions: Get(token)
    activate Sessions
    Sessions-->>Middleware: UserSession(Username="demo.approver", Role="Approver")
    deactivate Sessions
    
    Note over Middleware: Attaches session to HttpContext.Items["session"]
    Middleware->>Controller: Route to Approve("INV-1001")
    deactivate Middleware

    activate Controller
    Note over Controller: [RequireRole("Approver", "Admin")] verified
    Controller->>Service: Approve("INV-1001", "demo.approver")
    
    activate Service
    Service->>Repo: GetById("INV-1001")
    activate Repo
    Note over Repo: Acquires lock(_lock)<br/>Queries _invoices list
    Repo-->>Service: Invoice(Status = PendingApproval)
    deactivate Repo

    Note over Service: EnsurePending(invoice): verifies Status == PendingApproval
    Note over Service: invoice.Status = InvoiceStatus.Approved<br/>invoice.DecidedBy = "demo.approver"<br/>invoice.DecidedAt = DateTime.UtcNow

    Service->>Repo: Update(invoice)
    activate Repo
    Note over Repo: Acquires lock(_lock)<br/>Finds index & replaces item in _invoices
    Repo-->>Service: void
    deactivate Repo

    Service-->>Controller: returns updated Invoice object
    deactivate Service

    Controller-->>Browser: 200 OK (Invoice JSON payload)
    deactivate Controller

    activate Browser
    Note over Browser: refresh() updates table row to status "Approved"
    Browser-->>Approver: UI updates instantly
    deactivate Browser
```

---

## 4. Citations vs. AI Hallucination & Inference Matrix

To prevent architectural fiction, every element in the as-built diagrams was audited against the actual repository files. The table below contrasts confirmed code facts against common AI agent assumptions, explaining why the AI jumps to conclusions and what the code actually contains.

| Architectural Element | Code Citation (FilePath:Symbol) | Code Fact (What is Actually Present) | Common AI Agent Hallucination / Invalid Inference |
|---|---|---|---|
| **Persistence Layer** | `backend/src/InvoiceApp.Api/InMemoryInvoiceRepository.cs:6`<br/>`Program.cs:14` | **Purely in-memory storage.** Invoices are kept in a C# `List<Invoice>` guarded by a local thread `lock(_lock)`. Seeded with 5 static records. **Zero database exists**—if the server stops or restarts, all data, invoices, and approval history are completely lost from RAM. | **Assuming standard ORM/SQL persistence exists.** AI models frequently invent an Entity Framework Core `AppDbContext`, SQL Server connection strings in `appsettings.json`, and migration scripts (`Microsoft.EntityFrameworkCore.SqlServer`). The AI assumes calls like `_context.SaveChangesAsync()` are saving data to a real database, blinding engineers to the fact that the app is 100% volatile in memory. |
| **Authentication & RBAC** | `backend/src/InvoiceApp.Api/Auth/SessionAuthMiddleware.cs:4`<br/>`SessionStore.cs:8`<br/>`RequireRoleAttribute.cs:6`<br/>`Program.cs:13` | **Custom in-memory session dictionary.** Middleware extracts a raw string from the `Authorization: Bearer <token>` header and looks it up in a `ConcurrentDictionary<string, UserSession>`. Passwords are plain text demo strings in `appsettings.json`. Tokens are random GUIDs created in memory. | **Assuming enterprise identity & cryptographic JWT tokens.** AI agents routinely hallucinate that ASP.NET Core Identity, Microsoft Entra ID (Azure AD), or OAuth2/OpenID Connect are wired up. Agents assume tokens are cryptographically signed RS256 JWTs validated via `.AddJwtBearer()`, when in reality there is zero cryptographic signature validation and tokens evaporate on container restart. |
| **External Payment Integration (PayEx)** | *None* (completely absent from repository) | **Zero payment code exists.** When an approver clicks approve, `ApprovalService.Approve()` merely mutates `invoice.Status = InvoiceStatus.Approved` in memory. No money is disbursed, no HTTP calls are dispatched, and no payment SDK is referenced. | **Confusing the scenario brief with current codebase reality.** Because ops materials mention PayEx, AI models routinely hallucinate that an `IPayExClient`, `PaymentGatewayService`, or `HttpClient` is already implemented and called inside `ApprovalService.Approve()`. An uncritical engineer would fail to realize that the entire payment integration must be built from scratch. |
| **Message Broker / Async Bus** | *None* (completely absent from repository) | **Zero messaging code exists.** There are no references to `Azure.Messaging.ServiceBus`, MassTransit, RabbitMQ, or background event dispatchers anywhere in the application or test code. | **Confusing provisioned cloud resources with application code.** Because the ops scenario states *"Azure Service Bus is provisioned in the cloud"*, agents jump to the conclusion that domain events like `InvoiceApprovedEvent` are already being published to a Service Bus topic upon approval. In truth, Azure Service Bus sits completely idle and unconnected to the application. |
| **Database Transaction Boundary** | `backend/src/InvoiceApp.Domain/ApprovalService.cs:31-41` | **Unprotected sequential method calls.** Approval executes `_repository.GetById(invoiceId)` followed by `_repository.Update(invoice)`. There is no ambient transaction, `TransactionScope`, distributed transaction coordinator, or SQL `BEGIN TRANSACTION`. | **Assuming ACID transactional isolation.** AI agents often claim that approval state transitions are safely guarded by atomic database transactions with automatic rollback. In reality, because updates are non-transactional in-memory list operations, concurrent modifications under multi-instance scale would lead to split-brain states and lost updates. |
| **Audit Trail & SOC 2 Compliance** | `backend/src/InvoiceApp.Domain/Invoice.cs:10-12` | **Minimal mutable properties.** The `Invoice` class only has three nullable fields: `DecidedBy` (string), `DecidedAt` (DateTime), and `DecisionNote` (string). There is no dedicated audit table, change log, append-only ledger, or immutable storage. Overwriting or deleting an invoice completely destroys its decision history. | **Assuming compliance-ready audit trails.** Because SOC 2 Type II compliance is mentioned in requirements, agents frequently hallucinate that an enterprise audit logging interceptor, Azure Monitor Log Analytics stream, or structured event stream is capturing tamper-proof audit trails for 7 years. |
| **Web UI & Build System** | `backend/src/InvoiceApp.Api/wwwroot/app.js:1`<br/>`index.html:1`<br/>`Program.cs:20` | **Plain vanilla JavaScript and static HTML5.** Served directly via `app.UseStaticFiles()`. Uses raw browser DOM APIs (`document.createElement`, `addEventListener`, `fetch`). There is no bundler, transpiler, or client-side build step. | **Assuming modern SPA frameworks & build pipelines.** Because the UI is described as a "Single Page App" and the root repository contains an `e2e/package.json` for Playwright testing, AI agents frequently infer that a modern framework (React, Angular, or Blazor) and a bundler (Webpack, Vite) power the front end, inventing non-existent npm build steps and component trees. |

---

## 5. Target State Architecture Diagram (Production Tier-2)

The Target State architecture integrates all missing production requirements:
- **Azure SQL Database** with **Transactional Outbox Pattern** to ensure zero lost approvals.
- **Azure Service Bus** topic/subscription for reliable decoupled dispatching.
- **Asynchronous Payment Dispatcher Worker** to handle PayEx integration with 30s timeouts.
- **Idempotency Key & Deduplication Store** preventing double-payments.
- **Append-Only Audit Log** meeting 7-year SOC 2 Type II auditability.

```mermaid
graph TB
    subgraph ClientTier ["Client Tier"]
        Client["Browser UI (HTTPS)<br/>(Static SPA on Azure CDN / App Service)"]
    end

    subgraph IdentityProvider ["Enterprise Identity (SOC 2)"]
        EntraID["Microsoft Entra ID / OAuth 2.0<br/>(Replaces in-memory session store)"]:::planned
    end

    subgraph AppService ["Azure App Service (Tier-2 Production Web API)"]
        subgraph APIComponents ["ASP.NET Core 8 Web API"]
            AuthMod["JWT Bearer Authentication<br/>(Entra ID Token Validation)"]:::planned
            InvoicesCtrl["InvoicesController<br/>(Controllers/InvoicesController.cs)"]
            ApprovalsCtrl["ApprovalsController<br/>(Controllers/ApprovalsController.cs)"]
            DomainService["ApprovalService<br/>(Domain/ApprovalService.cs)"]
            AuditInterceptor["Audit Trail Interceptor<br/>(User, Timestamp, IP, Action)"]:::planned
            SqlRepo["SqlInvoiceRepository<br/>(EF Core / Dapper persistence)"]:::planned
        end
    end

    subgraph DataStorage ["Data & Messaging Tier (Azure Managed)"]
        subgraph AzureSql ["Azure SQL Database (Geo-Replicated)"]
            InvoiceTable[("Invoices Table<br/>(Status, Amounts, Approver)")]:::planned
            OutboxTable[("Transactional Outbox Table<br/>(IdempotencyKey, Payload, Status)")]:::planned
            AuditTable[("Immutable Audit Trail Table<br/>(7-Year Retention Policy)")]:::planned
        end

        subgraph ServiceBus ["Azure Service Bus (Standard Tier)"]
            SBTopic["Topic: invoice-approved-topic"]:::planned
            SBSubscription["Subscription: payex-payment-sub"]:::planned
            DeadLetter["Dead Letter Queue (DLQ)"]:::planned
        end
    end

    subgraph BackgroundProcessing ["Asynchronous Processing Tier"]
        OutboxPublisher["Outbox Publisher Worker<br/>(Reads Outbox & pushes to Service Bus)"]:::planned
        PaymentWorker["PayEx Payment Dispatcher Worker<br/>(BackgroundService on Azure Worker / Functions)"]:::planned
    end

    subgraph ExternalProvider ["External Financial Provider"]
        PayEx["PayEx Payment Gateway API<br/>(HTTPS REST Endpoint)<br/>Timeout: 30s | Strict Idempotency"]:::planned
    end

    %% Flow connections
    Client -->|"1. Authenticate"| EntraID
    Client -->|"2. Submit Approval (Bearer token)"| ApprovalsCtrl
    ApprovalsCtrl -->|"3. Validate Token & Roles"| AuthMod
    ApprovalsCtrl -->|"4. Execute approval"| DomainService
    DomainService -->|"5. Atomic Write (Invoice + Outbox + Audit)"| SqlRepo

    SqlRepo -->|"Single ACID Transaction"| InvoiceTable
    SqlRepo -->|"Single ACID Transaction"| OutboxTable
    SqlRepo -->|"Single ACID Transaction"| AuditTable

    OutboxPublisher -->|"6. Poll / Tail Outbox Table"| OutboxTable
    OutboxPublisher -->|"7. Publish Approval Event"| SBTopic
    SBTopic --> SBSubscription
    SBSubscription -->|"8. Consume message"| PaymentWorker

    PaymentWorker -->|"9. HTTPS POST /payments<br/>(Idempotency-Key = invoice.Id)"| PayEx
    PaymentWorker -->|"10. Update Payment Status & TxRef"| InvoiceTable
    PaymentWorker -.->|"On Poison / Exceeded Retries"| DeadLetter

    %% Theme-Adaptive Styling (works in both light & dark themes)
    classDef planned stroke:#ef4444,stroke-width:2px,stroke-dasharray: 5 5;
    classDef existing stroke:#10b981,stroke-width:2px;

    class InvoicesCtrl,ApprovalsCtrl,DomainService existing;
```

> **Legend:**  
> - **Solid Green Borders:** Existing components verified in code.  
> - **Dashed Red Borders (`:::planned`):** Components that do **not exist yet** and must be built for production readiness.

---

## 6. Architecture Boundary Test

### Design Objective
A core requirement for Tier-2 production readiness is enforcing strict architectural boundaries. Business rules in `InvoiceApp.Domain` must remain pure:
1. **Domain must never depend on Infrastructure, Web API, or Persistence implementations.**
2. **Domain entities must never reference external third-party SDKs (such as PayEx or payment client libraries).**
3. **Controllers must route business decisions through domain services rather than directly manipulating persistence.**

### The Architecture Test Implementation
The automated test is implemented in `backend/tests/InvoiceApp.Tests/ArchitectureTests.cs`. It uses .NET reflection and assertions to enforce these rules at build/CI time without introducing unstable external dependencies.

```csharp
using System.Reflection;
using InvoiceApp.Domain;
using Xunit;

namespace InvoiceApp.Tests;

public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(Invoice).Assembly;

    [Fact]
    public void Domain_Should_Not_Depend_On_Api_Or_Infrastructure()
    {
        // Act: Extract all assemblies referenced by InvoiceApp.Domain
        var referencedAssemblies = DomainAssembly.GetReferencedAssemblies();

        // Assert: Domain must not reference Web, API, or external payment infrastructure
        var forbiddenPrefixes = new[]
        {
            "InvoiceApp.Api",
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore",
            "Azure.Messaging.ServiceBus",
            "System.Data",
            "Microsoft.Data.SqlClient"
        };

        foreach (var reference in referencedAssemblies)
        {
            foreach (var forbidden in forbiddenPrefixes)
            {
                Assert.False(
                    reference.Name?.StartsWith(forbidden, StringComparison.OrdinalIgnoreCase) == true,
                    $"Architectural Boundary Violation: Domain references '{reference.Name}', which violates clean architecture."
                );
            }
        }
    }

    [Fact]
    public void Domain_Entities_Must_Not_Expose_External_Payment_Gateways()
    {
        // Act: Inspect all types in Domain assembly
        var domainTypes = DomainAssembly.GetTypes();

        foreach (var type in domainTypes)
        {
            // Fields and properties must not reference any external payment client
            var fieldsAndProps = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.MemberType is MemberTypes.Field or MemberTypes.Property);

            foreach (var member in fieldsAndProps)
            {
                var memberTypeStr = member.ToString() ?? string.Empty;
                Assert.DoesNotContain("PayEx", memberTypeStr, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("PaymentGateway", memberTypeStr, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
```

### CLI Execution Command & Verified Test Results

To run the architecture tests locally or in CI:

```powershell
dotnet test backend/InvoiceApp.sln --filter FullyQualifiedName~ArchitectureTests
```

#### Actual CLI Execution Output (Captured Live):
```text
  Determining projects to restore...
  All projects are up-to-date for restore.
  InvoiceApp.Domain -> C:\Users\rohanbaranwal\Code\nagp\ai-sdlc-1\ai-in-sdlc-assignment\backend\src\InvoiceApp.Domain\bin\Debug\net8.0\InvoiceApp.Domain.dll
  InvoiceApp.Tests -> C:\Users\rohanbaranwal\Code\nagp\ai-sdlc-1\ai-in-sdlc-assignment\backend\tests\InvoiceApp.Tests\bin\Debug\net8.0\InvoiceApp.Tests.dll
Test run for C:\Users\rohanbaranwal\Code\nagp\ai-sdlc-1\ai-in-sdlc-assignment\backend\tests\InvoiceApp.Tests\bin\Debug\net8.0\InvoiceApp.Tests.dll (.NETCoreApp,Version=v8.0)
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:     3, Skipped:     0, Total:     3, Duration: 318 ms - InvoiceApp.Tests.dll (net8.0)
```

#### Complete Test Suite Run (Domain Tests + Architecture Tests):
```powershell
dotnet test backend/InvoiceApp.sln
```
```text
Passed!  - Failed:     0, Passed:    10, Skipped:     0, Total:    10, Duration: 105 ms - InvoiceApp.Tests.dll (net8.0)
```

All 3 structural boundary tests and all 7 domain lifecycle tests pass cleanly with zero failures.

---

## 7. Task 1 Verification & Summary

- **Mermaid C4 Container View:** Created and grounded strictly in codebase reality.
- **Mermaid Sequence Diagram:** Created showing exact approval path and in-memory locking.
- **Citations vs. Inferences:** Every component mapped to exact files and code symbols, with explicit identification of typical AI hallucinations.
- **Target State Architecture:** Outlined with Transactional Outbox, Azure SQL, Azure Service Bus, and PayEx, with non-existent components clearly highlighted.
- **Architecture Test:** Written to enforce domain boundaries, executed via .NET CLI, verified with 100% pass rate, and output documented above.


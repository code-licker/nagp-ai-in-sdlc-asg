# Operations material

Evidence packs and artefacts for working on this service the way you would in
production. Nothing here runs as part of the application or its tests.

| Folder | What it holds |
| --- | --- |
| `incidents/INC-2041` | Approvals slow and failing. Logs, metrics and the deploy diff for a completed incident window. |
| `incidents/INC-2047` | Approved invoices not triggering notifications. A live incident, read-only output only. |
| `pipeline` | The service's full CI pipeline and per-step timings for the last 20 runs. |
| `evals` | Ten fixed review cases and a results template. |

## A note on the scenario

The invoice approval service in this repository is an internal tool. The
material here assumes the version of it that is going to production: a
24x7 on-call rotation, a database, a message topic, and an integration with
an external payment provider called PayEx. None of that is in the code yet,
which is the point — the diagrams, decisions and policies have to be made
before it is.

Names, hosts and figures are invented. No real system, customer or vendor
appears anywhere in this folder.

## No answers here

These packs deliberately do not state what the root cause is, which cases
contain defects, or which pipeline changes are safe. Working that out is the
exercise. If you are facilitating rather than participating, the answers are
in the session runbook, not in the repository.

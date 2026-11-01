# INC-2041 — Approvals slow and failing

Evidence pack for the invoice approval service, 14 January 2026.

## What you have

| File | What it is |
| --- | --- |
| `alert.md` | The alert that fired, and what the on-call engineer knew at the time |
| `app-logs.txt` | Application log export, 09:40Z to 11:10Z, levels INFO / WARN / ERROR |
| `metrics.csv` | Per-minute platform metrics across the same window |
| `deploy-diff.patch` | The diff for the release that went out that morning |

## How to use it

This pack is for analysis, not for fixing. Work out what happened and what you
would check next. The evidence is complete enough to support a ranked set of
hypotheses and to rule some out.

Nothing in these files states a conclusion. If you think you have found one,
the test is whether you can point at the lines that support it, and name what
you would look at to prove yourself wrong.

## Context you would have had on the day

- Release 2.14.0 was deployed that morning. The previous release, 2.13.4, had
  been running for nine days without incident.
- 14 January is the second-to-last working day of the month, which is the
  heaviest approval window of the cycle.
- PayEx is an external payment provider. The invoice approval service calls it
  over HTTPS. PayEx reported no incident on their status page that day.
- The service runs on three instances behind a load balancer.

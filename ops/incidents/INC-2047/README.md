# INC-2047 — Approved invoices are not triggering notifications

Evidence pack for the invoice approval service, 29 January 2026. Live incident:
treat this as in progress, not as a post-mortem.

## What you have

| File | What it is |
| --- | --- |
| `alert.md` | The alert, and the first report from finance |
| `publisher-logs.txt` | Log export from the approval service, publisher component |
| `worker-logs.txt` | Log export from the notification worker |
| `servicebus-status.txt` | Output of the read-only queue inspection commands |
| `recent-changes.md` | Releases and configuration changes in the last 14 days |

## How to use it

Everything here is read-only output. Nothing in this pack changes production,
and nothing in it should be treated as a fix.

The exercise is to investigate with read-only evidence, then put two or three
mitigation options in front of a human: what each one changes, what it costs,
and how to undo it. The decision is not yours to take alone, and neither is
the action.

## Standing constraints

- Any change to production needs a named approver. Out of hours, that is the
  incident commander.
- The service is in the month-end window. A rollback affects everything else
  in the release, not only the notification path.
- Notifications that were missed still matter to finance. Restoring the flow
  going forward is not the same as recovering what was lost.

# Alert — INC-2047

**Fired:** 2026-01-29 14:06Z
**Severity:** 2
**Rule:** `notification-worker / processed-rate` — fewer than 1 message processed in 30 minutes during business hours
**Current value at fire time:** 0

## Paged

s.brennan (primary on-call)

## First report

Finance raised it before the alert fired. At 13:48Z, in the team channel:

> "Has something changed with approval emails? I approved about forty invoices
> this morning and none of the suppliers have had the confirmation. The
> invoices themselves look approved in the app."

## What is known

- The invoices are approved. Status in the app is correct, and the approval
  audit trail is complete.
- No error is visible to the user. The approval request returns 200.
- The notification worker is running. It has not crashed or restarted.
- The last notification anyone can confirm arriving was on 27 January.

## Not yet done

- Nothing has been changed, restarted or rolled back.
- Finance has not been told how many notifications are affected, because
  nobody has counted yet.

# Alert — INC-2041

**Fired:** 2026-01-14 10:12Z
**Severity:** 2
**Rule:** `invoice-approval / http-5xx-rate` — 5xx rate above 2% of requests for 5 consecutive minutes
**Current value at fire time:** 4.1%

## Paged

d.whitfield (primary on-call)

## What the on-call engineer knew at 10:12Z

- Finance had already messaged the team channel at 10:09Z: "approvals are
  taking forever and some are erroring, is something going on?"
- Release 2.14.0 went out at 09:58Z.
- No change to infrastructure was scheduled for that morning.
- PayEx status page: all systems operational.

## What was tried before the pack was exported

- Restarted instance web-02 at 10:41Z. The error rate dropped for roughly
  three minutes, then returned.
- Checked the PayEx status page a second time at 10:44Z. Still green.

## Not yet done

- No rollback. The release is still live as of the export at 11:14Z.
- No change to instance count.

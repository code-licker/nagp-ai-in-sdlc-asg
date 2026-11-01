# Changes in the last 14 days

## Releases

| Date | Release | Commit | Summary |
| --- | --- | --- | --- |
| 2026-01-16 | 2.14.3 | `3f82ad1` | Retry policy for the PayEx client, pooled handler |
| 2026-01-21 | 2.15.0 | `a31c88f` | Supplier contact fields on the invoice record |
| 2026-01-28 | 2.15.1 | `e4b7d02` | Event schema alignment, audit export fields, dependency bumps |

### 2.15.1 in detail

Three changes shipped together.

1. **Event schema alignment.** Event type names moved to the dotted lowercase
   convention used by the other services in the estate, so that the central
   audit archive can parse every service's events with one schema. Reviewed
   and approved by two engineers.
2. **Audit export fields.** Two fields added to the nightly export.
3. **Dependency bumps.** Four patch-level package upgrades.

The release notes for 2.15.1 record the first item as:

> Event type names now follow `<entity>.<action>` in lower case, matching the
> platform convention. Consumers filtering on the old PascalCase names must
> update their subscriptions.

## Infrastructure and configuration

| Date | Change | By |
| --- | --- | --- |
| 2026-01-19 | Scaled the notification worker from 1 instance to 2 | platform |
| 2026-01-23 | Rotated the PayEx API key | s.brennan |
| 2026-01-26 | Raised the topic message time-to-live from 7 days to 14 | platform |

No change was made to any subscription or subscription rule in this window.

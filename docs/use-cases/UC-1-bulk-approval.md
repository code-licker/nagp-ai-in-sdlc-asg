# UC-1: Bulk invoice approval

Used in: Spec Kit demo (Demo 8) and BMAD demo (Demo 10).

## Background

Finance operations approvers open every pending invoice one at a time. For a school district with 40 invoices in a batch, that is 40 separate approvals.

## The idea

Approvers should be able to select several pending invoices and approve them together.

## What the business asked for

- Select multiple pending invoices from one screen.
- See a summary before confirming: number of invoices, total amount, customers involved.
- Exclude individual invoices from the batch before confirming.
- Keep an audit trail: who approved what, and when, for every invoice in the batch.
- Invoices over $1,000 need a second approver before they are final.

## Out of scope for the first release

- Email notifications (in-app only for now).

## Open questions (good prompts for clarifying questions)

- What happens to the rest of the batch if one invoice fails?
- Can the same person be both approvers on a large invoice?
- Is there a limit on batch size?

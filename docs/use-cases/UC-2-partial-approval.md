# UC-2: Partial approval

Used in: OpenSpec demo (Demo 9).

## Background

Today an invoice is approved or rejected as a whole. Two school districts asked for a way to approve less than the full amount when part of an invoice is disputed.

## The change

An approver can approve an invoice for an amount lower than the invoiced amount, with a required note explaining why.

## What the business asked for

- The approver enters the approved amount, which must be greater than zero and less than the invoiced amount.
- Both the invoiced amount and the approved amount are kept on the invoice.
- The invoice shows as partially approved, separate from fully approved.
- A note is required whenever the approved amount is lower than the invoiced amount.
- Existing approve and reject behaviour stays exactly as it is.

## Where it touches the code

The existing approval flow: `ApprovalService`, `ApprovalsController`, and the Approvals tab in the web UI.

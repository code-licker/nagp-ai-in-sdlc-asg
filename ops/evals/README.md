# Review eval

Ten fixed cases for measuring how well a review prompt performs on this
codebase. Each case is a diff in `cases/`. Some contain a defect and some do
not; which is which is not recorded here.

## Why run it

A review that finds something on one pull request has told you nothing about
how it behaves in general. This set gives you three numbers you can compare
over time, across models and across prompt versions:

- **Caught** — the case has a defect and the review named it.
- **Missed** — the case has a defect and the review did not.
- **False alarm** — the case is clean and the review reported a defect anyway.

The false alarm rate is the one people forget to measure, and it is the one
that decides whether anyone keeps using the reviewer.

## How to run it

1. Start a fresh session for each case. Context from a previous case will
   change the answer.
2. Give the agent the case diff and the review prompt below. Nothing else.
3. Record what it reported in `results.csv`, using `results-template.csv` as
   the starting point.
4. Repeat the whole set three times. Output varies between runs, so a single
   pass gives you an anecdote rather than a rate.
5. Change exactly one thing — the prompt, the model, or the context file —
   and run the set again.

## The review prompt

> Review this diff for defects. For each finding give the file, the line, what
> goes wrong, and how confident you are. If you find nothing, say so
> explicitly rather than listing general observations.

## Scoring

Score one case per row. A finding counts as caught only if it names the actual
defect; a vague note that something "could be improved" near the right line
does not count, and neither does a correct finding reached for the wrong
reason. Be strict about this, because a generous score here is a score you
cannot act on later.

For the clean cases, style suggestions are not false alarms. A false alarm is
the review asserting a defect that is not there.

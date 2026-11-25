# Features

Lazy-loaded feature areas, one folder per user type, each with its own `*.routes.ts`:

- `claimant/` (`/my-claims`): list own claims, report an incident, track a claim, answer questions, see the decision (UC-01 to UC-04).
- `staff/` (`/work`): the queue (needs me, waiting on claimant, unassigned; oldest in status first), pick up, ask
  the claimant, settle or reject (UC-05 to UC-07). `/work/team`: claimed exposure and team workload (UC-08, D7;
  RFC-0003).

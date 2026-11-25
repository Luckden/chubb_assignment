# claims-web

The Angular 21 SPA for the claims platform: one app for claimants and claims staff. For how to run the whole stack
and what the app does, see the [root README](../README.md). Coding conventions are in [AGENTS.md](AGENTS.md) and the
frontend section of the [root AGENTS.md](../AGENTS.md).

## Commands

| Task                                                                        | Command                                                  |
| --------------------------------------------------------------------------- | -------------------------------------------------------- |
| Dev server (http://localhost:4200, proxies `/api` to http://localhost:5080) | `npm start`                                              |
| Unit tests (Vitest)                                                         | `npm test`                                               |
| End-to-end tests (Playwright, API mocked; starts `ng serve` itself)         | `npm run e2e`                                            |
| End-to-end against the real stack, including `@stack` specs                 | `E2E_BASE_URL=http://localhost:4200 npm run e2e`         |
| Lint / format                                                               | `npm run lint` / `npm run format` (`format:check` in CI) |
| Regenerate API types after a backend change                                 | `npm run api:generate` (after `dotnet build backend`)    |
| Production build                                                            | `npm run build`                                          |

The first time, run `npx playwright install chromium`. Playwright reuses any server already on port 4200, so rebuild
or stop the Docker `web` container before running `npm run e2e`.

## Stack

Angular Material 3 for components (ADR-0005), Tailwind CSS 4 for layout only, TanStack Query for server state, and
types generated from the backend's OpenAPI document. Open queries poll every 10 seconds (ADR-0017).

## Structure

```
src/app/
  core/
    api/          generated/schema.ts (never edit), api-types.ts aliases, one *-api.service.ts and
                  *.queries.ts per API area (claims, notifications, workload, exposure), problem.ts, polling.ts
    query/        TanStack Query client defaults
    session/      CurrentUser: the "Acting as" demo user (no authentication, ADR-0009)
  features/
    claimant/     /my-claims: list, report an incident, claim detail with answer and decision (UC-01 to UC-04)
    staff/        /work: queue tabs, claim detail with pick up, ask, settle or reject (UC-05 to UC-07);
                  /work/team: claimed exposure and team workload (UC-08, D7)
  shared/         claim building blocks (status chip, facts, activity, table), notifications bell, query state
  testing/        test fixtures and TestBed setup for pages that use queries
e2e/              Playwright specs (*.e2e.ts); API mocked with page.route unless tagged @stack
```

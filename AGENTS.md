# Agent instructions (all AI tools)

Claims platform take-home: Angular SPA + .NET API + PostgreSQL + Kafka. Backend first; frontend if time permits.

## Layout

- `backend/` — .NET 10 solution `ClaimsPlatform.slnx`
  - `src/Claims.Api/` — ASP.NET Core API (`Controllers/`, `Application/`, `Domain/`, `Infrastructure/{Persistence,Messaging}`)
  - `tests/Claims.Api.Tests/` — xUnit + `WebApplicationFactory`
- `frontend/` — Angular 21 workspace. **Read `frontend/AGENTS.md` before touching frontend code.**
- `docs/` — RFCs, ADRs, architecture docs, AI journal. See `docs/README.md`.
- `docker-compose.yml` — postgres, kafka (KRaft), kafka-ui, api, web.

## Commands

| Task | Command |
| --- | --- |
| Infra only | `docker compose up -d postgres kafka kafka-ui` |
| Full stack | `docker compose up --build` |
| API | `dotnet run --project backend/src/Claims.Api` (http://localhost:5080) |
| API tests | `dotnet test backend` (needs Docker: Testcontainers). Blocked on the host? `scripts/test-backend-in-docker.sh` |
| Regenerate API contract + types | `dotnet build backend`, then `cd frontend && npm run api:generate` |
| SPA lint / format | `cd frontend && npm run lint` / `npm run format` |
| SPA | `cd frontend && npm start` (http://localhost:4200, proxies `/api`) |
| SPA tests | `cd frontend && npm test` |
| E2E (API mocked) | `cd frontend && npm run e2e` (starts `ng serve` itself) |
| E2E (full stack) | `docker compose up -d`, then `E2E_BASE_URL=http://localhost:4200 npm run e2e` in `frontend/` |
| EF migration | `cd backend && dotnet tool restore && dotnet ef migrations add <Name> --project src/Claims.Api --output-dir Infrastructure/Persistence/Migrations` |

## Conventions

- **Backend:** C# 14, nullable on, primary constructors, `async` all the way, `ProblemDetails` for errors.
  Controllers stay thin; logic lives in `Application/`; entities and rules live in `Domain/` with no framework dependencies.
  Warnings are errors (`backend/Directory.Build.props`), so fix them; don't suppress them without a comment explaining why.
- **Validation:** one FluentValidation `AbstractValidator<T>` per request DTO (auto-registered). Return validation
  failures as `ValidationProblemDetails` (400).
- **Persistence:** entity configuration in `IEntityTypeConfiguration<T>` classes (picked up automatically). Every schema
  change needs a migration. Demo data goes in an idempotent `IDataSeeder` registered in `Program.cs`.
- **API contract:** the backend owns it. Controllers return typed `ActionResult<T>` / `[ProducesResponseType]` so
  the OpenAPI schema is complete. After API changes, commit the regenerated `backend/openapi/claims-api.json` and
  `frontend/src/app/core/api/generated/schema.ts`; CI fails if they are stale. Never hand-write DTO interfaces in the
  frontend: alias the generated types in `core/api/api-types.ts` and call them with `HttpClient`.
- **Messaging:** publish through `IEventPublisher`; topic names live in `Infrastructure/Messaging/Topics.cs`.
  Events are past-tense facts (`ClaimSubmitted`), keyed by aggregate id.
- **Frontend:** follow `frontend/AGENTS.md` (standalone, signals, OnPush, `inject()`, native control flow).
- **Tests:** add or adjust tests with every behaviour change.
  - Backend tests use the shared `ClaimsApiFactory` (`[Collection(ApiCollection.Name)]`), which runs the real API
    against Testcontainers Postgres and Kafka. Don't mock infrastructure.
  - E2E specs live in `frontend/e2e/*.e2e.ts`. Use role-based locators (`getByRole`), and mock the API with
    `page.route` by default. Tag a spec `@stack` only if it needs the real backend.
- **Before finishing a change:** backend builds and tests pass; in the frontend, `npm run lint`, `npm run format:check`,
  `npm test` and `npm run e2e` pass.
- **Scope:** don't add features beyond what is asked; scope decisions belong to the human (`docs/scope-and-prioritisation.md`).
- **Docs:** a significant design decision needs an ADR (`docs/adr/`); a cross-cutting proposal starts as an RFC
  (`docs/rfcs/`). Use the templates and update the index tables.
- **No** Chubb branding, proprietary material or public hosting.

## AI journal

The brief requires `docs/ai-journal.md`. When you finish a meaningful chunk of work, propose a journal entry
(asked / accepted / challenged / overrode / why). Do not invent the human's opinions; leave `My notes` for them.

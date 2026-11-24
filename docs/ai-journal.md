# AI working journal

A running log of AI-assisted work: what I asked, what I accepted, what I challenged and what I overrode, with
brief reasoning. Newest entries at the bottom. Not polished on purpose.

**Tools used:** Claude Code (Claude Opus 5.5) in VS Code; Angular CLI MCP server.

## Entry template

```
### YYYY-MM-DD HH:MM: <topic>
- **Asked:**
- **Accepted:**
- **Challenged / overrode:**
- **Why:**
```

---

### 2026-11-24: Project scaffolding

- **Asked:** Set up an empty repo skeleton: .NET LTS Web API, Angular (latest LTS), Node, Kafka, PostgreSQL,
  Docker Compose, docs with an RFC + ADR flow, and AI instruction files / MCP config. No domain code yet.
- **Accepted:** .NET 10 + Angular 21 scaffold, Compose stack (Postgres 18, Kafka 4.1 KRaft, Kafka UI), docs structure.
- **Challenged / overrode (by the AI, to review):**
  - `dotnet add package` picked Npgsql EF Core 11, which targets .NET 11 only, so it was pinned to the 10.x line.
  - The template's `Microsoft.OpenApi` 2.0.0 has a high-severity advisory, so it was bumped to 2.13.1.
  - The Angular CLI's generated MCP config runs unpinned `@angular/cli` (v22, needs Node ≥ 24.15), so it was pinned to `@angular/cli@21`.
- **Why:** keep everything on LTS lines, and get a clean, warning-free restore.
- **My notes:** _TODO: add your own review comments here._

### 2026-11-24: Playwright

- **Asked:** Include Playwright.
- **Accepted:**
  - Playwright end-to-end tests in `frontend/e2e`. By default they mock the API with `page.route`. Specs tagged
    `@stack` run against the real Docker Compose stack when `E2E_BASE_URL` is set.
  - The Playwright MCP server registered next to the Angular CLI one.
- **Challenged / overrode (by the AI, to review):** named the spec files `*.e2e.ts`, not `*.spec.ts`, so the
  Vitest unit runner doesn't pick them up.
- **Why:** mocked end-to-end tests are fast and don't depend on the backend. One `@stack` smoke test proves the wiring end to end.
- **My notes:** _TODO_

### 2026-11-25: Guardrails for AI-assisted coding

- **Asked:** Gap analysis of docs and tech stack against the brief. Then: commit the scaffold and add the first group
  of gaps. Purpose: set the stack up for better AI-assisted coding later.
- **Accepted:**
  - Warnings as errors and analyzers (`Directory.Build.props`).
  - FluentValidation registration.
  - Migrations and seeders on startup.
  - Testcontainers integration tests (Postgres and Kafka round trip).
  - OpenAPI exported on build and turned into frontend types.
  - angular-eslint and Prettier.
  - GitHub Actions CI.
  - Drafts: scope doc, RFC-0001 and RFC-0002 (questions only), ADR-0003.
- **Challenged / overrode (by the AI, to review):**
  - Removed `openapi-fetch` right after adding it: it bypasses Angular `HttpClient`. Kept types-only generation.
  - The OpenAPI doc had no response schema because the endpoint returned an anonymous object. Changed it to a typed record.
  - The API Docker build broke because the Dockerfile didn't copy `Directory.Build.props`. Caught by rebuilding the image, then fixed.
  - `ng new` on Windows wrote CRLF files, so Prettier failed. Added `.gitattributes` with `eol=lf` and reformatted.
  - The RFCs were left as open questions on purpose. The domain and REST/Kafka decisions are mine to make.
- **Deferred:** login and roles, real-time push (SignalR), outbox, file uploads, UI component library, observability.
- **My notes:** _TODO_

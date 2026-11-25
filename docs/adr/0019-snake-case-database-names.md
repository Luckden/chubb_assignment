# ADR-0019: Database tables and columns use snake_case

- **Status:** Accepted
- **Date:** 2026-11-25
- **Deciders:** Project owner
- **Related:** [ADR-0007](0007-application-services-and-ef-core-conventions.md), [ADR-0016](0016-rest-for-commands-kafka-for-lifecycle-events-via-outbox.md)

## Context

Tables were already snake_case (`claims`, `outbox_messages`), but EF Core's default left columns in PascalCase
(`ClaimNumber`, `PublishedAt`). In PostgreSQL that means quoting every column in hand-written SQL, and it breaks the
usual convention.

## Decision

Use the `EFCore.NamingConventions` package (`UseSnakeCaseNamingConvention()`, version 10.0.1, the newest build that
supports .NET 10) so every column, key and index name is snake_case. A migration (`UseSnakeCaseColumnNames`) renames the
existing objects. The concurrency token keeps PostgreSQL's system column `xmin`. Raw SQL (for example the outbox
index filter `published_at IS NULL`) uses the snake_case names.

## Consequences

- Hand-written SQL and database tools need no quoting.
- One extra package. A renamed property needs no manual column mapping.
- The convention also renames EF's own `__EFMigrationsHistory` columns, so a database created before it could not be
  read by EF at all (found by running `docker compose up` on an existing volume). `DatabaseInitializer` renames those two
  columns once before migrating. It runs only where `Database:MigrateOnStartup` is on; a database migrated any other
  way needs the same two `ALTER TABLE ... RENAME COLUMN` statements by hand.

## Alternatives considered

- **Keep PascalCase:** no change, but quoted identifiers everywhere.
- **`HasColumnName` on every property:** no package, but easy to forget on new properties.

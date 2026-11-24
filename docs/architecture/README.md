# Architecture

Living documentation of the system *as it currently is*. Update it when an ADR is accepted.

## Sections (TODO)

- [ ] **System context**: actors (claimant, claims officer, manager) and external systems
- [ ] **Containers**: Angular SPA, API service(s), PostgreSQL, Kafka
- [ ] **Domain model**: core entities and relationships
- [ ] **Claim lifecycle**: states and transitions
- [ ] **Communication**: which interactions are REST and which are Kafka events, with topic and event catalogue
- [ ] **Frontend structure**: routes, feature areas, state management

## Current state

```
 Browser ──► Angular SPA (frontend/, :4200)
                │  /api (dev proxy or nginx)
                ▼
          Claims.Api (backend/, :5080) ──► PostgreSQL (:5432)
                │
                └──► Kafka (:9092)  ◄── Kafka UI (:8081)
```

Only plumbing exists: health checks, a placeholder `/api/system/info` endpoint, an empty `DbContext`, and a Kafka
publisher plus consumer skeleton.

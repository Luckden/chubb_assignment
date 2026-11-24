# ADR-0004: Frontend UI and server-state libraries

- **Status:** Proposed
- **Date:** 2026-11-25
- **Deciders:** Project owner
- **Related:** [ADR-0002](0002-baseline-technology-stack.md), [ADR-0003](0003-engineering-guardrails-for-ai-assisted-development.md)

## Context

The Angular scaffold had no styling system, component library or server-state handling. The owner is new to
Angular but familiar with the React ecosystem (Tailwind/daisyUI, TanStack Query). Time is capped, and most UI
code will be AI-generated, so good AI support for the chosen libraries matters.

Angular already includes routing, HTTP (`HttpClient`), forms, DI and signals, so no libraries are needed for those.

## Decision

| Concern | Choice |
| --- | --- |
| Styling | Tailwind CSS 4.3 via `@tailwindcss/postcss`; project switched from SCSS to CSS |
| Components (look) | daisyUI 5.8 (CSS only), light and dark themes |
| Components (behaviour) | `@angular/cdk` 21.2 (overlays, dialogs, a11y, tables) |
| Server state | `@tanstack/angular-query` 5.108 (the stable successor to `angular-query-experimental`) |
| Icons | `lucide-angular` |
| AI support | Official daisyUI skill (`.claude/skills/daisyui/SKILL.md`) + free daisyUI GitMCP server |

## Consequences

- Familiar mental model for the owner, so less time learning Angular-specific UI libraries.
- daisyUI is class-based, so templates stay plain HTML that AI tools generate reliably. Interactive accessibility
  (focus trapping, keyboard navigation) comes from the CDK and has to be added deliberately.
- TanStack Query provides caching, refetching and invalidation without hand-written stores. The cost is one
  more concept, and a test gotcha: in-flight queries count as pending tasks for `whenStable()`.
- No Sass: any component styles are plain CSS, ideally none at all (utilities in templates).

## Alternatives considered

- **Angular Material 21**: the standard library with accessible, feature-rich components (data table, date picker),
  but opinionated Material styling and a theming learning curve.
- **PrimeNG / Spartan**: large component sets, but less familiar to the owner and with weaker AI tooling than daisyUI.
- **Angular `httpResource()` / `resource()`**: built in and lighter, but no shared cache or invalidation across components.
- **NgRx Store**: too much ceremony for this scope. Signals in services plus TanStack Query cover local and server state.
- **daisyUI Blueprint MCP**: official and recommended by daisyUI, but needs a paid licence.

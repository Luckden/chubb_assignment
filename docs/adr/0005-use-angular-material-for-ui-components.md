# ADR-0005: Use Angular Material for UI components

- **Status:** Proposed
- **Date:** 2026-11-25
- **Deciders:** Project owner
- **Related:** Supersedes [ADR-0004](0004-frontend-ui-and-server-state-libraries.md) (UI part only)

## Context

ADR-0004 chose daisyUI (CSS-only components) because it was familiar from React. The claims staff views need
accessible, data-heavy components: tables with sorting and paging, form fields, date pickers, dialogs. daisyUI
provides only the styling for these, so the behaviour would have to be built with the CDK.

## Decision

- **Components:** Angular Material 21.2 (Material 3), replacing daisyUI.
- **Theme:** `src/material-theme.scss` using `mat.theme`, with an azure primary palette, light and dark modes that follow
  the OS, and Roboto typography.
- **Layout:** Tailwind CSS 4 stays for layout and spacing utilities only. Material's `--mat-sys-*` tokens are mapped
  to Tailwind colours in `src/styles.css`, so any colour utility follows the Material theme.
- **Icons:** `mat-icon` with Material Symbols, replacing lucide-angular.
- **Server state is unchanged:** TanStack Query (ADR-0004) still applies.
- **AI support:** the daisyUI skill and MCP server are removed. Material docs are reachable through the Angular CLI MCP server.

## Consequences

- Accessible, keyboard-friendly components out of the box (`mat-table` with `MatSort`/`MatPaginator`,
  `mat-datepicker`, `mat-dialog`), and they follow Angular conventions, so AI-generated code is idiomatic.
- One Sass file is needed for theming. Everything else stays plain CSS.
- Material's look is opinionated. Customising beyond the theme means using `mat.<component>-overrides`.
- Roboto and Material Symbols load from Google Fonts, so offline development shows fallback fonts.

## Alternatives considered

- **Keep daisyUI**: familiar, but table, form and dialog behaviour would have to be built by hand.
- **Material without Tailwind**: Material has no layout utilities, so layout would be custom CSS in every component.

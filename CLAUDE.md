@AGENTS.md

## Claude Code specifics

- Slash commands in `.claude/commands/`: `/new-rfc`, `/new-adr`, `/journal`.
- MCP (`.mcp.json`):
  - `angular-cli` (`ng mcp`): Angular docs, best practices and examples.
  - `playwright` (`@playwright/mcp`): drive a browser to check UI changes against the running app.
- Windows: run frontend tests from a shell whose path uses an uppercase drive letter (`C:\…`); Vitest fails with
  "failed to find the runner" when the cwd is `c:\…`.

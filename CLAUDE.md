@AGENTS.md

## Claude Code specifics

- Slash commands in `.claude/commands/`: `/new-rfc`, `/new-adr`, `/journal`.
- MCP (`.mcp.json`):
  - `angular-cli` (`ng mcp`): Angular docs, best practices and examples.
  - `playwright` (`@playwright/mcp`): drive a browser to check UI changes against the running app.
  - `daisyui` (GitMCP, free): daisyUI docs lookup. Blueprint, the official server, needs a paid licence, so it's not used.
- Skills (`.claude/skills/`): `daisyui`, the official daisyUI 5 skill from https://daisyui.com/SKILL.md. Use it
  for all template and styling work. To update it, re-download that URL.
- Windows: run frontend tests from a shell whose path uses an uppercase drive letter (`C:\…`); Vitest fails with
  "failed to find the runner" when the cwd is `c:\…`.

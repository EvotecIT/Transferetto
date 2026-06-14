# AGENTS.md

## Build And Publish Safety

Agents may run `Build\Build-Module.ps1` for local validation when publishing is not configured or is explicitly disabled. Prefer local proof with publishing off before changing consumer wrappers or shared tooling.

For Transferetto module builds:

- Use `$env:SignModule = 'false'` when local signing is not part of the task.
- Do not publish to PSGallery, GitHub releases, private feeds, or other external destinations unless the user explicitly asks for a publish.
- Before treating a build script as a publish path, inspect it for enabled `New-ConfigurationPublish` entries or equivalent publish commands.
- If a build failure points at shared packaging behavior, inspect sibling `PSPublishModule` / PowerForge under the Evotec repo root before adding Transferetto-local workaround logic.
- It is acceptable to rebuild sibling `PSPublishModule` for proof as long as publish actions are off.

## Evotec Repo Root

Treat the EvotecIT repository root as configurable. Use `$env:EVOTEC_GITHUB_ROOT` when it is set; otherwise use `C:\Support\GitHub` on Windows and `~/Support/GitHub` on macOS/Linux.

## Pull Request Follow-Through

For any GitHub PR work in this repo, use `github-pr-review-followthrough` when available. After each Codex-authored push, check PR state, checks, and unresolved review threads before declaring the PR clean.

## Shared Ownership

Keep Transferetto wrappers thin. Build, package, publish, installer, artifact, and reusable workflow behavior should live in PSPublishModule / PowerForge unless it is genuinely Transferetto-specific configuration.

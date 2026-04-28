# Prompt Engineering Assistant — Phase 2 Implementation Plan

## Context

The user wants a multi-platform native desktop application (macOS, Windows, Linux) that interviews the user and converts raw requests into Anthropic's canonical 10-part prompt template ("Prompt structure": Task context, Tone context, Background data, Detailed task description & rules, Examples, Conversation history, Immediate task, Step-by-step toggle, Output formatting, Prefilled response).

The app does **not** call any web API. Instead, it shells out to the user's locally-installed AI CLIs — Gemini CLI, Codex CLI, Claude Code CLI — via `System.Diagnostics.Process`, sending the assembled prompt to one CLI per session and showing the response.

**Why this shape:** The user is iterating on prompt quality and wants a reproducible, offline-capable tool that exercises whichever CLI they have configured, with hardcoded restrictive safety flags so the CLI never edits files. The 10-part template is a known-good prompt-engineering pattern; the app's value is making it fast to fill in correctly.

**Working directory state:** `/Users/luis/dev/prompt-generator` is empty at plan time. This is a clean greenfield bootstrap.

---

## Locked Decisions

| # | Decision |
|---|----------|
| Framework | Avalonia UI + CommunityToolkit.Mvvm |
| Provider model | **Single provider per session** (UI picker — no fan-out) |
| Streaming | **Non-streaming**: `IAiCliProvider` returns `Task<CliResult>` |
| Persistence | **SQLite** via `Microsoft.Data.Sqlite` (raw ADO.NET, no EF Core) |
| Generic Math C# feature | **Dropped** — no natural fit, won't be shoehorned |
| Provider safety flags | **Hardcoded** restrictive defaults: Claude `--bare`, Codex `--sandbox read-only --ask-for-approval never`, Gemini `--output-format json` |
| Output parsing | Gemini → JSON envelope, extract `response`; Claude → `--output-format json`, extract `result`; Codex → `-o <tempfile>`, read file after exit |
| Error handling | Non-zero exit = failure; stderr surfaced verbatim; Gemini & Claude JSON `error` shape checked even on exit 0; Codex tempfile absence/empty treated as failure |

---

## Modern C# Features — Mapping

The 11 retained features map to specific files:

| Feature | Lands in |
|---|---|
| Top-Level Statements | `src/PromptAssistant.Desktop/Program.cs` |
| File-Scoped Namespaces | every `.cs` file |
| Global Using Directives | `GlobalUsings.cs` per project |
| Raw String Literals | renderer XML wrapping; argument templates |
| Required Members | `PromptTemplate` record |
| Primary Constructors | all providers + `ProcessRunner` |
| Collection Expressions | provider registration; `ArgumentList` builders |
| List Patterns | parsing CLI `--version` output |
| `ref readonly` Parameters | `PromptRenderer.Render(ref readonly PromptTemplate)` |
| Enhanced `params` Collections | `params ReadOnlySpan<string>` in arg builders |
| `Lock` type | `ProcessRunner` lifecycle guard |

---

## Solution Layout

```
prompt-generator/
├── docs/
│   └── architecture-plan.md
├── PromptAssistant.sln
├── Directory.Build.props
├── src/
│   ├── PromptAssistant.Core/           ← pure domain, no I/O
│   ├── PromptAssistant.Cli/            ← Process interop only
│   ├── PromptAssistant.Persistence/    ← SQLite repository
│   ├── PromptAssistant.App/            ← Avalonia (cross-platform shared)
│   └── PromptAssistant.Desktop/        ← entry point
└── tests/
    ├── PromptAssistant.Core.Tests/
    └── PromptAssistant.Cli.Tests/
```

---

## Implementation Slices (sequential)

### Slice 0 — Repo init & plan copy
1. `git init`.
2. Create `docs/architecture-plan.md` (this file).
3. Add `.gitignore` for .NET.

### Slice 1 — Solution scaffold
1. `dotnet new sln -n PromptAssistant`.
2. Create 5 src projects + 2 test projects.
3. Wire project references.
4. Add `GlobalUsings.cs` per project.
5. `Directory.Build.props` with `LangVersion=preview`, `Nullable=enable`, `TreatWarningsAsErrors=true`, `TargetFramework=net9.0`.
6. **Verify:** `dotnet build` zero warnings.

### Slice 2 — Domain core (Core project)
1. `SectionKind` enum (10 values).
2. `PromptSection` discriminated shape: `FreeTextSection`, `ListSection`, `ToggleSection`.
3. `PromptTemplate` record with `required` members.
4. `IPromptRenderer` interface — `string Render(ref readonly PromptTemplate template)`.
5. `AnthropicPromptRenderer` skeleton with `// TODO(user)` for the optional-section omission policy. **🎓 User contribution point #1**.
6. xUnit golden-file tests for full and minimum templates.
7. **Verify:** full-template test passes; minimum-template test fails with TODO message until user fills it in.

### Slice 3 — CLI interop (Cli project)
1. `CliResult` record: `{ string Text, int ExitCode, string? StdErr, TimeSpan Duration }`.
2. `IAiCliProvider` interface.
3. `ProcessRunner` (internal, primary constructor, `Lock`-guarded, `ArgumentList`, async drain, `Kill(entireProcessTree: true)` cancellation).
4. `ProviderDefaults` — hardcoded base argument lists per provider. **🎓 User contribution point #2**.
5. Three providers:
   - `GeminiCliProvider` → JSON envelope, parse `response`.
   - `ClaudeCodeCliProvider` → `--output-format json`, parse `result`.
   - `CodexCliProvider` → `-o <tempfile>`, read file after exit, `try/finally` cleanup.
6. `CliDiscovery.FindAsync` — user-configured path → PATH → platform-correct extension.
7. Scripted fake CLIs in `tests/PromptAssistant.Cli.Tests/Fakes/`.
8. **Verify:** all provider tests green using fakes; cancellation kills child within 1s.

### Slice 4 — Persistence
1. `AppDb` connection factory (`Microsoft.Data.Sqlite`, DB under `ApplicationData/PromptAssistant/app.db`).
2. `Migrations/001_initial.sql` — `Templates`, `Runs` tables.
3. Schema bootstrap on first connection.
4. `PromptHistoryRepository`.
5. **Verify:** repository tests against `:memory:` SQLite.

### Slice 5 — Avalonia UI
1. `Program.cs` top-level statements, classic desktop lifetime.
2. `App.axaml` Fluent theme.
3. Hand-rolled DI container.
4. `MainWindowViewModel`, `InterviewViewModel`, `ReviewViewModel`.
5. Three section VMs (`FreeText`, `List`, `Toggle`) inheriting `SectionViewModelBase`.
6. Three section views (compiled bindings, `x:DataType`).
7. Use C# 13 `Lock` in `ReviewViewModel` for concurrent `SendAsync` guard.
8. **Verify:** `dotnet run` opens window; full interview → Send → response in pane.

### Slice 6 — Polish (only if scope/time permit)
- CLI availability indicators in toolbar.
- Save/Load template from history.
- Export rendered prompt.

---

## User Contribution Points (Learning Mode)

1. **`AnthropicPromptRenderer.Render`** — optional-section omission policy. ~10 lines.
2. **`ProviderDefaults`** — exact flag values per provider with security rationale. ~10 lines.

---

## Verification — End-to-End

1. `dotnet build` zero warnings.
2. `dotnet test` all green.
3. `dotnet run --project src/PromptAssistant.Desktop` opens window.
4. Pick installed provider → walk 10 sections → Send → response in pane.
5. History sidebar shows prior run after restart.
6. `sqlite3 ~/Library/Application Support/PromptAssistant/app.db` shows `Templates` and `Runs` rows.

---

## Out of Scope (explicitly)

- Web/API calls.
- Multi-provider fan-out / comparison view.
- Streaming token rendering.
- Authentication / sync.
- Generic Math C# feature.
- WASM/browser host.
- Argv-too-long fallback to stdin piping (Windows ~32 KB cap; v2 candidate).

## Future Enhancement Hooks (post-v1)

Architecture should not preclude:

- **Stateful sessions:** Claude `--continue` / `--resume`, Codex `resume --last`.
- **Structured output:** Claude `--json-schema`.
- **System prompt overlay:** Claude `--append-system-prompt`.
- **Streaming responses:** layer behind `IStreamingAiCliProvider : IAiCliProvider`.

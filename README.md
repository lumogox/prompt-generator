# Prompt Assistant

A native desktop app for crafting high-quality AI prompts using Anthropic's canonical 10-part prompt structure. Fill in the sections that matter, generate with your locally-installed AI CLI, and iterate — all without leaving your machine.

<!-- Screenshot -->
<!-- ↓ Replace this comment with your app screenshot ↓ -->

---

## What it does

Prompt Assistant interviews you through Anthropic's 10-section prompt template and assembles a well-structured prompt for you:

| # | Section | Purpose |
|---|---|---|
| 1 | Task context | Who the model is and why it exists |
| 2 | Tone & style | How it should communicate |
| 3 | Background data | Reference material to ground the response |
| 4 | Task rules | Constraints and guardrails |
| 5 | Examples | Few-shot demonstrations |
| 6 | Conversation history | Prior turns, if any |
| 7 | Immediate task | The actual request |
| 8 | Step-by-step | Chain-of-thought toggle |
| 9 | Output formatting | Structure, length, format |
| 10 | Assistant prefill | Partial response to continue from |

You choose which sections the AI generates for you, refine with preset lenses (Tighten, More formal, More technical, etc.), or build from a raw idea. The assembled prompt is sent to whichever CLI you have installed.

---

## Supported AI CLIs

The app shells out to locally-installed CLIs — no API keys, no network calls from the app itself.

| CLI | Install |
|---|---|
| **Claude Code** | `npm install -g @anthropic-ai/claude-code` |
| **Gemini CLI** | `npm install -g @google/gemini-cli` |
| **Codex CLI** | `npm install -g @openai/codex` |

Any combination works. The app detects which ones are on your PATH and lets you pick per session.

---

## Installation

Download the latest release for your platform from the [Releases](../../releases) page.

| Platform | File | How to install |
|---|---|---|
| **macOS** (Apple Silicon) | `PromptAssistant-*-osx-arm64.dmg` | Open DMG → drag **Prompt Assistant.app** to Applications |
| **Windows** (x64) | `PromptAssistant-*-win-x64.zip` | Extract → run `PromptAssistant.Desktop.exe` |
| **Linux** (x64) | `PromptAssistant-*-linux-x64.tar.gz` | `tar -xzf <file>` → run `./PromptAssistant.Desktop` |

> **macOS first launch:** right-click the app → Open (unsigned bundle — one-time Gatekeeper bypass).  
> **Windows first launch:** SmartScreen may warn — click **More info** → **Run anyway**.  
> **Linux prerequisites:** `libX11`, `libGL`, and `libfontconfig` must be present (pre-installed on all standard desktop distros).

---

## Building from source

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

```bash
git clone https://github.com/lumogox/prompt-generator.git
cd prompt-generator
dotnet build
dotnet run --project src/PromptAssistant.Desktop
```

### Build a distributable

```bash
# macOS (.dmg)
./scripts/publish-mac.sh

# macOS Intel
RUNTIME=osx-x64 ./scripts/publish-mac.sh

# Windows (.zip) — requires PowerShell 7+
pwsh scripts/publish-windows.ps1

# Linux (.tar.gz)
./scripts/publish-linux.sh

# Custom version
VERSION=1.2.0 ./scripts/publish-mac.sh
```

Output lands in `artifacts/`.

---

## Releasing a new version

1. Bump `<Version>` in [`Directory.Build.props`](Directory.Build.props).
2. Open a PR — write a meaningful description (it becomes the changelog).
3. Merge to `main`.

The [release workflow](.github/workflows/release.yml) picks up the version change, builds for all three platforms in parallel, and publishes a GitHub Release with the artifacts attached automatically.

Pre-releases: any version containing a hyphen (e.g. `1.1.0-beta.1`) is published as a GitHub pre-release and does not replace **latest**.

---

## Project structure

```
src/
  PromptAssistant.Core/        # Domain models, prompt renderer — no I/O
  PromptAssistant.Cli/         # Process interop with AI CLIs
  PromptAssistant.Persistence/ # SQLite history (Microsoft.Data.Sqlite)
  PromptAssistant.App/         # Avalonia UI — views, view models, styles
  PromptAssistant.Desktop/     # Entry point (top-level statements)
scripts/
  publish-mac.sh               # → artifacts/dmg/*.dmg
  publish-windows.ps1          # → artifacts/zip/*.zip
  publish-linux.sh             # → artifacts/tar/*.tar.gz
```

Built with [Avalonia UI](https://avaloniaui.net) · [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) · .NET 10

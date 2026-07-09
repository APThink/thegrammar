# TheGrammar

TheGrammar is a Windows tray app that fixes grammar wherever you're typing. Select some text, copy it, hit a hotkey, and the corrected version is instantly written back to your clipboard — ready to paste.

## How it works

1. Copy some text to the clipboard.
2. Press a configured hotkey (default: `Ctrl+Shift+Y`).
3. The text is sent to OpenAI for grammatical correction.
4. The corrected text replaces the original text on the clipboard, and the tray icon animates while the request is in flight.

## Features

- **Global hotkeys** — trigger corrections from any application, no window focus needed.
- **Multiple prompt bindings** — define several prompts, each with its own hotkey combination, from the Prompts page.
- **Model selection** — switch between OpenAI models (GPT-4o, GPT-5 and variants, GPT-4 Turbo, ...) from Settings.
- **Secure API key storage** — your OpenAI API key is stored in the Windows Credential Manager, not on disk in plain text.
- **Dashboard** — see the current model, request count, API key status, auto-start status, and your last 10 corrections.
- **Auto-start with Windows**, optional sound on request start, and other small quality-of-life settings.

## Requirements

- Windows
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (for building/running from source)
- An [OpenAI API key](https://platform.openai.com/api-keys)

## Getting started

### Install a release

Download the latest build (plain zip or Squirrel installer) from the [Releases](../../releases) page and run it.

### Run from source

```bash
dotnet restore TheGrammar.sln
dotnet run --project src/TheGrammar
```

On first launch, open the tray icon's window, go to **Settings**, and enter your OpenAI API key. Then set up your hotkey bindings on the **Prompts** page.

## Tech stack

- WinForms host with a Blazor Hybrid UI ([MudBlazor](https://mudblazor.com/))
- EF Core with SQLite for local storage of prompts, models, and request history
- Serilog for logging
- [Clowd.Squirrel](https://github.com/clowd/Clowd.Squirrel) for packaging/updates

## Tests

```bash
dotnet test TheGrammar.sln
```
# TLG Ollama English Helper

A Telegram bot that analyzes English words and phrases using Ollama LLMs, providing phonological breakdowns, word explanations, and text-to-speech (TTS) via Kokoro Speech.

## Overview

This bot runs as a .NET 10 console application and connects to a local Ollama instance to perform linguistic analysis. It supports a range of English-capable models and integrates with Kokoro Speech for high-quality TTS. Commands are exposed via Telegram: `/analyze`, `/explain`, `/spell`, and `/help`.

## Features

- **Phonological analysis** (`/analyze`): British IPA transcription, syllable breakdown, phoneme analysis, stress patterns, and prosody guidance.
- **Word explanation** (`/explain`): Synonyms, meaning, and usage examples for non-native speakers.
- **Text-to-speech** (`/spell`): Generates voice messages using Kokoro Speech.
- **Help command** (`/help`): Displays available commands.
- **Streaming responses**: Tokens are streamed in real time, with usage metrics reported.
- **Self-contained deployment**: Builds to a single executable with no external runtime dependencies.

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Telegram Bot Service                         │
│  ┌─────────────────────────────────────────────────────────────────┐│
│  │  Receives updates, routes to TlgCommandHandlerService            ││
│  └─────────────────────────────────────────────────────────────────┘│
│                                                                       │
│  ┌─────────────────────────────────────────────────────────────────┐│
│  │  TlgCommandHandlerService                                        ││
│  │  ┌─────────────────┐  ┌───────────────────┐  ┌───────────────┐ ││
│  │  │ /analyze →      │  │ /explain →        │  │ /spell →      │ ││
│  │  │ Phonological    │  │ WordExplainer     │  │ KokoroSpeech   │ ││
│  │  │ Service         │  │ Service           │  │ Client         │ ││
│  │  └─────────────────┘  └───────────────────┘  └───────────────┘ ││
│  └─────────────────────────────────────────────────────────────────┘│
│                                                                       │
│  ┌─────────────────────────────────────────────────────────────────┐│
│  │  PhonologicalService → Ollama "mo-shakib/clearwriter:latest"    ││
│  │  WordExplainerService → Ollama "mo-shakib/clearwriter:latest"    ││
│  │  KokoroSpeechClient → Kokoro Speech API (optional)               ││
│  └─────────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────────┘
```

## Supported Models

### Ollama

The bot uses the `mo-shakib/clearwriter:latest` model for both phonological analysis and word explanation. This model is tuned for clear, structured JSON responses and works well for non-native speaker explanations.

### Kokoro Speech

TTS is performed via the Kokoro Speech API (`http://localhost:8880/v1/audio/speech`). The default voice is `bf_lily` with the `kokoro` model, producing high-quality English audio.

## Setup

### Prerequisites

- [.NET 10 runtime](https://dotnet.microsoft.com/download) (or SDK for development)
- Ollama running locally with at least one English-capable model (see below)
- Telegram Bot Token from [@BotFather](https://t.me/BotFather)
- Kokoro Speech API running locally (optional; bot will still work without it)

### Running Ollama

Start Ollama and pull an English-capable model:

```bash
ollama serve
ollama pull mo-shakib/clearwriter
```

Verify the model is available:

```bash
curl -s http://localhost:11434/api/tags | jq -r '.models[] | select(.model | test("^en|^llama|^mistral|^gemma|^qwen|^deepseek|^phi|^nemotron|^llama|^gpt4o|^dolphin|^hermes|^nova|^magma")) | .model'
```

### Running Kokoro Speech (optional)

If you want TTS, run Kokoro Speech locally:

```bash
kokoro-speech serve
```

Verify the API is responding:

```bash
curl -s http://localhost:8880/v1/models | jq -r '.models[] | select(.model | test("^en|^llama|^mistral|^gemma|^qwen|^deepseek|^phi|^nemotron|^llama|^gpt4o|^dolphin|^hermes|^nova|^magma")) | .model'
```

If Kokoro Speech isn't running, the bot will skip TTS and only provide `/analyze` and `/explain`.

### Configuration

Create `appsettings.json` in the project root:

```json
{
  "App": {
    "OllamaApi": "http://localhost:11434",
    "TlgBotToken": "YOUR_TELEGRAM_BOT_TOKEN",
    "DefaultVoice": "en_US-lessac-medium",
    "KokoroSpeechApi": "http://localhost:8880"
  }
}
```

- `OllamaApi`: URL of your Ollama instance.
- `TlgBotToken`: Telegram bot token from @BotFather.
- `DefaultVoice`: Default voice for TTS (used by Kokoro Speech).
- `KokoroSpeechApi`: URL of the Kokoro Speech API (omit or set to empty if not using TTS).

### Building and Running

```bash
dotnet build
dotnet run
```

The bot will start long-polling for Telegram updates. Press Ctrl+C to stop.

## Usage

### Commands

- `/analyze [text]` — Phonological breakdown of a single word.
- `/explain [text]` — Explanation with synonyms, meaning, and example.
- `/spell [text]` — Text-to-speech voice message.
- `/help` — List of available commands.

If a message doesn't start with `/`, the bot treats it as a `/spell` request.

### Examples

**Phonological analysis**

```
/analyze apple
```

Response:

```
Phonological Breakdown

[æpəl]

Syllables: ap·ple (2)
Phonemes: /æ/ /p/ /ə/ /l/ /ɚ/ /ə/
Primary stress: first syllable (ap-)
Secondary stresses: none
Speed/Rhythm: steady, even pace
Tone: neutral, informative
Pacing: even, measured
```

**Word explanation**

```
/explain apple
```

Response:

```
Term: apple
Synonyms: fruit, pomaceous fruit, pome
Meaning: a round, red or green fruit with crisp flesh and a core containing seeds.
Example: "I ate a crisp green apple for a snack."
```

**Text-to-speech**

```
/spell hello world
```

The bot sends a voice message with the audio of "hello world".

## Configuration Reference

| Setting | Default | Description |
|---------|---------|-------------|
| `App.OllamaApi` | `http://localhost:11434` | Ollama API endpoint. |
| `App.TlgBotToken` | — | Telegram bot token (required). |
| `App.DefaultVoice` | `en_US-lessac-medium` | Default voice for TTS. |
| `App.KokoroSpeechApi` | `http://localhost:8880` | Kokoro Speech API endpoint. |

## Troubleshooting

- **Ollama not responding**: Ensure Ollama is running and the model is loaded. Check with `curl http://localhost:11434/api/tags`.
- **Kokoro Speech not responding**: If TTS is not working, verify the Kokoro Speech API is running and the model is loaded.
- **Bot not receiving messages**: Confirm the bot token is correct and you've added the bot to the chat.
- **Too long text**: `/analyze` accepts up to 50 characters; `/explain` up to 100; `/spell` up to 300.

## License

MIT License.

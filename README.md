
# MCP Server (Model Contextual Protocol)

## Features
- Asynchronous FastAPI web server for robust communication
- Real-time WebSocket endpoint for Unity integration
- Session context management: goals, history, scripts, logs, plans
- LLM (Gemini API) integration, multimodal prompt support
- Strict JSON-based command/response protocol
- Robust error handling (API, network, file I/O)
- Modular, extensible architecture
- Environment configuration via `.env`
- Unit tests for WebSocket and command processing

## Folder Structure
- `src/` — Python source code
  - `core/` — Server logic, agent management
  - `communication/` — Protocols, models, logging
- `Assets/` — Unity assets (prefabs, scenes, scripts)
- `Packages/` — Unity package manifests and protocol specs
- `requirements.txt` — Python dependencies
- `LICENSE` — Apache 2.0 License




## How to Run the MCP Server and Connect Unity (Local User-Hosted)

### 1. Start the MCP Server Locally
**Prerequisites:** Install Docker and Docker Compose (or Python 3.10+ if running without Docker).

**Start the server:**
```sh
docker-compose up -d
```
This will build and run the MCP server on your own PC, locally on port 8000, with the default token `mcp-local-user`. No central server or remote hosting is required.

Alternatively, you can run the server directly:
```sh
python src/core/server.py
```

### 2. Connect the Unity Client
**Steps:**
1. Open your Unity project.
2. Add `MCPClient.cs` to `Assets/Scripts` (if not already present).
3. In Unity, go to `Window > MCP Client` to open the MCP Client window.
4. The client will automatically connect to `ws://localhost:8000` using the default token.
5. If you want to use a custom token, click `Set Token` in the MCP Client window and select your token file.
6. When connected, both server and client will display "Connection Established".



### 3. Usage & Advanced Features

- **Send commands from the server to Unity and receive responses in real time.**
- **All file operations are sandboxed to the Unity project directory for safety.**

#### Advanced Features

1. **Interactive Visual Feedback 🎨**
   - In the Unity MCP Client window, click "Take Screenshot for Annotation" to capture the game view.
   - Draw directly on the screenshot (e.g., circle UI issues) using your mouse.
   - Click "Send Annotated Screenshot to Agent" to send the image and your prompt to the server for multimodal analysis and fixes.

2. **Agent "Personalities" or Modes 🤖**
   - In the MCP Client window, select the agent mode:
     - *Cautious Mode*: Agent asks for confirmation before major actions (e.g., file writes).
     - *Autonomous Mode*: Agent executes plans without interruption unless errors occur.
     - *Teaching Mode*: Agent explains its reasoning and code before acting.
   - The selected mode is sent to the server and affects agent behavior.

3. **Performance & Caching Layer**
   - The server caches file contents in memory and only reloads files if modified, speeding up repeated analysis and commands for large projects.

---

The system is robust and feature-complete for its core purpose. These advanced options further enhance usability, security, and performance for power users.

## Security
- All file operations are sandboxed to the Unity project directory.
- Default token is only used for local authentication; no external API keys required.
- For advanced users, you can change the port or token in `docker-compose.yml` and Unity settings.

## Communication Protocol
See `message_types.json` and `Packages/protocol.json` for full specs.

## License
Licensed under the Apache License, Version 2.0. See LICENSE for details.

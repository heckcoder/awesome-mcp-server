
from fastapi import FastAPI, WebSocket, WebSocketDisconnect, Request, status
from fastapi.responses import JSONResponse
import asyncio
import json
import os
import logging
from pathlib import Path
import time

app = FastAPI()
connections = {}

# Setup centralized logging
logging.basicConfig(filename="server.log", level=logging.INFO,
                    format="%(asctime)s %(levelname)s %(message)s")

# Health check endpoint
@app.get("/health")
async def health():
    return {"status": "ok"}

# Token-based authentication for WebSocket
API_TOKEN = os.getenv("API_TOKEN", "changeme")

# Define project sandbox directory
PROJECT_ROOT = Path(os.getenv("PROJECT_ROOT", os.getcwd())).resolve()

# File cache: {path: (content, last_modified)}
file_cache = {}

def is_safe_path(base: Path, target: Path) -> bool:
    try:
        return base in target.resolve().parents or base == target.resolve()
    except Exception:
        return False

def sanitize_path(path: str) -> Path:
    p = (PROJECT_ROOT / path).resolve()
    if not is_safe_path(PROJECT_ROOT, p):
        raise ValueError("Unsafe path detected")
    return p

def read_file_with_cache(path: Path):
    stat = path.stat()
    last_modified = stat.st_mtime
    cache_entry = file_cache.get(str(path))
    if cache_entry and cache_entry[1] == last_modified:
        return cache_entry[0]
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()
    file_cache[str(path)] = (content, last_modified)
    return content

@app.websocket("/ws/{session_id}")
async def websocket_endpoint(websocket: WebSocket, session_id: str):
    # Expect token as query param
    token = websocket.query_params.get("token")
    if token != API_TOKEN:
        await websocket.close(code=status.WS_1008_POLICY_VIOLATION)
        logging.warning(f"Unauthorized connection attempt for session {session_id}")
        return
    await websocket.accept()
    connections[session_id] = websocket
    logging.info(f"Connection established for session {session_id}")
    try:
        while True:
            data = await websocket.receive_text()
            asyncio.create_task(process_message(session_id, data))
    except WebSocketDisconnect:
        del connections[session_id]
        logging.info(f"Connection closed for session {session_id}")

async def process_message(session_id: str, message: str):
    try:
        msg = json.loads(message)
        # Agent mode handling
        agent_mode = msg.get("agent_mode", "Cautious")
        command = msg.get("command")
        payload = msg.get("payload", {})
        # Example: file read/write commands with sandboxing and caching
        if command == "read_file":
            path = sanitize_path(payload.get("path", ""))
            try:
                content = read_file_with_cache(path)
                response = {"response_to_id": msg.get("command_id"), "status": "success", "payload": {"content": content}}
            except Exception as e:
                response = {"response_to_id": msg.get("command_id"), "status": "error", "payload": {"message": str(e)}}
        elif command == "write_file":
            path = sanitize_path(payload.get("path", ""))
            content = payload.get("content", "")
            # Cautious mode: ask for confirmation (simulate)
            if agent_mode == "Cautious":
                # In real implementation, send confirmation request to client
                response = {"response_to_id": msg.get("command_id"), "status": "confirm", "payload": {"message": "Confirm write to file?"}}
            else:
                try:
                    with open(path, "w", encoding="utf-8") as f:
                        f.write(content)
                    # Update cache
                    file_cache[str(path)] = (content, time.time())
                    response = {"response_to_id": msg.get("command_id"), "status": "success", "payload": {"message": "File written."}}
                except Exception as e:
                    response = {"response_to_id": msg.get("command_id"), "status": "error", "payload": {"message": str(e)}}
        elif "image" in msg:
            # Multimodal: annotated screenshot received
            # Here, you would process the image and prompt with LLM
            response = {"response_to_id": msg.get("command_id", ""), "status": "success", "payload": {"message": "Annotated screenshot received. Processing..."}}
        else:
            response = {"response_to_id": msg.get("command_id"), "status": "error", "payload": {"message": "Unknown command."}}
        await connections[session_id].send_text(json.dumps(response))
        logging.info(f"Processed command {command} for session {session_id} in mode {agent_mode}")
    except Exception as e:
        logging.error(f"Error processing message for session {session_id}: {e}")
# MCP Server Source

This folder contains the Python source code for the Model Contextual Protocol (MCP) server.


## Production Readiness Upgrades
- `/health` endpoint for monitoring
- Token-based authentication for WebSocket (`API_TOKEN` in `.env`)
- Input sanitization to prevent path traversal and malicious commands
- Sandboxing: restrict file operations to the Unity project directory (`PROJECT_ROOT`)
- Asynchronous task management for long-running operations
- Centralized logging to `server.log`
- Dockerfile for containerized deployment
- Basic unit test for health endpoint
- GitHub Actions CI/CD workflow for automated testing

See the main README.md for setup, usage, and security instructions.

Licensed under the Apache License 2.0.

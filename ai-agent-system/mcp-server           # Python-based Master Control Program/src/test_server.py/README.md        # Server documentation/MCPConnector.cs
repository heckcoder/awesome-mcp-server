# File: /ai-agent-system/mcp-server/src/tests/test_server.py

import unittest
from fastapi.testclient import TestClient
from core.server import app

class TestWebSocketConnections(unittest.TestCase):
    def setUp(self):
        self.client = TestClient(app)

    def test_websocket_connection(self):
        with self.client.websocket_connect("/ws/test_session") as websocket:
            self.assertIsNotNone(websocket)

    def test_command_processing(self):
        with self.client.websocket_connect("/ws/test_session") as websocket:
            websocket.send_json({"command": "test_command"})
            response = websocket.receive_json()
            self.assertEqual(response["status"], "success")

if __name__ == "__main__":
    unittest.main()
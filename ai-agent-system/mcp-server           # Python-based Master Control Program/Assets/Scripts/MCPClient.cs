using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using NativeWebSocket;
using System.Threading.Tasks;

public class MCPClient : EditorWindow
{
    private WebSocket webSocket;
    private string serverUrl = "ws://localhost:8000/ws/unity_session";
    private string token = "mcp-local-user";
    private string status = "Disconnected";

    // Agent mode selection
    private enum AgentMode { Cautious, Autonomous, Teaching }
    private AgentMode agentMode = AgentMode.Cautious;

    // Screenshot annotation
    private Texture2D screenshot;
    private bool isAnnotating = false;
    private Color annotationColor = Color.red;
    private Vector2 lastDrawPos;

    [MenuItem("Window/MCP Client")]
    public static void ShowWindow()
    {
        GetWindow<MCPClient>("MCP Client");
    }

    private void OnEnable()
    {
        // Load token from EditorPrefs if set, else use default
        string userToken = EditorPrefs.GetString("MCPServerToken", "");
        if (!string.IsNullOrEmpty(userToken))
            token = userToken;
        ConnectToServer();
    }

    private async void ConnectToServer()
    {
        string urlWithToken = serverUrl + "?token=" + token;
        webSocket = new WebSocket(urlWithToken);
        webSocket.OnOpen += () =>
        {
            status = "Connection Established";
            Repaint();
        };
        webSocket.OnError += (e) =>
        {
            status = "Error: " + e;
            Repaint();
        };
        webSocket.OnClose += (e) =>
        {
            status = "Disconnected";
            Repaint();
        };
        webSocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            OnMessageReceived(message);
        };
        await webSocket.Connect();
    }

    private void OnGUI()
    {
        GUILayout.Label("MCP Client Status: " + status);
        GUILayout.Label("Token: " + token);
        if (GUILayout.Button("Set Token"))
        {
            string newToken = EditorUtility.OpenFilePanel("Select Token File", "", "txt");
            if (!string.IsNullOrEmpty(newToken))
            {
                token = File.ReadAllText(newToken).Trim();
                EditorPrefs.SetString("MCPServerToken", token);
                status = "Token Set. Restart window to connect.";
            }
        }

        // Agent mode selection
        GUILayout.Label("Agent Mode:");
        agentMode = (AgentMode)EditorGUILayout.EnumPopup(agentMode);

        // Screenshot annotation UI
        if (GUILayout.Button("Take Screenshot for Annotation"))
        {
            TakeScreenshot();
            isAnnotating = true;
        }
        if (screenshot != null)
        {
            GUILayout.Label("Annotate Screenshot (draw with mouse)");
            Rect imageRect = GUILayoutUtility.GetRect(screenshot.width, screenshot.height);
            GUI.DrawTexture(imageRect, screenshot);
            HandleAnnotation(imageRect);
            if (GUILayout.Button("Send Annotated Screenshot to Agent"))
            {
                SendAnnotatedScreenshot();
                isAnnotating = false;
                screenshot = null;
            }
        }
    }

    private void TakeScreenshot()
    {
        int width = Screen.width;
        int height = Screen.height;
        screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();
    }

    private void HandleAnnotation(Rect imageRect)
    {
        if (isAnnotating && Event.current.type == EventType.MouseDrag && imageRect.Contains(Event.current.mousePosition))
        {
            Vector2 mousePos = Event.current.mousePosition - new Vector2(imageRect.x, imageRect.y);
            DrawOnTexture(screenshot, (int)mousePos.x, (int)mousePos.y, annotationColor);
            lastDrawPos = mousePos;
            Repaint();
        }
    }

    private void DrawOnTexture(Texture2D tex, int x, int y, Color color)
    {
        int size = 5;
        for (int dx = -size; dx <= size; dx++)
        {
            for (int dy = -size; dy <= size; dy++)
            {
                int px = Mathf.Clamp(x + dx, 0, tex.width - 1);
                int py = Mathf.Clamp(y + dy, 0, tex.height - 1);
                tex.SetPixel(px, py, color);
            }
        }
        tex.Apply();
    }

    private void SendAnnotatedScreenshot()
    {
        byte[] imageBytes = screenshot.EncodeToPNG();
        string base64Image = Convert.ToBase64String(imageBytes);
        string prompt = EditorUtility.DisplayDialogComplex("Prompt", "Describe the issue for the agent:", "OK", "Cancel", "");
        var msg = new AnnotatedScreenshotMessage
        {
            image = base64Image,
            prompt = prompt.ToString(),
            agent_mode = agentMode.ToString()
        };
        string json = JsonUtility.ToJson(msg);
        webSocket.SendText(json);
    }

    [Serializable]
    public class AnnotatedScreenshotMessage
    {
        public string image;
        public string prompt;
        public string agent_mode;
    }

    private void OnMessageReceived(string jsonMessage)
    {
        // Validate and execute commands securely
        try
        {
            var msg = JsonUtility.FromJson<ServerMessage>(jsonMessage);
            if (msg.command == "read_file" || msg.command == "write_file")
            {
                string path = msg.payload.path;
                if (!IsSafePath(path))
                {
                    SendResponse(msg.command_id, "error", new { message = "Unsafe path." });
                    return;
                }
            }
            // ...existing command execution logic...
        }
        catch (Exception ex)
        {
            Debug.LogError("Error handling message: " + ex.Message);
        }
    }

    private bool IsSafePath(string path)
    {
        string assetsPath = Application.dataPath.Replace("\\", "/");
        string fullPath = Path.GetFullPath(Path.Combine(assetsPath, path)).Replace("\\", "/");
        return fullPath.StartsWith(assetsPath) && !path.Contains("..") && !Path.IsPathRooted(path);
    }

    private void SendResponse(string responseToId, string status, object payload)
    {
        var response = new ClientResponse
        {
            response_to_id = responseToId,
            status = status,
            payload = payload
        };
        string json = JsonUtility.ToJson(response);
        webSocket.SendText(json);
    }

    [Serializable]
    public class ServerMessage
    {
        public string command_id;
        public string command;
        public Payload payload;
    }
    [Serializable]
    public class Payload
    {
        public string path;
        public string content;
    }
    [Serializable]
    public class ClientResponse
    {
        public string response_to_id;
        public string status;
        public object payload;
    }
}

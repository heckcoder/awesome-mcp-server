using System;
using UnityEngine;

public class MessageHandler : MonoBehaviour
{
    private void Start()
    {
        // Initialize message handling
    }

    public void HandleMessage(string jsonMessage)
    {
        // Parse the incoming JSON message and execute corresponding commands
        try
        {
            // Deserialize the JSON message
            var message = JsonUtility.FromJson<Message>(jsonMessage);
            ExecuteCommand(message);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error handling message: {ex.Message}");
        }
    }

    private void ExecuteCommand(Message message)
    {
        // Implement command execution logic based on the message type
        switch (message.type)
        {
            case "commandType1":
                // Handle command type 1
                break;
            case "commandType2":
                // Handle command type 2
                break;
            default:
                Debug.LogWarning("Unknown command type received.");
                break;
        }
    }

    [Serializable]
    public class Message
    {
        public string type;
        public string content;
    }
}
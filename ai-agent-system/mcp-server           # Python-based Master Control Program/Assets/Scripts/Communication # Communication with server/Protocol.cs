using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Communication
{
    public static class Protocol
    {
        public static string SerializeMessage(Message message)
        {
            return JsonConvert.SerializeObject(message);
        }

        public static Message DeserializeMessage(string json)
        {
            return JsonConvert.DeserializeObject<Message>(json);
        }
    }

    [Serializable]
    public class Message
    {
        public string MessageType { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }
}
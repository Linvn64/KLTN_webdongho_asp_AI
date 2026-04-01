using System.Reflection.Metadata.Ecma335;

namespace WebDongHoLG.Models
{
    public class ChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }

    }
    public class ChatRequest
    {
        public List<ChatMessage> Messages { get; set; } = new();
    }

    public class LMRequest
    {
        public string Model { get; set; } = "local-model";
        public List<ChatMessage> Messages { get; set; } = new();
        public float Temperature { get; set; } = 0.7f;
        public bool Stream { get; set; } = false; 
    }

    public class LMResponse
    {
        public List<LMChoice> Choices { get; set; } = new(); 
    }
    public class LMChoice
    {
        public ChatMessage Message { get; set; } = new(); 
    }
}

namespace Startup_IELTS.DTOs
{
    
    public class AIResponse
    {
        public List<Choice> choices { get; set; }
    }

    public class Choice
    {
        public Messages message { get; set; }
    }

    public class Messages
    {
        public string role { get; set; }
        public string content { get; set; }
    }
}

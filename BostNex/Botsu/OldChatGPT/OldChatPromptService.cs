//using Azure.AI.OpenAI;

//namespace BostNex.Services
//{
//    /// <summary>
//    /// Chatデータ
//    /// </summary>
//    public interface IOldChatPromptService
//    {
//        public Dictionary<string, List<ChatMessage>> Chat { get; }
//    }

//    public class OldChatPromptService : IOldChatPromptService
//    {
//        private readonly Dictionary<string, List<ChatMessage>> chat; // 今の所読み込まないと思う。完全固定。
//        public Dictionary<string, List<ChatMessage>> Chat => chat;

//        public OldChatPromptService() {
//            chat = new()
//            {
//                { "DefaultPrompt", GetChat() },
//            };
//        }

//        private static List<ChatMessage> GetChat(string prompt = "")
//        {
//            if (string.IsNullOrWhiteSpace(prompt))
//            {
//                return new();
//            }
//            return new()
//            {
//                new ChatMessage(ChatRole.System, prompt)
//            };
//        }
//    }

//}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Completions;
using OpenAI.Edits;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace BostNex.Services
{
    public interface IOpenAiService
    {
        /// <summary>
        /// これまでのやり取りをリセットして最初からにします。
        /// </summary>
        /// <param name="prompt">最初の指示</param>
        public void InitializeChat(string prompt);
        
        /// <summary>
        /// ユーザの入力を受け取って、セッションを進める
        /// </summary>
        /// <param name="request"></param>
        /// <returns>AIからの返答</returns>
        public Task<string> GetNextSessionAsync(string input);

        /// <summary>
        /// API
        /// クライアント側でも使えるようpublicにしておく
        /// </summary>
        public OpenAIClient Api { get; }
    }

    public class OpenAiService : IOpenAiService
    {
        private readonly string RoleUser = "user";
        private readonly string RoleSystem = "system";
        private readonly string RoleAssistant = "assistant";

        /// <summary>
        /// API
        /// クライアント側でも使えるようpublicにしておく
        /// </summary>
        public OpenAIClient Api { get; private set; }

        private readonly OpenAiOption _options;

        private readonly List<ChatPrompt> _chatPrompts = new();

        public OpenAiService(IOptions<OpenAiOption> options)
        {
            _options = options.Value;
            Api = new OpenAIClient(new OpenAIAuthentication(_options.ApiKey));

            InitializeChat();
        }
        
        public void InitializeChat(string prompt = null!)
        {
            _chatPrompts.Clear();
            if (!string.IsNullOrWhiteSpace(prompt))
            {
                _chatPrompts.Add(new ChatPrompt(RoleSystem, prompt));
            }
        }

        public async Task<string> GetNextSessionAsync(string input)
        {
            _chatPrompts.Add(new ChatPrompt(RoleUser, input));
            var chatRequest = new ChatRequest(_chatPrompts);
            var result = await Api.ChatEndpoint.GetCompletionAsync(chatRequest);
            _chatPrompts.Add(new ChatPrompt(RoleAssistant, result.FirstChoice));

            return result.FirstChoice;
        }



    }



    /// <summary>
    /// 設定項目
    /// </summary>
    public class OpenAiOption
    {
        /// <summary>
        /// APIキー
        /// OpenAIのアカウントでログインして取得すること
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;
    }

}

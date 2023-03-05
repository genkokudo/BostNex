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
        /// これまでのやり取りをリセットして最初からにする
        /// プロンプトがあれば再設定する、無ければそのまま
        /// </summary>
        /// <param name="prompt">プロンプト</param>
        public void InitializeChat(List<ChatPrompt> prompts = null!);

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
        /// <summary>
        /// API
        /// クライアント側でも使えるようpublicにしておく
        /// </summary>
        public OpenAIClient Api { get; private set; }

        private readonly OpenAiOption _options;

        /// <summary>
        /// チャットプロンプト
        /// リセット時にこのプロンプトで設定する
        /// </summary>
        private List<ChatPrompt> _chatPrompts = new();

        /// <summary>
        /// 今までのチャットログ、プロンプトは含まない
        /// カットしていない全ての生データをここに保持する
        /// </summary>
        private readonly List<ChatPrompt> _chatLogs = new();

        public OpenAiService(IOptions<OpenAiOption> options)
        {
            _options = options.Value;
            Api = new OpenAIClient(new OpenAIAuthentication(_options.ApiKey));
            InitializeChat();
        }
        
        public void InitializeChat(List<ChatPrompt> prompts = null!)
        {
            // プロンプト再設定、無かったらそのまま
            if (prompts != null)
            {
                _chatPrompts = prompts;
            }
            _chatLogs.Clear();
        }

        public async Task<string> GetNextSessionAsync(string input)
        {
            _chatLogs.Add(new ChatPrompt(ChatRoles.user.ToString(), input));
            var chatRequest = new ChatRequest(GetAllChat(), maxTokens: _options.MaxTokens);
            var result = await Api.ChatEndpoint.GetCompletionAsync(chatRequest);
            _chatLogs.Add(new ChatPrompt(ChatRoles.assistant.ToString(), result.FirstChoice));

            return result.FirstChoice;
        }

        /// <summary>
        /// 現状、GPTに送るチャットログを取得する
        /// プロンプトと件数を制限した会話ログを連結する
        /// </summary>
        /// <returns></returns>
        private List<ChatPrompt> GetAllChat()
        {
            var result = new List<ChatPrompt>();
            result.AddRange(_chatPrompts);
            result.AddRange(_chatLogs.Skip(Math.Max(0, _chatLogs.Count - _options.MaxChatLogCount)));   //_chatLogsの件数を新しい方から指定件数取る
            return result;
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

        /// <summary>
        /// 保持するユーザとアシスタントのチャットログの上限を設定してトークン数を節約する。
        /// </summary>
        public int MaxChatLogCount { get; set; } = 10;

        /// <summary>
        /// 返答のトークン上限を設定してトークン数を節約する。
        /// </summary>
        public int MaxTokens { get; set; } = 180;
    }

    /// <summary>
    /// チャットの役割
    /// ToStringをして使う
    /// </summary>
    public enum ChatRoles
    {
        /// <summary>最初の指示のみ使用する</summary>
        system,
        /// <summary>ユーザの入力</summary>
        user,
        /// <summary>GPTからの返答</summary>
        assistant
    }
}

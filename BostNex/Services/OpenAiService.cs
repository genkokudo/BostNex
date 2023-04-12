using Azure;
using Azure.AI.OpenAI;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using NuGet.Packaging;
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
        public void InitializeChat(Display prompts = null!);

        /// <summary>
        /// 主に開発用
        /// 現在のプロンプトに上書きする
        /// <end>で区切ってプロンプトを入力する
        /// 最初はsystem、その後はuserとassistantの入力扱いとなる
        /// </summary>
        /// <param name="prompts"></param>
        public void InitializeChat(string prompts);

        /// <summary>
        /// ユーザの入力を受け取って、セッションを進める
        /// </summary>
        /// <param name="request"></param>
        /// <returns>AIからの返答</returns>
        public Task<string> GetNextSessionAsync(string input, double temperature = 1.0);

        /// <summary>
        /// 今までのチャットログを取得する
        /// 次回の送信からカットされるものは含まない
        /// </summary>
        public List<ChatMessage> ChatLog { get; }

        /// <summary>
        /// 現在の画面データ
        /// プロンプトもここに格納
        /// </summary>
        public Display CurrentDisplay { get; }

        /// <summary>
        /// ChatGPTからの返答をログに登録する。
        /// </summary>
        /// <param name="aiMessage"></param>
        public void AddAiChatLog(string aiMessage);
    }

    public class OpenAiService : IOpenAiService
    {
        private readonly OpenAIClient _api;
        private readonly OpenAiOption _options;

        public Display CurrentDisplay { get => currentDisplay; }

        /// <summary>
        /// プロンプト含む全チャットログ
        /// </summary>
        public List<ChatMessage> ChatLog => GetAllChat();

        private Display currentDisplay = null!;

        /// <summary>
        /// 今までのチャットログ、プロンプトは含まない
        /// カットしていない全ての生データをここに保持する
        /// </summary>
        private readonly List<ChatMessage> _chatLogs = new();

        /// <summary>
        /// チャットを飛ばす数
        /// トークン上限に引っかかったら、2ずつ増やす
        /// </summary>
        private int _skipLogs = 0;

        public OpenAiService(IOptions<OpenAiOption> options)
        {
            _options = options.Value;
            _api = _options.UseAzureOpenAI ? new OpenAIClient(
                new Uri(_options.AzureUri),
                new AzureKeyCredential(_options.AzureApiKey))
                : new OpenAIClient(_options.ApiKey);
            InitializeChat();
        }
        
        public void InitializeChat(Display display = null!)
        {
            // プロンプト再設定、無かったらそのまま
            if (display != null)
            {
                display.ApplyOption();
                currentDisplay = display;
            }
            _skipLogs = 0;
            _chatLogs.Clear();
        }

        public async Task<string> GetNextSessionAsync(string input, double temperature = 1.0)
        {
            // ユーザの入力をログに追加
            _chatLogs.Add(new ChatMessage(ChatRole.User, input));

            // 返事を貰うか、原因が分からないエラーが来るまで実行
            Response<StreamingChatCompletions> response = null!;
            var count = 0; 
            while (response == null)
            {
                try
                {
                    // リクエストの作成。設定項目と、今までの会話ログをセット
                    var allChat = GetAllChat();
                    var chatCompletionsOptions = new ChatCompletionsOptions()
                    {
                        MaxTokens = _options.MaxTokens,
                    };
                    chatCompletionsOptions.Messages.AddRange(allChat);

                    // 送信
                    response = await _api.GetChatCompletionsStreamingAsync(
                                deploymentOrModelName: "gpt-3.5-turbo",         // gpt-3.5-turbo, gpt-4, gpt-4-0314
                                chatCompletionsOptions);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("maximum context length"))
                    {
                        // トークン数の上限を超えたので、ログを1個消して再送が良さそう
                        _skipLogs += 2;
                        count++;
                        if (count > 4)
                        {
                            // ユーザの入力を削除して中断
                            _chatLogs.RemoveAt(_chatLogs.Count - 1);
                            throw new Exception("リトライ回数を超えました。チャットログのトークンが多すぎるみたいです。"); // どうすればいいんだろう。ソース書いて貰う時とか困るよね。
                        }
                        continue;
                    }
                    else
                    {
                        // 原因不明のエラー
                        // ユーザの入力を削除して中断
                        _chatLogs.RemoveAt(_chatLogs.Count - 1);
                        throw;
                    }
                }
            }

            // ストリーミングで受け取る
            var sb = new StringBuilder();
            using StreamingChatCompletions streamingChatCompletions = response.Value;
            await foreach (StreamingChatChoice choice in streamingChatCompletions.GetChoicesStreaming())    // こっちを返した方が良いかな？
            {
                await foreach (ChatMessage message in choice.GetMessageStreaming())                         // わからん。StringBuilderじゃなくてこれを返したいんだけど。改行はクライアント側で\nだけBRに置換
                {
                    Console.Write(message.Content);
                    sb.Append(message.Content);
                }
                Console.WriteLine();
            }

            // 改行コードが"\n"で送られてくるが、仕様変更があるかもしれないので\r\nに変換しておく
            var aiMessage =
                sb.ToString()
                .Replace("\r\n", "\n")
                .Replace("\n", "\r\n");

            // チャットログに追加
            AddAiChatLog(aiMessage);

            return aiMessage;
        }

        public void AddAiChatLog(string aiMessage)
        {
            _chatLogs.Add(new ChatMessage(ChatRole.Assistant, aiMessage));
        }
        
        /// <summary>
        /// 現状、GPTに送るチャットログを取得する
        /// プロンプトと件数を制限した会話ログを連結する
        /// </summary>
        /// <returns></returns>
        private List<ChatMessage> GetAllChat()
        {
            var result = new List<ChatMessage>();
            result.AddRange(currentDisplay.CurrentPrompt);
            result.AddRange(_chatLogs.Skip(Math.Max(0, _chatLogs.Count - _options.MaxChatLogCount) + _skipLogs));   //_chatLogsの件数を新しい方から指定件数取る
            return result;
        }

        // 開発用
        // MasterPromptを上書きするので注意
        public void InitializeChat(string prompts)
        {
            var result = new List<ChatMessage>();
            var splited = prompts.Replace("\n", "\r\n").Split(_options.Separate);
            foreach (var item in splited)
            {
                var role = result.Count % 2 == 0 ? ChatRole.Assistant : ChatRole.User;
                role = result.Count == 0 ? ChatRole.System : role;
                result.Add(new ChatMessage(role.ToString(), item));
            }
            currentDisplay.MasterPrompt = result;
            InitializeChat(currentDisplay);
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

        /// <summary>
        /// 開発用
        /// プロンプトの区切りを示す文字列
        /// </summary>
        public string Separate { get; set; } = "#end#";

        /// <summary>
        /// Azureを使う
        /// </summary>
        public bool UseAzureOpenAI { get; set; } = false;
        /// <summary>
        /// AzureのAPIキー
        /// </summary>
        public string AzureApiKey { get; set; } = string.Empty;
        /// <summary>
        /// Azureのエンドポイント
        /// </summary>
        public string AzureUri { get; set; } = string.Empty;
    }
}

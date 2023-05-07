using Azure;
using Azure.AI.OpenAI;
using Microsoft.DeepDev;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using NuGet.Configuration;
using NuGet.Packaging;

namespace BostNex.Services.SemanticKernel
{
    public interface IChatService
    {
        /// <summary>
        /// これまでのやり取りをリセットして最初からにする
        /// プロンプトがあれば再設定する、無ければそのまま
        /// </summary>
        /// <param name="prompt">プロンプト</param>
        public void InitializeChat(Display prompts);

        /// <summary>
        /// 主に開発用
        /// 現在のプロンプトに上書きする
        /// "#end#"で区切ってプロンプトを入力する
        /// 最初はsystem、その後はuserとassistantの入力扱いとなる
        /// </summary>
        /// <param name="prompts"></param>
        public void InitializeChat(string prompts);

        /// <summary>
        /// ユーザの入力を受け取って、セッションを進める
        /// </summary>
        /// <param name="request"></param>
        /// <returns>AIからの返答</returns>
        public Task<string> GetNextSessionAsync(string input);

        /// <summary>
        /// 今までのチャットログを取得する
        /// 次回の送信からカットされるものは含まない
        /// </summary>
        public ChatHistory ChatLog { get; }

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

        /// <summary>
        /// トークン数を数える
        /// </summary>
        /// <param name="message"></param>
        public int CountToken(string message);

        /// <summary>
        /// SemanticKernel関数を実行する
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<string> ExecuteSemanticKernelAsync(string functionName, string input);
    }

    public class ChatService : IChatService
    {
        private IChatCompletion _api = null!;
        private readonly IKernelService _kernel;
        private readonly ChatServiceOption _options;

        public Display CurrentDisplay { get => _currentDisplay; }
        private ITokenizer? Tokenizer { get; set; }

        /// <summary>
        /// プロンプト含むチャットログ
        /// 件数制限でカットした分は含まない
        /// </summary>
        public ChatHistory ChatLog => GetCurrentChat();

        private Display _currentDisplay = null!;

        /// <summary>
        /// 今までのチャットログ
        /// カットしていない全ての生データをここに保持する
        /// プロンプトは含まない、初期チャットは含む
        /// </summary>
        private OpenAIChatHistory _chatHistory = null!;

        /// <summary>
        /// チャットを飛ばす数
        /// トークン上限に引っかかったら、2ずつ増やす
        /// </summary>
        private int _skipLogs = 0;

        /// <summary>
        /// Semantic Kernel関数を呼び出す
        /// </summary>
        private readonly ISummaryService _semantic;

        /// <summary>
        /// オプションを取得
        /// オプションを適用したプロンプトを取得
        /// </summary>
        private readonly IChatPromptService _prompt;

        public ChatService(IOptions<ChatServiceOption> options, ISummaryService semantic, IKernelService kernel, IChatPromptService prompt)
        {
            _semantic = semantic;
            _options = options.Value;
            _kernel = kernel;
            _prompt = prompt;
        }
        
        public void InitializeChat(Display display)
        {
            _currentDisplay = display;
            
            // プロンプトと初期チャットの設定
            var prompt = _prompt.GetPrompt(_currentDisplay.MasterPromptKey, CurrentDisplay.Options);
            _currentDisplay.CurrentPrompt = prompt.Messages[0]; // TODO:CurrentDisplay.Optionsっていつ入力するんだっけ？入力画面出す前に_promptから取得も必要。
            _chatHistory = GetChatHistory(prompt);

            // その他の初期化
            Tokenizer = TokenizerBuilder.CreateByModelName(display.GptTokenModel);
            _skipLogs = 0;
            _api = _kernel.GetChatKernel(display.GptModel).GetService<IChatCompletion>();
        }

        // 開発用
        // プロンプトだけ差し替える
        public void InitializeChat(string prompts)
        {
            var prompt = _prompt.GetCustomChat(prompts, CurrentDisplay.Options);
            _currentDisplay.CurrentPrompt = prompt.Messages[0];
            _chatHistory = GetChatHistory(prompt);
        }

        /// <summary>
        /// GetPromptしたOpenAIChatHistoryから、0番目のメッセージを除いた履歴を作成
        /// </summary>
        /// <param name="prompt">Option適用済みプロンプト</param>
        /// <returns></returns>
        private static OpenAIChatHistory GetChatHistory(OpenAIChatHistory prompt)
        {
            var chatHistory = new OpenAIChatHistory();
            if (prompt.Messages.Count > 1)
            {
                for (int i = 1; i < prompt.Messages.Count; i++)
                {
                    chatHistory.Messages.Add(prompt.Messages[i]);
                }
            }
            return chatHistory;
        }

        public async Task<string> ExecuteSemanticKernelAsync(string functionName ,string input)
        {
            var result = await _semantic.Execute(functionName, input);
            return result;
        }

        public async Task<string> GetNextSessionAsync(string input)
        {
            // ユーザの入力をログに追加
            _chatHistory.AddUserMessage(input);

            // 返事を貰うか、原因が分からないエラーが来るまで実行
            string response = null!;
            while (response == null)
            {
                try
                {
                    // リクエストの作成。設定項目と、今までの会話ログをセット
                    var allChat = GetCurrentChat();

                    // おおよそのトークン数を数える（正確ではない）
                    if (CountToken(allChat) > CurrentDisplay.TokenLimitByModel)
                    {
                        _skipLogs += 2;
                        allChat = GetCurrentChat();
                    }

                    var settings = new ChatRequestSettings { Temperature = _currentDisplay.Temperature, TopP = 1, PresencePenalty = _currentDisplay.PresencePenalty, FrequencyPenalty = 0, MaxTokens = 256 };    // いつもの設定

                    // 送信
                    response = await _api.GenerateMessageAsync(allChat, settings);
                }
                catch
                {
                    // 原因不明のエラー
                    // ユーザの入力を削除して中断
                    _chatHistory.Messages.RemoveAt(_chatHistory.Messages.Count - 1);
                    throw;
                }
            }

            // BOTからの返答
            _chatHistory.AddAssistantMessage(response);

            return response;
        }

        // TODO:Streamingじゃなくなったから要らなさそう。
        public void AddAiChatLog(string aiMessage)
        {
            _chatHistory.AddAssistantMessage( aiMessage);
        }
        
        /// <summary>
        /// 現状、GPTに送るチャットログを取得する
        /// プロンプトと件数を制限した会話ログを連結する
        /// </summary>
        /// <returns></returns>
        private ChatHistory GetCurrentChat()
        {
            var result = new ChatHistory();
            result.Messages.Add(_currentDisplay.CurrentPrompt);
            result.Messages.AddRange(_chatHistory.Messages.Skip(Math.Max(0, _chatHistory.Messages.Count - _options.MaxChatLogCount) + _skipLogs));   //_chatLogsの件数を新しい方から指定件数取る
            return result;
        }

        /// <summary>
        /// トークン数を数える
        /// カンマとか入れてるし、roleの扱いが分からないので
        /// 実際のトークン数よりも多いはず
        /// </summary>
        /// <param name="allChat"></param>
        /// <returns></returns>
        private int CountToken(ChatHistory allChat)
        {
            var text = string.Join(',', allChat.Messages.Select(x => x.AuthorRole).ToArray()) + string.Join(',', allChat.Messages.Select(x => x.Content).ToArray());
            return CountToken(text);
        }

        // Tokenizerを使う。
        public int CountToken(string message)
        {
            var encoded = Tokenizer?.Encode(message, Array.Empty<string>());
            return encoded!.Count;
        }
    }

    /// <summary>
    /// 設定項目
    /// </summary>
    public class ChatServiceOption
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
        /// プロンプトの区切りを示す文字列
        /// </summary>
        public string Separate { get; set; } = "#end#";

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

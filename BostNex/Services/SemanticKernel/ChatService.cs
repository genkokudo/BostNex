using Microsoft.DeepDev;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using Microsoft.SemanticKernel.Orchestration;

namespace BostNex.Services.SemanticKernel
{
    // チャットから関数は呼べないはず。やりたかったらそういう仕組みを自分で組み込むしかない。
    public interface IChatService
    {
        /// <summary>
        /// これまでのやり取りをリセットして最初からにする
        /// プロンプトがあれば再設定する、無ければそのまま
        /// </summary>
        /// <param name="prompt">画面情報</param>
        public void InitializeChat(Display prompts);

        /// <summary>
        /// オプションがあれば適用してチャット開始する（なくてもよい）
        /// オプション無しのプロンプトの初期化か、
        /// オプション入力完了時に呼ばれる
        /// </summary>
        /// <param name="prompts"></param>
        public void ApplyOption(Display prompts);

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
        public IAsyncEnumerable<string> GetNextSessionStream(string input);

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
        public Task<string> ExecuteSemanticKernelAsync(string skillName, string functionName, string input);

        // BOTからの返答を登録
        public void AddAssistantMessage(string message);
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
        public ChatHistory ChatLog => CurrentDisplay.IsSemanticKernel ? null! : GetCurrentChat();

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
        /// オプションを取得
        /// オプションを適用したプロンプトを取得
        /// </summary>
        private readonly IChatPromptService _prompt;

        private ChatRequestSettings _settings = null!;

        public ChatService(IOptions<ChatServiceOption> options, IKernelService kernel, IChatPromptService prompt)
        {
            _options = options.Value;
            _kernel = kernel;
            _prompt = prompt;
        }
        
        public void InitializeChat(Display display)
        {
            // 最初にGetOptionsを呼んで、オプションを取る。
            _currentDisplay = display;
            if (display.IsSemanticKernel)
            {
                // 関数呼び出し画面の場合はここまで。
                return;
            }

            var options = _prompt.GetOptions(display.MasterPromptKey);
            if (options.Count == 0)
            {
                // オプションが無ければApplyOption（プロンプト取得＆GetCustomChat）して開始。
                ApplyOption(display);
            }

            // その他の初期化
            Tokenizer = TokenizerBuilder.CreateByModelName(display.GptTokenModel);
            _skipLogs = 0;
            _api = _kernel.Kernel.GetService<IChatCompletion>(display.GptModel.ToString());
            _settings = new ChatRequestSettings { Temperature = _currentDisplay.Temperature, TopP = 1, PresencePenalty = _currentDisplay.PresencePenalty, FrequencyPenalty = _currentDisplay.FrequencyPenalty, MaxTokens = 256 };    // いつもの設定
        }

        public async void ApplyOption(Display prompts)
        {
            // プロンプトと初期チャットの設定
            var prompt = await _prompt.GetPromptAsync(_currentDisplay.MasterPromptKey, CurrentDisplay.Options);    // オプションを適用
            _currentDisplay.CurrentPrompt = prompt.Messages.Count > 0 ? prompt.Messages[0] : new ChatHistory.Message(ChatHistory.AuthorRoles.System, string.Empty);
            _chatHistory = GetChatHistory(prompt);      // 初期チャット設定（無い場合もある）
        }

        // 開発用
        // プロンプトだけ差し替える
        public async void InitializeChat(string prompts)
        {
            var prompt = await _prompt.GetCustomChatAsync(prompts, CurrentDisplay.Options);
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

        // 単独で関数を実行する。Kernelサービスに持って行っても良い。
        public async Task<string> ExecuteSemanticKernelAsync(string skillName, string functionName, string input)
        {
            var func = _kernel.Kernel.Func(skillName, functionName);
            var variables = new ContextVariables(input);
            var context = await _kernel.Kernel.RunAsync(variables, func);
            return context.Result;
        }

        public IAsyncEnumerable<string> GetNextSessionStream(string input)
        {
            // ユーザの入力をログに追加
            _chatHistory.AddUserMessage(input);

            // 返事を貰うか、原因が分からないエラーが来るまで実行
            IAsyncEnumerable<string> response = null!;
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

                    // 送信
                    response = _api.GenerateMessageStreamAsync(allChat, _settings);
                }
                catch(AIException ex)
                {
                    // 原因不明のエラー
                    // ユーザの入力を削除して中断
                    _chatHistory.Messages.RemoveAt(_chatHistory.Messages.Count - 1);
                    if (ex.Detail != null && ex.Detail.Contains("This model's maximum context length"))
                    {
                        _skipLogs += 2;
                    }
                    throw;
                }
            }

            return response;
        }

        // BOTからの返答を登録
        public void AddAssistantMessage(string message)
        {
            _chatHistory.AddAssistantMessage(message);
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

using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SemanticFunctions;
using System.Reflection.Metadata;

namespace BostNex.Services.SemanticKernel
{
    // TODO:要約だけでなく、いろんな関数を呼べるようにしよう
    // TODO:IChatCompletion を試す
    // ISummaryService -> ISemanticServiceに名前変更

    /// <summary>
    /// Semantic Kernelを使用して要約する
    /// モデルは現在の所固定
    /// </summary>
    public interface ISummaryService
    {
        /// <summary>
        /// 要約を作ってくれる
        /// </summary>
        /// <param name="function">関数名</param>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<string> Execute(string function, string input);

        // TODO:後で消す
        public Task<string> ChatTest(string input);
    }

    public class SummaryService : ISummaryService
    {
        private readonly string _skillName = "DarkMagic";
        private readonly IKernelService _kernel;

        public SummaryService(IKernelService kernel)
        {
            _kernel = kernel;

        }

        public async Task<string> Execute(string function, string input)
        {
            var variables = new ContextVariables(input);
            variables["target"] = "ワイン";
            variables["keywords"] = "一陣の風、芳醇な香り、命を吹き込んだ";
            variables["viewpoints"] = "高齢者へのリーチ";

            var context = await _kernel.Kernel.RunAsync(variables, _kernel.Kernel.Func(_skillName, function));   // RunAsyncはISKFunction[]を渡すしかないみたい。
            Console.WriteLine("## 結果");
            Console.WriteLine(context);

            return context.Result;
        }


        // パラメータの付け方は？
        // プロンプトにこういうのを追加。 {{ $text }} 
        // CreateSemanticFunctionの引数に追加して、その中のInputに、
        // Input = new PromptTemplateConfig.InputConfig { Parameters = { new PromptTemplateConfig.InputParameter { Name = "text", Description = "パラメータのコメント", DefaultValue = "文章本体"} } }
        // ってやるとできる。

        // {{ $input }} は？
        // 1 つ前の出力が {{ $input }} 変数に入る。
        // なので、いくつか$inputを入れる方法がある。

        // 1. summarizeで関数を作り、$tsxt等のパラメータの設定した状態でInvokeAsyncすると、その引数が$inputになる。
        // 2. _kernel.RunAsync(vars, summarize); で関数を呼ぶ時に、vars はContextVariablesとなるが、その引数が$inputになる。
        // 3. ContextVariablesはDictionaryで各パラメータを登録できる。それで、kernel.RunAsyncで連続で関数を呼び出し（可変長引数）たときに、その出力が$inputになる。

        // プロンプトって関数にして登録できるの？
        // できる。
        // kernel.CreateSemanticFunction(_prompt);

        // チャットを作る場合は例えば、$"\nHuman: {input}\nMelody: {answer}\n"みたいなのを追加していく。

        // Planは使わなくていいだろうと思う。
        // →問題を入力すると、登録したスキルを組み合わせて、自動的に問題解決の出力まで作ってくれる機能。

        // ネイティブスキルとは？
        // 一定のお約束に従った C# のクラスで、スキルの関数として利用できます。
        // （プロンプトで出来ることはなるべくプロンプトでやってあげた方が良いです。）
        // https://zenn.dev/microsoft/articles/semantic-kernel-3

        // 登録した関数だったら、プロンプトに代入してくれる
        // 例えば、{{ DarkMagic.Summarize $Name }} というプロンプトを作ると、Summarizeに$Nameをinputした結果が代入される。



        // 以下、チャットのテスト
        public async Task<string> ChatTest(string input)
        {
            // 使用するモデルを指定するオプションはない。
            // チャットごとにカーネルを作るか、モデルごとに違うカーネルを使うかしなきゃダメ。
            // スキルはチャットと組み合わせて呼べばいい。
            // ChatHistoryのプロンプト途中書き換えも出来そう。

            // チャット作成
            // 使用するモデルを選択
            IChatCompletion chatGPT = _kernel.GetChatKernel(ModelType.OpenAIGpt35Turbo).GetService<IChatCompletion>();

            // 設定を作成
            var settings = new ChatRequestSettings { Temperature = 1, TopP = 1, PresencePenalty = 0, FrequencyPenalty = 0, MaxTokens = 256};    // いつもの設定

            // チャット開始
            var chatHistory = (OpenAIChatHistory)chatGPT.CreateNewChat(     // OpenAIChatHistoryってAzureでも使える？
                "You're chatting with a user.");    // ここにプロンプトを入れる

            // 人間のメッセージ入力
            var msg = input;
            chatHistory.AddUserMessage(msg);
            
            // BOTからの返答
            string reply = await chatGPT.GenerateMessageAsync(chatHistory, settings);
            chatHistory.AddAssistantMessage(reply);

            return "";
        }




    }

}

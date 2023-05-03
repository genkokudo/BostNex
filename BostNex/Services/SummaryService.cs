using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SemanticFunctions;

namespace BostNex.Services
{
    /// <summary>
    /// Semantic Kernelを使用して要約する
    /// モデルは現在の所固定
    /// </summary>
    public interface ISummaryService
    {
        /// <summary>
        /// 要約を作ってくれる
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<string> MakeSummary(string input);
    }

    public class SummaryService : ISummaryService
    {
        private IKernel _kernel = Kernel.Builder.Build();
        private readonly OpenAiOption _options;
        private readonly ChatOption _chatOptions;
        private readonly bool IsUseAzureOpenAI = false;                 // 手で書き換えてね。

        //private readonly string _prompt = "# 命令書\r\nあなたはプロの編集者です。以下の制約条件に従って、入力する文章を要約してください。\r\n\r\n# 制約条件\r\n- 重要なキーワードを取りこぼさない。\r\n- 文章の意味を変更しない。\r\n- 架空の表現や言葉を使用しない。\r\n- 入力する文章を150文字以内にまとめて出力。\r\n- 要約した文章の句読点を含めた文字数を出力。\r\n- 文章中の数値には変更を加えない。\r\n\r\n# 出力形式\r\n要約した文章:\r\n出力した文章の句読点を含めた文字数:";

        private readonly string _prompt = """
    *****
    {{$input}}
    *****

    長すぎるので1文で最小限の文字数で要約してください。

    要約:

    """;
        // これを呼べば良い。MSかOpenAIかは現在の所固定で、IsUseAzureOpenAIによる。
        private ISKFunction _summarize;

        public SummaryService(IOptions<OpenAiOption> options, IOptions<ChatOption> chatOptions)
        {
            _options = options.Value;
            _chatOptions = chatOptions.Value;

            // カーネルを作成、複数登録できる
            _kernel.Config.AddOpenAIChatCompletionService("sampleOpenAI", "gpt-3.5-turbo", _options.ApiKey);  // この"sampleOpenAI"は_summarize
            _kernel.Config.AddAzureTextCompletionService("sampleAzure",
                _chatOptions.Model1,
                _options.AzureUri,
                _options.AzureApiKey);  // new AzureCliCredential()を使っても良い

            // 関数作成
            _summarize = _kernel.CreateSemanticFunction(_prompt, new PromptTemplateConfig
            {
                // temperatureみたいなパラメータはCompletionで設定
                // Type は分からん。"completion", "embeddings"とかを設定する。
                // Input で、パラメータを設定する
                DefaultServices = { IsUseAzureOpenAI ? "sampleAzure" : "sampleOpenAI" }
            });
        }

        public async Task<string> MakeSummary(string input)
        {
            SKContext context = null!;
            while (context == null)
            {
                context = await _summarize.InvokeAsync(input);
                Console.WriteLine($"""
    ErrorCccurred: {context.ErrorOccurred}
    ErrorDescription: {context.LastErrorDescription}
    Result: {context.Result}
    """);
            }

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


    }

}

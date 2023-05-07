using Microsoft.SemanticKernel.Orchestration;

namespace BostNex.Services.SemanticKernel
{
    // TODO:要約だけでなく、いろんな関数を呼べるようにしよう

    /// <summary>
    /// Semantic Kernelを使用してスキルを登録し、管理する。
    /// </summary>
    public interface ISkillService
    {
        /// <summary>
        /// 要約を作ってくれる
        /// </summary>
        /// <param name="function">関数名</param>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<string> Execute(string function, string input);
    }

    public class SkillService : ISkillService
    {
        private readonly string _skillName = "DarkMagic";
        private readonly IKernelService _kernel;

        public SkillService(IKernelService kernel)
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







    }

}

using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SemanticFunctions;
using System.Reflection.Metadata;

namespace BostNex.Services.SemanticKernel
{

    /// <summary>
    /// 使用する可能性のあるSkillを登録する
    /// 取り敢えずジャンル分けしておく
    /// </summary>
    public interface ISkillService
    {
        /// <summary>
        /// 存在するスキルを全て登録する。
        /// </summary>
        /// <param name="kernel"></param>
        public void RegisterAllSkill(IKernel kernel);
    }
    public enum SkillCategory
    {
        Test,
        DarkMagic
    }

    public class SkillService : ISkillService
    {
        private readonly bool IsUseAzureOpenAI = false;                 // 手で書き換えてね。
        private readonly string _prompt = """
# 命令書
あなたはプロの編集者です。以下の制約条件に従って、入力する文章を要約してください。
# 制約条件
- 重要なキーワードを取りこぼさない。
- 文章の意味を変更しない。
- 架空の表現や言葉を使用しない。
- 入力する文章を150文字以内にまとめて出力。
- 要約した文章の句読点を含めた文字数を出力。
- 文章中の数値には変更を加えない。
# 入力する文章
{{$input}}
# 出力形式
要約した文章:
出力した文章の句読点を含めた文字数:
""";

    //    private readonly string _prompt = """
    //*****
    //{{$input}}
    //*****

    //長すぎるので1文で最小限の文字数で要約してください。

    //要約:

    //""";

        public void RegisterAllSkill(IKernel kernel)
        {
            RegisterDarkMagicSkill(kernel);
        }

        private void RegisterDarkMagicSkill(IKernel kernel)
        {
            // 関数を登録：要約
            kernel.CreateSemanticFunction(_prompt, new PromptTemplateConfig
            {
                // temperatureみたいなパラメータはCompletionで設定
                // Type は"completion", "embeddings"とかを設定する。
                // Input で、パラメータを設定する
                DefaultServices = { IsUseAzureOpenAI ? ModelType.Azure35.ToString() : ModelType.OpenAIGpt35Turbo.ToString() }
            }, functionName: "Summarize", skillName: SkillCategory.DarkMagic.ToString());       // skillNameを指定しない場合、グローバル関数となる。要するにskillはスコープの役割。
        }

        // 作成した関数の取得方法
        //var test = _kernel.Kernel.Func("skillName", "aaaafunctionName");      // skillnameは
        //var skillstest = _kernel.Kernel.Skills; // ここからも取れる。登録一覧を取得するのはできないっぽい。
        //var aaaa = skillstest.GetSemanticFunction("skillName", "aaaafunctionName");
        //var bbbb = skillstest.GetSemanticFunction("bbbbfunctionName");        // グローバル関数の取得は、skillNameを指定しない。
        //var cccc = skillstest.GetFunction("bbbbfunctionName");
        // Semantic以外に、Nativeがある。これは、C#の処理を挟んだ関数だと思われる。
    }

}

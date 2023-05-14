using Microsoft.Identity.Client;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace BostNex.Services.SemanticKernel
{
    // var skill = kernel.ImportSkill(new SampleNativeSkill(), skillName: SkillCategory.DarkMagic.ToString()); // こうやって登録して使う。
    //var input = new ContextVariables("Your Name");
    //var context = await kernel.RunAsync(input, skill["SayHello"]);
    // https://zenn.dev/microsoft/articles/semantic-kernel-3
    public class LightMagic
    {
        private const string s_defaultInput = "Tanaka Taro";
        private const string s_messageTemplateDefaultValue = "Hello, {0}!!";
        private const string s_messageTemplateName = "MessageTemplate";

        private ISKFunction _summarize;
        public LightMagic(IKernel kernel)
        {
            _summarize = kernel.CreateSemanticFunction(
                """
            長すぎるので要約してください。

            ### 入力
            {{$input}}

            ### 要約

            """, maxTokens: 1024);
        }

        // メソッド名の"SayHello"として登録される
        [SKFunction("挨拶をします。")]
        [SKFunctionInput(DefaultValue = s_defaultInput, Description = "Your name.")]    // {{ $Input }}
        [SKFunctionContextParameter(DefaultValue = s_messageTemplateDefaultValue, Description = "Template for the greeting message.", Name = s_messageTemplateName)]    // {{ $MessageTemplate }}   // 複数定義可
        public string SayHello(string name, SKContext context)
        {
            // 値が入ってなければ、デフォルト値を設定する
            if (string.IsNullOrWhiteSpace(name))
            {
                name = s_defaultInput;
            }
            if (!context.Variables.Get(s_messageTemplateName, out var messageTemplate))
            {
                messageTemplate = s_messageTemplateDefaultValue;
            }

            // 何か加工して返す
            return string.Format(messageTemplate, name);
        }


        // メソッド名の"Summarize"として登録される
        [SKFunction("長い文章を要約します。")]
        [SKFunctionInput(Description = "要約元の文章。")]
        public Task<SKContext> Summarize(string input) =>
            // 純粋に ISKFunction を呼び出すだけでも OK だし独自の処理を入れても OK
            _summarize.InvokeAsync(input);

        // このどれかで実装できる。
        //string MySkill(string input);
        //Task<string> MySkill(string input);
        //string MySkill(string input, SKContext context);
        //Task<string> MySkill(string input, SKContext context);
        //SKContext MySkill(string input, SKContext context);
        //Task<SKContext> MySkill(string input, SKContext context);
        //string MySkill(SKContext context);
        //Task<string> MySkill(SKContext context);
        //SKContext MySkill(SKContext context);
        //Task<SKContext> MySkill(SKContext context);
        //void MySkill(string input, SKContext context);
        //void MySkill(string input);
        //void MySkill();
        //string MySkill();
        //Task<string> MySkill();

    }
}

using BostNex.Services.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.SkillDefinition;
using static System.Net.Mime.MediaTypeNames;

// https://zenn.dev/microsoft/articles/semantic-kernel-3
// var skill = kernel.ImportSkill(new LightMagicSkill(), skillName: SkillCategory.LightMagic.ToString()); // こうやって登録して使う。
//var input = new ContextVariables("Your Name");
//var context = await kernel.RunAsync(input, skill["SayHello"]);
namespace BostNex.Skills
{
    public enum LightMagicFunction
    {
        CreateNativeFunction
    }

    public class LightMagicSkill
    {
        // Planで実行する
        // ネイティブ関数の生成
        // 1.EnumeratingFunctionElements
        // 2.GenerateNativeFunction

        [SKFunction("概要からネイティブ関数を生成する")]
        [SKFunctionName("CreateNativeFunction")]
        [SKFunctionInput(DefaultValue = "", Description = "作成したい関数の概要")]
        public async Task<string> CreateNativeFunctionAsync(string objective, SKContext context)
        {
            // 値が入ってなければ、デフォルト値を設定する
            var input = string.IsNullOrWhiteSpace(objective) ? CurrentStatusDefaultValue : objective;

            // 概要から関数を生成するPlanを作る
            var plan1 = new Plan(context.Skills!.GetFunction(SemanticSkillCategory.SkillMaker.ToString(), SkillMakerFunction.EnumeratingFunctionElements.ToString()))
            {
                // 出力を plan1Result という名前で Context に格納する。こうすると、次の関数でどの変数をInputとして受け取るか指定できる。
                // これは設定しなくても良い。設定しない場合はいつものように出力文字列が次のInputとなる。
                //Outputs = { "plan1Result" },
            };
            var plan2 = new Plan(context.Skills.GetFunction(SemanticSkillCategory.SkillMaker.ToString(), SkillMakerFunction.GenerateNativeFunction.ToString()))
            {
                //Outputs = { "plan2Result" },
                // Parameters = new ContextVariables() {["plan1Result"] = "",} を設定すると、plan1Resultを入力として受け取れる
            };
            var plan = new Plan("ネイティブ関数を作成する。", plan1, plan2);

            // 実行して結果を表示
            var result = await plan.InvokeAsync(input);
            return result.Result;
        }


        // ChatGPTに作ってもらう関数サンプル //
        // 現在の状況を入力すると、指定中の人物の行動を生成します。そして、その人物の台詞を出力します。
        //[SKFunction("When the current situation is entered, the system generates the actions of the person being specified. It then outputs the person's dialogue.")]

        private const string CurrentStatusDefaultValue = "何も異常がありません。";
        private const string CharacterPersonality = "CharacterPersonality";
        private const string CharacterPersonalityDefaultValue = "冷酷非情";
        private const string CharacterStatus = "CharacterStatus";
        private const string CharacterStatusDefaultValue = "健康";

        [SKFunction("現在の状況を入力すると、指定中の人物の行動を生成します。そして、その人物の台詞を出力します。")]
        [SKFunctionName("GetDialogue")]
        [SKFunctionInput(DefaultValue = CurrentStatusDefaultValue, Description = "現在の状況")]
        [SKFunctionContextParameter(DefaultValue = CharacterPersonalityDefaultValue, Description = "キャラクターの性格", Name = CharacterPersonality)]
        [SKFunctionContextParameter(DefaultValue = CharacterStatusDefaultValue, Description = "キャラクターの状態", Name = CharacterStatus)]
        public async Task<string> GetDialogueAsync(string currentStatus, SKContext context)
        {
            // 値が入ってなければ、デフォルト値を設定する
            var input = string.IsNullOrWhiteSpace(currentStatus) ? CurrentStatusDefaultValue : currentStatus;
            var characterPersonality = context.Variables.ContainsKey(CharacterPersonality) ? context[CharacterPersonality] : CharacterPersonalityDefaultValue;
            var characterStatus = context.Variables.ContainsKey(CharacterStatus) ? context[CharacterStatus] : CharacterStatusDefaultValue;

            // キャラクターの行動を作成し、パラメータに反映する。
            var action = "キャラクターの行動（ダミー）";
            context.Log.LogTrace("キャラクターの行動：{0}", action);
            context.Variables["Action"] = action;

            // キャラクターの台詞を作成し、出力する。
            var result = "キャラクターの台詞（ダミー）";
            context.Log.LogTrace("キャラクターの台詞：{0}", result);

            return result;
        }

    }
}

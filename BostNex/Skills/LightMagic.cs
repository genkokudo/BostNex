using BostNex.Services.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

// https://zenn.dev/microsoft/articles/semantic-kernel-3
// var skill = kernel.ImportSkill(new LightMagicSkill(), skillName: SkillCategory.LightMagic.ToString()); // こうやって登録して使う。
//var input = new ContextVariables("Your Name");
//var context = await kernel.RunAsync(input, skill["SayHello"]);
namespace BostNex.Skills
{
    public enum LightMagicFunction
    {
        SayHello
    }

    // TODO:まずこのメソッドを作っていくスキルを作ってみない？
    public class LightMagicSkill
    {
        // 要約するメソッドの例
        // 入力：入力した文章を要約するネイティブスキルを作成したい
        // SKFunctionの引数：処理の名前（Descriptionを1～3個程度の単語で表題にして作成する）
        // メソッド名：SKFunctionの引数をPascalCaseにして、Asyncを付けたもの
        // SKFunctionNameの引数：メソッド名からAsyncを除いたもの
        // SKFunctionInputの引数DefaultValue:入力が無かった場合の値
        // SKFunctionInputの引数Description:メソッドの説明

        // SKFunctionContextParameterは、その処理の実行に必要なパラメータの数だけ定義する
        // SKFunctionContextParameterの引数DefaultValue:そのパラメータに入力が無かった場合の値
        // SKFunctionContextParameterの引数Description:そのパラメータの説明
        // SKFunctionContextParameterの引数Name:そのパラメータの名前

        private const string SummarizeInputDefaultValue = "何も入力されていません。";

        private const string SummarizeTemplateDefaultValue = "Hello, {0}!!";
        private const string SummarizeTemplateName = "Template";

        [SKFunction("Summarize")]
        [SKFunctionName("Summarize")]
        [SKFunctionInput(DefaultValue = SummarizeInputDefaultValue, Description = "Name of the method you want to create.")]
        [SKFunctionContextParameter(DefaultValue = SummarizeTemplateDefaultValue, Description = "Create a minute to greet the person whose name you entered.", Name = SummarizeTemplateName)]
        public async Task<string> SummarizeAsync(string document, SKContext context)
        {
            // 値が入ってなければ、デフォルト値を設定する
            document = string.IsNullOrWhiteSpace(document) ? SummarizeTemplateDefaultValue : document;
            var template = context.Variables.ContainsKey(SummarizeTemplateName) ? context[SummarizeTemplateName] : SummarizeTemplateDefaultValue;

            // 何か処理を行う
            var result = string.Format(template, document);
            context.Log.LogTrace("{0}文字の文章を{1}文字に要約しました。", document.Length, result.Length);

            // 結果を出力する
            return result;
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
            var characterPersonality = context.Variables.ContainsKey(CharacterPersonality) ? context[SummarizeTemplateName] : CharacterPersonality;
            var characterStatus = context.Variables.ContainsKey(CharacterStatus) ? context[SummarizeTemplateName] : CharacterStatus;

            // キャラクターの行動を作成し、パラメータに反映する。
            var action = "キャラクターの行動（ダミー）";
            context.Log.LogTrace("キャラクターの行動：{0}", action);
            context.Variables["Action"] = action;

            // キャラクターの台詞を作成し、出力する。
            var result = "キャラクターの台詞（ダミー）";
            context.Log.LogTrace("キャラクターの台詞：{0}", result);

            return result;
        }

        // これを作ってもらうのに必要な要素は何か？
        // やりたいこと（これを入力）：現在の状況を入力すると、指定中の人物の行動を生成します。そして、その人物の台詞を出力します。

        // ここから以下のように要素が出力される
        // |要素|値|
        // |----|----|
        // |関数名|GetDialogue|
        // |入力|現在の状況|
        // |パラメータ入力[0]|キャラクターの性格|
        // |パラメータ入力[1]|キャラクターの状態|
        // |戻り値|キャラクターの台詞|
        // |パラメータ出力[0]|キャラクターの行動|






    }
}

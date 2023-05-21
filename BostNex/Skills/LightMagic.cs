using BostNex.Services.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

// https://zenn.dev/microsoft/articles/semantic-kernel-3
// var skill = kernel.ImportSkill(new SampleNativeSkill(), skillName: SkillCategory.DarkMagic.ToString()); // こうやって登録して使う。
//var input = new ContextVariables("Your Name");
//var context = await kernel.RunAsync(input, skill["SayHello"]);
namespace BostNex.Skills
{
    public enum LightMagicFunction
    {
        SayHello
    }

    public class LightMagic
    {
        private const string DefaultName = "Ginpay Iwatobi";
        private const string DefaultMessageTemplate = "Hello, {0}!!";
        private const string MessageTemplateName = "MessageTemplate";

        private const string DefaultMonsterName = "Goblin";

        // SKFunctionName属性で関数名を指定していないので、メソッド名の"SayHello"として登録される
        [SKFunction("Greetings")]
        [SKFunctionName("SayHello")]
        [SKFunctionInput(DefaultValue = DefaultName, Description = "Your name.")]    // {{ $Input }}
        [SKFunctionContextParameter(DefaultValue = DefaultMessageTemplate, Description = "Create a minute to greet the person whose name you entered.", Name = MessageTemplateName)]  // {{ $MessageTemplate }} // 複数定義可
        public async Task<string> SayHelloAsync(string name, SKContext context)
        {
            // 値が入ってなければ、デフォルト値を設定する
            // 今のバージョンだと属性から取得してくれないらしい。愚かな。
            name = string.IsNullOrWhiteSpace(name) ? DefaultName : name;
            var messageTemplate = context.Variables.ContainsKey(MessageTemplateName) ? context[MessageTemplateName] : DefaultMessageTemplate;

            //// test
            //await CreateMonsterAsync(name, context);
            //await GetMonsterRaceListAsync(context);



            // 何か加工して返す
            context.Log.LogTrace("'{0}'に挨拶しました。", name);
            return string.Format(messageTemplate, name);
        }

        // 新しい種族のモンスターを創造します。
        [SKFunction("Create Monster")]
        [SKFunctionName("CreateMonster")]
        [SKFunctionInput(DefaultValue = DefaultMonsterName, Description = "Name of the monster's race.")]
        //[SKFunctionContextParameter(DefaultValue = "HP:10", Description = "モンスターの標準ステータス", Name = "$MonsterStatus")]  // {{ $MonsterStatus }}
        public async Task CreateMonsterAsync(string name, SKContext context)
        {
            // デフォルト値の設定
            name = string.IsNullOrWhiteSpace(name) ? DefaultName : name;

            // メモリに追加
            // TODO:複数なので、Jsonにシリアライズすることになるはず。
            var saveFunc = context.Func(NativeSkillCategory.TextMemory.ToString(), "Save");
            context.Variables.Update(name);
            context.Variables["collection"] = "world";
            context.Variables["key"] = "monster_race";
            var result = await saveFunc.InvokeAsync(context);

            context.Log.LogTrace("新たなモンスター'{0}'が創られました。", name);
        }

        // モンスターを創造して世界に加えます。
        [SKFunction("Add Monster")]
        [SKFunctionName("AddMonster")]
        [SKFunctionInput(DefaultValue = DefaultMonsterName, Description = "Name of the monster's race.")]    // {{ $Input }}
        //[SKFunctionContextParameter(DefaultValue = "ブリザードハウント", Description = "モンスターを出現させる地方とか？", Name = "Location")]
        public async Task AddMonsterAsync(string name, SKContext context)
        {
            // デフォルト値の設定
            name = string.IsNullOrWhiteSpace(name) ? DefaultName : name;

            //// TODO:メモリに追加
            //var saveFunc = context.Func(NativeSkillCategory.TextMemory.ToString(), "Save");
            //context.Variables.Update(name);
            //context.Variables["collection"] = "world";
            //context.Variables["key"] = $"{name}_count";
            //await saveFunc.InvokeAsync(context);

            context.Log.LogTrace("モンスター'{0}'が出現しました。", name);
        }

        // 現在存在するモンスターの種類の一覧を取得します
        [SKFunction("Get Monster race List")]
        [SKFunctionName("GetMonsterRaceList")]
        //[SKFunctionInput(DefaultValue = DefaultMonsterName, Description = "Name of the monster's race.")]    // {{ $Input }}
        //[SKFunctionContextParameter(DefaultValue = "ブリザードハウント", Description = "モンスターを出現させる地方とか？", Name = "Location")]
        public async Task GetMonsterRaceListAsync(SKContext context)
        {
            // メモリから取得
            var retrieveFunc = context.Func(NativeSkillCategory.TextMemory.ToString(), "Retrieve");
            context.Variables["collection"] = "world";
            context.Variables["key"] = "monster_race";
            var result = await retrieveFunc.InvokeAsync(context);

            context.Log.LogTrace("モンスターの種類の一覧を取得します。");
        }




        // 現在存在するモンスターの個体の一覧

        // 世界を作る
        // モンスターを倒す
        // キャラクターの設定とかもcollectionで分けてぶっこんでいけば良さそう。

        // このどれかで実装できる。
        //string MySkill(string input);
        //Task<string> MySkill(string input);
        //string MySkill(string input, SKContext context);
        //Task<string> MySkill(string input, SKContext context);
        //string MySkill(SKContext context);
        //Task<string> MySkill(SKContext context);
        //SKContext MySkill(SKContext context);
        //void MySkill(string input, SKContext context);
        //void MySkill(string input);
        //void MySkill();
        //string MySkill();
        //Task<string> MySkill();
        //Task MySkill();

    }
}

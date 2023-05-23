using BostNex.Services.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

// https://zenn.dev/microsoft/articles/semantic-kernel-3
// var skill = kernel.ImportSkill(new GeedSkill(), skillName: SkillCategory.Geed.ToString()); // こうやって登録して使う。
//var input = new ContextVariables("Your Name");
//var context = await kernel.RunAsync(input, skill["SayHello"]);

// 使い方を考える場合はここを参考に。
// https://zenn.dev/microsoft/articles/semantic-kernel-8
namespace BostNex.Skills
{
    public enum GeedFunction
    {
        Test
    }

    public class GeedSkill
    {
        private const string DefaultName = "Geed";
        private const string DefaultMessageTemplate = "Hello, {0}!!";
        private const string MessageTemplateName = "MessageTemplate";

        // 初期化する
        [SKFunction("Greetings")]       // 題名（多分Planが参照する）
        [SKFunctionName("Test")]    // メソッド名（私が呼び出すときに使う）
        [SKFunctionInput(DefaultValue = DefaultName, Description = "Your name.")]    // {{ $Input }}
        [SKFunctionContextParameter(DefaultValue = DefaultMessageTemplate, Description = "Create a minute to greet the person whose name you entered.", Name = MessageTemplateName)]  // {{ $MessageTemplate }} // 複数定義可
        public async Task<string> TestAsync(string input, SKContext context)
        {
            input = string.IsNullOrWhiteSpace(input) ? DefaultName : input;
            var messageTemplate = context.Variables.ContainsKey(MessageTemplateName) ? context[MessageTemplateName] : DefaultMessageTemplate;

            // test：結局何が出来るの？
            // ジードの設定をひたすらメモリに入れてみる。

            //// 新しいモンスター作成
            //await CreateMonsterAsync(input, context);
            //// 種類の一覧を取得
            //await GetMonsterRaceListAsync(context);
            //// モンスターを発生させる
            //await AddMonsterAsync(input, context);


            // 何か加工して返す
            context.Log.LogTrace("'{0}'に挨拶しました。", input);
            return string.Format(messageTemplate, input);
        }

        // 新しい種族のモンスターを創造します。
        [SKFunction("Create Monster")]
        [SKFunctionName("CreateMonster")]
        [SKFunctionInput(DefaultValue = DefaultName, Description = "Name of the monster's race.")]
        //[SKFunctionContextParameter(DefaultValue = "HP:10", Description = "モンスターの標準ステータス", Name = "$MonsterStatus")]  // {{ $MonsterStatus }}
        public async Task CreateMonsterAsync(string name, SKContext context)
        {
            // デフォルト値の設定
            name = string.IsNullOrWhiteSpace(name) ? DefaultName : name;

            // メモリに追加
            // TODO:複数なので、Jsonにシリアライズすることになるはず。

            // こういうことやってるけど、await kernel.Memory.SaveInformationAsync("Me", "私の生年月日は1985年5月30日です。", "info1");    と同じだよ。
            // context.Memory.SaveInformationAsyncでもいいのかな？？
            // await kernel.Memory.SearchAsync("Me", "Meについて何か質問文").FirstOrDefaultAsync(); ってやると、適合した回答が得られる。

            var saveFunc = context.Func(NativeSkillCategory.TextMemory.ToString(), "Save");
            context.Variables.Update(name);
            context.Variables["collection"] = "world";
            context.Variables["key"] = "monster_race";              // "monster_race"に上書きなんだよね。リストにしてJSONにしなきゃダメ。

            // 保存
            await saveFunc.InvokeAsync(context);

            context.Log.LogTrace("新たなモンスター'{0}'が創られました。", name);
        }

        // モンスターを創造して世界に加えます。
        [SKFunction("Add Monster")]
        [SKFunctionName("AddMonster")]
        [SKFunctionInput(DefaultValue = DefaultName, Description = "Name of the monster's race.")]    // {{ $Input }}
        //[SKFunctionContextParameter(DefaultValue = "ブリザードハウント", Description = "モンスターを出現させる地方とか？", Name = "Location")]
        public async Task AddMonsterAsync(string name, SKContext context)
        {
            // デフォルト値の設定
            name = string.IsNullOrWhiteSpace(name) ? DefaultName : name;

            // nameでモンスター検索
            var recallFunc = context.Func(NativeSkillCategory.TextMemory.ToString(), "Recall");
            context.Variables.Update(name);
            context.Variables["collection"] = "world";
            context.Variables["key"] = "monster_race";
            context.Variables["relevance"] = "0.5"; // 一致度。1だと完全一致
            context.Variables["limit"] = "20";     // いくつまでデータを取るか？
            var aaaa = await recallFunc.InvokeAsync(context);   // "world"の中から、name（"orc"など）に関する情報を20個まで取得。キー名は多分関係なし。

            // 【考察】オークという個体を出すには？
            // "collection"は"world"じゃなくて、"monster_race"みたいな感じにする。ここには"orc"というキーでカタカナで名前（と説明文？）を入れる。
            // それとは別に、"monster_orc"という"collection"を作って、色んなkeyでオークに関する情報を追加していく。

            // オリキャラとかもそうやって追加
            // "collection"にキャラ名、"key"に情報を入れていく。
            // キャラの持ち物とか、アイテムの効果などのリストデータはどう格納するの？JSONだとしても検索には向かなさそう。
            // 「このキャラの口調リストはこのkeyで格納してます」みたいな前提で関数作るしかない？
            // それとも"口調例：ああああ"、"口調例：いいいい"みたいな感じで適当なキーで入れれば大丈夫？こっちが正解な気がするが…。

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

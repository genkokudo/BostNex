using Azure.AI.OpenAI;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace BostNex.Services
{
    /// <summary>
    /// TRPGで使う画面とデータを管理する
    /// </summary>
    public interface ITrpgFormatService
    {
        public Dictionary<string, Display> PageData { get; }
        public bool IsDebugMode { get; }

        /// <summary>
        /// 有効なページの一覧を取得する
        /// </summary>
        /// <returns></returns>
        public List<Display> GetKeys();
    }

    public class TrpgFormatService : ITrpgFormatService
    {
        private readonly TrpgOption _options;
        ITrpgService _trpg;

        public Dictionary<string, Display> PageData => _pageData;
        public Dictionary<string, Display> _pageData = new();
        public bool IsDebugMode => _options.IsLocalDevelopMode;

        public TrpgFormatService(ITrpgService trpg, IOptions<TrpgOption> options)
        {
            _trpg = trpg;
            _options = options.Value;

            // 追加していくこと
            _pageData.Add("Default", new Display {
                Address = "Default",
                Title = "デフォルトモデル",
                Headline = "？？？？",
                Introduction = "特に何もプロンプトを与えていません",
                Placeholder = "あなたの質問",
                SubmitText = "送る",
                MasterPrompt = _trpg.DefaultPrompt,
                IsPublic = false
            });
            _pageData.Add("Ojisan", new Display
            {
                Address = "Ojisan",
                Title = "おぢさん",
                Headline = "おぢさんとお話しよう！",
                Introduction = "おぢさんとお話できます。",
                Placeholder = "喋ってみよう！",
                SubmitText = "喋ってみる",
                MasterPrompt = _trpg.Ojisan,
                Options = new List<DisplayOption> { new DisplayOption() },   // 名前入力
                IsPublic = true
            });
            _pageData.Add("Tsunko", new Display
            {
                MasterPrompt = _trpg.Tsunko,
                Title = "ツン子",
                Address = "Tsunko",
                Headline = "ツン子とお話しよう！"
            });
            _pageData.Add("DonpenKarma", new Display
            {
                Address = "DonpenKarma",
                Title = "魔王（兄）",
                Headline = "魔王（兄）",
                Introduction = "人間を鹿金することを目指す冷酷非情な魔王。\r\n戦って勝利を目指すか、服従して一緒に世界を征服してください。\r\n名前は「ドンペン・カルマ」です。\r\n容姿設定はありません。（他のキャラもそう）",
                Placeholder = "ここがあの魔王のHouseね",
                SubmitText = "発言する",
                MasterPrompt = _trpg.DonpenKarma,
                IsPublic = true
            });
            _pageData.Add("PrankDaemon", new Display
            {
                Address = "PrankDaemon",
                Title = "魔王（弟）",
                Headline = "魔王（弟）",
                Introduction = "人間を玩具にすることを目指す好戦的な魔王。\r\n戦いを挑んでルールを説明すると、その通りに勝負してくれます。\r\n名前は「プランク・デーモン」です。\r\nよく主語を間違えるのでスルーしてください。",
                Placeholder = "勝負を挑もう！",
                SubmitText = "闘う",
                MasterPrompt = _trpg.PrankDaemon,
                IsPublic = true
            });
            _pageData.Add("FemaleOverLoad", new Display
            {
                Address = "FemaleOverLoad",
                Title = "魔王♀",
                Headline = "魔王♀",
                Introduction = "500年間封印され続けた古の大魔王。今その封印が解かれようとしている！？\r\n名前は「翠闇（すいあん）」です。\r\n封印を解いてあげるとかしてください。",
                Placeholder = "女魔王とお話する",
                SubmitText = "発言する",
                MasterPrompt = _trpg.FemaleOverLoad,
                IsPublic = true
            });
            _pageData.Add("Yanko", new Display
            {
                Address = "Yanko",
                Title = "ヤン子",
                Headline = "ヤン子",
                Introduction = "ヤ、ヤン子…",
                Placeholder = "ヤン子とお話する",
                MasterPrompt = _trpg.Yanko,
                IsPublic = true
            });
            _pageData.Add("Geed", new Display
            {
                Address = "Geed",
                Title = "獣人♂",
                Headline = "獣人♂",
                Introduction = "人間は獣人に服従すべき脆弱な種族だ。\r\n名前は「ジード」です。\r\n種族は決まってないので、()を使ってト書きをするとその通りになってくれるはずです。\r\n喧嘩したり冒険したりしてください。",
                Placeholder = "ジードとお話する",
                MasterPrompt = _trpg.Geed,
                IsPublic = true
            });
            _pageData.Add("Giant", new Display
            {
                Address = "Giant",
                Title = "巨人♂",
                Headline = "巨人♂",
                Introduction = "巨人に支配された街で生き残ろう。\r\n怪しい男の名前は「オウガ」です。\r\n巨人は弱点がありますが、普通の方法ではなかなか勝てません。",
                Placeholder = "こいつ怪しいなあ",
                MasterPrompt = _trpg.Giant,
                IsPublic = true
            });
            _pageData.Add("Villain", new Display
            {
                Address = "Villain",
                Title = "怪人♂",
                Headline = "怪人♂",
                Introduction = "怪人の名前は「ザッハーク」です。\r\n怪人は周囲にある動植物を吸収して能力を手に入れます。\r\nあなたは必殺技や武器で怪人を倒してください。",
                Placeholder = "怪人め、許さないぞ",
                SubmitText = "闘う",
                MasterPrompt = _trpg.Villain,
                IsPublic = true
            });

            // 開発モードの場合
            if (!_options.IsLocalDevelopMode)
            {
                // _pageDataからIsPublicではないものを除外する
                _pageData = _pageData.Where(x => x.Value.IsPublic).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public List<Display> GetKeys()
        {
            if (!_options.IsLocalDevelopMode)
            {
                // IsPublic = falseのものは除外する
                return _pageData.Where(x => x.Value.IsPublic).Select(x => x.Value).ToList();
            }
            return _pageData.Select(x => x.Value).ToList();
        }
    }

    /// <summary>
    /// 画面データ
    /// </summary>
    public class Display
    {
        /// <summary>アドレス</summary>
        public string Address { get; set; } = "Tsunko";
        /// <summary>NavMenuに表示するタイトル</summary>
        public string Title { get; set; } = "ツン子";
        public string Headline { get; set; } = "ツン子とお話しよう！";
        public string Introduction { get; set; } = "ツン子という少女とお話できます。";
        public string Placeholder { get; set; } = "喋ってみよう！";
        /// <summary>送信ボタン</summary>
        public string SubmitText { get; set; } = "送信";
        ///// <summary>
        ///// 0から2の範囲で指定するけど、大きすぎると壊れる
        ///// </summary>
        //public double Temperature { get; set; } = 2.0;
        /// <summary>
        /// マスター扱い。上書きとかしないこと（開発モードは別）
        /// </summary>
        public List<ChatMessage> MasterPrompt { get; set; } = new();

        /// <summary>これがfalseの場合、"IsLocalDevelopMode": falseならば表示しない</summary>
        public bool IsPublic { get; set; } = true;

        /// <summary>ユーザに入力させる項目</summary>
        public List<DisplayOption> Options { get; set; } = new();

        /// <summary>
        /// 実際に画面で使用するプロンプト
        /// </summary>
        public List<ChatMessage> CurrentPrompt { get; set; } = new();

        /// <summary>
        /// Option入力後に呼ぶ
        /// CurrentPromptを新しく作り直す。
        /// Option.Valueの内容をPromptの0番目のContentに適用する。
        /// </summary>
        public void ApplyOption()
        {
            CurrentPrompt.Clear();
            CurrentPrompt.AddRange(MasterPrompt);

            if (CurrentPrompt.Count == 0)
            {
                return;
            }
            var values = Options.Select(x => x.Value ?? string.Empty).ToArray();
            var content = CurrentPrompt[0].Content.FormatWith(values);
            CurrentPrompt[0] = new ChatMessage(MasterPrompt[0].Role, content);
        }
    }

    /// <summary>
    /// ユーザが入力する設定項目
    /// </summary>
    public class DisplayOption
    {
        public string Subject { get; set; } = "あなたの名前";
        public string Value { get; set; } = "";
        /// <summary>
        /// 入力行数
        /// 2以上だとマルチライン入力欄にする
        /// </summary>
        public int Rows { get; set; } = 1;
    }

    /// <summary>
    /// 設定項目
    /// </summary>
    public class TrpgOption
    {
        /// <summary>
        /// trueだと開発モードとなり非公開のプロンプトも適用される
        /// </summary>
        public bool IsLocalDevelopMode { get; set; } = false;
    }

}
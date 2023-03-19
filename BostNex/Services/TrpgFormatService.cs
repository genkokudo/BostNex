using Azure.Core;
using BostNex.Pages;
using Humanizer;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using OpenAI.Completions;
using System.Text;

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
            _pageData.Add("Trpg", new Display
            {
                Address = "Trpg",
                Title = "TRPG",
                Headline = "TRPG",
                Introduction = "ここはよくある普通の剣と魔法のファンタジーRPGの世界。物語はここから始まる。",
                Placeholder = "あなたの行動",
                SubmitText = "行くぜ！",
                MasterPrompt = _trpg.RoBot,
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
            _pageData.Add("Tsunko2", new Display { MasterPrompt = _trpg.Tsunko2, Title = "つん子2", Address = "Tsunko2", Headline = "ツン子とお話しよう！改" });
            _pageData.Add("DonpenKarma", new Display
            {
                Address = "DonpenKarma",
                Title = "魔王（兄）",
                Headline = "魔王（兄）",
                Introduction = "ここはよくある普通の剣と魔法のファンタジーRPGの世界。物語はここから始まる。",
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
                Introduction = "人間と勝負するのが好きな魔王",
                Placeholder = "勝負を挑もう！",
                SubmitText = "闘う",
                MasterPrompt = _trpg.PrankDaemon,
                IsPublic = false
            });
            _pageData.Add("FemaleOverLoad", new Display
            {
                Address = "FemaleOverLoad",
                Title = "魔王♀",
                Headline = "魔王♀",
                Introduction = "500年間封印され続けた古の大魔王。今その封印が解かれようとしている！？",
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
                SubmitText = "送信",
                MasterPrompt = _trpg.Yanko,
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
        /// <summary>
        /// マスター扱い。上書きとかしないこと（開発モードは別）
        /// </summary>
        public List<ChatPrompt> MasterPrompt { get; set; } = new();
        /// <summary>これがfalseの場合、"IsLocalDevelopMode": falseならば表示しない</summary>
        public bool IsPublic { get; set; } = true;

        /// <summary>ユーザに入力させる項目</summary>
        public List<DisplayOption> Options { get; set; } = new();

        /// <summary>
        /// 実際に画面で使用するプロンプト
        /// </summary>
        public List<ChatPrompt> CurrentPrompt { get; set; } = new();

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
            CurrentPrompt[0] = new ChatPrompt(MasterPrompt[0].Role, content);
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
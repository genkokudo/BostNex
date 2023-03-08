using Azure.Core;
using BostNex.Pages;
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

        public TrpgFormatService(ITrpgService trpg, IOptions<TrpgOption> options)
        {
            _trpg = trpg;
            _options = options.Value;

            // 追加していくこと
            // TODO: これをクライアント側で一覧表示する。
            _pageData.Add("Default", new Display {
                Address = "Default",
                Title = "デフォルトモデル",
                Headline = "？？？？",
                Introduction = "特に何もプロンプトを与えていません",
                Placeholder = "あなたの質問",
                SubmitText = "送る",
                Prompt = _trpg.DefaultPrompt,
                IsPublic = false
            });
            _pageData.Add("Tsunko", new Display { Prompt = _trpg.Tsunko });
            _pageData.Add("Trpg", new Display
            {
                Address = "Trpg",
                Title = "TRPG",
                Headline = "TRPG",
                Introduction = "ここはよくある普通の剣と魔法のファンタジーRPGの世界。物語はここから始まる。",
                Placeholder = "あなたの行動",
                SubmitText = "行くぜ！",
                Prompt = _trpg.RoBot,
                IsPublic = false
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
        public string Address { get; set; } = "Tsunko";
        public string Title { get; set; } = "ツン子";
        public string Headline { get; set; } = "ツン子とお話しよう！";
        public string Introduction { get; set; } = "ツン子という少女とお話できます。";
        public string Placeholder { get; set; } = "喋ってみよう！";
        public string SubmitText { get; set; } = "送信";
        public List<ChatPrompt> Prompt { get; set; } = new();
        public bool IsPublic { get; set; } = true;  // TODO:これがfalseの場合、"IsLocalDevelopMode": falseならば表示しない
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
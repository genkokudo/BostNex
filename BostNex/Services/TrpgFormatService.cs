using Azure.Core;
using BostNex.Pages;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
    }

    public class TrpgFormatService : ITrpgFormatService
    {
        ITrpgService _trpg;

        public Dictionary<string, Display> PageData => _pageData;
        public Dictionary<string, Display> _pageData = new();

        public TrpgFormatService(ITrpgService trpg)
        {
            _trpg = trpg;
            _pageData.Add("Tsunko", new Display { Prompt = _trpg.Tsunko });
            _pageData.Add("Trpg", new Display { Prompt = _trpg.Tsunko, Title = "TRPG", Headline = "TRPG", Introduction = "ここはよくある普通の剣と魔法のファンタジーRPGの世界。物語はここから始まる。", Placeholder="あなたの行動" });
        }
    }

    /// <summary>
    /// 画面データ
    /// </summary>
    public class Display
    {
        public string Title { get; set; } = "ツン子";
        public string Headline { get; set; } = "ツン子とお話しよう！";
        public string Introduction { get; set; } = "ツン子という少女とお話できます。";
        public string Placeholder { get; set; } = "喋ってみよう！";
        public string SubmitText { get; set; } = "送信";
        public List<ChatPrompt> Prompt { get; set; } = new();
        public bool IsPublic { get; set; } = true;  // TODO:これがfalseの場合、"IsLocalDevelopMode": falseならば表示しない
    }

}
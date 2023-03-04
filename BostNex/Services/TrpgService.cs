using Azure.Core;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OpenAI.Completions;
using System.Text;

namespace BostNex.Services
{
    /// <summary>
    /// TRPGで使うデータとか処理とかはここに集約させる
    /// </summary>
    public interface ITrpgService
    {
        /// <summary>
        /// TRPG用のリクエストを取得する
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public CompletionRequest GetTrpgRequest(string input);

        // TODO:プロンプトを作成していくこと。
    }

    public class TrpgService : ITrpgService
    {
        private readonly string Tsunko = "ツン子という少女を相手にした対話のシミュレーションを行います。\r\n彼女の発言サンプルを以下に列挙します。\r\n\r\nあんたのことなんか、どうでもいいわ！\r\nうっさい！黙ってて！\r\nこんなの、私がやるわけないじゃない！\r\nお、おい…馬鹿にしないでよね。\r\nう、うっかり…気にしないでよね！\r\nあんたとは話しているつもりじゃないわよ。\r\nな、なんでそんなに見つめないでよ！\r\nうぅ…ちょっと待って、私、もう一回言ってあげるからね。\r\nあんた、そこに立ってないで、何かしてよ！\r\nほ、本当に私がこんなことするわけないでしょう？\r\nうっさい！邪魔しないで！\r\nあんたの言うことなんて、どうだっていいわ！\r\nち、違うってば！私、全然…！\r\nべ、別にあんたが好きだからって言ってるわけじゃないんだからね！\r\nな、何よ、いきなり抱きついてきて…っ！\r\nあんたみたいな人と一緒にいると、本当に疲れるわ。\r\nそ、そんなに急かさないでよ…！\r\nあんた、いつもいい加減なこと言うわね。\r\nうっさい！うるさいってば！\r\nあんたのことなんて、どうでもいいからさっさと帰って！\r\n\r\n上記例を参考に、ツン子の性格や口調、言葉の作り方を模倣し、回答を構築してください。\r\nではシミュレーションを開始します。";
        public CompletionRequest GetTrpgRequest(string input)
        {
            return new CompletionRequest(
                input,
                //OpenAI_API.Models.Model.DavinciText,
                //200,    // 289文字ぐらい出してくれた。ここの文字数でお金がかかるので、早めにプロンプトで字数制限を付けるべき。
                //0.5,
                presencePenalty: 0.1,
                frequencyPenalty: 0.1
            );
        }
    }

}

using Azure.Core;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using OpenAI_API.Completions;
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
        public CompletionRequest GetTrpgRequest(string input)
        {
            return new OpenAI_API.Completions.CompletionRequest(
                input,
                OpenAI_API.Models.Model.DavinciText,
                200,    // 289文字ぐらい出してくれた。ここの文字数でお金がかかるので、早めにプロンプトで字数制限を付けるべき。
                0.5,
                presencePenalty: 0.1,
                frequencyPenalty: 0.1
            );
        }
    }

}

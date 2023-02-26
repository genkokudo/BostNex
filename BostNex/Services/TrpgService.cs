using Azure.Core;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using OpenAI_API;
using OpenAI_API.Completions;
using System.Diagnostics;
using System.Security.Cryptography;
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
    }

    public class TrpgService : ITrpgService
    {
        public CompletionRequest GetTrpgRequest(string input)
        {
            return new OpenAI_API.Completions.CompletionRequest(
                input,
                OpenAI_API.Models.Model.DavinciText,
                200,
                0.5,
                presencePenalty: 0.1,
                frequencyPenalty: 0.1
            );
        }
    }

}

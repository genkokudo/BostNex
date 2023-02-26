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
    public interface IOpenAiService
    {
        /// <summary>
        /// ユーザの入力を受け取って、セッションを進める
        /// AIからの返事はストリーミングされるため、全て受け取ってから返す
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<string> GetNextSessionAsync(CompletionRequest request);

        /// <summary>
        /// API
        /// クライアント側でも使えるようpublicにしておく
        /// </summary>
        public OpenAIAPI Api { get; }
    }

    public class OpenAiService : IOpenAiService
    {
        /// <summary>
        /// API
        /// クライアント側でも使えるようpublicにしておく
        /// </summary>
        public OpenAIAPI Api { get; private set; }

        private readonly OpenAiOption _options;

        public OpenAiService(IOptions<OpenAiOption> options)
        {
            _options = options.Value;
            Api = new OpenAIAPI(_options.ApiKey);
        }

        // 今の所使ってない
        public async Task<string> GetNextSessionAsync(CompletionRequest request)
        {
            // ストリーミング
            var sb = new StringBuilder();
            await foreach (var token in Api.Completions.StreamCompletionEnumerableAsync(request))
            {
                sb.Append(token.ToString());
            }
            
            return sb.ToString();
        }

    }



    /// <summary>
    /// 設定項目
    /// </summary>
    public class OpenAiOption
    {
        /// <summary>
        /// APIキー
        /// OpenAIのアカウントでログインして取得すること
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;
    }

}

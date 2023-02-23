using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using OpenAI_API;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace BostNex.Services
{
    public interface IOpenAiService
    {
        /// <summary>
        /// ユーザの入力を受け取って、セッションを進める
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<string> GetNextSessionAsync(string input);
    }

    public class OpenAiService : IOpenAiService
    {
        private readonly OpenAiOption _options;
        private OpenAIAPI _openAiApi;

        public OpenAiService(IOptions<OpenAiOption> options)
        {
            _options = options.Value;
            _openAiApi = new OpenAIAPI(_options.ApiKey);
        }

        public async Task<string> GetNextSessionAsync(string action)
        {
            // なんか冒頭部分しか表示されない。多分徐々に返ってくるので、最後まで返ってきたところで反映させるべき。
            var result = await _openAiApi.Completions.GetCompletion(action);
            return result;
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

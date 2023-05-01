using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using NuGet.Packaging;
using System.Text;

namespace BostNex.Services
{
    /// <summary>
    /// 要約する
    /// </summary>
    public interface ISummaryService
    {
        public Task<string> MakeSummary(string input);
    }

    public class SummaryService : ISummaryService
    {
        private readonly OpenAiOption _options;
        private readonly OpenAIClient _msApi;
        private readonly OpenAIClient _openAiApi;
        private readonly string _prompt = "# 命令書\r\nあなたはプロの編集者です。以下の制約条件に従って、入力する文章を要約してください。\r\n\r\n# 制約条件\r\n- 重要なキーワードを取りこぼさない。\r\n- 文章の意味を変更しない。\r\n- 架空の表現や言葉を使用しない。\r\n- 入力する文章を150文字以内にまとめて出力。\r\n- 要約した文章の句読点を含めた文字数を出力。\r\n- 文章中の数値には変更を加えない。\r\n\r\n# 出力形式\r\n要約した文章:\r\n出力した文章の句読点を含めた文字数:";

        private readonly bool IsUseAzureOpenAI = false;
        private OpenAIClient Api { get => IsUseAzureOpenAI ? _msApi : _openAiApi; }

        public SummaryService(IOptions<OpenAiOption> options)
        {
            _options = options.Value;
            _msApi = new OpenAIClient(
                new Uri(_options.AzureUri),
                new AzureKeyCredential(_options.AzureApiKey));
            _openAiApi = new OpenAIClient(_options.ApiKey);
        }

        public async Task<string> MakeSummary(string input)
        {
            // 返事を貰うか、原因が分からないエラーが来るまで実行
            Response<ChatCompletions> response = null!;
            var count = 0;
            while (response == null)
            {
                try
                {
                    // リクエストの作成。設定項目と、今までの会話ログをセット
                    var allChat = GetAllChat();
                    var chatCompletionsOptions = new ChatCompletionsOptions()
                    {
                        //MaxTokens = _currentDisplay.MaxTokens,        // 取り敢えず無しで。
                    };
                    chatCompletionsOptions.Messages.AddRange(allChat);

                    // 送信
                    response = await Api.GetChatCompletionsAsync(
                                deploymentOrModelName: "gpt-3.5-turbo",            // 取り敢えず3.5で
                                chatCompletionsOptions);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("maximum context length"))
                    {
                        count++;
                        if (count > 4)
                        {
                            // ユーザの入力を削除して中断
                            throw new Exception("リトライ回数を超えました。チャットログのトークンが多すぎるみたいです。"); // どうすればいいんだろう。ソース書いて貰う時とか困るよね。
                        }
                        continue;
                    }
                    else
                    {
                        // 原因不明のエラー
                        throw;
                    }
                }
            }

            return "";
        }

        /// <summary>
        /// 現状、GPTに送るチャットログを取得する
        /// プロンプトと件数を制限した会話ログを連結する
        /// </summary>
        /// <returns></returns>
        private List<ChatMessage> GetAllChat()
        {
            return new List<ChatMessage> { new ChatMessage(ChatRole.System, _prompt) };
        }
    }

}

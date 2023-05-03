using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace BostNex.Services
{
    /// <summary>
    /// 使用する可能性のあるモデルを持ったKernel
    /// </summary>
    public interface IKernelService
    {
        public IKernel Kernel { get; }
    }

    /// <summary>
    /// 使用する可能性があるモデル
    /// </summary>
    public enum ModelType
    {
        OpenAIGpt35Turbo,
        OpenAIGpt4,
        OpenAIGpt40314,
        Azure35,
        Azure4,
        Azure432k,
        AzureCode,
    }

    public class KernelService : IKernelService
    {
        private static IKernel _kernel = null!;

        public IKernel Kernel { get {
                if (_kernel == null)
                {
                    InitializeKernel();
                }
                return _kernel!;
            } }
        private readonly OpenAiOption _options;
        private readonly ChatOption _chatOptions;

        public KernelService(IOptions<OpenAiOption> options, IOptions<ChatOption> chatOptions)
        {
            _options = options.Value;
            _chatOptions = chatOptions.Value;
            InitializeKernel();
        }

        /// <summary>
        /// カーネル初期化処理
        /// OpenAI本家もAzureも登録しておく
        /// OpenAIの方は固定。
        /// </summary>
        private void InitializeKernel()
        {
            _kernel = Microsoft.SemanticKernel.Kernel.Builder.Build();
            // カーネルを作成、複数登録できる
            // OpenAI
            _kernel.Config.AddOpenAIChatCompletionService(ModelType.OpenAIGpt35Turbo.ToString(), "gpt-3.5-turbo", _options.ApiKey);  // この第1引数は_summarizeで指定する。
            _kernel.Config.AddOpenAIChatCompletionService(ModelType.OpenAIGpt4.ToString(), "gpt-4", _options.ApiKey);
            _kernel.Config.AddOpenAIChatCompletionService(ModelType.OpenAIGpt40314.ToString(), "gpt-4-0314", _options.ApiKey);

            // Azure
            var azureModels = new ModelType[] { ModelType.Azure35, ModelType.Azure4, ModelType.Azure432k, ModelType.AzureCode };
            for (int i = 0; i < _chatOptions.Models.Length; i++)
            {
                if (azureModels.Length >= i)
                {
                    break;
                }
                _kernel.Config.AddAzureTextCompletionService(azureModels[i].ToString(),
                    _chatOptions.Models[i],
                    _options.AzureUri,
                    _options.AzureApiKey);  // new AzureCliCredential()を使っても良い
            }
        }


    }

}

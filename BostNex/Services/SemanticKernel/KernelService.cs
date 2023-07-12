using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;

namespace BostNex.Services.SemanticKernel
{
    /// <summary>
    /// 使用する可能性のあるモデルを持ったKernel
    /// AddSingletonで使用
    /// </summary>
    public interface IKernelService
    {
        /// <summary>
        /// スキルを登録したカーネル
        /// </summary>
        public IKernel Kernel { get; }
    }

    /// <summary>
    /// 使用する可能性があるモデル
    /// </summary>
    public enum ModelType
    {
        // チャット用
        OpenAIGpt35Turbo,
        OpenAIGpt4,
        OpenAIGpt40314,
        Azure35,
        Azure4,
        Azure432k,

        // テキスト補間用
        AzureCode,
    }

    public class KernelService : IKernelService
    {
        /// <summary>
        /// 単独処理用のカーネル
        /// スキル登録はこっち
        /// </summary>
        private static IKernel _kernel = null!;

        public IKernel Kernel
        {
            get
            {
                if (_kernel == null)
                {
                    InitializeKernel();
                }
                return _kernel!;
            }
        }
        private readonly ChatServiceOption _options;
        private readonly ChatOption _chatOptions;

        public KernelService(IOptions<ChatServiceOption> options, IOptions<ChatOption> chatOptions, ISkillPromptService skill)
        {
            _options = options.Value;
            _chatOptions = chatOptions.Value;
            InitializeKernel();

            // スキルを全部登録しちゃう
            skill.RegisterAllSkill(_kernel);
        }

        /// <summary>
        /// カーネル初期化処理
        /// OpenAI本家もAzureも登録しておく
        /// OpenAIの方は固定。
        /// </summary>
        private void InitializeKernel()
        {
            var azureModels = new ModelType[] { ModelType.Azure35, ModelType.Azure4, ModelType.Azure432k, ModelType.AzureCode };

            var builder = Microsoft.SemanticKernel.Kernel.Builder
                .WithOpenAITextEmbeddingGenerationService("text-embedding-ada-002", _options.ApiKey)//c.AddAzureTextEmbeddingGenerationService(_chatOptions.Models[4], azureEndpoint, apiKey);    // Azureはこっち。
                .WithMemoryStorage(new VolatileMemoryStore())
                // 最初に登録されたやつがデフォルトになる
                // OpenAI
                .WithOpenAIChatCompletionService("gpt-3.5-turbo", _options.ApiKey, serviceId: ModelType.OpenAIGpt35Turbo.ToString())
                .WithOpenAIChatCompletionService("gpt-4", _options.ApiKey, serviceId: ModelType.OpenAIGpt4.ToString())
                .WithOpenAIChatCompletionService("gpt-4-0314", _options.ApiKey, serviceId: ModelType.OpenAIGpt40314.ToString());

            // Azure
            for (int i = 0; i < _chatOptions.Models.Length; i++)
            {
                if (azureModels.Length > i)
                {
                    if (azureModels[i] == ModelType.AzureCode)
                    {
                        // Azure：テキスト補間用
                        builder = builder.WithAzureTextCompletionService(
                            _chatOptions.Models[i],
                            _options.AzureUri,
                            _options.AzureApiKey,  // new AzureCliCredential()を使っても良い
                            serviceId: azureModels[i].ToString());
                    }
                    else
                    {
                        // Azure：チャット用
                        builder = builder.WithAzureChatCompletionService(
                            _chatOptions.Models[i],
                            _options.AzureUri,
                            _options.AzureApiKey,  // new AzureCliCredential()を使っても良い
                            serviceId: azureModels[i].ToString());
                    }
                }
            }

            //_kernel.Config.SetDefaultTextCompletionService(ModelType.OpenAIGpt35Turbo.ToString());  // 指定がない場合はOpenAIの3.5を使用する。

            _kernel = builder.Build();
        }

    }

}

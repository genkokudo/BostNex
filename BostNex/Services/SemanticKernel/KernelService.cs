using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.SemanticFunctions;

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

        ///// <summary>
        ///// チャット用のカーネルを取得
        ///// スキル無し
        ///// プールする。基本的にこっちを使う。
        ///// </summary>
        ///// <returns></returns>
        //public IKernel GetChatKernel(ModelType type);
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
        /// チャット用のカーネル
        /// スキル無し
        /// チャットではモデル指定できないので代わりに辞書にする
        /// </summary>
        private static readonly Dictionary<ModelType, IKernel> _chatKernels = new();

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
            _kernel = Microsoft.SemanticKernel.Kernel.Builder.Build();

            // 最初に登録されたやつがデフォルトになる
            // OpenAI
            _kernel.Config.AddOpenAIChatCompletionService("gpt-3.5-turbo", _options.ApiKey, serviceId: ModelType.OpenAIGpt35Turbo.ToString());
            _kernel.Config.AddOpenAIChatCompletionService("gpt-4", _options.ApiKey, serviceId: ModelType.OpenAIGpt4.ToString());
            _kernel.Config.AddOpenAIChatCompletionService("gpt-4-0314", _options.ApiKey, serviceId: ModelType.OpenAIGpt40314.ToString());

            // Azure
            var azureModels = new ModelType[] { ModelType.Azure35, ModelType.Azure4, ModelType.Azure432k, ModelType.AzureCode };
            for (int i = 0; i < _chatOptions.Models.Length; i++)
            {
                if (azureModels.Length < i)
                {
                    if (azureModels[i] == ModelType.AzureCode)
                    {
                        // Azure：テキスト補間用
                        _kernel.Config.AddAzureTextCompletionService(
                            _chatOptions.Models[i],
                            _options.AzureUri,
                            _options.AzureApiKey,  // new AzureCliCredential()を使っても良い
                            serviceId: azureModels[i].ToString());
                    }
                    else
                    {
                        // Azure：チャット用
                        _kernel.Config.AddAzureChatCompletionService(       // なんか、このメソッドはAddAzureTextCompletionServiceも兼ねているっぽい
                            _chatOptions.Models[i],
                            _options.AzureUri,
                            _options.AzureApiKey,  // new AzureCliCredential()を使っても良い
                            serviceId: azureModels[i].ToString());
                    }
                }
            }

            _kernel.Config.SetDefaultTextCompletionService(ModelType.OpenAIGpt35Turbo.ToString());  // 指定がない場合はOpenAIの3.5を使用する。
        }

        ///// <summary>
        ///// チャット用のカーネルを作成
        ///// スキル無し
        ///// プールしていないので、返り値のIKernelは保持しておくこと
        ///// </summary>
        ///// <returns></returns>
        //private IKernel CreateChatKernel(ModelType type)
        //{
        //    var kernel = Kernel.Builder.Build();
        //    switch (type)
        //    {
        //        case ModelType.OpenAIGpt35Turbo:
        //            kernel.Config.AddOpenAIChatCompletionService("gpt-3.5-turbo", _options.ApiKey, serviceId: type.ToString());
        //            break;
        //        case ModelType.OpenAIGpt4:
        //            kernel.Config.AddOpenAIChatCompletionService("gpt-4", _options.ApiKey, serviceId: type.ToString());
        //            break;
        //        case ModelType.OpenAIGpt40314:
        //            kernel.Config.AddOpenAIChatCompletionService("gpt-4-0314", _options.ApiKey, serviceId: type.ToString());
        //            break;
        //        case ModelType.Azure35:
        //            if (_chatOptions.Models.Length <= 0)
        //            {
        //                return null!;
        //            }
        //            kernel.Config.AddAzureChatCompletionService(_chatOptions.Models[0], _options.AzureUri, _options.AzureApiKey, serviceId: type.ToString());
        //            break;
        //        case ModelType.Azure4:
        //            if (_chatOptions.Models.Length <= 1)
        //            {
        //                return null!;
        //            }
        //            kernel.Config.AddAzureChatCompletionService(_chatOptions.Models[1], _options.AzureUri, _options.AzureApiKey, serviceId: type.ToString());
        //            break;
        //        case ModelType.Azure432k:
        //            if (_chatOptions.Models.Length <= 2)
        //            {
        //                return null!;
        //            }
        //            kernel.Config.AddAzureChatCompletionService(_chatOptions.Models[2], _options.AzureUri, _options.AzureApiKey, serviceId: type.ToString());
        //            break;
        //        case ModelType.AzureCode:
        //            if (_chatOptions.Models.Length <= 3)
        //            {
        //                return null!;
        //            }   // 多分、AddAzureTextEmbeddingGenerationServiceが正解。
        //            kernel.Config.AddAzureChatCompletionService(_chatOptions.Models[3], _options.AzureUri, _options.AzureApiKey, serviceId: type.ToString());
        //            break;
        //        default:
        //            break;
        //    }
        //    return kernel;
        //}

        //public IKernel GetChatKernel(ModelType type)
        //{
        //    _chatKernels.TryGetValue(type, out var kernel);
        //    if (kernel is null)
        //    {
        //        kernel = CreateChatKernel(type);
        //        _chatKernels.Add(type, kernel);
        //    }
        //    return kernel;
        //}
    }

}

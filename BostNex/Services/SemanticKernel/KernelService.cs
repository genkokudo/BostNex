using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

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
        /// チャットはモデルが選べないのでGetChatKernelを使用すること
        /// </summary>
        public IKernel Kernel { get; }

        /// <summary>
        /// チャット用のカーネルを取得
        /// スキル無し
        /// プールする。基本的にこっちを使う。
        /// </summary>
        /// <returns></returns>
        public IKernel GetChatKernel(ModelType type);
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

        public KernelService(IOptions<ChatServiceOption> options, IOptions<ChatOption> chatOptions, ISkillService skill)
        {
            _options = options.Value;
            _chatOptions = chatOptions.Value;
            InitializeKernel();
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
            // カーネルを作成、複数登録できる
            // OpenAI
            _kernel.Config.AddOpenAIChatCompletionService(ModelType.OpenAIGpt35Turbo.ToString(), "gpt-3.5-turbo", _options.ApiKey);
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

        /// <summary>
        /// チャット用のカーネルを作成
        /// スキル無し
        /// プールしていないので、返り値のIKernelは保持しておくこと
        /// </summary>
        /// <returns></returns>
        private IKernel CreateChatKernel(ModelType type)
        {
            var kernel = Microsoft.SemanticKernel.Kernel.Builder.Build();
            switch (type)
            {
                case ModelType.OpenAIGpt35Turbo:
                    kernel.Config.AddOpenAIChatCompletionService(type.ToString(), "gpt-3.5-turbo", _options.ApiKey);
                    break;
                case ModelType.OpenAIGpt4:
                    kernel.Config.AddOpenAIChatCompletionService(type.ToString(), "gpt-4", _options.ApiKey);
                    break;
                case ModelType.OpenAIGpt40314:
                    kernel.Config.AddOpenAIChatCompletionService(type.ToString(), "gpt-4-0314", _options.ApiKey);
                    break;
                case ModelType.Azure35:
                    if (_chatOptions.Models.Length <= 0)
                    {
                        return null!;
                    }
                    kernel.Config.AddOpenAIChatCompletionService(type.ToString(), _chatOptions.Models[0], _options.AzureUri, _options.AzureApiKey);
                    break;
                case ModelType.Azure4:
                    if (_chatOptions.Models.Length <= 1)
                    {
                        return null!;
                    }
                    kernel.Config.AddOpenAIChatCompletionService(type.ToString(), _chatOptions.Models[1], _options.AzureUri, _options.AzureApiKey);
                    break;
                case ModelType.Azure432k:
                    if (_chatOptions.Models.Length <= 2)
                    {
                        return null!;
                    }
                    kernel.Config.AddOpenAIChatCompletionService(type.ToString(), _chatOptions.Models[2], _options.AzureUri, _options.AzureApiKey);
                    break;
                case ModelType.AzureCode:
                    if (_chatOptions.Models.Length <= 3)
                    {
                        return null!;
                    }
                    kernel.Config.AddOpenAIChatCompletionService(type.ToString(), _chatOptions.Models[3], _options.AzureUri, _options.AzureApiKey);
                    break;
                default:
                    break;
            }
            return kernel;
        }

        public IKernel GetChatKernel(ModelType type)
        {
            _chatKernels.TryGetValue(type, out var kernel);
            if (kernel is null)
            {
                kernel = CreateChatKernel(type);
                _chatKernels.Add(type, kernel);
            }
            return kernel;
        }
    }

}

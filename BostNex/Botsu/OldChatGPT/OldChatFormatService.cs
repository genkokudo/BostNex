﻿//using Azure.AI.OpenAI;
//using BostNex.Services.SemanticKernel;
//using Microsoft.CodeAnalysis;
//using Microsoft.Extensions.Options;

//namespace BostNex.Services
//{
//    /// <summary>
//    /// チャットで使う画面とデータを管理する
//    /// </summary>
//    public interface IOldChatFormatService
//    {
//        public Dictionary<string, Display> PageData { get; }
//        public bool IsDebugMode { get; }

//        /// <summary>
//        /// 有効なページの一覧を取得する
//        /// </summary>
//        /// <returns></returns>
//        public List<Display> GetKeys();
//    }

//    public class OldChatFormatService : IOldChatFormatService
//    {
//        private readonly OldChatOption _options;
//        private readonly IOldChatPromptService _chat;

//        public Dictionary<string, Display> PageData => _pageData;
//        public Dictionary<string, Display> _pageData = new();
//        public bool IsDebugMode => _options.IsLocalDevelopMode;

//        public OldChatFormatService(IOldChatPromptService chat, IOptions<OldChatOption> options)
//        {
//            _chat = chat;
//            _options = options.Value;

//            // 追加していくこと
//            _pageData.Add("Openai3", new Display
//            {
//                Address = "Openai3",
//                Title = "OpenAI:gpt-3.5-turbo",
//                Headline = "OpenAI:gpt-3.5-turbo",
//                Introduction = "安い。基本的にこれを使うこと。\n$0.002 / 1K tokens",
//                Placeholder = "あなたの質問",
//                MasterPrompt = _chat.Chat["DefaultPrompt"],
//                GptModel = "gpt-3.5-turbo",
//                IsPublic = false
//            });
//            _pageData.Add("Openai4", new Display
//            {
//                Address = "Openai4",
//                Title = "OpenAI:gpt-4",
//                Headline = "OpenAI:gpt-4",
//                Introduction = "高い。基本的に使わないこと。\n8K context, プロンプト: $0.03 / 1K tokens, 補完: $0.06 / 1K tokens\n32K context, プロンプト: $0.06 / 1K tokens, 補完: $0.12 / 1K tokens",
//                Placeholder = "あなたの質問",
//                MasterPrompt = _chat.Chat["DefaultPrompt"],
//                GptModel = "gpt-4", // gpt-4, gpt-4-0314, gpt-4-32k, gpt-4-32k-0314（32kはまだ非公開）
//                IsPublic = false,
//                MaxTokens = 2048
//            });
//            _pageData.Add("Openai4-0314", new Display
//            {
//                Address = "Openai4-0314",
//                Title = "OpenAI:gpt-4-0314",
//                Headline = "OpenAI:gpt-4-0314",
//                Introduction = "高い。基本的に使わないこと。\n8K context, プロンプト: $0.03 / 1K tokens, 補完: $0.06 / 1K tokens\n32K context, プロンプト: $0.06 / 1K tokens, 補完: $0.12 / 1K tokens",
//                Placeholder = "あなたの質問",
//                MasterPrompt = _chat.Chat["DefaultPrompt"],
//                GptModel = "gpt-4-0314",
//                IsPublic = false,
//                MaxTokens = 2048
//            });
//            _pageData.Add("Azure3", new Display
//            {
//                Address = "Azure3",
//                Title = "Azure:gpt-35-turbo",
//                Headline = "Azure OpenAI Service:gpt-35-turbo",
//                Introduction = "会社用1。\n8K context, $0.002 / 1K tokens",
//                Placeholder = "あなたの質問",
//                MasterPrompt = _chat.Chat["DefaultPrompt"],
//                UseAzureOpenAI = true,
//                GptModel = options.Value.Models[0],
//                IsPublic = false,
//                MaxTokens = 2048
//            });
//            //_pageData.Add("AzureCode2", new Display
//            //{
//            //    Address = "AzureCode2",
//            //    Title = "Azure:code-davinci-002",
//            //    Headline = "Azure OpenAI Service:code-davinci-002",
//            //    Introduction = "会社用2、コード生成用だけど割高。他とはUIが違うので要書き換え",
//            //    Placeholder = "あなたの質問",
//            //    MasterPrompt = _chat.DefaultPrompt,
//            //    UseAzureOpenAI = true,
//            //    GptModel = options.Value.Models[3],
//            //    IsPublic = false,
//            //    IsCode = true,
//            //    MaxTokens = 2048
//            //});
//            _pageData.Add("Azure4", new Display
//            {
//                Address = "Azure4",
//                Title = "Azure:gpt-4",
//                Headline = "Azure OpenAI Service:gpt-4",
//                Introduction = "会社用2、高い。\n8K context, プロンプト: $0.03 / 1K tokens, 補完: $0.06 / 1K tokens",
//                Placeholder = "あなたの質問",
//                MasterPrompt = _chat.Chat["DefaultPrompt"],
//                UseAzureOpenAI = true,
//                GptModel = options.Value.Models[1],
//                IsPublic = false,
//                MaxTokens = 2048
//            });
//            _pageData.Add("Azure4-32k", new Display
//            {
//                Address = "Azure4-32k",
//                Title = "Azure:gpt-4-32k",
//                Headline = "Azure OpenAI Service:gpt-4",
//                Introduction = "会社用3、高い。\n32K context, プロンプト: $0.06 / 1K tokens, 補完: $0.12 / 1K tokens",
//                Placeholder = "あなたの質問",
//                MasterPrompt = _chat.Chat["DefaultPrompt"],
//                UseAzureOpenAI = true,
//                GptModel = options.Value.Models[2],
//                IsPublic = false,
//                MaxTokens = 2048
//            });

//            _pageData.Add("Ojisan", new Display
//            {
//                Address = "Ojisan",
//                Title = "おぢさん",
//                Headline = "おぢさんとお話しよう！",
//                Introduction = "おぢさんとお話できます。",
//                Placeholder = "喋ってみよう！",
//                SubmitText = "喋ってみる",
//                MasterPrompt = _chat.Chat["Ojisan"],
//                Options = new List<DisplayOption> { new DisplayOption() },   // 名前入力
//                IsPublic = true
//            });
//            _pageData.Add("Tsunko", new Display
//            {
//                MasterPrompt = _chat.Chat["Tsunko"],
//                Title = "ツン子",
//                Address = "Tsunko",
//                Headline = "ツン子とお話しよう！"
//            });
//            _pageData.Add("DonpenKarma", new Display
//            {
//                Address = "DonpenKarma",
//                Title = "魔王（兄）",
//                Headline = "魔王（兄）",
//                Introduction = "人間を鹿金することを目指す冷酷非情な魔王。\r\n戦って勝利を目指すか、服従して一緒に世界を征服してください。\r\n名前は「ドンペン・カルマ」です。\r\n容姿設定はありません。（他のキャラもそう）",
//                Placeholder = "ここがあの魔王のHouseね",
//                SubmitText = "発言する",
//                MasterPrompt = _chat.Chat["DonpenKarma"],
//                IsPublic = true
//            });
//            _pageData.Add("PrankDaemon", new Display
//            {
//                Address = "PrankDaemon",
//                Title = "魔王（弟）",
//                Headline = "魔王（弟）",
//                Introduction = "人間を玩具にすることを目指す好戦的な魔王。\r\n戦いを挑んでルールを説明すると、その通りに勝負してくれます。\r\n名前は「プランク・デーモン」です。\r\nよく主語を間違えるのでスルーしてください。",
//                Placeholder = "勝負を挑もう！",
//                SubmitText = "闘う",
//                MasterPrompt = _chat.Chat["PrankDaemon"],
//                IsPublic = true
//            });
//            _pageData.Add("FemaleOverLoad", new Display
//            {
//                Address = "FemaleOverLoad",
//                Title = "魔王♀",
//                Headline = "魔王♀",
//                Introduction = "500年間封印され続けた古の大魔王。今その封印が解かれようとしている！？\r\n名前は「翠闇（すいあん）」です。\r\n封印を解いてあげるとかしてください。",
//                Placeholder = "女魔王とお話する",
//                SubmitText = "発言する",
//                MasterPrompt = _chat.Chat["FemaleOverLoad"],
//                IsPublic = true
//            });
//            _pageData.Add("Yanko", new Display
//            {
//                Address = "Yanko",
//                Title = "ヤン子",
//                Headline = "ヤン子",
//                Introduction = "ヤ、ヤン子…",
//                Placeholder = "ヤン子とお話する",
//                MasterPrompt = _chat.Chat["Yanko"],
//                IsPublic = true
//            });
//            _pageData.Add("Geed", new Display
//            {
//                Address = "Geed",
//                Title = "獣人♂",
//                Headline = "獣人♂",
//                Introduction = "人間は獣人に服従すべき脆弱な種族だ。\r\n名前は「ジード」です。\r\n種族は決まってないので、()を使ってト書きをするとその通りになってくれるはずです。\r\n喧嘩したり冒険したりしてください。",
//                Placeholder = "ジードとお話する",
//                MasterPrompt = _chat.Chat["Geed"],
//                IsPublic = true
//            });
//            _pageData.Add("Giant", new Display
//            {
//                Address = "Giant",
//                Title = "巨人♂",
//                Headline = "巨人♂",
//                Introduction = "巨人に支配された街で生き残ろう。\r\n怪しい男の名前は「オウガ」です。\r\n巨人は弱点がありますが、普通の方法ではなかなか勝てません。",
//                Placeholder = "こいつ怪しいなあ",
//                MasterPrompt = _chat.Chat["Giant"],
//                IsPublic = true
//            });
//            _pageData.Add("Villain", new Display
//            {
//                Address = "Villain",
//                Title = "怪人♂",
//                Headline = "怪人♂",
//                Introduction = "怪人の名前は「ザッハーク」です。\r\n怪人は周囲にある動植物を吸収して能力を手に入れます。\r\nあなたは必殺技や武器で怪人を倒してください。",
//                Placeholder = "怪人め、許さないぞ",
//                SubmitText = "闘う",
//                MasterPrompt = _chat.Chat["Villain"],
//                IsPublic = true
//            });
//            _pageData.Add("Geed2", new Display
//            {
//                Address = "Geed2",
//                Title = "獣人2♂",
//                Headline = "獣人♂特別編",
//                Introduction = "人間は獣人に服従すべき脆弱な種族だ。",
//                Placeholder = "ジードとお話する",
//                MasterPrompt = _chat.Chat["Geed2"],
//                Temperature = 0.9f,
//                PresencePenalty = 1.5f,
//                IsPublic = false
//            });
//            _pageData.Add("Giant2", new Display
//            {
//                Address = "Giant2",
//                Title = "巨人2♂",
//                Headline = "巨人♂特別編",
//                Introduction = "巨人に支配された街で生き残ろう。",
//                Placeholder = "こいつ怪しいなあ",
//                MasterPrompt = _chat.Chat["Giant2"],
//                Temperature = 0.9f,
//                PresencePenalty = 1.5f,
//                IsPublic = false
//            });

//            // 開発モードの場合
//            if (!_options.IsLocalDevelopMode)
//            {
//                // _pageDataからIsPublicではないものを除外する
//                _pageData = _pageData.Where(x => x.Value.IsPublic).ToDictionary(x => x.Key, x => x.Value);
//            }

//            _pageData.Add(DarkMagicFunction.Summarize.ToString(), new Display
//            {
//                Address = DarkMagicFunction.Summarize.ToString(),
//                Title = "要約",
//                Headline = "要約",
//                Introduction = "入力した文章を要約してくれます。",
//                Placeholder = "要約したい文章を入力",
//                Temperature = 0.2f,
//                IsPublic = false,
//                SemanticFunctionName = DarkMagicFunction.Summarize.ToString()
//            });
//        }

//        public List<Display> GetKeys()
//        {
//            if (!_options.IsLocalDevelopMode)
//            {
//                // IsPublic = falseのものは除外する
//                return _pageData.Where(x => x.Value.IsPublic).Select(x => x.Value).ToList();
//            }
//            return _pageData.Select(x => x.Value).ToList();
//        }
//    }

//    /// <summary>
//    /// 画面データ
//    /// </summary>
//    public class Display
//    {
//        /// <summary>アドレス</summary>
//        public string Address { get; set; } = "Tsunko";
//        /// <summary>NavMenuに表示するタイトル</summary>
//        public string Title { get; set; } = "ツン子";
//        public string Headline { get; set; } = "ツン子とお話しよう！";
//        public string Introduction { get; set; } = "ツン子という少女とお話できます。";
//        public string Placeholder { get; set; } = "喋ってみよう！";
//        /// <summary>送信ボタン</summary>
//        public string SubmitText { get; set; } = "送信";

//        /// <summary>これがfalseの場合、"IsLocalDevelopMode": falseならば表示しない</summary>
//        public bool IsPublic { get; set; } = true;

//        /// <summary>SemanticKernelで作成したスキルである。</summary>
//        public bool IsSemanticKernel => !string.IsNullOrWhiteSpace(SemanticFunctionName);
//        /// <summary>SemanticKernelの関数名</summary>
//        public string SemanticFunctionName { get; set; } = "";

//        /// <summary>ユーザに入力させる項目</summary>
//        public List<DisplayOption> Options { get; set; } = new();

//        /// <summary>
//        /// ChatServiceから取ってきたプロンプト。
//        /// マスター扱い。上書きとかしないこと（開発モードは別）
//        /// </summary>
//        public List<ChatMessage> MasterPrompt { get; set; } = new();

//        /// <summary>
//        /// 実際に画面で使用するプロンプト
//        /// MasterPromptにOptionsを適用した物。
//        /// </summary>
//        public List<ChatMessage> CurrentPrompt { get; set; } = new();

//        /// <summary>
//        /// AzureOpenAIを使う
//        /// falseの時は本家のAPIを使う。
//        /// </summary>
//        public bool UseAzureOpenAI { get; set; } = false;

//        /// <summary>
//        /// GPTのモデル
//        /// OpenAI:gpt-3.5-turbo, gpt-4, gpt-4-0314
//        /// MS:gpt-35-turbo, code-davinci-002, gpt-4, gpt-4-32k（デプロイ名で指定する必要があるので、名前はノートを見ること。）
//        /// </summary>
//        public string GptModel { get; set; } = "gpt-3.5-turbo";

//        /// <summary>
//        /// code-davinci等はこっち
//        /// 補間系の画面処理にする（未実装）
//        /// </summary>
//        public bool IsCode { get; set; } = false;

//        #region OpenAIのパラメータ

//        /// <summary>
//        /// デフォルトは1。サンプリング温度は0～2の間で指定します。
//        /// 0.8のような高い値は出力をよりランダムにし、0.2のような低い値は出力をより集中させて、決定論的にします。
//        /// 1を超えると壊れる可能性があるというか、全く使い物にならない。
//        /// </summary>
//        public float Temperature { get; set; } = 1.0f;

//        /// <summary>
//        /// デフォルトは0。-2.0から2.0の間の数値。
//        /// 正の値は、新しいトークンがこれまでのテキストに出現したトークンを繰り返すとペナルティを与え、新しいトピックについて話す可能性を高めます。
//        /// </summary>
//        public float PresencePenalty { get; set; } = 0f;

//        /// <summary>
//        /// 返答のトークン上限を設定
//        /// 主にトークン数を節約するため。
//        /// </summary>
//        public int MaxTokens { get; set; } = 280;

//        #endregion 

//        /// <summary>
//        /// Option入力後に呼ぶ
//        /// CurrentPromptを新しく作り直す。
//        /// Option.Valueの内容をPromptの0番目のContentに適用する。
//        /// </summary>
//        public void ApplyOption()
//        {
//            CurrentPrompt.Clear();
//            CurrentPrompt.AddRange(MasterPrompt);

//            if (CurrentPrompt.Count == 0)
//            {
//                return;
//            }
//            var values = Options.Select(x => x.Value ?? string.Empty).ToArray();
//            var content = string.Format(CurrentPrompt[0].Content, values);
//            CurrentPrompt[0] = new ChatMessage(MasterPrompt[0].Role, content);
//        }

//        #region トークン数に関するフィールド
//        /// <summary>
//        /// トークン数カウントで使用するモデル
//        /// "gpt-3.5-turbo"か、"gpt-4"のどちらかにしておく
//        /// </summary>
//        public string GptTokenModel { get
//            {
//                _gptTokenModel = _gptTokenModel == null ? GetTokenModel(GptModel) : _gptTokenModel;
//                return _gptTokenModel;
//            } 
//        }
//        private string _gptTokenModel = null!;

//        /// <summary>
//        /// モデルのトークン数の限界値
//        /// </summary>
//        public int TokenLimitByModel
//        {
//            get
//            {
//                _tokenLimitByModel = _tokenLimitByModel < 0 ? GetTokenLimitByModel(GptModel) : _tokenLimitByModel;
//                return _tokenLimitByModel;
//            }
//        }
//        private int _tokenLimitByModel = -1;

//        /// <summary>
//        /// トークン数カウントで使用するモデルを決める
//        /// "4"が含んでたら"gpt-4"にするという適当っぷり
//        /// </summary>
//        /// <param name="model">モデル名</param>
//        /// <returns></returns>
//        private string GetTokenModel(string model)
//        {
//            var result = "gpt-3.5-turbo";
//            if (model.Contains("4"))
//            {
//                result = "gpt-4";   // むっちゃ適当
//            }
//            return result;
//        }

//        /// <summary>
//        /// モデルに対するトークン数の限界を決める
//        /// </summary>
//        /// <param name="model"></param>
//        /// <returns></returns>
//        private int GetTokenLimitByModel(string model)
//        {
//            if (model.Contains("8k"))
//            {
//                return 8192;
//            }
//            if (model.Contains("32k"))
//            {
//                return 32768;
//            }
//            return 4096;
//        }
//        #endregion

//    }

//    /// <summary>
//    /// 設定項目
//    /// </summary>
//    public class OldChatOption
//    {
//        /// <summary>
//        /// trueだと開発モードとなり非公開のプロンプトも適用される
//        /// </summary>
//        public bool IsLocalDevelopMode { get; set; } = false;

//        // 会社用
//        public string[] Models { get; set; } = null!;
//    }

//}
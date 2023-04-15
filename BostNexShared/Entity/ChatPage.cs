
namespace BostNexShared.Entity
{
    // TODO:暇なとき考えて。
    /// <summary>
    /// ページ管理
    /// </summary>
    public record ChatPage : BaseEntity
    {
        /// <summary>
        /// アドレス
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// プロンプトを辞書から引き出すキー
        /// 基本的にはアドレスと同じものを入れるつもりだけど、同一プロンプトでパラメータ違いにする場合にアドレス分けしたい
        /// </summary>
        public string? PromptKey { get; set; }

        /// <summary>
        /// メニューに表示するタイトル
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// ページのタイトル欄に表示
        /// </summary>
        public string? Headline { get; set; }

        /// <summary>
        /// ページの導入欄に表示
        /// </summary>
        public string? Introduction { get; set; }

        /// <summary>
        /// チャットの入力欄に表示
        /// </summary>
        public string? Placeholder { get; set; }

        /// <summary>
        /// チャットの送信ボタンに表示
        /// </summary>
        public string? SubmitText { get; set; }

        /// <summary>
        /// チャットを公開するか
        /// </summary>
        public bool IsPublic { get; set; }
    }


}

﻿namespace BostNexShared.Dto
{

    /// <summary>
    /// Chatのやり取り1回分
    /// </summary>
    public class ChatSession
    {
        // この辺はまだ決まっていない。いくつテキスト表示するとか。
        /// <summary>
        /// プレイヤーの行動を文章として表した入力
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Textに対する回答
        /// </summary>
        public string AiResponse { get; set; } = string.Empty;

        /// <summary>
        /// 古くなってAPI送信対象から外れていたらtrue
        /// </summary>
        public bool IsDisposed = false;
    }
}

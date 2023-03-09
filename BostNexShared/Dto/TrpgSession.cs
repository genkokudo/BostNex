﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BostNexShared.Dto
{

    /// <summary>
    /// TRPGのやり取り1回分
    /// </summary>
    public class TrpgSession
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
        /// 送信ボタンを押したらtrue
        /// （遷移した時の画面のリフレッシュに必要）
        /// 以降はこのセッションに書き込めなくする。
        /// </summary>
        public bool IsSubmitted = false;
    }
}

using System.ComponentModel.DataAnnotations;

namespace BostNexShared.Entity
{
    // TODO:暇なとき考えて。
    // 日本語化する行としない行があるとか
    // 1人称と2人称とか
    // 出来たらDbContextのコメントアウト外してからAdd-Migration AddCharactorSheet
    /// <summary>
    /// キャラシートのDB化
    /// （まだ詳しく決まっていない）
    /// </summary>
    public record CharactorSheet : BaseEntity
    {
        public string? Name { get; set; }
    }

    // 取り敢えず、画面定義やパラメータなどは1対1ではあるけどここに入れないでおく。
    // やりたいことは？
    // 日本語で入力してリスト化。
    // 1行ずつ英訳。台詞例は訳さないのでフラグを持たせる。
    // 使用する行と使用しない行の切り替えができるようにする。


    // 画面定義クラス
    // CharactorSheetのリスト
}

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
}

using System.ComponentModel.DataAnnotations;

namespace BostNexShared.Entity
{
    /// <summary>
    /// 共通の定義をここに書いちゃう
    /// </summary>
    public record BaseEntity
    {
        [Key]
        public long Id { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime UpdatedTime { get; set; }
    }
}

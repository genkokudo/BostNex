using System.ComponentModel.DataAnnotations;

namespace BostNexShared.Entity
{
    /// <summary>
    /// 汎用的なデータを格納
    /// </summary>
    public record General
    {
        [Key]
        public long Id { get; set; }

        public string? Name { get; set; }

        public string? Value { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime UpdatedTime { get; set; }
    }
}

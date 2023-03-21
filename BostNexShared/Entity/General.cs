using System.ComponentModel.DataAnnotations;

namespace BostNexShared.Entity
{
    /// <summary>
    /// 汎用的なデータを格納
    /// </summary>
    public record General : BaseEntity
    {
        public string? Name { get; set; }

        public string? Value { get; set; }
    }
}

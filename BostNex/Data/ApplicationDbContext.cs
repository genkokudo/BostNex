using BostNexShared.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BostNex.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        /// <summary>汎用的データ</summary>
        public DbSet<General> Generals { get; set; }

        ///// <summary>キャラシート</summary>
        //public DbSet<CharactorSheet> CharactorSheets { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
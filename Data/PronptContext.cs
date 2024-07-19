using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // 追記
using Microsoft.EntityFrameworkCore;                     // 追記
using PronptModel_ver2.Models;                           // 追記

namespace PronptModel_ver2.Data
{
    public class PronptContext : IdentityDbContext<StudentUser>
    {
        public PronptContext(DbContextOptions<PronptContext> contextOptions)
            : base(contextOptions)      
        { }
        public DbSet<Day> Days => Set<Day>();
    }
}

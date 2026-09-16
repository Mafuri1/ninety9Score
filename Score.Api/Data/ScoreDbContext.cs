using Microsoft.EntityFrameworkCore;
using Score.Api.Models;

namespace Score.Api.Data
{
    public class ScoreDbContext : DbContext
    {
        public ScoreDbContext(DbContextOptions<ScoreDbContext> options)
            : base(options)
        {
        }

        public DbSet<ScoreEntity> Scores { get; set; }
    }
}
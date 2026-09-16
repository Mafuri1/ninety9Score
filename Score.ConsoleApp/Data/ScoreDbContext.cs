using Microsoft.EntityFrameworkCore;
using Score.ConsoleApp.Models;

namespace Score.ConsoleApp.Data
{
    public class ScoreDbContext : DbContext
    {
        public ScoreDbContext(DbContextOptions<ScoreDbContext> options)
            : base(options)
        {
        }

        public DbSet<Enitity> Scores { get; set; }
    }
}
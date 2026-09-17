using Microsoft.EntityFrameworkCore;
using Score.Core.Models;

namespace Score.Infrastructure.Data;

public sealed class ScoreDbContext : DbContext
{
    public ScoreDbContext(DbContextOptions<ScoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<ScoreRecord> Scores => Set<ScoreRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScoreRecord>(entity =>
        {
            entity.ToTable("Scores");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.Property(x => x.FirstName).IsRequired();
            entity.Property(x => x.SecondName).IsRequired();
            entity.Property(x => x.Score).IsRequired();
        });
    }
}

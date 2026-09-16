using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Score.ConsoleApp.Data
{
    public class ScoreDbContextFactory
        : IDesignTimeDbContextFactory<ScoreDbContext>
    {
        public ScoreDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                @"Data Source=MAFURY;Initial Catalog=RichCravings;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30";

            var optionsBuilder =
                new DbContextOptionsBuilder<ScoreDbContext>();

            optionsBuilder.UseSqlServer(connectionString);

            return new ScoreDbContext(optionsBuilder.Options);
        }
    }
}
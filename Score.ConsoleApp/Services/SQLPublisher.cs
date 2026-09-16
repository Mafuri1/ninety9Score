using Microsoft.EntityFrameworkCore;
using Score.ConsoleApp.Data;
using Score.ConsoleApp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Score.ConsoleApp.Services
{
    public class SQLPublisher
    {
        public static async Task publishScoreAsync(string connectionString, List<Enitity> entity)
        {

            // -------------------------
            // Configure EF Core
            // -------------------------

            var options = new DbContextOptionsBuilder<ScoreDbContext>()
                .UseSqlServer(connectionString)
                .Options;


            // -------------------------
            // Save to SQL
            // -------------------------

            using var dbContext = new ScoreDbContext(options);

            await dbContext.Scores.AddRangeAsync(entity);

            int recordsSaved = await dbContext.SaveChangesAsync();

            Console.WriteLine();
            Console.WriteLine($"{recordsSaved} records successfully saved to database.");

        }
    }
}

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Score.Core.Models;
using Score.Infrastructure.Data;
using Score.Infrastructure.Repositories;
using Xunit;

namespace Score.Tests.Infrastructure;

public sealed class EfScoreRepositoryTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private ScoreDbContext _dbContext = null!;
    private EfScoreRepository _repository = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ScoreDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ScoreDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        _repository = new EfScoreRepository(_dbContext);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task GetTopScorersAsync_ReturnsAllTiesAlphabetically()
    {
        await _repository.AddRangeAsync([
            new ScoreRecord { FirstName = "Sipho", SecondName = "Lolo", Score = 78 },
            new ScoreRecord { FirstName = "George", SecondName = "Of The Jungle", Score = 78 },
            new ScoreRecord { FirstName = "Dee", SecondName = "Moore", Score = 56 }
        ]);

        var result = await _repository.GetTopScorersAsync();

        Assert.Equal(78, result.Score);
        Assert.Collection(
            result.Scorers,
            x => Assert.Equal("George", x.FirstName),
            x => Assert.Equal("Sipho", x.FirstName));
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsMatchingPerson()
    {
        await _repository.AddAsync(
            new ScoreRecord { FirstName = "George", SecondName = "Of The Jungle", Score = 78 });

        var result = await _repository.GetByNameAsync("George", "Of The Jungle");

        var record = Assert.Single(result);
        Assert.Equal(78, record.Score);
    }
}

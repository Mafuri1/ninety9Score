using Microsoft.EntityFrameworkCore;
using Score.Core.Models;
using Score.Core.Repositories;
using Score.Infrastructure.Data;

namespace Score.Infrastructure.Repositories;

public sealed class EfScoreRepository : IScoreRepository
{
    private readonly ScoreDbContext _dbContext;

    public EfScoreRepository(ScoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ScoreRecord> AddAsync(
        ScoreRecord record,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        await _dbContext.Scores.AddAsync(record, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return record;
    }

    public async Task AddRangeAsync(
        IEnumerable<ScoreRecord> records,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(records);

        await _dbContext.Scores.AddRangeAsync(records, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScoreRecord>> GetByNameAsync(
        string firstName,
        string secondName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(secondName))
            throw new ArgumentException("Second name is required.", nameof(secondName));

        return await _dbContext.Scores
            .AsNoTracking()
            .Where(x => x.FirstName == firstName && x.SecondName == secondName)
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.SecondName)
            .ToListAsync(cancellationToken);
    }

    public async Task<TopScorersResult> GetTopScorersAsync(
        CancellationToken cancellationToken = default)
    {
        var highestScore = await _dbContext.Scores
            .MaxAsync(x => (int?)x.Score, cancellationToken);

        if (highestScore is null)
        {
            return new TopScorersResult();
        }

        var scorers = await _dbContext.Scores
            .AsNoTracking()
            .Where(x => x.Score == highestScore.Value)
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.SecondName)
            .ToListAsync(cancellationToken);

        return new TopScorersResult
        {
            Score = highestScore.Value,
            Scorers = scorers
        };
    }
}

using Score.Core.Models;

namespace Score.Core.Repositories;

public interface IScoreRepository
{
    Task<ScoreRecord> AddAsync(
        ScoreRecord record,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<ScoreRecord> records,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScoreRecord>> GetByNameAsync(
        string firstName,
        string secondName,
        CancellationToken cancellationToken = default);

    Task<TopScorersResult> GetTopScorersAsync(
        CancellationToken cancellationToken = default);
}

namespace Score.Core.Models;

public sealed class TopScorersResult
{
    public int? Score { get; init; }
    public IReadOnlyList<ScoreRecord> Scorers { get; init; } = Array.Empty<ScoreRecord>();

    public static TopScorersResult From(IEnumerable<ScoreRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        var materialized = records.ToList();
        if (materialized.Count == 0)
        {
            return new TopScorersResult();
        }

        var highestScore = materialized.Max(x => x.Score);
        var scorers = materialized
            .Where(x => x.Score == highestScore)
            .OrderBy(x => x.FirstName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.SecondName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new TopScorersResult
        {
            Score = highestScore,
            Scorers = scorers
        };
    }
}

namespace Score.Api.Dtos;

public sealed class TopScorersResponse
{
    public int? Score { get; init; }
    public IReadOnlyList<ScoreResponse> Scorers { get; init; } = Array.Empty<ScoreResponse>();
}

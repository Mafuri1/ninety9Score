namespace Score.Api.Dtos;

public sealed class ScoreResponse
{
    public int Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string SecondName { get; init; } = string.Empty;
    public int Score { get; init; }
}

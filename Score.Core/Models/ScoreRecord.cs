namespace Score.Core.Models;

public sealed class ScoreRecord
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string SecondName { get; set; } = string.Empty;
    public int Score { get; set; }
}

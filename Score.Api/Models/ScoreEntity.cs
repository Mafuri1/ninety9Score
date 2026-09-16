namespace Score.Api.Models
{
    public class ScoreEntity
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string SecondName { get; set; } = string.Empty;

        public int Score { get; set; }
    }
}
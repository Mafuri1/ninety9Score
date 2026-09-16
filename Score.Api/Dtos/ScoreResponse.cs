namespace Score.Api.Dtos
{
    public class ScoreResponse
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string SecondName { get; set; } = string.Empty;

        public int Score { get; set; }
    }
}
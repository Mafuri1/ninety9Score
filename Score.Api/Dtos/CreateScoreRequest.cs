using System.ComponentModel.DataAnnotations;

namespace Score.Api.Dtos
{
    public class CreateScoreRequest
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string SecondName { get; set; } = string.Empty;

        [Range(0, 100)]
        public int Score { get; set; }
    }
}
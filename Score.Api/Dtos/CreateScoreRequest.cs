using System.ComponentModel.DataAnnotations;

namespace Score.Api.Dtos;

public sealed class CreateScoreRequest
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string SecondName { get; set; } = string.Empty;

    public int Score { get; set; }
}

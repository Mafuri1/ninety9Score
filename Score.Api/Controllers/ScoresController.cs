using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Score.Api.Dtos;
using Score.Core.Models;
using Score.Core.Repositories;

namespace Score.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ScoresController : ControllerBase
{
    private readonly IScoreRepository _repository;

    public ScoresController(IScoreRepository repository)
    {
        _repository = repository;
    }

    /// <summary>Creates a new score record.</summary>
    [HttpPost]
    [Authorize(Policy = "WriteScores")]
    [ProducesResponseType(typeof(ScoreResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ScoreResponse>> Create(
        CreateScoreRequest request,
        CancellationToken cancellationToken)
    {
        var record = new ScoreRecord
        {
            FirstName = request.FirstName.Trim(),
            SecondName = request.SecondName.Trim(),
            Score = request.Score
        };

        await _repository.AddAsync(record, cancellationToken);

        var response = Map(record);
        var location = Url.ActionLink(
            nameof(GetByName),
            values: new { firstName = record.FirstName, secondName = record.SecondName });

        return Created(location ?? $"/api/scores/by-name?firstName={Uri.EscapeDataString(record.FirstName)}&secondName={Uri.EscapeDataString(record.SecondName)}", response);
    }

    /// <summary>Retrieves score records for a specific first-name and second-name pair.</summary>
    [HttpGet("by-name")]
    [ProducesResponseType(typeof(IReadOnlyList<ScoreResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ScoreResponse>>> GetByName(
        [FromQuery] string firstName,
        [FromQuery] string secondName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(secondName))
        {
            return BadRequest("Both firstName and secondName are required.");
        }

        var records = await _repository.GetByNameAsync(
            firstName.Trim(),
            secondName.Trim(),
            cancellationToken);

        if (records.Count == 0)
        {
            return NotFound();
        }

        return Ok(records.Select(Map).ToList());
    }

    /// <summary>Retrieves all scorers sharing the highest score, ordered alphabetically.</summary>
    [HttpGet("top")]
    [ProducesResponseType(typeof(TopScorersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TopScorersResponse>> GetTop(
        CancellationToken cancellationToken)
    {
        var result = await _repository.GetTopScorersAsync(cancellationToken);

        return Ok(new TopScorersResponse
        {
            Score = result.Score,
            Scorers = result.Scorers.Select(Map).ToList()
        });
    }

    private static ScoreResponse Map(ScoreRecord record) => new()
    {
        Id = record.Id,
        FirstName = record.FirstName,
        SecondName = record.SecondName,
        Score = record.Score
    };
}

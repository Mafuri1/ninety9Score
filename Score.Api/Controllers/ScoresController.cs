using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Score.Api.Data;
using Score.Api.Dtos;
using Score.Api.Models;

namespace Score.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScoresController : ControllerBase
    {
        private readonly ScoreDbContext _dbContext;

        public ScoresController(ScoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ScoreResponse>>> GetAll()
        {
            var scores = await _dbContext.Scores
                .AsNoTracking()
                .Select(x => new ScoreResponse
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    SecondName = x.SecondName,
                    Score = x.Score
                })
                .ToListAsync();

            return Ok(scores);
        }

        [HttpGet("search/{search}")]
        [Authorize]
        public async Task<ActionResult<ScoreResponse>> GetBySearchString(string search)
        {
            var score = await _dbContext.Scores
                .AsNoTracking()
                .Where(x =>
                    x.FirstName.Contains(search) ||
                    x.SecondName.Contains(search))
                .Select(x => new ScoreResponse
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    SecondName = x.SecondName,
                    Score = x.Score
                })
                .FirstOrDefaultAsync();

            if (score == null)
            {
                return NotFound();
            }

            return Ok(score);
        }
        [HttpPost]
        [Authorize(Policy = "WriteScores")]
        public async Task<ActionResult<ScoreResponse>> Create(CreateScoreRequest request)
        {
            var entity = new ScoreEntity
            {
                FirstName = request.FirstName.Trim(),
                SecondName = request.SecondName.Trim(),
                Score = request.Score
            };

            _dbContext.Scores.Add(entity);

            await _dbContext.SaveChangesAsync();

            var response = new ScoreResponse
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                SecondName = entity.SecondName,
                Score = entity.Score
            };

            return CreatedAtAction(
                nameof(GetBySearchString),
                new { id = entity.Id },
                response);
        }

        [HttpGet("highest")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ScoreResponse>>> GetHighest()
        {
            var highestScore = await _dbContext.Scores
                .MaxAsync(x => (int?)x.Score);

            if (highestScore == null)
            {
                return Ok(Array.Empty<ScoreResponse>());
            }

            var highestScorers = await _dbContext.Scores
                .AsNoTracking()
                .Where(x => x.Score == highestScore)
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.SecondName)
                .Select(x => new ScoreResponse
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    SecondName = x.SecondName,
                    Score = x.Score
                })
                .ToListAsync();

            return Ok(highestScorers);
        }

    }
}
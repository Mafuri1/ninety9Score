using System.Net;
using System.Net.Http.Json;
using Score.Api.Dtos;
using Score.Core.Models;
using Score.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Score.Tests.Api;

public sealed class ScoresApiTests : IClassFixture<ScoreApiFactory>
{
    private readonly ScoreApiFactory _factory;

    public ScoresApiTests(ScoreApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetTop_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/scores/top");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetByName_ReturnsMatchingScore()
    {
        await SeedAsync(new ScoreRecord
        {
            FirstName = "George",
            SecondName = "Of The Jungle",
            Score = 78
        });

        using var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(
            "/api/scores/by-name?firstName=George&secondName=Of%20The%20Jungle");

        response.EnsureSuccessStatusCode();
        var scores = await response.Content.ReadFromJsonAsync<List<ScoreResponse>>();

        Assert.NotNull(scores);
        Assert.Contains(scores, x => x.FirstName == "George" && x.Score == 78);
    }

    [Fact]
    public async Task Post_WithoutAdminRole_ReturnsForbidden()
    {
        using var client = CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/scores", new CreateScoreRequest
        {
            FirstName = "Thabo",
            SecondName = "Mokoena",
            Score = 82
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithAdminRole_CreatesScore()
    {
        using var client = CreateAuthenticatedClient("admin");

        var response = await client.PostAsJsonAsync("/api/scores", new CreateScoreRequest
        {
            FirstName = "Thabo",
            SecondName = "Mokoena",
            Score = 82
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private HttpClient CreateAuthenticatedClient(string? role = null)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-User", "true");

        if (!string.IsNullOrWhiteSpace(role))
        {
            client.DefaultRequestHeaders.Add("X-Test-Role", role);
        }

        return client;
    }

    private async Task SeedAsync(ScoreRecord record)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ScoreDbContext>();
        db.Scores.Add(record);
        await db.SaveChangesAsync();
    }
}

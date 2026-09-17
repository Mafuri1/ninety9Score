using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Score.Core.Models;
using Score.Core.Parsing;
using Score.Core.Repositories;
using Score.Infrastructure;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var configuredPath = configuration["CsvSettings:FilePath"];
if (string.IsNullOrWhiteSpace(configuredPath))
{
    Console.Error.WriteLine("CsvSettings:FilePath is not configured.");
    return;
}

var filePath = Path.IsPathRooted(configuredPath)
    ? configuredPath
    : Path.Combine(AppContext.BaseDirectory, configuredPath);

if (!File.Exists(filePath))
{
    Console.Error.WriteLine($"File not found: {filePath}");
    return;
}

try
{
    var csv = await File.ReadAllTextAsync(filePath);
    var parser = new ScoreCsvParser();
    var records = parser.Parse(csv);

    var result = TopScorersResult.From(records);
    if (result.Score is null)
    {
        Console.WriteLine("No data found.");
        return;
    }

    foreach (var scorer in result.Scorers)
    {
        Console.WriteLine($"{scorer.FirstName} {scorer.SecondName}");
    }

    Console.WriteLine($"Score: {result.Score}");

    var insertIntoDatabase = bool.TryParse(
        configuration["DatabaseSettings:InsertIntoDatabase"],
        out var insertEnabled) && insertEnabled;

    if (!insertIntoDatabase)
    {
        return;
    }

    var connectionString = configuration.GetConnectionString("ScoreDb");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        Console.Error.WriteLine("ConnectionStrings:ScoreDb is not configured.");
        return;
    }

    var services = new ServiceCollection();
    services.AddScoreInfrastructure(connectionString);

    await using var serviceProvider = services.BuildServiceProvider();
    await using var scope = serviceProvider.CreateAsyncScope();

    var repository = scope.ServiceProvider.GetRequiredService<IScoreRepository>();
    await repository.AddRangeAsync(records);

    Console.WriteLine($"{records.Count} record(s) saved to the database.");
}
catch (FormatException ex)
{
    Console.Error.WriteLine($"Invalid CSV: {ex.Message}");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Processing failed: {ex.Message}");
}

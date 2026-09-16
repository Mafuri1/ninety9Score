using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Score.ConsoleApp.Data;
using Score.ConsoleApp.Models;
using Score.ConsoleApp.Services;

// ------------------------------------------------------
// Load configuration
// ------------------------------------------------------

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile(
        "appsettings.json",
        optional: false,
        reloadOnChange: true)
    .Build();

// ------------------------------------------------------
// Read configuration values
// ------------------------------------------------------

var filepath =
    configuration["CsvSettings:FilePath"];

var connectionString =
    configuration["ConnectionStrings:ScoreDb"];

var insertIntoDatabaseValue =
    configuration["DatabaseSettings:InsertIntoDatabase"];

bool.TryParse(
    insertIntoDatabaseValue,
    out bool insertIntoDatabase);

//Before code execute validate if the file path exist, fail early rather than later
if (!File.Exists(filepath))
{
    Console.WriteLine($"File not found: {filepath}");
    return;
}

// Declare a list to store the csv data 
List<Enitity> entities = new List<Enitity>();

//Read all line in memory
var lines = File.ReadAllLines(filepath);

//Loop through the lines by first skinpping the header lines, split them by ',' delimeter 
// and store them into a list for further processing
foreach(var line in lines.Skip(1))
{
    if(string.IsNullOrWhiteSpace(line)) continue;

    var column = line.Split(',');

    if (column.Length < 3) continue;

    var entity = new Enitity
    {
        FirstName = column[0].Trim(),
        SecondName = column[1].Trim(),
        Score = Convert.ToInt32(column[2].Trim())
    };

    entities.Add(entity);
}

// now lets try and find the highest score
if(entities.Count == 0)
{
    Console.WriteLine("No data found.");
    return;
}

int highestScore = entities.Max(x => x.Score);

// find people with the highest score
var highestScorers = entities
    .Where(x => x.Score == highestScore)
    .OrderBy(x => x.FirstName)
    .ThenBy(x => x.SecondName)
    .ToList();

// Display final result to STDOUT
foreach (var person in highestScorers)
{
    Console.WriteLine($"{person.FirstName} {person.SecondName}");
}
Console.WriteLine($"Score: {highestScore}");

// ------------------------------------------------------
// Insert into database if enabled
// ------------------------------------------------------

if (!insertIntoDatabase)
{
    Console.WriteLine();
    Console.WriteLine(
        "Database insert is disabled in configuration.");

    return;
}


// ------------------------------------------------------
// Validate connection string
// ------------------------------------------------------

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine(
        "Database connection string has not been configured.");

    return;
}
await SQLPublisher.publishScoreAsync(connectionString, entities);
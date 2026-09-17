using Score.Core.Models;
using Score.Core.Parsing;
using Xunit;

namespace Score.Tests.Core;

public sealed class ScoreCsvParserTests
{
    private readonly ScoreCsvParser _parser = new();

    [Fact]
    public void Parse_ParsesProvidedShape()
    {
        const string csv = """
            First Name,Second Name,Score
            Dee,Moore,56
            Sipho,Lolo,78
            Noosrat,Hoosain,64
            George,Of The Jungle,78
            """;

        var records = _parser.Parse(csv);

        Assert.Equal(4, records.Count);
        Assert.Equal("George", records[3].FirstName);
        Assert.Equal(78, records[3].Score);
    }

    [Fact]
    public void Parse_SupportsQuotedCommasAndEscapedQuotes()
    {
        const string csv = "First Name,Second Name,Score\n\"George, Jr\",\"Of \"\"The\"\" Jungle\",78";

        var record = Assert.Single(_parser.Parse(csv));

        Assert.Equal("George, Jr", record.FirstName);
        Assert.Equal("Of \"The\" Jungle", record.SecondName);
        Assert.Equal(78, record.Score);
    }

    [Fact]
    public void Parse_InvalidScore_ThrowsFormatException()
    {
        const string csv = "First Name,Second Name,Score\nDee,Moore,not-a-number";

        Assert.Throws<FormatException>(() => _parser.Parse(csv));
    }

    [Fact]
    public void TopScorersResult_ReturnsTiesAlphabetically()
    {
        var result = TopScorersResult.From([
            new ScoreRecord { FirstName = "Sipho", SecondName = "Lolo", Score = 78 },
            new ScoreRecord { FirstName = "George", SecondName = "Of The Jungle", Score = 78 },
            new ScoreRecord { FirstName = "Dee", SecondName = "Moore", Score = 56 }
        ]);

        Assert.Equal(78, result.Score);
        Assert.Collection(
            result.Scorers,
            x => Assert.Equal("George", x.FirstName),
            x => Assert.Equal("Sipho", x.FirstName));
    }
}

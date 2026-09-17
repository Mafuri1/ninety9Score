using System.Text;
using Score.Core.Models;

namespace Score.Core.Parsing;

/// <summary>
/// Parses score CSV content without using a CSV parsing library.
/// Supports quoted values, commas inside quoted values, escaped double-quotes,
/// CRLF/LF line endings and blank lines.
/// </summary>
public sealed class ScoreCsvParser
{
    private static readonly string[] ExpectedHeaders = ["First Name", "Second Name", "Score"];

    public IReadOnlyList<ScoreRecord> Parse(string csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return Array.Empty<ScoreRecord>();
        }

        var rows = ParseRows(csv);
        if (rows.Count == 0)
        {
            return Array.Empty<ScoreRecord>();
        }

        ValidateHeader(rows[0]);

        var result = new List<ScoreRecord>();

        for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];

            if (row.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            if (row.Count != 3)
            {
                throw new FormatException(
                    $"CSV row {rowIndex + 1} contains {row.Count} columns; exactly 3 are required.");
            }

            var firstName = row[0].Trim();
            var secondName = row[1].Trim();

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(secondName))
            {
                throw new FormatException($"CSV row {rowIndex + 1} must contain both first and second name.");
            }

            if (!int.TryParse(row[2].Trim(), out var score))
            {
                throw new FormatException(
                    $"CSV row {rowIndex + 1} contains an invalid score '{row[2]}'.");
            }

            result.Add(new ScoreRecord
            {
                FirstName = firstName,
                SecondName = secondName,
                Score = score
            });
        }

        return result;
    }

    private static void ValidateHeader(IReadOnlyList<string> header)
    {
        if (header.Count != ExpectedHeaders.Length)
        {
            throw new FormatException("CSV header must contain First Name, Second Name and Score columns.");
        }

        for (var i = 0; i < ExpectedHeaders.Length; i++)
        {
            if (!string.Equals(header[i].Trim(), ExpectedHeaders[i], StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException(
                    $"Unexpected CSV header '{header[i]}'. Expected '{ExpectedHeaders[i]}' at column {i + 1}.");
            }
        }
    }

    private static List<List<string>> ParseRows(string csv)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var insideQuotes = false;

        for (var i = 0; i < csv.Length; i++)
        {
            var current = csv[i];

            if (current == '"')
            {
                if (insideQuotes && i + 1 < csv.Length && csv[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                    continue;
                }

                insideQuotes = !insideQuotes;
                continue;
            }

            if (current == ',' && !insideQuotes)
            {
                row.Add(field.ToString());
                field.Clear();
                continue;
            }

            if ((current == '\r' || current == '\n') && !insideQuotes)
            {
                row.Add(field.ToString());
                field.Clear();
                rows.Add(row);
                row = new List<string>();

                if (current == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n')
                {
                    i++;
                }

                continue;
            }

            field.Append(current);
        }

        if (insideQuotes)
        {
            throw new FormatException("CSV contains an unterminated quoted field.");
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }

        return rows;
    }
}

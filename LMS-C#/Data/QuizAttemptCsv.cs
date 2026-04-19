using System.Globalization;
using System.Text;
using KICSITManagementSystem.Models;

namespace KICSITManagementSystem.Data;

/// <summary>Writes quiz attempt rows as CSV (UTF-8 with BOM for Excel on Windows).</summary>
internal static class QuizAttemptCsv
{
    public static void WriteFile(IReadOnlyList<QuizAttemptDocument> attempts, string path)
    {
        var sb = new StringBuilder(capacity: Math.Max(256, attempts.Count * 96));
        sb.AppendLine(Line(
            "TakenUtc",
            "LocalTime",
            "Login",
            "SheetName",
            "Roll",
            "Marks",
            "QuestionCount",
            "Percentage",
            "SecondsTaken"));

        foreach (QuizAttemptDocument a in attempts)
        {
            string local = a.TakenUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            sb.AppendLine(Line(
                a.TakenUtc.ToString("o", CultureInfo.InvariantCulture),
                local,
                a.UsernameNormalized,
                a.SheetName,
                a.Roll,
                a.Marks.ToString(CultureInfo.InvariantCulture),
                a.QuestionCount.ToString(CultureInfo.InvariantCulture),
                a.Percentage.ToString(CultureInfo.InvariantCulture),
                a.SecondsTaken.ToString(CultureInfo.InvariantCulture)));
        }

        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    private static string Line(params string[] cells) => string.Join(",", Array.ConvertAll(cells, Csv));

    private static string Csv(string? value)
    {
        string s = value ?? string.Empty;
        return "\"" + s.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }
}

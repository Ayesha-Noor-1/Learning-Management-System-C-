using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using KICSITManagementSystem.Models;
using Spectre.Console;

namespace KICSITManagementSystem
{
    /// <summary>
    /// Central console presentation using Spectre.Console (panels, prompts, markup).
    /// </summary>
    internal static class UI
    {
        public static void ClearScreen() => AnsiConsole.Clear();

        public static void MainLogo() => WriteRoleHeader("KICSIT Management System", Color.DeepSkyBlue1);

        public static void StudentLogo() => WriteRoleHeader("Student Portal", Color.SeaGreen1);

        public static void TeacherLogo() => WriteRoleHeader("Teacher Portal", Color.MediumPurple1);

        public static void AdminLogo() => WriteRoleHeader("Admin Portal", Color.OrangeRed1);

        private static void WriteRoleHeader(string title, Color accent)
        {
            var panel = new Panel(
                    new Align(
                        new Markup($"[bold {accent.ToMarkup()}]{Markup.Escape(title)}[/]"),
                        HorizontalAlignment.Center))
                .Border(BoxBorder.Rounded)
                .BorderStyle(new Style(accent))
                .Padding(1, 0)
                .Expand();

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
        }

        public static void Loading(string message = "Loading…")
        {
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2)
                .SpinnerStyle(Style.Parse("green"))
                .Start(message, _ => Thread.Sleep(1200));
        }

        public static string AskLine(string prompt)
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>(Markup.Escape(prompt))
                    .PromptStyle("grey"));
        }

        public static string AskPassword(string prompt = "Password")
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>(Markup.Escape(prompt))
                    .Secret()
                    .PromptStyle("grey"));
        }

        public static void DisplayFile(string fileName, string? header = null)
        {
            if (!File.Exists(fileName))
            {
                AnsiConsole.MarkupLine($"[red]File not found:[/] {Markup.Escape(fileName)}");
                return;
            }

            string[] lines = File.ReadAllLines(fileName);
            DisplayLines(lines, header);
        }

        public static void DisplayLines(IReadOnlyList<string> lines, string? header = null)
        {
            string body = lines.Count == 0
                ? "[dim](empty)[/]"
                : string.Join(Environment.NewLine, Array.ConvertAll(lines.ToArray(), Markup.Escape));

            var panel = new Panel(new Markup(body))
                .Expand()
                .Border(BoxBorder.Square)
                .BorderColor(Color.Grey);

            if (!string.IsNullOrWhiteSpace(header))
                panel.Header = new PanelHeader(Markup.Escape(header));

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
        }

        /// <summary>
        /// Renders attendance as a table. Raw file lines use tabs (e.g. <c>Name\tP\tA</c>); dumping that in a
        /// <see cref="Panel"/> does not align columns, so we parse and use Spectre's <see cref="Table"/>.
        /// </summary>
        public static void DisplayAttendanceTable(string filePath)
        {
            if (!File.Exists(filePath))
            {
                AnsiConsole.MarkupLine($"[red]File not found:[/] {Markup.Escape(filePath)}");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn(new TableColumn("Student").LeftAligned());
            table.AddColumn(new TableColumn("Marks (P = present, A = absent)").LeftAligned());

            int rowCount = 0;
            foreach (string raw in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                List<string> parts = raw.Split('\t')
                    .Select(s => s.Trim())
                    .Where(s => s.Length > 0)
                    .ToList();

                if (parts.Count == 0)
                    continue;

                string student = parts[0];
                Markup marksCell = parts.Count == 1
                    ? new Markup("[dim]—[/]")
                    : new Markup(string.Join("  ", parts.Skip(1).Select(FormatAttendanceMark)));

                table.AddRow(new Markup(Markup.Escape(student)), marksCell);
                rowCount++;
            }

            if (rowCount == 0)
            {
                AnsiConsole.MarkupLine("[dim](Attendance file is empty.)[/]");
                return;
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }

        private static string FormatAttendanceMark(string mark) =>
            mark.Equals("P", StringComparison.OrdinalIgnoreCase)
                ? "[green]P[/]"
                : mark.Equals("A", StringComparison.OrdinalIgnoreCase)
                    ? "[red]A[/]"
                    : Markup.Escape(mark);

        /// <summary>Faculty directory as a proper table (replaces ASCII-art lines in a panel).</summary>
        public static void DisplayFacultyTable(IReadOnlyList<FacultyMemberDocument> members)
        {
            if (members.Count == 0)
            {
                AnsiConsole.MarkupLine("[dim](No faculty records.)[/]");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn(new TableColumn("Name").LeftAligned());
            table.AddColumn(new TableColumn("Department").LeftAligned());
            table.AddColumn(new TableColumn("Email").LeftAligned());

            foreach (FacultyMemberDocument f in members)
                table.AddRow(Markup.Escape(f.Name), Markup.Escape(f.Department), Markup.Escape(f.Email));

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }

        /// <summary>Plain text lines (courses, admission, …) as a numbered two-column table.</summary>
        public static void DisplayPlainLinesTable(IReadOnlyList<string> lines)
        {
            var rows = lines
                .Select(s => s.TrimEnd())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (rows.Count == 0)
            {
                AnsiConsole.MarkupLine("[dim](No content.)[/]");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn(new TableColumn("#").RightAligned());
            table.AddColumn(new TableColumn("Line").LeftAligned());

            for (int i = 0; i < rows.Count; i++)
                table.AddRow((i + 1).ToString(), Markup.Escape(rows[i]));

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }

        /// <summary>Notice bodies as a table (no ASCII borders).</summary>
        public static void DisplayNoticesTable(IReadOnlyList<string> noticeBodies)
        {
            if (noticeBodies.Count == 0)
            {
                AnsiConsole.MarkupLine("[dim](No notices.)[/]");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn(new TableColumn("#").RightAligned());
            table.AddColumn(new TableColumn("Notice").LeftAligned());

            for (int i = 0; i < noticeBodies.Count; i++)
                table.AddRow((i + 1).ToString(), Markup.Escape(noticeBodies[i]));

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }

        /// <summary>
        /// Timetable files use <c>|</c>-separated cells; we parse into a Spectre table instead of a raw panel.
        /// </summary>
        public static void DisplayTimetableFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                AnsiConsole.MarkupLine($"[red]File not found:[/] {Markup.Escape(filePath)}");
                return;
            }

            var parsedRows = new List<List<string>>();
            foreach (string raw in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                string line = raw.TrimEnd();
                if (IsTimetableSeparatorLine(line))
                    continue;

                List<string> cells = line.Split('|', StringSplitOptions.None)
                    .Select(s => s.Trim())
                    .Where(s => s.Length > 0)
                    .ToList();

                if (cells.Count == 0)
                    continue;

                parsedRows.Add(cells);
            }

            if (parsedRows.Count == 0)
            {
                AnsiConsole.MarkupLine("[dim](Timetable file is empty.)[/]");
                return;
            }

            int maxCols = parsedRows.Max(r => r.Count);
            if (maxCols < 2)
            {
                DisplayPlainLinesTable(File.ReadAllLines(filePath).Where(l => !string.IsNullOrWhiteSpace(l)).ToList());
                return;
            }

            int headerIndex = -1;
            for (int i = 0; i < parsedRows.Count; i++)
            {
                if (parsedRows[i].Count >= 3)
                {
                    headerIndex = i;
                    break;
                }
            }

            if (headerIndex < 0)
            {
                DisplayPlainLinesTable(File.ReadAllLines(filePath).Where(l => !string.IsNullOrWhiteSpace(l)).ToList());
                return;
            }

            int colCount = parsedRows.Skip(headerIndex).Max(r => r.Count);
            List<string> headerCells = PadCells(parsedRows[headerIndex], colCount);

            var table = new Table().Border(TableBorder.Rounded);
            foreach (string h in headerCells)
                table.AddColumn(new TableColumn(Markup.Escape(h)).LeftAligned());

            for (int r = headerIndex + 1; r < parsedRows.Count; r++)
            {
                List<string> cells = PadCells(parsedRows[r], colCount);
                string[] escaped = cells
                    .Select(c => string.IsNullOrWhiteSpace(c) ? " " : Markup.Escape(c))
                    .ToArray();
                table.AddRow(escaped);
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }

        private static bool IsTimetableSeparatorLine(string line)
        {
            string t = line.Trim();
            if (t.Length < 3)
                return false;

            return t.All(c => c is '=' or '-' or '_' or '|' or ' ');
        }

        private static List<string> PadCells(IReadOnlyList<string> row, int colCount)
        {
            var list = row.Take(colCount).ToList();
            while (list.Count < colCount)
                list.Add(string.Empty);

            return list;
        }

        /// <summary>Quiz results saved with <c>\t|\t</c> delimiters — shown as a table when possible.</summary>
        public static void DisplayQuizResultsFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                AnsiConsole.MarkupLine($"[red]File not found:[/] {Markup.Escape(filePath)}");
                return;
            }

            const string delim = "\t|\t";
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Name");
            table.AddColumn("Roll");
            table.AddColumn("Marks");
            table.AddColumn("%");
            table.AddColumn("Time");

            int added = 0;
            foreach (string raw in File.ReadAllLines(filePath))
            {
                string line = raw.Trim();
                if (line.Length == 0)
                    continue;

                if (line.Contains("NAME", StringComparison.OrdinalIgnoreCase)
                    && line.Contains("ROLL", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!line.Contains(delim, StringComparison.Ordinal))
                {
                    table.AddRow(Markup.Escape(line), "—", "—", "—", "—");
                    added++;
                    continue;
                }

                string[] parts = line.Split(delim, StringSplitOptions.None)
                    .Select(p => p.Trim())
                    .ToArray();

                if (parts.Length >= 5)
                {
                    table.AddRow(
                        Markup.Escape(parts[0]),
                        Markup.Escape(parts[1]),
                        Markup.Escape(parts[2]),
                        Markup.Escape(parts[3]),
                        Markup.Escape(parts[4]));
                    added++;
                }
                else
                {
                    table.AddRow(Markup.Escape(line), "—", "—", "—", "—");
                    added++;
                }
            }

            if (added == 0)
            {
                AnsiConsole.MarkupLine("[dim](No quiz results yet.)[/]");
                return;
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }

        /// <summary>Quiz attempts loaded from MongoDB (teacher = all students; student = own rows).</summary>
        public static void DisplayQuizAttemptsTable(IReadOnlyList<QuizAttemptDocument> attempts, bool showLoginColumn)
        {
            if (attempts.Count == 0)
            {
                AnsiConsole.MarkupLine("[dim](No quiz attempts in the database yet.)[/]");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("When (local)");
            if (showLoginColumn)
                table.AddColumn("Login");
            table.AddColumn("Sheet name");
            table.AddColumn("Roll");
            table.AddColumn("Score");
            table.AddColumn("%");
            table.AddColumn("Sec");

            foreach (QuizAttemptDocument a in attempts)
            {
                string when = a.TakenUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
                string score = $"{a.Marks} / {a.QuestionCount}";
                string pct = $"{a.Percentage:F1}%";

                if (showLoginColumn)
                {
                    table.AddRow(
                        Markup.Escape(when),
                        Markup.Escape(a.UsernameNormalized),
                        Markup.Escape(a.SheetName),
                        Markup.Escape(a.Roll),
                        Markup.Escape(score),
                        Markup.Escape(pct),
                        Markup.Escape(a.SecondsTaken.ToString()));
                }
                else
                {
                    table.AddRow(
                        Markup.Escape(when),
                        Markup.Escape(a.SheetName),
                        Markup.Escape(a.Roll),
                        Markup.Escape(score),
                        Markup.Escape(pct),
                        Markup.Escape(a.SecondsTaken.ToString()));
                }
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }

        /// <summary>Admin dashboard: counts + a small bar chart.</summary>
        public static void DisplayDashboardStats(
            long studentUsers,
            long teacherUsers,
            long adminUsers,
            long rosterRows,
            long facultyRows,
            long notices,
            long quizAttempts)
        {
            var summary = new Table().Border(TableBorder.Rounded);
            summary.AddColumn("Metric");
            summary.AddColumn("Count");
            summary.AddRow("Student login accounts", studentUsers.ToString());
            summary.AddRow("Teacher login accounts", teacherUsers.ToString());
            summary.AddRow("Admin login accounts", adminUsers.ToString());
            summary.AddRow("Student roster rows", rosterRows.ToString());
            summary.AddRow("Faculty records", facultyRows.ToString());
            summary.AddRow("Notices", notices.ToString());
            summary.AddRow("Quiz attempts saved", quizAttempts.ToString());
            AnsiConsole.Write(summary);
            AnsiConsole.WriteLine();

            var chart = new BarChart()
                .Width(60)
                .Label("Users by role (logins)");

            chart.AddItem("Students", studentUsers, Color.SeaGreen1);
            chart.AddItem("Teachers", teacherUsers, Color.MediumPurple1);
            chart.AddItem("Admins", adminUsers, Color.OrangeRed1);
            AnsiConsole.Write(chart);
            AnsiConsole.WriteLine();
        }

        public static void Success(string message) =>
            AnsiConsole.MarkupLine($"[green]{Markup.Escape(message)}[/]");

        public static void Error(string message) =>
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(message)}[/]");

        public static void Info(string message) =>
            AnsiConsole.MarkupLine($"[grey]{Markup.Escape(message)}[/]");

        public static void Goodbye()
        {
            AnsiConsole.Write(new Panel(
                    new Markup("[bold green]Thank you for visiting.[/]"))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green));
            Thread.Sleep(900);
        }

        public static void PressAnyKey(string caption = "Press any key to continue…")
        {
            AnsiConsole.MarkupLine($"[dim]{Markup.Escape(caption)}[/]");
            try
            {
                // In detached Docker containers / CI / redirected stdin there may be no interactive console.
                if (Console.IsInputRedirected)
                    return;

                Console.ReadKey(intercept: true);
            }
            catch (InvalidOperationException)
            {
                // No console available (e.g. docker -d); just continue.
            }
        }

        public static bool Confirm(string message) =>
            AnsiConsole.Confirm(Markup.Escape(message));
    }
}

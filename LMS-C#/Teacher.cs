using System.IO;
using System.Linq;
using System.Threading;
using KICSITManagementSystem.Data;
using KICSITManagementSystem.Oop;
using Spectre.Console;

namespace KICSITManagementSystem
{
    /// <summary>Teacher portal — inherits shared portal loop from <see cref="PortalBase"/>.</summary>
    internal sealed class Teacher : PortalBase
    {
        private readonly string _attendanceFile = AppPaths.Attendance;
        private readonly string _timetableFile = AppPaths.Timetable;
        private readonly string _quizFile = AppPaths.QuizFile;
        private readonly string _resultFile = AppPaths.QuizResult;

        public Teacher(string name)
            : base(name)
        {
        }

        public void ShowMenu() => Run();

        protected override void DrawRoleHeader() => UI.TeacherLogo();

        protected override string GetWelcomeAccentOpening() => "[mediumpurple1]";

        protected override string GetWelcomeAccentClosing() => "[/]";

        protected override string ReturnConfirmPrompt =>
            "Return to the teacher menu? (No will sign you out)";

        protected override SelectionPrompt<string> BuildMenuPrompt()
        {
            return new SelectionPrompt<string>()
                .Title("[bold]Teacher menu[/]")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Black, Color.MediumPurple1))
                .AddChoices(
                    "Timetable",
                    "Quiz results (all students)",
                    "Download quiz results (CSV)",
                    "Manage quiz & results",
                    "Make a quiz",
                    "Mark attendance",
                    "View attendance",
                    "Logout");
        }

        protected override bool ExecuteSelection(string action)
        {
            switch (action)
            {
                case "Timetable":
                    ShowTimetable();
                    break;
                case "Quiz results (all students)":
                    ViewAllQuizResults();
                    break;
                case "Download quiz results (CSV)":
                    DownloadAllQuizResultsCsv();
                    break;
                case "Manage quiz & results":
                    ManageQuizAndResults();
                    break;
                case "Make a quiz":
                    MakeQuiz();
                    break;
                case "Mark attendance":
                    MarkAttendance();
                    break;
                case "View attendance":
                    ShowAttendance();
                    break;
            }

            return true;
        }

        private void ShowTimetable()
        {
            UI.MainLogo();
            AnsiConsole.MarkupLine("[bold]Timetable[/]\n");
            UI.DisplayTimetableFromFile(_timetableFile);
        }

        private void ViewAllQuizResults()
        {
            UI.MainLogo();
            AnsiConsole.MarkupLine("[bold]Quiz results[/]\n");
            AnsiConsole.MarkupLine("[grey]Stored attempts (MongoDB)[/]\n");

            var attempts = QuizAttemptRepository.GetAllOrderedAsync()
                .ConfigureAwait(false).GetAwaiter().GetResult();
            UI.DisplayQuizAttemptsTable(attempts, showLoginColumn: true);

            if (File.Exists(_resultFile))
            {
                string[] legacy = File.ReadAllLines(_resultFile).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
                if (legacy.Length > 0)
                {
                    AnsiConsole.MarkupLine("\n[bold]Legacy file[/] [grey](quizresult.txt — older runs before database storage)[/]\n");
                    UI.DisplayQuizResultsFromFile(_resultFile);
                }
            }
        }

        private void DownloadAllQuizResultsCsv()
        {
            UI.MainLogo();
            AnsiConsole.MarkupLine("[bold]Download quiz results[/]\n");

            var attempts = QuizAttemptRepository.GetAllOrderedAsync()
                .ConfigureAwait(false).GetAwaiter().GetResult();

            if (attempts.Count == 0)
            {
                UI.Info("No quiz attempts in the database to export yet.");
                return;
            }

            string path = Path.Combine(
                AppPaths.UserDataFolder,
                $"quiz_attempts_all_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            QuizAttemptCsv.WriteFile(attempts, path);
            UI.Success($"Saved {attempts.Count} row(s) to:\n{path}");
        }

        private void ManageQuizAndResults()
        {
            while (true)
            {
                UI.ClearScreen();
                UI.TeacherLogo();
                AnsiConsole.MarkupLine("[bold]Manage quiz & results[/]\n");

                string choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Choose an action")
                        .PageSize(5)
                        .AddChoices(
                            "Replace quiz questions (clear MCQ file)",
                            "Clear all database quiz attempts",
                            "Back to teacher menu"));

                if (choice == "Back to teacher menu")
                    return;

                if (choice == "Replace quiz questions (clear MCQ file)")
                {
                    if (!UI.Confirm("This deletes every line in the quiz question file. Continue?"))
                        continue;

                    File.WriteAllText(_quizFile, string.Empty);
                    UI.Success("Quiz file cleared. Use [bold]Make a quiz[/] to add fresh questions.");
                    Thread.Sleep(1200);
                    continue;
                }

                if (choice == "Clear all database quiz attempts")
                {
                    if (!UI.Confirm("Delete ALL saved quiz attempts from MongoDB? This cannot be undone."))
                        continue;

                    QuizAttemptRepository.DeleteAllAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    UI.Success("All quiz attempts removed from the database.");
                    Thread.Sleep(1200);
                }
            }
        }

        private void MakeQuiz()
        {
            UI.TeacherLogo();

            int n = AnsiConsole.Prompt(
                new TextPrompt<int>("How many MCQs do you want to add?")
                    .PromptStyle("grey")
                    .ValidationErrorMessage("Enter a positive whole number.")
                    .Validate(count => count > 0));

            using (StreamWriter quiz = File.AppendText(_quizFile))
            {
                quiz.WriteLine();

                for (int i = 0; i < n; i++)
                {
                    UI.ClearScreen();
                    UI.TeacherLogo();
                    AnsiConsole.MarkupLine($"[bold]Question {i + 1} of {n}[/]\n");

                    quiz.WriteLine(UI.AskLine("Question text: "));

                    for (int j = 1; j <= 4; j++)
                        quiz.WriteLine(UI.AskLine($"Option {j}: "));

                    quiz.WriteLine(UI.AskLine("Correct answer (match an option exactly): "));
                }
            }

            UI.Success("Quiz saved.");
            Thread.Sleep(900);
        }

        private void MarkAttendance()
        {
            UI.TeacherLogo();
            AnsiConsole.MarkupLine("[bold]Mark attendance[/]\n");

            if (!File.Exists(_attendanceFile))
            {
                UI.Error("Attendance file not found.");
                return;
            }

            string[] students = File.ReadAllLines(_attendanceFile);

            if (students.Length == 0)
            {
                UI.Error("No students found in the attendance file.");
                return;
            }

            var updated = new string[students.Length];

            for (int i = 0; i < students.Length; i++)
            {
                string studentLabel = students[i].Split('\t')
                    .Select(s => s.Trim())
                    .FirstOrDefault(s => s.Length > 0) ?? students[i];
                AnsiConsole.MarkupLine($"[bold]{Markup.Escape(studentLabel)}[/]");

                string mark = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Attendance")
                        .AddChoices("Present", "Absent"));

                char att = mark == "Present" ? 'P' : 'A';
                updated[i] = students[i] + "\t" + att;
                AnsiConsole.WriteLine();
            }

            File.WriteAllLines(_attendanceFile, updated);
            UI.Success("Attendance saved.");
            Thread.Sleep(900);
        }

        private void ShowAttendance()
        {
            UI.TeacherLogo();
            AnsiConsole.MarkupLine("[bold]Attendance[/]\n");
            UI.DisplayAttendanceTable(_attendanceFile);
        }
    }
}

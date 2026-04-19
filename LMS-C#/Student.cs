using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using KICSITManagementSystem.Data;
using KICSITManagementSystem.Models;
using KICSITManagementSystem.Oop;
using Spectre.Console;

namespace KICSITManagementSystem
{
    /// <summary>Student portal — <see cref="PortalBase"/> for shared menu lifecycle (inheritance + template method).</summary>
    internal sealed class Student : PortalBase
    {
        private readonly string _attendanceFile = AppPaths.Attendance;
        private readonly string _timetableFile = AppPaths.Timetable;
        private readonly string _quizFile = AppPaths.QuizFile;

        public Student(string name)
            : base(name)
        {
        }

        /// <summary>Kept for readability at call sites; forwards to <see cref="IPortal.Run"/>.</summary>
        public void ShowMenu() => Run();

        protected override void DrawRoleHeader() => UI.StudentLogo();

        protected override string GetWelcomeAccentOpening() => "[aqua]";

        protected override string GetWelcomeAccentClosing() => "[/]";

        protected override string ReturnConfirmPrompt =>
            "Return to the student menu? (No will sign you out)";

        protected override SelectionPrompt<string> BuildMenuPrompt()
        {
            return new SelectionPrompt<string>()
                .Title("[bold]Student menu[/]")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Black, Color.SeaGreen1))
                .AddChoices(
                    "Attendance",
                    "Notice board",
                    "Timetable",
                    "Take quiz",
                    "My quiz results",
                    "Download my quiz results (CSV)",
                    "Logout");
        }

        protected override bool ExecuteSelection(string action)
        {
            switch (action)
            {
                case "Attendance":
                    ShowAttendance();
                    break;
                case "Notice board":
                    ShowNoticeBoard();
                    break;
                case "Timetable":
                    ShowTimetable();
                    break;
                case "Take quiz":
                    TakeQuiz();
                    break;
                case "My quiz results":
                    ViewMyQuizResults();
                    break;
                case "Download my quiz results (CSV)":
                    DownloadMyQuizResultsCsv();
                    break;
            }

            return true;
        }

        private void ShowAttendance()
        {
            UI.StudentLogo();
            AnsiConsole.MarkupLine("[bold]Attendance[/]\n");
            UI.DisplayAttendanceTable(_attendanceFile);
        }

        private void ShowNoticeBoard()
        {
            UI.StudentLogo();
            AnsiConsole.MarkupLine("[bold]Notice board[/]\n");

            List<NoticeDocument> notices = NoticeRepository.GetAllOrderedAsync()
                .ConfigureAwait(false).GetAwaiter().GetResult();

            if (notices.Count == 0)
            {
                UI.Info("(No notices yet.)");
                return;
            }

            UI.DisplayNoticesTable(notices.ConvertAll(n => n.Body));
        }

        private void ShowTimetable()
        {
            UI.MainLogo();
            AnsiConsole.MarkupLine("[bold]Timetable[/]\n");
            UI.DisplayTimetableFromFile(_timetableFile);
        }

        private void TakeQuiz()
        {
            UI.Loading("Loading quiz…");
            UI.ClearScreen();
            UI.StudentLogo();

            UserDocument? account = UserRepository.FindByUsernameAndRoleAsync(DisplayUsername, "Student")
                .ConfigureAwait(false).GetAwaiter().GetResult();
            if (account == null)
            {
                UI.Error("Could not resolve your account. Please sign in again.");
                return;
            }

            string quizName = UI.AskLine("Name (for result sheet): ");
            string roll = UI.AskLine("Registration number: ");

            if (!InputValidation.IsMeaningful(quizName) || !InputValidation.IsMeaningful(roll, maxLength: 64))
            {
                UI.Error("Name and registration number are required (registration max 64 characters).");
                return;
            }

            if (!File.Exists(_quizFile))
            {
                UI.Error("No quiz available yet.");
                return;
            }

            string[] allLines = File.ReadAllLines(_quizFile);
            var lines = new List<string>();
            foreach (string line in allLines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    lines.Add(line);
            }

            if (lines.Count == 0)
            {
                UI.Error("Quiz file is empty.");
                return;
            }

            int marks = 0;
            int questionCount = 0;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i + 5 < lines.Count; i += 6)
            {
                questionCount++;
                AnsiConsole.MarkupLine($"\n[bold]Question {questionCount}[/]\n");

                for (int j = i; j < i + 5; j++)
                    AnsiConsole.WriteLine(Markup.Escape(lines[j]));

                string correctAnswer = lines[i + 5].Trim();
                string userAnswer = UI.AskLine("Your answer: ").Trim();

                if (userAnswer.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase))
                {
                    UI.Success("Correct.");
                    marks++;
                }
                else
                {
                    UI.Error($"Wrong. Correct answer: {correctAnswer}");
                }
            }

            stopwatch.Stop();
            long secondsTaken = stopwatch.ElapsedMilliseconds / 1000;

            float percentage = questionCount > 0 ? (float)marks / questionCount * 100 : 0;

            var summary = new Table().Border(TableBorder.Rounded);
            summary.AddColumn("Metric");
            summary.AddColumn("Value");
            summary.AddRow("Time taken (seconds)", secondsTaken.ToString());
            summary.AddRow("Score", $"{marks} / {questionCount}");
            summary.AddRow("Percentage", $"{percentage:F1}%");
            AnsiConsole.Write(summary);

            var attempt = new QuizAttemptDocument
            {
                UserId = account.Id,
                UsernameNormalized = account.UsernameNormalized,
                SheetName = quizName.Trim(),
                Roll = roll.Trim(),
                Marks = marks,
                QuestionCount = questionCount,
                Percentage = percentage,
                SecondsTaken = secondsTaken,
                TakenUtc = DateTime.UtcNow
            };

            QuizAttemptRepository.InsertAsync(attempt).ConfigureAwait(false).GetAwaiter().GetResult();

            UI.Success("Result saved to the database.");
        }

        private void ViewMyQuizResults()
        {
            UI.MainLogo();
            AnsiConsole.MarkupLine("[bold]My quiz results[/]\n");

            UserDocument? account = UserRepository.FindByUsernameAndRoleAsync(DisplayUsername, "Student")
                .ConfigureAwait(false).GetAwaiter().GetResult();
            if (account == null)
            {
                UI.Error("Could not resolve your account.");
                return;
            }

            List<QuizAttemptDocument> mine = QuizAttemptRepository.GetForUserAsync(account.Id)
                .ConfigureAwait(false).GetAwaiter().GetResult();

            UI.DisplayQuizAttemptsTable(mine, showLoginColumn: false);
        }

        private void DownloadMyQuizResultsCsv()
        {
            UI.MainLogo();
            AnsiConsole.MarkupLine("[bold]Download my quiz results[/]\n");

            UserDocument? account = UserRepository.FindByUsernameAndRoleAsync(DisplayUsername, "Student")
                .ConfigureAwait(false).GetAwaiter().GetResult();
            if (account == null)
            {
                UI.Error("Could not resolve your account.");
                return;
            }

            List<QuizAttemptDocument> mine = QuizAttemptRepository.GetForUserAsync(account.Id)
                .ConfigureAwait(false).GetAwaiter().GetResult();

            if (mine.Count == 0)
            {
                UI.Info("You have no saved quiz attempts to export yet.");
                return;
            }

            string path = Path.Combine(
                AppPaths.UserDataFolder,
                $"my_quiz_results_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            QuizAttemptCsv.WriteFile(mine, path);
            UI.Success($"Saved {mine.Count} row(s) to:\n{path}");
        }
    }
}

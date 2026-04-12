using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Spectre.Console;

namespace KICSITManagementSystem
{
    internal class Student
    {
        private readonly string _name;

        private readonly string AttendanceFile = AppPaths.Attendance;
        private readonly string NoticeFile = AppPaths.NoticeBoard;
        private readonly string TimetableFile = AppPaths.Timetable;
        private readonly string QuizFile = AppPaths.QuizFile;
        private readonly string ResultFile = AppPaths.QuizResult;

        public Student(string name)
        {
            _name = name;
        }

        public void ShowMenu()
        {
            while (true)
            {
                UI.ClearScreen();
                UI.StudentLogo();
                AnsiConsole.MarkupLine($"[bold]Welcome,[/] [aqua]{Markup.Escape(_name.ToUpperInvariant())}[/]\n");

                string action = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Student menu[/]")
                        .PageSize(10)
                        .HighlightStyle(new Style(Color.Black, Color.SeaGreen1))
                        .AddChoices(
                            "Attendance",
                            "Notice board",
                            "Timetable",
                            "Take quiz",
                            "Quiz results",
                            "Logout"));

                if (action == "Logout")
                    break;

                UI.ClearScreen();

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
                    case "Quiz results":
                        ViewResult();
                        break;
                }

                if (!UI.Confirm("Return to the student menu? (No will sign you out)"))
                    break;
            }
        }

        private void ShowAttendance()
        {
            UI.StudentLogo();
            AnsiConsole.MarkupLine("[bold]Attendance[/]\n");
            UI.DisplayFile(AttendanceFile, "Attendance");
        }

        private void ShowNoticeBoard()
        {
            UI.StudentLogo();
            AnsiConsole.MarkupLine("[bold]Notice board[/]\n");
            UI.DisplayFile(NoticeFile, "Notices");
        }

        private void ShowTimetable()
        {
            UI.MainLogo();
            AnsiConsole.MarkupLine("[bold]Timetable[/]\n");
            UI.DisplayFile(TimetableFile, "Timetable");
        }

        private void TakeQuiz()
        {
            UI.Loading("Loading quiz…");
            UI.ClearScreen();
            UI.StudentLogo();

            string quizName = UI.AskLine("Name (for result sheet): ");
            string roll = UI.AskLine("Registration number: ");

            if (!File.Exists(QuizFile))
            {
                UI.Error("No quiz available yet.");
                return;
            }

            string[] allLines = File.ReadAllLines(QuizFile);
            List<string> lines = new List<string>();
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

            Stopwatch stopwatch = new Stopwatch();
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

            string resultLine =
                $"{Environment.NewLine}{quizName}\t|\t{roll}\t|\t{marks}\t|\t{percentage:F1}%\t|\t{secondsTaken} seconds";
            File.AppendAllText(ResultFile, resultLine);

            UI.Success("Result saved.");
        }

        private void ViewResult()
        {
            UI.MainLogo();
            AnsiConsole.MarkupLine("[bold]Quiz results[/]\n");
            UI.DisplayFile(ResultFile, "Results");
        }
    }
}

using System.IO;
using System.Threading;
using Spectre.Console;

namespace KICSITManagementSystem
{
    internal class Teacher
    {
        private readonly string _name;

        private readonly string AttendanceFile = AppPaths.Attendance;
        private readonly string TimetableFile = AppPaths.Timetable;
        private readonly string QuizFile = AppPaths.QuizFile;
        private readonly string ResultFile = AppPaths.QuizResult;

        public Teacher(string name)
        {
            _name = name;
        }

        public void ShowMenu()
        {
            while (true)
            {
                UI.ClearScreen();
                UI.TeacherLogo();
                AnsiConsole.MarkupLine($"[bold]Welcome,[/] [mediumpurple1]{Markup.Escape(_name.ToUpperInvariant())}[/]\n");

                string action = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Teacher menu[/]")
                        .PageSize(10)
                        .HighlightStyle(new Style(Color.Black, Color.MediumPurple1))
                        .AddChoices(
                            "Timetable",
                            "Quiz results",
                            "Make a quiz",
                            "Mark attendance",
                            "View attendance",
                            "Logout"));

                if (action == "Logout")
                    break;

                UI.ClearScreen();

                switch (action)
                {
                    case "Timetable":
                        ShowTimetable();
                        break;
                    case "Quiz results":
                        ViewResult();
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

                if (!UI.Confirm("Return to the teacher menu? (No will sign you out)"))
                    break;
            }
        }

        private void ShowTimetable()
        {
            UI.MainLogo();
            AnsiConsole.MarkupLine("[bold]Timetable[/]\n");
            UI.DisplayFile(TimetableFile, "Timetable");
        }

        private void ViewResult()
        {
            UI.MainLogo();
            AnsiConsole.MarkupLine("[bold]Quiz results[/]\n");
            UI.DisplayFile(ResultFile, "Results");
        }

        private void MakeQuiz()
        {
            UI.TeacherLogo();

            int n = AnsiConsole.Prompt(
                new TextPrompt<int>("How many MCQs do you want to add?")
                    .PromptStyle("grey")
                    .ValidationErrorMessage("Enter a positive whole number.")
                    .Validate(count => count > 0));

            using (StreamWriter quiz = File.AppendText(QuizFile))
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

            if (!File.Exists(AttendanceFile))
            {
                UI.Error("Attendance file not found.");
                return;
            }

            string[] students = File.ReadAllLines(AttendanceFile);

            if (students.Length == 0)
            {
                UI.Error("No students found in the attendance file.");
                return;
            }

            string[] updated = new string[students.Length];

            for (int i = 0; i < students.Length; i++)
            {
                AnsiConsole.WriteLine(Markup.Escape(students[i]));

                string mark = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Attendance")
                        .AddChoices("Present", "Absent"));

                char att = mark == "Present" ? 'P' : 'A';
                updated[i] = students[i] + "\t" + att;
                AnsiConsole.WriteLine();
            }

            File.WriteAllLines(AttendanceFile, updated);
            UI.Success("Attendance saved.");
            Thread.Sleep(900);
        }

        private void ShowAttendance()
        {
            UI.TeacherLogo();
            AnsiConsole.MarkupLine("[bold]Attendance[/]\n");
            UI.DisplayFile(AttendanceFile, "Attendance");
        }
    }
}

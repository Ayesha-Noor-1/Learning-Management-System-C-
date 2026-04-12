using System.Collections.Generic;
using System.IO;
using System.Threading;
using Spectre.Console;

namespace KICSITManagementSystem
{
    internal class Admin
    {
        private readonly string _name;

        private readonly string StudentLoginFile = AppPaths.StudentLogin;
        private readonly string TeacherLoginFile = AppPaths.TeacherLogin;
        private readonly string StudentDataFile = AppPaths.StudentData;
        private readonly string FacultyFile = AppPaths.FacultyData;
        private readonly string NoticeFile = AppPaths.NoticeBoard;
        private readonly string TimetableFile = AppPaths.Timetable;

        public Admin(string name)
        {
            _name = name;
        }

        public void ShowMenu()
        {
            while (true)
            {
                UI.ClearScreen();
                UI.AdminLogo();
                AnsiConsole.MarkupLine($"[bold]Welcome,[/] [orangered1]{Markup.Escape(_name.ToUpperInvariant())}[/]\n");

                string action = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Admin menu[/]")
                        .PageSize(10)
                        .HighlightStyle(new Style(Color.Black, Color.OrangeRed1))
                        .AddChoices(
                            "Add student",
                            "Remove student",
                            "Notice board",
                            "Add teacher account",
                            "Update timetable",
                            "Return to main menu",
                            "Logout"));

                if (action == "Logout")
                    break;

                if (action == "Return to main menu")
                {
                    UI.ClearScreen();
                    GeneralView gv = new GeneralView();
                    gv.Show();
                    return;
                }

                UI.ClearScreen();

                switch (action)
                {
                    case "Add student":
                        AddStudent();
                        break;
                    case "Remove student":
                        RemoveStudent();
                        break;
                    case "Notice board":
                        ManageNotices();
                        break;
                    case "Add teacher account":
                        AddTeacher();
                        break;
                    case "Update timetable":
                        UpdateTimetable();
                        break;
                }

                if (!UI.Confirm("Return to the admin menu? (No will sign you out)"))
                    break;
            }
        }

        private void AddStudent()
        {
            UI.AdminLogo();
            AnsiConsole.MarkupLine("[bold]Add student[/]\n");

            string name = UI.AskLine("Student name: ");
            string password = UI.AskLine("Student password: ");
            string roll = UI.AskLine("Roll number: ");
            string gender = UI.AskLine("Gender: ");

            File.AppendAllText(StudentLoginFile,
                name + Environment.NewLine + password + Environment.NewLine);

            File.AppendAllText(StudentDataFile,
                $"|  {roll}\t|  {name}\t|  {gender}  |" + Environment.NewLine);

            UI.Success("Student added.");
            Thread.Sleep(800);
        }

        private void RemoveStudent()
        {
            UI.AdminLogo();
            AnsiConsole.MarkupLine("[bold]Remove student[/]\n");

            if (!File.Exists(StudentDataFile))
            {
                UI.Error("Student data file not found.");
                return;
            }

            List<string> students = new List<string>(File.ReadAllLines(StudentDataFile));

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Index");
            table.AddColumn("Record");
            for (int i = 0; i < students.Count; i++)
                table.AddRow(i.ToString(), Markup.Escape(students[i]));

            AnsiConsole.Write(table);

            int index = AnsiConsole.Prompt(
                new TextPrompt<int>("Index to remove:")
                    .PromptStyle("grey")
                    .ValidationErrorMessage("Enter a valid index.")
                    .Validate(i => i >= 0 && i < students.Count));

            string removed = students[index];
            students.RemoveAt(index);
            File.WriteAllLines(StudentDataFile, students);

            UI.Success($"Removed: {removed}");
            Thread.Sleep(900);
        }

        private void ManageNotices()
        {
            while (true)
            {
                UI.ClearScreen();
                UI.AdminLogo();
                AnsiConsole.MarkupLine("[bold]Notice board manager[/]\n");

                string choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Choose an action")
                        .PageSize(6)
                        .AddChoices(
                            "Display notices",
                            "Add notice",
                            "Remove notice",
                            "Back to admin menu"));

                switch (choice)
                {
                    case "Back to admin menu":
                        return;

                    case "Display notices":
                        UI.ClearScreen();
                        UI.AdminLogo();
                        AnsiConsole.MarkupLine("[bold]Notice board[/]\n");
                        UI.DisplayFile(NoticeFile, "Notices");
                        UI.PressAnyKey();
                        break;

                    case "Add notice":
                        UI.ClearScreen();
                        UI.AdminLogo();
                        string notice = UI.AskLine("New notice: ");
                        File.AppendAllText(NoticeFile,
                            $"|  {notice}  \t\t|" + Environment.NewLine +
                            "__________________________________________________________________________"
                            + Environment.NewLine);
                        UI.Success("Notice added.");
                        Thread.Sleep(800);
                        break;

                    case "Remove notice":
                        RemoveNotice();
                        break;
                }
            }
        }

        private void RemoveNotice()
        {
            UI.ClearScreen();
            UI.AdminLogo();
            AnsiConsole.MarkupLine("[bold]Remove notice[/]\n");

            if (!File.Exists(NoticeFile))
            {
                UI.Error("Notice file not found.");
                return;
            }

            List<string> lines = new List<string>(File.ReadAllLines(NoticeFile));

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Index");
            table.AddColumn("Line");
            for (int i = 0; i < lines.Count; i++)
                table.AddRow(i.ToString(), Markup.Escape(lines[i]));

            AnsiConsole.Write(table);

            int index = AnsiConsole.Prompt(
                new TextPrompt<int>("Index to remove:")
                    .PromptStyle("grey")
                    .ValidationErrorMessage("Enter a valid index.")
                    .Validate(i => i >= 0 && i < lines.Count));

            lines.RemoveAt(index);
            File.WriteAllLines(NoticeFile, lines);

            UI.Success("Notice line removed.");
            Thread.Sleep(800);
        }

        private void AddTeacher()
        {
            UI.AdminLogo();
            AnsiConsole.MarkupLine("[bold]Add teacher[/]\n");

            string name = UI.AskLine("Teacher name: ");
            string password = UI.AskLine("Teacher password: ");
            string email = UI.AskLine("Email: ");
            string department = UI.AskLine("Department: ");

            File.AppendAllText(TeacherLoginFile,
                Environment.NewLine + name + Environment.NewLine + password + Environment.NewLine);

            File.AppendAllText(FacultyFile,
                $"{name}\t|\t{department}\t|\t{email}\t|" + Environment.NewLine);

            UI.Success("Teacher added.");
            AnsiConsole.MarkupLine("\n[bold]Updated faculty list[/]\n");
            UI.DisplayFile(FacultyFile, "Faculty");
            Thread.Sleep(1200);
        }

        private void UpdateTimetable()
        {
            UI.AdminLogo();
            AnsiConsole.MarkupLine("[bold]Update timetable[/]\n");

            if (!File.Exists(TimetableFile))
            {
                UI.Error("Timetable file not found.");
                return;
            }

            List<string> lines = new List<string>(File.ReadAllLines(TimetableFile));

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Line #");
            table.AddColumn("Content");
            for (int i = 0; i < lines.Count; i++)
                table.AddRow(i.ToString(), Markup.Escape(lines[i]));

            AnsiConsole.Write(table);

            int lineNum = AnsiConsole.Prompt(
                new TextPrompt<int>("Line number to update:")
                    .PromptStyle("grey")
                    .ValidationErrorMessage("Enter a valid line number.")
                    .Validate(n => n >= 0 && n < lines.Count));

            AnsiConsole.MarkupLine($"[grey]Current:[/] {Markup.Escape(lines[lineNum])}");
            string updated = UI.AskLine("Updated content: ");

            lines[lineNum] = updated;
            File.WriteAllLines(TimetableFile, lines);

            UI.ClearScreen();
            AnsiConsole.MarkupLine("[bold]Updated timetable[/]\n");
            UI.DisplayFile(TimetableFile, "Timetable");
            UI.Success("Timetable saved.");
            Thread.Sleep(900);
        }
    }
}

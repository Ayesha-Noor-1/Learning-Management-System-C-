using System.Collections.Generic;
using System.IO;
using System.Threading;
using KICSITManagementSystem.Data;
using KICSITManagementSystem.Models;
using KICSITManagementSystem.Oop;
using MongoDB.Bson;
using Spectre.Console;

namespace KICSITManagementSystem
{
    /// <summary>Admin portal — <see cref="PortalBase"/> with an extra exit path to the main menu.</summary>
    internal sealed class Admin : PortalBase
    {
        private readonly string _timetableFile = AppPaths.Timetable;

        public Admin(string name)
            : base(name)
        {
        }

        public void ShowMenu() => Run();

        protected override void DrawRoleHeader() => UI.AdminLogo();

        protected override string GetWelcomeAccentOpening() => "[orangered1]";

        protected override string GetWelcomeAccentClosing() => "[/]";

        protected override string ReturnConfirmPrompt =>
            "Return to the admin menu? (No will sign you out)";

        protected override SelectionPrompt<string> BuildMenuPrompt()
        {
            return new SelectionPrompt<string>()
                .Title("[bold]Admin menu[/]")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Black, Color.OrangeRed1))
                .AddChoices(
                    "Add student",
                    "Remove student",
                    "Notice board",
                    "Add teacher account",
                    "Update timetable",
                    "Statistics",
                    "Return to main menu",
                    "Logout");
        }

        protected override bool ExecuteSelection(string action)
        {
            if (action == "Return to main menu")
            {
                UI.ClearScreen();
                var gv = new GeneralView();
                gv.Show();
                return false;
            }

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
                case "Statistics":
                    ShowStatistics();
                    break;
            }

            return true;
        }

        private static void ShowStatistics()
        {
            UI.AdminLogo();
            AnsiConsole.MarkupLine("[bold]Statistics[/]\n");

            (long studentUsers, long teacherUsers, long adminUsers, long roster, long faculty, long notices, long attempts) =
                DashboardStats.LoadAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            UI.DisplayDashboardStats(studentUsers, teacherUsers, adminUsers, roster, faculty, notices, attempts);
            UI.PressAnyKey();
        }

        private void AddStudent()
        {
            UI.AdminLogo();
            AnsiConsole.MarkupLine("[bold]Add student[/]\n");

            string name = UI.AskLine("Student name (login): ").Trim();
            string password = UI.AskPassword("Student password: ");
            string roll = UI.AskLine("Roll number: ").Trim();
            string gender = UI.AskLine("Gender: ").Trim();

            if (!InputValidation.IsMeaningful(name) || !InputValidation.IsMeaningful(password))
            {
                UI.Error("Name and password are required.");
                Thread.Sleep(900);
                return;
            }

            if (UserRepository.UsernameExistsAsync(name).ConfigureAwait(false).GetAwaiter().GetResult())
            {
                UI.Error("That login name is already in use. Choose another.");
                Thread.Sleep(900);
                return;
            }

            var user = new UserDocument
            {
                Username = name,
                UsernameNormalized = UserRepository.Normalize(name),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "Student"
            };

            UserRepository.InsertAsync(user).ConfigureAwait(false).GetAwaiter().GetResult();

            var student = new StudentDocument
            {
                UserId = user.Id,
                Roll = roll,
                Name = name,
                Gender = gender
            };

            StudentRepository.InsertAsync(student).ConfigureAwait(false).GetAwaiter().GetResult();

            UI.Success("Student added (saved in MongoDB).");
            Thread.Sleep(800);
        }

        private void RemoveStudent()
        {
            UI.AdminLogo();
            AnsiConsole.MarkupLine("[bold]Remove student[/]\n");

            List<StudentDocument> students = StudentRepository.GetAllOrderedAsync()
                .ConfigureAwait(false).GetAwaiter().GetResult();

            if (students.Count == 0)
            {
                UI.Error("No students in the database.");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Index");
            table.AddColumn("Roll");
            table.AddColumn("Name");
            table.AddColumn("Gender");
            for (int i = 0; i < students.Count; i++)
            {
                StudentDocument s = students[i];
                table.AddRow(
                    i.ToString(),
                    Markup.Escape(s.Roll),
                    Markup.Escape(s.Name),
                    Markup.Escape(s.Gender));
            }

            AnsiConsole.Write(table);

            int index = AnsiConsole.Prompt(
                new TextPrompt<int>("Index to remove:")
                    .PromptStyle("grey")
                    .ValidationErrorMessage("Enter a valid index.")
                    .Validate(i => i >= 0 && i < students.Count));

            StudentDocument removed = students[index];
            if (removed.UserId is ObjectId uid)
                UserRepository.DeleteByIdAsync(uid).ConfigureAwait(false).GetAwaiter().GetResult();

            StudentRepository.DeleteByIdAsync(removed.Id).ConfigureAwait(false).GetAwaiter().GetResult();

            UI.Success($"Removed: {removed.Roll} — {removed.Name}");
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
                        DisplayNoticesFromDatabase();
                        UI.PressAnyKey();
                        break;

                    case "Add notice":
                        UI.ClearScreen();
                        UI.AdminLogo();
                        string notice = UI.AskLine("New notice: ");
                        if (!InputValidation.IsMeaningful(notice))
                        {
                            UI.Error("Notice text cannot be empty.");
                            Thread.Sleep(800);
                            break;
                        }

                        var doc = new NoticeDocument
                        {
                            Body = notice.Trim(),
                            CreatedUtc = DateTime.UtcNow
                        };
                        NoticeRepository.InsertAsync(doc).ConfigureAwait(false).GetAwaiter().GetResult();
                        UI.Success("Notice added.");
                        Thread.Sleep(800);
                        break;

                    case "Remove notice":
                        RemoveNotice();
                        break;
                }
            }
        }

        private static void DisplayNoticesFromDatabase()
        {
            List<NoticeDocument> notices = NoticeRepository.GetAllOrderedAsync()
                .ConfigureAwait(false).GetAwaiter().GetResult();

            if (notices.Count == 0)
            {
                UI.Info("(No notices yet.)");
                return;
            }

            UI.DisplayNoticesTable(notices.ConvertAll(n => n.Body));
        }

        private void RemoveNotice()
        {
            UI.ClearScreen();
            UI.AdminLogo();
            AnsiConsole.MarkupLine("[bold]Remove notice[/]\n");

            List<NoticeDocument> notices = NoticeRepository.GetAllOrderedAsync()
                .ConfigureAwait(false).GetAwaiter().GetResult();

            if (notices.Count == 0)
            {
                UI.Error("No notices to remove.");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Index");
            table.AddColumn("Body");
            for (int i = 0; i < notices.Count; i++)
                table.AddRow(i.ToString(), Markup.Escape(notices[i].Body));

            AnsiConsole.Write(table);

            int index = AnsiConsole.Prompt(
                new TextPrompt<int>("Index to remove:")
                    .PromptStyle("grey")
                    .ValidationErrorMessage("Enter a valid index.")
                    .Validate(i => i >= 0 && i < notices.Count));

            NoticeRepository.DeleteByIdAsync(notices[index].Id).ConfigureAwait(false).GetAwaiter().GetResult();

            UI.Success("Notice removed.");
            Thread.Sleep(800);
        }

        private void AddTeacher()
        {
            UI.AdminLogo();
            AnsiConsole.MarkupLine("[bold]Add teacher[/]\n");

            string name = UI.AskLine("Teacher name (login): ").Trim();
            string password = UI.AskPassword("Teacher password: ");
            string email = UI.AskLine("Email: ").Trim();
            string department = UI.AskLine("Department: ").Trim();

            if (!InputValidation.IsMeaningful(name) || !InputValidation.IsMeaningful(password))
            {
                UI.Error("Name and password are required.");
                Thread.Sleep(900);
                return;
            }

            if (UserRepository.UsernameExistsAsync(name).ConfigureAwait(false).GetAwaiter().GetResult())
            {
                UI.Error("That login name is already in use. Choose another.");
                Thread.Sleep(900);
                return;
            }

            var user = new UserDocument
            {
                Username = name,
                UsernameNormalized = UserRepository.Normalize(name),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "Teacher"
            };

            UserRepository.InsertAsync(user).ConfigureAwait(false).GetAwaiter().GetResult();

            var faculty = new FacultyMemberDocument
            {
                UserId = user.Id,
                Name = name,
                Department = department,
                Email = email
            };

            FacultyRepository.InsertAsync(faculty).ConfigureAwait(false).GetAwaiter().GetResult();

            UI.Success("Teacher added (saved in MongoDB).");
            AnsiConsole.MarkupLine("\n[bold]Updated faculty list[/]\n");

            List<FacultyMemberDocument> members = FacultyRepository.GetAllAsync()
                .ConfigureAwait(false).GetAwaiter().GetResult();
            UI.DisplayFacultyTable(members);
            Thread.Sleep(1200);
        }

        private void UpdateTimetable()
        {
            UI.AdminLogo();
            AnsiConsole.MarkupLine("[bold]Update timetable[/]\n");

            if (!File.Exists(_timetableFile))
            {
                UI.Error("Timetable file not found.");
                return;
            }

            var lines = new List<string>(File.ReadAllLines(_timetableFile));

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
            File.WriteAllLines(_timetableFile, lines);

            UI.ClearScreen();
            AnsiConsole.MarkupLine("[bold]Updated timetable[/]\n");
            UI.DisplayTimetableFromFile(_timetableFile);
            UI.Success("Timetable saved.");
            Thread.Sleep(900);
        }
    }
}

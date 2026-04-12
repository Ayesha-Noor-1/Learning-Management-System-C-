using System.IO;
using System.Threading;
using Spectre.Console;

namespace KICSITManagementSystem
{
    internal class AuthManager
    {
        private static bool ValidateLogin(string fileName, string name, string password)
        {
            if (!File.Exists(fileName))
            {
                UI.Error($"Login data file not found: {fileName}");
                return false;
            }

            string[] lines = File.ReadAllLines(fileName);
            string lowerName = name.ToLower().Trim();

            for (int j = 0; j + 1 < lines.Length; j += 2)
            {
                string filName = lines[j].ToLower().Trim();
                string filPass = lines[j + 1].Trim();

                if (filName == lowerName && filPass == password)
                    return true;
            }

            return false;
        }

        public static void LoginStudent()
        {
            while (true)
            {
                UI.ClearScreen();
                UI.MainLogo();

                string name = UI.AskLine("Name: ");
                string password = UI.AskPassword("Password: ");

                if (ValidateLogin(AppPaths.StudentLogin, name, password))
                {
                    AnsiConsole.MarkupLine("[green]You are successfully signed in.[/]");
                    Thread.Sleep(900);
                    UI.ClearScreen();

                    Student student = new Student(name);
                    student.ShowMenu();
                    break;
                }

                UI.Error("Invalid credentials. Try again.");
                Thread.Sleep(1200);
            }
        }

        public static void LoginTeacher()
        {
            while (true)
            {
                UI.ClearScreen();
                UI.MainLogo();

                string name = UI.AskLine("Name: ");
                string password = UI.AskPassword("Password: ");

                if (ValidateLogin(AppPaths.TeacherLogin, name, password))
                {
                    AnsiConsole.MarkupLine("[green]You are successfully signed in.[/]");
                    Thread.Sleep(900);
                    UI.ClearScreen();

                    Teacher teacher = new Teacher(name);
                    teacher.ShowMenu();
                    break;
                }

                UI.Error("Invalid credentials. Try again.");
                Thread.Sleep(1200);
            }
        }

        public static void LoginAdmin()
        {
            while (true)
            {
                UI.ClearScreen();
                UI.MainLogo();

                string name = UI.AskLine("Username: ");
                string password = UI.AskPassword("Password: ");

                if (ValidateLogin(AppPaths.AdminLogin, name, password))
                {
                    AnsiConsole.MarkupLine("[green]You are successfully signed in.[/]");
                    Thread.Sleep(900);
                    UI.ClearScreen();

                    Admin admin = new Admin(name);
                    admin.ShowMenu();
                    break;
                }

                UI.Error("Invalid credentials. Try again.");
                Thread.Sleep(1200);
            }
        }

        public static void ShowLoginMenu()
        {
            UI.MainLogo();
            AnsiConsole.MarkupLine("[grey]Choose how you are signing in.[/]\n");

            string role = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Sign in[/]")
                    .PageSize(5)
                    .HighlightStyle(new Style(Color.Black, Color.Aqua))
                    .AddChoices("Student", "Teacher", "Admin"));

            UI.Loading("Preparing workspace…");
            UI.ClearScreen();

            switch (role)
            {
                case "Student":
                    LoginStudent();
                    break;
                case "Teacher":
                    LoginTeacher();
                    break;
                default:
                    LoginAdmin();
                    break;
            }
        }
    }
}

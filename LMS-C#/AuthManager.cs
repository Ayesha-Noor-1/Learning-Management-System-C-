using System.Threading;
using KICSITManagementSystem.Oop;
using Spectre.Console;

namespace KICSITManagementSystem
{
    internal static class AuthManager
    {
        /// <summary>Polymorphism: code depends on <see cref="ILoginValidator"/>, not a concrete type.</summary>
        private static readonly ILoginValidator CredentialValidator = new MongoCredentialValidator();

        private static void OpenPortalAfterSignIn(IPortal portal)
        {
            AnsiConsole.MarkupLine("[green]You are successfully signed in.[/]");
            Thread.Sleep(900);
            UI.ClearScreen();
            portal.Run();
        }

        public static void LoginStudent()
        {
            while (true)
            {
                UI.ClearScreen();
                UI.MainLogo();

                string name = UI.AskLine("Name: ");
                string password = UI.AskPassword("Password: ");

                if (CredentialValidator.IsValid("Student", name, password))
                {
                    IPortal portal = new Student(name);
                    OpenPortalAfterSignIn(portal);
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

                if (CredentialValidator.IsValid("Teacher", name, password))
                {
                    IPortal portal = new Teacher(name);
                    OpenPortalAfterSignIn(portal);
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

                if (CredentialValidator.IsValid("Admin", name, password))
                {
                    IPortal portal = new Admin(name);
                    OpenPortalAfterSignIn(portal);
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

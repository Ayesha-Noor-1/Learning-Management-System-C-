using KICSITManagementSystem.Data;
using KICSITManagementSystem.Models;
using Spectre.Console;

namespace KICSITManagementSystem
{
    internal class GeneralView
    {
        public void Show()
        {
            while (true)
            {
                UI.ClearScreen();
                UI.MainLogo();
                AnsiConsole.MarkupLine("[grey]Use ↑/↓ and Enter to choose an option.[/]\n");

                string choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Welcome — main menu[/]")
                        .PageSize(10)
                        .HighlightStyle(new Style(Color.Black, Color.Aqua))
                        .AddChoices(
                            "Faculty information",
                            "Available courses",
                            "Admission portal",
                            "Log in",
                            "Exit"));

                if (choice == "Exit")
                {
                    UI.Goodbye();
                    return;
                }

                UI.ClearScreen();

                switch (choice)
                {
                    case "Faculty information":
                        ShowFaculty();
                        break;
                    case "Available courses":
                        ShowCourses();
                        break;
                    case "Admission portal":
                        ShowAdmissionPortal();
                        break;
                    case "Log in":
                        UI.Loading("Opening sign-in…");
                        UI.ClearScreen();
                        AuthManager.ShowLoginMenu();
                        continue;
                }

                if (!UI.Confirm("Return to the main menu? (No will close the application)"))
                {
                    UI.Goodbye();
                    return;
                }
            }
        }

        private void ShowFaculty()
        {
            UI.MainLogo();
            AnsiConsole.MarkupLine("[bold]Faculty information[/]\n");
            List<FacultyMemberDocument> members = FacultyRepository.GetAllAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            UI.DisplayFacultyTable(members);
        }

        private void ShowCourses()
        {
            UI.MainLogo();
            AnsiConsole.MarkupLine("[bold]Available courses[/]\n");
            List<string> lines = SiteContentRepository.GetLinesAsync(SiteContentRepository.IdCourses)
                .ConfigureAwait(false).GetAwaiter().GetResult();
            UI.DisplayPlainLinesTable(lines);
        }

        private void ShowAdmissionPortal()
        {
            UI.MainLogo();
            AnsiConsole.MarkupLine("[bold]Admission portal[/]\n");
            List<string> admission = SiteContentRepository.GetLinesAsync(SiteContentRepository.IdAdmission)
                .ConfigureAwait(false).GetAwaiter().GetResult();
            UI.DisplayPlainLinesTable(admission);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]Available courses[/]\n");
            List<string> courses = SiteContentRepository.GetLinesAsync(SiteContentRepository.IdCourses)
                .ConfigureAwait(false).GetAwaiter().GetResult();
            UI.DisplayPlainLinesTable(courses);
        }
    }
}

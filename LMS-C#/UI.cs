using System.IO;
using System.Threading;
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
            string body = lines.Length == 0
                ? "[dim](empty file)[/]"
                : string.Join(Environment.NewLine, Array.ConvertAll(lines, Markup.Escape));

            var panel = new Panel(new Markup(body))
                .Expand()
                .Border(BoxBorder.Square)
                .BorderColor(Color.Grey);

            if (!string.IsNullOrWhiteSpace(header))
                panel.Header = new PanelHeader(Markup.Escape(header));

            AnsiConsole.Write(panel);
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
            Console.ReadKey(intercept: true);
        }

        public static bool Confirm(string message) =>
            AnsiConsole.Confirm(Markup.Escape(message));
    }
}

using System;
using System.Net.Sockets;
using KICSITManagementSystem.Data;
using Spectre.Console;

namespace KICSITManagementSystem
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            AppPaths.Initialize();

            Console.Title = "KICSIT Management System";

            try
            {
                await LmsDatabase.InitializeAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                UI.ClearScreen();
                AnsiConsole.MarkupLine("[red]Could not connect to MongoDB.[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine(Markup.Escape(ExplainMongoFailure(ex)));
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[grey]Fix:[/]");
                AnsiConsole.MarkupLine("[grey]- Local:[/] Install MongoDB Community Server, then run [bold]services.msc[/] → ensure [bold]MongoDB Server[/] is [green]Running[/].");
                AnsiConsole.MarkupLine("[grey]- Cloud:[/] Put your Atlas [bold]mongodb+srv://…[/] URI in [bold]appsettings.json[/] or set env [bold]MONGODB_CONNECTION_STRING[/].");
                UI.PressAnyKey();
                return;
            }

            UI.ClearScreen();
            GeneralView app = new GeneralView();
            app.Show();

            Environment.Exit(0);
        }

        /// <summary>
        /// The driver often throws long nested messages; students only need the reason (refused / timeout).
        /// </summary>
        private static string ExplainMongoFailure(Exception ex)
        {
            if (ContainsConnectionRefused(ex))
            {
                return "Nothing answered on that host and port (connection refused). "
                    + "Your appsettings point at localhost:27017, but no MongoDB process is listening there.";
            }

            if (ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
                return "Timed out while contacting MongoDB. Check VPN, firewall, and Atlas Network Access if using Atlas.";

            return ex.GetBaseException().Message;
        }

        private static bool ContainsConnectionRefused(Exception ex)
        {
            for (Exception? e = ex; e != null; e = e.InnerException)
            {
                if (e is SocketException se && se.SocketErrorCode == SocketError.ConnectionRefused)
                    return true;

                if (e.Message.Contains("actively refused", StringComparison.OrdinalIgnoreCase)
                    || e.Message.Contains("10061", StringComparison.Ordinal))
                    return true;
            }

            return ex.Message.Contains("actively refused", StringComparison.OrdinalIgnoreCase);
        }
    }
}

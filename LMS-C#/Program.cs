using System;

namespace KICSITManagementSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AppPaths.Initialize();

            Console.Title = "KICSIT Management System";

            UI.ClearScreen();
            GeneralView app = new GeneralView();
            app.Show();

            Environment.Exit(0);
        }
    }
}

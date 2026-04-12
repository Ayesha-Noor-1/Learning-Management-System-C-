using System;
using System.IO;

namespace KICSITManagementSystem
{
    // ── WHY THIS CLASS? ──────────────────────────────────────────────────────
    // All file paths were hardcoded as plain filenames like "loginstd.txt"
    // This means the app saves to wherever the .exe runs — which Visual Studio
    // keeps overwriting with your original empty files on every build.
    //
    // This class builds absolute paths pointing to:
    // C:\Users\YourName\Documents\KICSITData\
    // Visual Studio NEVER touches that folder — your data is safe permanently.

    internal static class AppPaths
    {
        // Base folder: Documents\KICSITData
        private static readonly string BaseFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "KICSITData"
        );

        // ── ALL FILE PATHS ───────────────────────────────────────────────────
        public static string StudentLogin => Path.Combine(BaseFolder, "loginstd.txt");
        public static string TeacherLogin => Path.Combine(BaseFolder, "logintea.txt");
        public static string AdminLogin => Path.Combine(BaseFolder, "loginadm.txt");
        public static string FacultyData => Path.Combine(BaseFolder, "facultydata.txt");
        public static string CourseView => Path.Combine(BaseFolder, "courseview.txt");
        public static string AdmPortal => Path.Combine(BaseFolder, "admportal.txt");
        public static string Timetable => Path.Combine(BaseFolder, "timetableCS-1A.txt");
        public static string Attendance => Path.Combine(BaseFolder, "attendanceCS-1A.txt");
        public static string NoticeBoard => Path.Combine(BaseFolder, "noticeboardCS-1A.txt");
        public static string QuizFile => Path.Combine(BaseFolder, "quizmakeCS1-A.txt");
        public static string QuizResult => Path.Combine(BaseFolder, "quizresult.txt");
        public static string StudentData => Path.Combine(BaseFolder, "CS-1A.txt");

        // ── SETUP ────────────────────────────────────────────────────────────
        // Call this ONCE at startup from Program.cs
        // It creates the folder and copies your seed files into it
        // BUT only if they don't already exist — so saved data is never overwritten

        public static void Initialize()
        {
            // Create the folder if it doesn't exist yet
            Directory.CreateDirectory(BaseFolder);

            // For each file: if it doesn't exist in Documents yet,
            // copy the seed version from the project's output folder.
            // "Seed" = the initial data files you set up in your project.
            // Once copied, they are NEVER touched again by Visual Studio.

            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;

            CopySeedFile(exeFolder, "loginstd.txt", StudentLogin);
            CopySeedFile(exeFolder, "logintea.txt", TeacherLogin);
            CopySeedFile(exeFolder, "loginadm.txt", AdminLogin);
            CopySeedFile(exeFolder, "facultydata.txt", FacultyData);
            CopySeedFile(exeFolder, "courseview.txt", CourseView);
            CopySeedFile(exeFolder, "admportal.txt", AdmPortal);
            CopySeedFile(exeFolder, "timetableCS-1A.txt", Timetable);
            CopySeedFile(exeFolder, "attendanceCS-1A.txt", Attendance);
            CopySeedFile(exeFolder, "noticeboardCS-1A.txt", NoticeBoard);
            CopySeedFile(exeFolder, "quizmakeCS1-A.txt", QuizFile);
            CopySeedFile(exeFolder, "quizresult.txt", QuizResult);
            CopySeedFile(exeFolder, "CS-1A.txt", StudentData);

            Console.WriteLine($"Data folder: {BaseFolder}");
        }

        private static void CopySeedFile(string exeFolder, string fileName, string destination)
        {
            // Only copy if it doesn't already exist in Documents
            // This is the KEY logic — saved data is NEVER overwritten
            if (!File.Exists(destination))
            {
                string source = Path.Combine(exeFolder, fileName);
                if (File.Exists(source))
                {
                    File.Copy(source, destination);
                }
                else
                {
                    // If seed file is missing too, just create an empty one
                    // so the app doesn't crash on first run
                    File.WriteAllText(destination, "");
                }
            }
        }
    }
}
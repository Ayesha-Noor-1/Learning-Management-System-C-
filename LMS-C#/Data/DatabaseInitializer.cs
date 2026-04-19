using System.Text.RegularExpressions;
using KICSITManagementSystem.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KICSITManagementSystem.Data;

internal static class DatabaseInitializer
{
    public static async Task RunAsync(IMongoDatabase db)
    {
        await UserRepository.EnsureIndexesAsync().ConfigureAwait(false);
        await QuizAttemptRepository.EnsureIndexesAsync().ConfigureAwait(false);

        if (await UserRepository.CountAsync().ConfigureAwait(false) == 0)
            await SeedUsersAsync().ConfigureAwait(false);

        if (await StudentRepository.CountAsync().ConfigureAwait(false) == 0)
            await SeedStudentsAsync().ConfigureAwait(false);

        if (await FacultyRepository.CountAsync().ConfigureAwait(false) == 0)
            await SeedFacultyAsync().ConfigureAwait(false);

        if (await NoticeRepository.CountAsync().ConfigureAwait(false) == 0)
            await SeedNoticesAsync().ConfigureAwait(false);

        if (!await SiteContentRepository.ExistsAsync(SiteContentRepository.IdCourses).ConfigureAwait(false)
            || !await SiteContentRepository.ExistsAsync(SiteContentRepository.IdAdmission).ConfigureAwait(false))
        {
            await SeedSiteContentAsync().ConfigureAwait(false);
        }

        await LinkFacultyToTeacherAccountsAsync().ConfigureAwait(false);
    }

    private static IEnumerable<(string Username, string Password)> ReadLoginPairs(string path)
    {
        if (!File.Exists(path))
            yield break;

        string[] lines = File.ReadAllLines(path);
        for (int j = 0; j + 1 < lines.Length; j += 2)
        {
            string u = lines[j].Trim();
            string p = lines[j + 1].Trim();
            if (u.Length > 0 && p.Length > 0)
                yield return (u, p);
        }
    }

    private static async Task SeedUsersAsync()
    {
        var users = new List<UserDocument>();

        foreach ((string u, string p) in ReadLoginPairs(AppPaths.StudentLogin))
        {
            users.Add(new UserDocument
            {
                Username = u,
                UsernameNormalized = UserRepository.Normalize(u),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(p),
                Role = "Student"
            });
        }

        foreach ((string u, string p) in ReadLoginPairs(AppPaths.TeacherLogin))
        {
            users.Add(new UserDocument
            {
                Username = u,
                UsernameNormalized = UserRepository.Normalize(u),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(p),
                Role = "Teacher"
            });
        }

        foreach ((string u, string p) in ReadLoginPairs(AppPaths.AdminLogin))
        {
            users.Add(new UserDocument
            {
                Username = u,
                UsernameNormalized = UserRepository.Normalize(u),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(p),
                Role = "Admin"
            });
        }

        if (users.Count > 0)
            await LmsDatabase.Db.GetCollection<UserDocument>(LmsCollections.Users)
                .InsertManyAsync(users).ConfigureAwait(false);
    }

    private static bool TryParseStudentRosterLine(string line, out string roll, out string name, out string gender)
    {
        roll = name = gender = "";
        if (!line.Contains('|', StringComparison.Ordinal))
            return false;

        string[] parts = line.Split('|', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
            return false;

        roll = parts[0].Trim();
        name = parts[1].Trim();
        gender = parts[2].Trim().TrimEnd('|').Trim();

        if (name.Contains("NAME", StringComparison.OrdinalIgnoreCase)
            && (name.Contains("ROLL", StringComparison.OrdinalIgnoreCase) || roll.Contains("ROLL", StringComparison.OrdinalIgnoreCase)))
            return false;

        return roll.Length > 0 && name.Length > 0;
    }

    private static async Task SeedStudentsAsync()
    {
        if (!File.Exists(AppPaths.StudentData))
            return;

        List<UserDocument> studentUsers = await LmsDatabase.Db.GetCollection<UserDocument>(LmsCollections.Users)
            .Find(u => u.Role == "Student")
            .ToListAsync()
            .ConfigureAwait(false);

        var roster = new List<StudentDocument>();
        foreach (string line in File.ReadAllLines(AppPaths.StudentData))
        {
            if (!TryParseStudentRosterLine(line, out string roll, out string name, out string gender))
                continue;

            ObjectId? userId = studentUsers
                .FirstOrDefault(u => UserRepository.Normalize(name) == u.UsernameNormalized)?.Id;

            roster.Add(new StudentDocument
            {
                Roll = roll,
                Name = name,
                Gender = gender,
                UserId = userId
            });
        }

        if (roster.Count > 0)
            await LmsDatabase.Db.GetCollection<StudentDocument>(LmsCollections.Students)
                .InsertManyAsync(roster).ConfigureAwait(false);
    }

    private static bool TryParseFacultyLine(string line, out string name, out string department, out string email)
    {
        name = department = email = "";
        if (!line.Contains('|', StringComparison.Ordinal))
            return false;
        if (line.Contains("====", StringComparison.Ordinal) || line.Contains("___", StringComparison.Ordinal))
            return false;

        string[] parts = line.Split('|', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
            return false;

        name = parts[0].Trim();
        department = parts[1].Trim();
        email = parts[2].Trim();

        if (name.Contains("NAME", StringComparison.OrdinalIgnoreCase)
            && department.Contains("DEPARTMENT", StringComparison.OrdinalIgnoreCase))
            return false;

        return name.Length > 2 && email.Contains('@');
    }

    private static async Task SeedFacultyAsync()
    {
        if (!File.Exists(AppPaths.FacultyData))
            return;

        var members = new List<FacultyMemberDocument>();
        foreach (string line in File.ReadAllLines(AppPaths.FacultyData))
        {
            if (!TryParseFacultyLine(line, out string name, out string department, out string email))
                continue;

            members.Add(new FacultyMemberDocument
            {
                Name = name,
                Department = department,
                Email = email,
                UserId = null
            });
        }

        if (members.Count > 0)
            await LmsDatabase.Db.GetCollection<FacultyMemberDocument>(LmsCollections.FacultyMembers)
                .InsertManyAsync(members).ConfigureAwait(false);
    }

    private static async Task SeedNoticesAsync()
    {
        if (!File.Exists(AppPaths.NoticeBoard))
            return;

        var notices = new List<NoticeDocument>();
        DateTime anchor = DateTime.UtcNow;
        int order = 0;
        foreach (string line in File.ReadAllLines(AppPaths.NoticeBoard))
        {
            if (!line.Contains('|', StringComparison.Ordinal))
                continue;

            string[] parts = line.Split('|', StringSplitOptions.None);
            if (parts.Length < 2)
                continue;

            string inner = parts[1].Trim();
            if (inner.Length < 10)
                continue;
            if (inner.Contains("NOTICE BOARD", StringComparison.OrdinalIgnoreCase))
                continue;
            if (Regex.IsMatch(inner, "^_+$"))
                continue;

            notices.Add(new NoticeDocument
            {
                Body = inner,
                CreatedUtc = anchor.AddMilliseconds(order++)
            });
        }

        if (notices.Count > 0)
            await LmsDatabase.Db.GetCollection<NoticeDocument>(LmsCollections.Notices)
                .InsertManyAsync(notices).ConfigureAwait(false);
    }

    private static async Task SeedSiteContentAsync()
    {
        if (File.Exists(AppPaths.CourseView))
        {
            string[] lines = File.ReadAllLines(AppPaths.CourseView);
            await SiteContentRepository.UpsertAsync(SiteContentRepository.IdCourses, lines).ConfigureAwait(false);
        }

        if (File.Exists(AppPaths.AdmPortal))
        {
            string[] lines = File.ReadAllLines(AppPaths.AdmPortal);
            await SiteContentRepository.UpsertAsync(SiteContentRepository.IdAdmission, lines).ConfigureAwait(false);
        }
    }

    private static async Task LinkFacultyToTeacherAccountsAsync()
    {
        List<UserDocument> teachers = await LmsDatabase.Db.GetCollection<UserDocument>(LmsCollections.Users)
            .Find(u => u.Role == "Teacher")
            .ToListAsync()
            .ConfigureAwait(false);

        if (teachers.Count == 0)
            return;

        List<FacultyMemberDocument> faculty = await FacultyRepository.GetAllAsync().ConfigureAwait(false);
        IMongoCollection<FacultyMemberDocument> col =
            LmsDatabase.Db.GetCollection<FacultyMemberDocument>(LmsCollections.FacultyMembers);

        foreach (FacultyMemberDocument f in faculty)
        {
            if (f.UserId != null)
                continue;

            string[] nameParts = f.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length == 0)
                continue;

            string first = nameParts[0];
            UserDocument? match = teachers.FirstOrDefault(t =>
                string.Equals(first, t.Username, StringComparison.OrdinalIgnoreCase)
                || string.Equals(first, t.UsernameNormalized, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                await col.UpdateOneAsync(
                    x => x.Id == f.Id,
                    Builders<FacultyMemberDocument>.Update.Set(x => x.UserId, match.Id)).ConfigureAwait(false);
            }
        }
    }
}

namespace KICSITManagementSystem.Data;

/// <summary>Lightweight counts for the admin statistics screen.</summary>
internal static class DashboardStats
{
    public static async Task<(long Students, long Teachers, long Admins, long Roster, long Faculty, long Notices, long QuizAttempts)> LoadAsync()
    {
        long students = await UserRepository.CountByRoleAsync("Student").ConfigureAwait(false);
        long teachers = await UserRepository.CountByRoleAsync("Teacher").ConfigureAwait(false);
        long admins = await UserRepository.CountByRoleAsync("Admin").ConfigureAwait(false);
        long roster = await StudentRepository.CountAsync().ConfigureAwait(false);
        long faculty = await FacultyRepository.CountAsync().ConfigureAwait(false);
        long notices = await NoticeRepository.CountAsync().ConfigureAwait(false);
        long attempts = await QuizAttemptRepository.CountAsync().ConfigureAwait(false);

        return (students, teachers, admins, roster, faculty, notices, attempts);
    }
}

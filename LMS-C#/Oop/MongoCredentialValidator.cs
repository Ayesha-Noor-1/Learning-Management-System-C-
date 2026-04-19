using KICSITManagementSystem.Data;
using KICSITManagementSystem.Models;

namespace KICSITManagementSystem.Oop;

/// <summary>Concrete validator backed by MongoDB + BCrypt.</summary>
internal sealed class MongoCredentialValidator : ILoginValidator
{
    /// <inheritdoc />
    public bool IsValid(string role, string username, string password)
    {
        UserDocument? user = UserRepository.FindByUsernameAndRoleAsync(username, role)
            .ConfigureAwait(false).GetAwaiter().GetResult();

        if (user == null)
            return false;

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }
}

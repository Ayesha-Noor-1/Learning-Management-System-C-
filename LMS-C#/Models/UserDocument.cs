using MongoDB.Bson;

namespace KICSITManagementSystem.Models;

internal sealed class UserDocument
{
    public ObjectId Id { get; set; }
    public string Username { get; set; } = "";
    public string UsernameNormalized { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "";
}

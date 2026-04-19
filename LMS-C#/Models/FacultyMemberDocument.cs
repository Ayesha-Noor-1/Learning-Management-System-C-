using MongoDB.Bson;

namespace KICSITManagementSystem.Models;

internal sealed class FacultyMemberDocument
{
    public ObjectId Id { get; set; }
    public ObjectId? UserId { get; set; }
    public string Name { get; set; } = "";
    public string Department { get; set; } = "";
    public string Email { get; set; } = "";
}

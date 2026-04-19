using MongoDB.Bson;

namespace KICSITManagementSystem.Models;

internal sealed class StudentDocument
{
    public ObjectId Id { get; set; }
    public ObjectId? UserId { get; set; }
    public string Roll { get; set; } = "";
    public string Name { get; set; } = "";
    public string Gender { get; set; } = "";
}

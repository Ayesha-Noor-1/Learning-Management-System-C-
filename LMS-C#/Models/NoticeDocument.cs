using MongoDB.Bson;

namespace KICSITManagementSystem.Models;

internal sealed class NoticeDocument
{
    public ObjectId Id { get; set; }
    public string Body { get; set; } = "";
    public DateTime CreatedUtc { get; set; }
}

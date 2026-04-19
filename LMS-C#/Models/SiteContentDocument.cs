using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KICSITManagementSystem.Models;

internal sealed class SiteContentDocument
{
    [BsonId]
    public string Id { get; set; } = "";

    public List<string> Lines { get; set; } = new();
}

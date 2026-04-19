using KICSITManagementSystem.Models;
using MongoDB.Driver;

namespace KICSITManagementSystem.Data;

internal static class SiteContentRepository
{
    public const string IdCourses = "courses";
    public const string IdAdmission = "admission";

    private static IMongoCollection<SiteContentDocument> Col =>
        LmsDatabase.Db.GetCollection<SiteContentDocument>(LmsCollections.SiteContent);

    public static async Task<List<string>> GetLinesAsync(string id)
    {
        var doc = await Col.Find(c => c.Id == id).FirstOrDefaultAsync().ConfigureAwait(false);
        return doc?.Lines ?? new List<string>();
    }

    public static async Task UpsertAsync(string id, IReadOnlyList<string> lines)
    {
        var doc = new SiteContentDocument { Id = id, Lines = lines.ToList() };
        await Col.ReplaceOneAsync(
            c => c.Id == id,
            doc,
            new ReplaceOptions { IsUpsert = true }).ConfigureAwait(false);
    }

    public static async Task<long> CountAsync() =>
        await Col.CountDocumentsAsync(FilterDefinition<SiteContentDocument>.Empty).ConfigureAwait(false);

    public static async Task<bool> ExistsAsync(string id)
    {
        long n = await Col.CountDocumentsAsync(x => x.Id == id).ConfigureAwait(false);
        return n > 0;
    }
}

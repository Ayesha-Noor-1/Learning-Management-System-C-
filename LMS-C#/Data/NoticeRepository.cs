using KICSITManagementSystem.Models;
using MongoDB.Driver;

namespace KICSITManagementSystem.Data;

internal static class NoticeRepository
{
    private static IMongoCollection<NoticeDocument> Col =>
        LmsDatabase.Db.GetCollection<NoticeDocument>(LmsCollections.Notices);

    public static async Task<List<NoticeDocument>> GetAllOrderedAsync()
    {
        return await Col.Find(FilterDefinition<NoticeDocument>.Empty)
            .SortBy(n => n.CreatedUtc)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public static async Task InsertAsync(NoticeDocument notice) =>
        await Col.InsertOneAsync(notice).ConfigureAwait(false);

    public static async Task DeleteByIdAsync(MongoDB.Bson.ObjectId id) =>
        await Col.DeleteOneAsync(n => n.Id == id).ConfigureAwait(false);

    public static async Task<long> CountAsync() =>
        await Col.CountDocumentsAsync(FilterDefinition<NoticeDocument>.Empty).ConfigureAwait(false);
}

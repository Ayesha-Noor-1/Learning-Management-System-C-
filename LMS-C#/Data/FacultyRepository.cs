using KICSITManagementSystem.Models;
using MongoDB.Driver;

namespace KICSITManagementSystem.Data;

internal static class FacultyRepository
{
    private static IMongoCollection<FacultyMemberDocument> Col =>
        LmsDatabase.Db.GetCollection<FacultyMemberDocument>(LmsCollections.FacultyMembers);

    public static async Task<List<FacultyMemberDocument>> GetAllAsync()
    {
        return await Col.Find(FilterDefinition<FacultyMemberDocument>.Empty)
            .SortBy(f => f.Name)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public static async Task InsertAsync(FacultyMemberDocument member) =>
        await Col.InsertOneAsync(member).ConfigureAwait(false);

    public static async Task UpdateUserLinkAsync(MongoDB.Bson.ObjectId facultyId, MongoDB.Bson.ObjectId userId)
    {
        await Col.UpdateOneAsync(
            f => f.Id == facultyId,
            Builders<FacultyMemberDocument>.Update.Set(f => f.UserId, userId)).ConfigureAwait(false);
    }

    public static async Task<long> CountAsync() =>
        await Col.CountDocumentsAsync(FilterDefinition<FacultyMemberDocument>.Empty).ConfigureAwait(false);
}

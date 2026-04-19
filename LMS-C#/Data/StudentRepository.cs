using KICSITManagementSystem.Models;
using MongoDB.Driver;

namespace KICSITManagementSystem.Data;

internal static class StudentRepository
{
    private static IMongoCollection<StudentDocument> Col =>
        LmsDatabase.Db.GetCollection<StudentDocument>(LmsCollections.Students);

    public static async Task<List<StudentDocument>> GetAllOrderedAsync()
    {
        return await Col.Find(FilterDefinition<StudentDocument>.Empty)
            .SortBy(s => s.Roll)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public static async Task InsertAsync(StudentDocument student) =>
        await Col.InsertOneAsync(student).ConfigureAwait(false);

    public static async Task DeleteByIdAsync(MongoDB.Bson.ObjectId id) =>
        await Col.DeleteOneAsync(s => s.Id == id).ConfigureAwait(false);

    public static async Task<long> CountAsync() =>
        await Col.CountDocumentsAsync(FilterDefinition<StudentDocument>.Empty).ConfigureAwait(false);
}

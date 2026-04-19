using KICSITManagementSystem.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace KICSITManagementSystem.Data;

internal static class QuizAttemptRepository
{
    private static IMongoCollection<QuizAttemptDocument> Col =>
        LmsDatabase.Db.GetCollection<QuizAttemptDocument>(LmsCollections.QuizAttempts);

    public static async Task EnsureIndexesAsync()
    {
        var keys = Builders<QuizAttemptDocument>.IndexKeys
            .Ascending(a => a.UserId)
            .Descending(a => a.TakenUtc);
        var model = new CreateIndexModel<QuizAttemptDocument>(
            keys,
            new CreateIndexOptions { Name = "ix_quiz_attempts_user_time" });
        try
        {
            await Col.Indexes.CreateOneAsync(model).ConfigureAwait(false);
        }
        catch (MongoCommandException ex) when (
            ex.Code == 85
            || ex.Code == 86
            || ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
        {
        }
    }

    public static async Task InsertAsync(QuizAttemptDocument attempt) =>
        await Col.InsertOneAsync(attempt).ConfigureAwait(false);

    public static async Task<List<QuizAttemptDocument>> GetForUserAsync(ObjectId userId)
    {
        return await Col.Find(a => a.UserId == userId)
            .SortByDescending(a => a.TakenUtc)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public static async Task<List<QuizAttemptDocument>> GetAllOrderedAsync()
    {
        return await Col.Find(FilterDefinition<QuizAttemptDocument>.Empty)
            .SortByDescending(a => a.TakenUtc)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public static async Task<long> CountAsync() =>
        await Col.CountDocumentsAsync(FilterDefinition<QuizAttemptDocument>.Empty).ConfigureAwait(false);

    public static async Task DeleteAllAsync() =>
        await Col.DeleteManyAsync(FilterDefinition<QuizAttemptDocument>.Empty).ConfigureAwait(false);
}

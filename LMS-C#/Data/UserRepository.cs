using KICSITManagementSystem.Models;
using MongoDB.Driver;

namespace KICSITManagementSystem.Data;

internal static class UserRepository
{
    private static IMongoCollection<UserDocument> Col =>
        LmsDatabase.Db.GetCollection<UserDocument>(LmsCollections.Users);

    public static async Task EnsureIndexesAsync()
    {
        var keys = Builders<UserDocument>.IndexKeys.Ascending(u => u.UsernameNormalized);
        var model = new CreateIndexModel<UserDocument>(
            keys,
            new CreateIndexOptions { Name = "ix_users_username_norm", Unique = true });
        try
        {
            await Col.Indexes.CreateOneAsync(model).ConfigureAwait(false);
        }
        catch (MongoCommandException ex) when (
            ex.Code == 85
            || ex.Code == 86
            || ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
        {
            // Index already created on a previous run.
        }
    }

    public static string Normalize(string username) => username.ToLowerInvariant().Trim();

    public static async Task<UserDocument?> FindByUsernameAndRoleAsync(string username, string role)
    {
        string norm = Normalize(username);
        return await Col.Find(u => u.UsernameNormalized == norm && u.Role == role)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public static async Task<bool> UsernameExistsAsync(string username)
    {
        string norm = Normalize(username);
        long count = await Col.CountDocumentsAsync(u => u.UsernameNormalized == norm).ConfigureAwait(false);
        return count > 0;
    }

    public static async Task InsertAsync(UserDocument user) =>
        await Col.InsertOneAsync(user).ConfigureAwait(false);

    public static async Task DeleteByIdAsync(MongoDB.Bson.ObjectId id) =>
        await Col.DeleteOneAsync(u => u.Id == id).ConfigureAwait(false);

    public static async Task<long> CountAsync() =>
        await Col.CountDocumentsAsync(FilterDefinition<UserDocument>.Empty).ConfigureAwait(false);

    public static async Task<long> CountByRoleAsync(string role) =>
        await Col.CountDocumentsAsync(u => u.Role == role).ConfigureAwait(false);
}

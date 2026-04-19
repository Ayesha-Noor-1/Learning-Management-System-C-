using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace KICSITManagementSystem.Data;

internal static class LmsDatabase
{
    public static IMongoDatabase Db { get; private set; } = null!;

    public static async Task InitializeAsync()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(baseDir)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        string conn = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
            ?? config["MongoDB:ConnectionString"]
            ?? "mongodb://localhost:27017";

        string dbName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME")
            ?? config["MongoDB:DatabaseName"]
            ?? "KICSITLMS";

        var client = new MongoClient(conn);
        Db = client.GetDatabase(dbName);
        await DatabaseInitializer.RunAsync(Db).ConfigureAwait(false);
    }
}

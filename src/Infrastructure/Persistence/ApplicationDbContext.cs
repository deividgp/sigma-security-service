namespace Infrastructure.Persistence;

public class ApplicationDbContext
{
    private readonly IMongoDatabase _database;

    public ApplicationDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);

        IMongoCollection<UserCredential> userCredentialsCollection =
            GetCollection<UserCredential>();

        var indexOptions = new CreateIndexOptions { Unique = true };

        List<CreateIndexModel<UserCredential>> indexModelList =
        [
            new(Builders<UserCredential>.IndexKeys.Ascending(user => user.Username), indexOptions),
            new(Builders<UserCredential>.IndexKeys.Ascending(user => user.Email), indexOptions)
        ];

        userCredentialsCollection.Indexes.CreateMany(indexModelList);
    }

    public IMongoCollection<T> GetCollection<T>()
    {
        return _database.GetCollection<T>(typeof(T).Name.ToLower() + "s");
    }
}

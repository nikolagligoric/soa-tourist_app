using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace Blog.Tests.Integration;

public class MongoFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer;

    public IMongoDatabase Database { get; private set; }

    public MongoFixture()
    {
        _mongoContainer = new MongoDbBuilder()
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();

        var client = new MongoClient(
            _mongoContainer.GetConnectionString());

        Database = client.GetDatabase("blog_test_db");
    }

    public async Task DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
    }
}
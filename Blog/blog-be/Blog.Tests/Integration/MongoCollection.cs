namespace Blog.Tests.Integration;

[CollectionDefinition("Mongo collection")]
public class MongoCollection : ICollectionFixture<MongoFixture>
{
}
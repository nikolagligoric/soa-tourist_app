using Blog.Application.DTOs;
using Blog.Application.Interfaces;
using Blog.Application.Services;
using Blog.Domain.Entities;
using Blog.Infrastructure.Repositories;
using MongoDB.Driver;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Blog.Tests.Integration
{
    [Collection("Mongo collection")]
    public class BlogIntegrationTests
    {

        private readonly MongoFixture _fixture;

        public BlogIntegrationTests(MongoFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task AddComment_WhenFollowingAuthor_SavesComment()
        {
            var collection = _fixture.Database.GetCollection<Blog.Domain.Entities.Blog>("blogs");

            var blog = new Blog.Domain.Entities.Blog
            {
                AuthorUsername = "marko"
            };

            await collection.InsertOneAsync(blog);

            var repo = new BlogRepository(_fixture.Database);
            var httpClient = HttpClientMockFactory.CreateFollowCheckClient(true);
            var service = new BlogService(repo, httpClient);

            var result = await service.AddCommentAsync(
                blog.Id.ToString(),
                new CreateCommentDTO { Text = "Hello" },
                "nikola",
                "token"
            );

            var comments = await repo.GetCommentsByBlogIdAsync(blog.Id.ToString());

            Assert.Single(comments);
        }

        [Fact]
        public async Task AddComment_WhenNotFollowingAuthor_ThrowsException()
        {
            var collection = _fixture.Database.GetCollection<Blog.Domain.Entities.Blog>("blogs");

            await collection.DeleteManyAsync(Builders<Blog.Domain.Entities.Blog>.Filter.Empty);

            var blog = new Blog.Domain.Entities.Blog
            {
                AuthorUsername = "marko"
            };

            await collection.InsertOneAsync(blog);

            var repo = new BlogRepository(_fixture.Database);

            var httpClient = HttpClientMockFactory.CreateFollowCheckClient(false);

            var service = new BlogService(repo, httpClient);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.AddCommentAsync(
                    blog.Id.ToString(),
                    new CreateCommentDTO { Text = "Hello" },
                    "nikola",
                    "token"
                )
            );

            var comments = await repo.GetCommentsByBlogIdAsync(blog.Id.ToString());

            Assert.Empty(comments);
        }

        [Fact]
        public async Task GetCommentsByBlogId_ReturnsStoredComments()
        {
            var collection = _fixture.Database
                .GetCollection<Blog.Domain.Entities.Blog>("blogs");

            await collection.DeleteManyAsync(Builders<Blog.Domain.Entities.Blog>.Filter.Empty);

            var blog = new Blog.Domain.Entities.Blog
            {
                AuthorUsername = "marko",
                Comments = new List<Comment>
        {
            new Comment
            {
                Id = "507f1f77bcf86cd799439011",
                AuthorUsername = "nikola",
                Text = "Test",
                CreatedAt = System.DateTime.UtcNow
            }
        }
            };

            await collection.InsertOneAsync(blog);

            var repo = new BlogRepository(_fixture.Database);

            var service = new BlogService(
                repo,
                HttpClientMockFactory.CreateFollowCheckClient(true)
            );

            var result = await service.GetCommentsByBlogIdAsync(blog.Id.ToString());

            Assert.Single(result);
            Assert.Equal("nikola", result[0].AuthorUsername);
        }

        [Fact]
        public async Task GetBlogsFromFollowing_ReturnsOnlyFollowedAuthors()
        {
            var collection = _fixture.Database
                .GetCollection<Blog.Domain.Entities.Blog>("blogs");

            await collection.DeleteManyAsync(Builders<Blog.Domain.Entities.Blog>.Filter.Empty);

            await collection.InsertManyAsync(new List<Blog.Domain.Entities.Blog>
                {
                    new Blog.Domain.Entities.Blog { AuthorUsername = "marko" },
                    new Blog.Domain.Entities.Blog { AuthorUsername = "ana" },
                    new Blog.Domain.Entities.Blog { AuthorUsername = "john" }
                });

            var repo = new BlogRepository(_fixture.Database);

            var httpClient = HttpClientMockFactory.CreateFollowingListClient(
                new[] { "marko", "ana" }
            );

            var service = new BlogService(repo, httpClient);

            var result = await service.GetBlogsFromFollowingAsync("token", "nikola");

            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.AuthorUsername == "marko");
            Assert.Contains(result, x => x.AuthorUsername == "ana");
        }
    }
}
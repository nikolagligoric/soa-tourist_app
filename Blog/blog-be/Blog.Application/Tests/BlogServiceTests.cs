using Blog.Application.DTOs;
using Blog.Application.Interfaces;
using Blog.Application.Services;
using Blog.Domain.Entities;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace Blog.Application.Tests
{
    public class BlogServiceTests
    {
        private readonly Mock<IBlogRepository> _blogRepositoryMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly BlogService _blogService;

        public BlogServiceTests()
        {
            _blogRepositoryMock = new Mock<IBlogRepository>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);

            _blogService = new BlogService(_blogRepositoryMock.Object, httpClient);
        }

        [Fact]
        public async Task AddCommentAsync_ValidFollower_ReturnsComment()
        {
            // Arrange
            var blogId = "b1";
            var blogAuthor = "marko";
            var commenter = "nikola";
            var dto = new CreateCommentDTO { Text = "Sjajan blog post!" };

            _blogRepositoryMock.Setup(r => r.GetByIdAsync(blogId))
                .ReturnsAsync(new Blog.Domain.Entities.Blog { Id = blogId, AuthorUsername = blogAuthor });

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"isFollowing\": true}")
            };

            _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString().Contains("api/followers/check")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

            _blogRepositoryMock.Setup(r => r.AddCommentAsync(blogId, It.IsAny<Comment>()))
                .ReturnsAsync(new Comment { AuthorUsername = commenter, Text = dto.Text });

            // Act
            var result = await _blogService.AddCommentAsync(blogId, dto, commenter, "token");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Sjajan blog post!", result.Text);

            _blogRepositoryMock.Verify(r => r.AddCommentAsync(blogId, It.IsAny<Comment>()), Times.Once);

            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task AddCommentAsync_NotFollowingAuthor_ThrowsArgumentException()
        {
            // Arrange
            var blogId = "b1";
            var blogAuthor = "marko";
            var commenter = "nikola";
            var dto = new CreateCommentDTO { Text = "Hocu da komentarisem iako te ne pratim" };

            _blogRepositoryMock.Setup(r => r.GetByIdAsync(blogId))
                .ReturnsAsync(new Blog.Domain.Entities.Blog { Id = blogId, AuthorUsername = blogAuthor });

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"isFollowing\": false}")
            };

            _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString().Contains("api/followers/check")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _blogService.AddCommentAsync(blogId, dto, commenter, "token"));

            Assert.Equal("You must follow this user to comment on their blog.", exception.Message);

            _blogRepositoryMock.Verify(r => r.AddCommentAsync(blogId, It.IsAny<Comment>()), Times.Never);
        }
    }
}

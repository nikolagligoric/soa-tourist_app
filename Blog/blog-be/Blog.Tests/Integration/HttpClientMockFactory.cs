using Blog.Application.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Blog.Tests.Integration
{
    public static class HttpClientMockFactory
    {
        public static HttpClient CreateFollowCheckClient(bool isFollowing)
        {
            var handler = new Mock<HttpMessageHandler>();

            var json = JsonSerializer.Serialize(new FollowCheckResponse
            {
                IsFollowing = isFollowing
            });

            Setup(handler, json);

            return new HttpClient(handler.Object)
            {
                BaseAddress = new System.Uri("http://followers")
            };
        }

        public static HttpClient CreateFollowingListClient(string[] users)
        {
            var handler = new Mock<HttpMessageHandler>();

            var json = JsonSerializer.Serialize(users);

            Setup(handler, json);

            return new HttpClient(handler.Object)
            {
                BaseAddress = new System.Uri("http://followers")
            };
        }

        private static void Setup(Mock<HttpMessageHandler> handler, string json)
        {
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
        }
    }
}
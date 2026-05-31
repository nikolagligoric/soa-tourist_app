using System.Text.Json;
using Blog.Application.DTOs;
using Blog.Application.Services;
using NATS.Client;

namespace Blog.API.Messaging
{
    public class TourPublishCommandSubscriber : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;
        private IConnection? _connection;

        public TourPublishCommandSubscriber(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var natsUrl = _configuration["Nats:Url"] ?? "nats://nats:4222";
            var commandSubject = _configuration["Nats:TourPublishCommandSubject"] ?? "tour.publish.command";

            var connectionFactory = new ConnectionFactory();
            _connection = connectionFactory.CreateConnection(natsUrl);

            var subscription = _connection.SubscribeAsync(commandSubject);

            subscription.MessageHandler += (_, args) => HandleCommand(args.Message.Data);
            subscription.Start();

            stoppingToken.Register(() =>
            {
                subscription.Unsubscribe();
                _connection?.Close();
            });

            return Task.CompletedTask;
        }

        private void HandleCommand(byte[] payload)
        {
            TourPublishCommandDTO? command = null;
            TourPublishReplyDTO reply;

            try
            {
                command = JsonSerializer.Deserialize<TourPublishCommandDTO>(payload, _jsonOptions);

                if (command == null || command.Type != "CreateTourBlog")
                {
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var blogService = scope.ServiceProvider.GetRequiredService<BlogService>();

                var blog = blogService.CreateTourAnnouncementBlog(new CreateTourBlogDTO
                {
                    TourId = command.TourId,
                    TourName = command.TourName,
                    TourDescription = command.TourDescription,
                    AuthorUsername = command.AuthorUsername
                });

                reply = new TourPublishReplyDTO
                {
                    TourId = command.TourId,
                    BlogId = blog.Id,
                    Type = "BlogCreated"
                };
            }
            catch (Exception ex)
            {
                reply = new TourPublishReplyDTO
                {
                    TourId = command?.TourId ?? 0,
                    Type = "BlogCreationFailed",
                    FailureReason = ex.Message
                };
            }

            PublishReply(reply);
        }

        private void PublishReply(TourPublishReplyDTO reply)
        {
            var replySubject = _configuration["Nats:TourPublishReplySubject"] ?? "tour.publish.reply";
            var payload = JsonSerializer.SerializeToUtf8Bytes(reply, _jsonOptions);

            _connection?.Publish(replySubject, payload);
            _connection?.Flush();
        }

        public override void Dispose()
        {
            _connection?.Dispose();
            base.Dispose();
        }
    }
}

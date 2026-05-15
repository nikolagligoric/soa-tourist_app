using Blog.Application.DTOs;
using Blog.Application.Interfaces;
using Blog.Domain.Entities;
using System.Net.Http;
using System.Net.Http.Json;


namespace Blog.Application.Services
{
    public class FollowCheckResponse
    {
        public bool IsFollowing { get; set; }
    }
    public class BlogService
    {
        private readonly IBlogRepository _blogRepository;

        public BlogService(IBlogRepository blogRepository)
        {
            _blogRepository = blogRepository;
        }

        public async Task<Blog.Domain.Entities.Blog> CreateBlogAsync(CreateBlogDTO createBlogDto, string authorUsername)
        {
            if (string.IsNullOrWhiteSpace(createBlogDto.Title))
                throw new ArgumentException("Title is required.");

            if (string.IsNullOrWhiteSpace(createBlogDto.Description))
                throw new ArgumentException("Description is required.");

            if (string.IsNullOrWhiteSpace(authorUsername))
                throw new ArgumentException("Author username is required.");

            var imageEntities = new List<BlogImage>();

            if (createBlogDto.Images != null && createBlogDto.Images.Any())
            {
                var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                if (!Directory.Exists(imagesFolder))
                    Directory.CreateDirectory(imagesFolder);

                foreach (var image in createBlogDto.Images)
                {
                    if (image.Length > 0)
                    {
                        var uniqueFileName = $"{Guid.NewGuid()}_{image.FileName}";
                        var filePath = Path.Combine(imagesFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        imageEntities.Add(new BlogImage
                        {
                            ImageUrl = $"/images/{uniqueFileName}"
                        });
                    }
                }
            }

            var blog = new Blog.Domain.Entities.Blog
            {
                Title = createBlogDto.Title,
                Description = createBlogDto.Description,
                CreatedAt = DateTime.UtcNow,
                AuthorUsername = authorUsername,
                Images = imageEntities
            };

            return _blogRepository.Add(blog);
        }

        //comments
        public async Task<Comment> AddCommentAsync(int blogId, CreateCommentDTO createCommentDto, string authorUsername, string token)
        {
            if (string.IsNullOrWhiteSpace(authorUsername))
                throw new ArgumentException("Author username is required.");

            if (createCommentDto == null)
                throw new ArgumentException("Comment data is required.");

            if (string.IsNullOrWhiteSpace(createCommentDto.Text))
                throw new ArgumentException("Comment text is required.");

            var blog = await _blogRepository.GetByIdAsync(blogId);

            if (blog == null)
                throw new ArgumentException("Blog not found.");

            if (authorUsername != blog.AuthorUsername)
            {
                using var client = new HttpClient();

                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(
                    $"http://followers:8082/api/followers/check/{blog.AuthorUsername}"
                );

                if (!response.IsSuccessStatusCode)
                {
                    throw new ArgumentException("Could not verify following status.");
                }

                var result = await response.Content.ReadFromJsonAsync<FollowCheckResponse>();

                if (result == null || !result.IsFollowing)
                {
                    throw new ArgumentException("You must follow this user to comment on their blog.");
                }
            }

            var comment = new Comment
            {
                BlogId = blogId,
                AuthorUsername = authorUsername,
                Text = createCommentDto.Text,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            return await _blogRepository.AddCommentAsync(comment);
        }

        public async Task<List<Comment>> GetCommentsByBlogIdAsync(int blogId)
        {
            var blog = await _blogRepository.GetByIdAsync(blogId);

            if (blog == null)
                throw new ArgumentException("Blog not found.");

            return await _blogRepository.GetCommentsByBlogIdAsync(blogId);
        }

        // Likes
        public int LikeBlog(int blogId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required.");

            var blog = _blogRepository.GetByIdAsync(blogId).GetAwaiter().GetResult();
            if (blog == null)
                throw new ArgumentException("Blog not found.");

            var already = _blogRepository.UserHasLiked(blogId, userId);
            if (already)
                throw new InvalidOperationException("User has already liked this blog.");

            var like = new Like
            {
                BlogId = blogId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _blogRepository.AddLike(like);
            return _blogRepository.GetLikesCount(blogId);
        }

        public int UnlikeBlog(int blogId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required.");

            var blog = _blogRepository.GetByIdAsync(blogId).GetAwaiter().GetResult();
            if (blog == null)
                throw new ArgumentException("Blog not found.");

            var like = _blogRepository.GetLikeByBlogAndUser(blogId, userId);
            if (like == null)
                throw new InvalidOperationException("Like not found for this user on the blog.");

            _blogRepository.RemoveLike(like);
            return _blogRepository.GetLikesCount(blogId);
        }

        public int GetLikesCount(int blogId)
        {
            var blog = _blogRepository.GetByIdAsync(blogId).GetAwaiter().GetResult();
            if (blog == null)
                throw new ArgumentException("Blog not found.");

            return _blogRepository.GetLikesCount(blogId);
        }

        public bool UserHasLiked(int blogId, string userId)
        {
            var blog = _blogRepository.GetByIdAsync(blogId).GetAwaiter().GetResult();
            if (blog == null)
                throw new ArgumentException("Blog not found.");

            return _blogRepository.UserHasLiked(blogId, userId);
        }

        public async Task<List<Blog.Domain.Entities.Blog>> GetBlogsFromFollowingAsync(string token, string username)
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var following = await client.GetFromJsonAsync<List<string>>(
                "http://followers:8082/api/followers/following"
            );

            if (following == null || !following.Any())
            {
                return new List<Blog.Domain.Entities.Blog>();
            }

            if (!following.Contains(username))
            {
                following.Add(username);
            }

            return await _blogRepository.GetBlogsByAuthorsAsync(following);
        }
    }
}
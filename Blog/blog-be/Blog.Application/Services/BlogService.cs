using Blog.Application.DTOs;
using Blog.Application.Interfaces;
using Blog.Domain.Entities;

namespace Blog.Application.Services
{
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
    }
}
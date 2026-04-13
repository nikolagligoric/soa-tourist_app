using Blog.Application.DTOs;
using Blog.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Blog.API.Controllers
{
    [ApiController]
    [Route("api/blogs")]
    public class BlogController : ControllerBase
    {
        private readonly BlogService _blogService;

        public BlogController(BlogService blogService)
        {
            _blogService = blogService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateBlogDTO createBlogDto)
        {
            try
            {
                var authorUsername = User.FindFirst("username")?.Value;

                if (string.IsNullOrWhiteSpace(authorUsername))
                {
                    return Unauthorized("Username claim not found in token.");
                }

                var blog = await _blogService.CreateBlogAsync(createBlogDto, authorUsername);

                var result = new BlogCreatedDto
                {
                    Id = blog.Id,
                    Title = blog.Title,
                    Description = blog.Description,
                    CreatedAt = blog.CreatedAt,
                    AuthorUsername = blog.AuthorUsername,
                    ImageUrls = blog.Images.Select(i => i.ImageUrl).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //comments
        [Authorize]
        [HttpPost("{blogId}/comments")]
        public async Task<IActionResult> AddComment(int blogId, [FromBody] CreateCommentDTO createCommentDto)
        {
            try
            {
                var authorUsername = User.FindFirst("username")?.Value;

                if (string.IsNullOrWhiteSpace(authorUsername))
                {
                    return Unauthorized("Username claim not found in token.");
                }

                var comment = await _blogService.AddCommentAsync(blogId, createCommentDto, authorUsername);

                var result = new CommentDto
                {
                    Id = comment.Id,
                    BlogId = comment.BlogId,
                    AuthorUsername = comment.AuthorUsername,
                    Text = comment.Text,
                    CreatedAt = comment.CreatedAt,
                    LastModifiedAt = comment.LastModifiedAt
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{blogId}/comments")]
        public async Task<IActionResult> GetComments(int blogId)
        {
            try
            {
                var comments = await _blogService.GetCommentsByBlogIdAsync(blogId);

                var result = comments.Select(comment => new CommentDto
                {
                    Id = comment.Id,
                    BlogId = comment.BlogId,
                    AuthorUsername = comment.AuthorUsername,
                    Text = comment.Text,
                    CreatedAt = comment.CreatedAt,
                    LastModifiedAt = comment.LastModifiedAt
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
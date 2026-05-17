using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blog.Domain.Entities;

namespace Blog.Application.Interfaces
{
    public interface IBlogRepository
    {
        Blog.Domain.Entities.Blog Add(Blog.Domain.Entities.Blog blog);
        Task<List<Blog.Domain.Entities.Blog>> GetAllAsync();
        Task<Blog.Domain.Entities.Blog?> GetByIdAsync(string blogId);
        Task<Comment> AddCommentAsync(string blogId, Comment comment);
        Task<List<Comment>> GetCommentsByBlogIdAsync(string blogId);
        Task<List<Blog.Domain.Entities.Blog>> GetBlogsByAuthorsAsync(List<string> authors);

        // Likes
        Like AddLike(string blogId, Like like);
        void RemoveLike(string blogId, Like like);
        int GetLikesCount(string blogId);
        bool UserHasLiked(string blogId, string userId);
        Like? GetLikeByBlogAndUser(string blogId, string userId);
    }
}

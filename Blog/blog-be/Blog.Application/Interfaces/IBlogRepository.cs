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
        Task<Blog.Domain.Entities.Blog?> GetByIdAsync(int blogId);
        Task<Comment> AddCommentAsync(Comment comment);
        Task<List<Comment>> GetCommentsByBlogIdAsync(int blogId);
        Task<List<Blog.Domain.Entities.Blog>> GetBlogsByAuthorsAsync(List<string> authors);

        // Likes
        Like AddLike(Like like);
        void RemoveLike(Like like);
        int GetLikesCount(int blogId);
        bool UserHasLiked(int blogId, string userId);
        Like? GetLikeByBlogAndUser(int blogId, string userId);
    }
}

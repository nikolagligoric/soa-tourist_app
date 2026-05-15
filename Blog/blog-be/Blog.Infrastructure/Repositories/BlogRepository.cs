using Blog.Application.Interfaces;
using Blog.Domain.Entities;
using Blog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Infrastructure.Repositories
{
    public class BlogRepository : IBlogRepository
    {
        private readonly AppDbContext _context;
        public BlogRepository(AppDbContext context)
        {
            _context = context;
        }

        public Blog.Domain.Entities.Blog Add(Blog.Domain.Entities.Blog blog)
        {
            _context.Blogs.Add(blog);
            _context.SaveChanges();
            return blog;
        }

        public async Task<Blog.Domain.Entities.Blog?> GetByIdAsync(int blogId)
        {
            return await _context.Blogs
                .Include(b => b.Images)
                .Include(b => b.Comments)
                .Include(b => b.Likes)
                .FirstOrDefaultAsync(b => b.Id == blogId);
        }

        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<List<Comment>> GetCommentsByBlogIdAsync(int blogId)
        {
            return await _context.Comments
                .Where(c => c.BlogId == blogId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // Likes
        public Like AddLike(Like like)
        {
            _context.Likes.Add(like);
            _context.SaveChanges();
            return like;
        }

        public void RemoveLike(Like like)
        {
            _context.Likes.Remove(like);
            _context.SaveChanges();
        }

        public int GetLikesCount(int blogId)
        {
            return _context.Likes.Count(l => l.BlogId == blogId);
        }

        public bool UserHasLiked(int blogId, string userId)
        {
            return _context.Likes.Any(l => l.BlogId == blogId && l.UserId == userId);
        }

        public Like? GetLikeByBlogAndUser(int blogId, string userId)
        {
            return _context.Likes.FirstOrDefault(l => l.BlogId == blogId && l.UserId == userId);
        }

        public async Task<List<Blog.Domain.Entities.Blog>> GetBlogsByAuthorsAsync(List<string> authors)
        {
            return await _context.Blogs
                .Include(b => b.Images)
                .Include(b => b.Comments)
                .Include(b => b.Likes)
                .Where(b => authors.Contains(b.AuthorUsername))
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }
    }
}

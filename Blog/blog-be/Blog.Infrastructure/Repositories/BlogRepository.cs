using Blog.Application.Interfaces;
using Blog.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Blog.Infrastructure.Repositories
{
    public class BlogRepository : IBlogRepository
    {
        private readonly IMongoCollection<Blog.Domain.Entities.Blog> _blogs;

        public BlogRepository(IMongoDatabase database)
        {
            _blogs = database.GetCollection<Blog.Domain.Entities.Blog>("blogs");
        }

        public Blog.Domain.Entities.Blog Add(Blog.Domain.Entities.Blog blog)
        {
            if (string.IsNullOrWhiteSpace(blog.Id))
            {
                blog.Id = ObjectId.GenerateNewId().ToString();
            }

            _blogs.InsertOne(blog);
            return blog;
        }

        public async Task<List<Blog.Domain.Entities.Blog>> GetAllAsync()
        {
            return await _blogs
                .Find(_ => true)
                .SortByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Blog.Domain.Entities.Blog?> GetByIdAsync(string blogId)
        {
            return await _blogs.Find(b => b.Id == blogId).FirstOrDefaultAsync();
        }

        public async Task<Comment> AddCommentAsync(string blogId, Comment comment)
        {
            if (string.IsNullOrWhiteSpace(comment.Id))
            {
                comment.Id = ObjectId.GenerateNewId().ToString();
            }

            var update = Builders<Blog.Domain.Entities.Blog>.Update
                .Push(b => b.Comments, comment);

            await _blogs.UpdateOneAsync(b => b.Id == blogId, update);
            return comment;
        }

        public async Task<List<Comment>> GetCommentsByBlogIdAsync(string blogId)
        {
            var blog = await _blogs.Find(b => b.Id == blogId).FirstOrDefaultAsync();
            return blog?.Comments
                .OrderByDescending(c => c.CreatedAt)
                .ToList() ?? new List<Comment>();
        }

        // Likes
        public Like AddLike(string blogId, Like like)
        {
            if (string.IsNullOrWhiteSpace(like.Id))
            {
                like.Id = ObjectId.GenerateNewId().ToString();
            }

            var update = Builders<Blog.Domain.Entities.Blog>.Update
                .Push(b => b.Likes, like);

            _blogs.UpdateOne(b => b.Id == blogId, update);
            return like;
        }

        public void RemoveLike(string blogId, Like like)
        {
            if (!string.IsNullOrWhiteSpace(like.Id))
            {
                var updateById = Builders<Blog.Domain.Entities.Blog>.Update
                    .PullFilter(b => b.Likes, l => l.Id == like.Id);

                _blogs.UpdateOne(b => b.Id == blogId, updateById);
                return;
            }

            var updateByUser = Builders<Blog.Domain.Entities.Blog>.Update
                .PullFilter(b => b.Likes, l => l.UserId == like.UserId);

            _blogs.UpdateOne(b => b.Id == blogId, updateByUser);
        }

        public int GetLikesCount(string blogId)
        {
            var blog = _blogs.Find(b => b.Id == blogId).FirstOrDefault();
            return blog?.Likes.Count ?? 0;
        }

        public bool UserHasLiked(string blogId, string userId)
        {
            var filter = Builders<Blog.Domain.Entities.Blog>.Filter.And(
                Builders<Blog.Domain.Entities.Blog>.Filter.Eq(b => b.Id, blogId),
                Builders<Blog.Domain.Entities.Blog>.Filter.ElemMatch(b => b.Likes, l => l.UserId == userId));

            return _blogs.Find(filter).Any();
        }

        public Like? GetLikeByBlogAndUser(string blogId, string userId)
        {
            var blog = _blogs.Find(b => b.Id == blogId).FirstOrDefault();
            return blog?.Likes.FirstOrDefault(l => l.UserId == userId);
        }

        public async Task<List<Blog.Domain.Entities.Blog>> GetBlogsByAuthorsAsync(List<string> authors)
        {
            return await _blogs
                .Find(b => authors.Contains(b.AuthorUsername))
                .SortByDescending(b => b.CreatedAt)
                .ToListAsync();
        }
    }
}

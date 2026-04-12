using Microsoft.AspNetCore.Http;

namespace Blog.Application.DTOs
{
    public class CreateBlogDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; 
        //public int AuthorId { get; set; }
        //public string AuthorUsername { get; set; }
        public List<IFormFile>? Images { get; set; }
    }
}

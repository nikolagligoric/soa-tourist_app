using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Domain.Entities
{
    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        // Autor
        //public int AuthorId { get; set; }
        public string AuthorUsername { get; set; }
        // Slike (opciono)
        public List<BlogImage> Images { get; set; } = new List<BlogImage>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}

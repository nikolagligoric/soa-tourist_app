using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Domain.Entities
{
    public class BlogImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }
}

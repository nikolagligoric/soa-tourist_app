using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Blog.Application.DTOs
{
    public class BlogCreatedDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? TourId { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new();
    }
}

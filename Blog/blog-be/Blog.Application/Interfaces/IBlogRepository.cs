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
    }
}

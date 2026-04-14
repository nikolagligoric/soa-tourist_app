using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stakeholders.Domain.Enums;

namespace Stakeholders.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public bool IsBlocked { get; set; } = false;
        public string? ProfileImageUrl { get; set; }
        public string? Bio { get; set; }
        public string? Motto { get; set; }

    }
}

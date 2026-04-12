using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stakeholders.Application.DTOs
{
    public class UpdateProfileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfileImageUrl { get; set; }
        public string Bio { get; set; }
        public string Motto {  get; set; }
    }
}

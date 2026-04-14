using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stakeholders.Domain.Entities;

namespace Stakeholders.Application.Interfaces
{
    public interface IUserRepository
    {      
        User? GetUserByUsername(string username);
        void AddUser(User user);

        List<User> GetAllUsers();
        User? GetUserById(int id);
        void UpdateUser(User user);
    }
}

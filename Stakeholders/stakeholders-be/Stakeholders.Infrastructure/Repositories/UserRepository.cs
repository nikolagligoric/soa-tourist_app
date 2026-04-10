using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stakeholders.Domain.Entities;
using Stakeholders.Application.Interfaces;
using Stakeholders.Infrastructure.Persistence;

namespace Stakeholders.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public User? GetUserByUsername(string username)
        {
            return _dbContext.Users.FirstOrDefault(u => u.UserName == username);
        }

        public void AddUser(User user)
        {
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }

        public List<User> GetAllUsers()
        {
            return _dbContext.Users.ToList();
        }
    }
}

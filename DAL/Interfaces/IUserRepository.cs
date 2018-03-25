using AquariumMonitor.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AquariumMonitor.DAL.Interfaces
{
    public interface IUserRepository
    {
        Task Add(User user);

        Task Update(User user);

        Task<User> Get(string userName);

        Task<User> Get(int Id);

        Task Delete(int Id);

        Task UpdateLastLogin(int id);

        Task<List<Claim>> GetClaims(int UserId);

        Task<bool> Exists(int userId);
    }
}

using AquariumMonitor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AquariumMonitor.DAL.Interfaces
{
    public interface IPasswordRepository
    {
        Task<List<UserPassword>> Get(int userId);

        Task Add(UserPassword userPassword);
    }
}

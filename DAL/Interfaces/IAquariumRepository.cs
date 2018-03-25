using AquariumMonitor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AquariumMonitor.DAL.Interfaces
{
    public interface IAquariumRepository
    {
        Task<bool> Exists(int userId, int id);

        Task<bool> Exists(string username, int id);

        Task<List<Aquarium>> GetForUser(int userId);

        Task<Aquarium> Get(int userId, int id);

        Task Add(Aquarium aquarium);

        Task Update(Aquarium aquarium);

        Task Delete( int id);
    }
}

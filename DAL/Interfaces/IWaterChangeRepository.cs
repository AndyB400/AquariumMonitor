using AquariumMonitor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AquariumMonitor.DAL.Interfaces
{
    public interface IWaterChangeRepository
    {
        Task Add(WaterChange waterChange);

        Task<List<WaterChange>> GetForAquarium(int userId, int aquariumId);

        Task<WaterChange> Get(int Id);

        Task Update(WaterChange waterChange);

        Task Delete(int Id);
    }
}

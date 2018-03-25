using System.Collections.Generic;
using System.Threading.Tasks;
using AquariumMonitor.Models;

namespace AquariumMonitor.DAL.Interfaces
{
    public interface IMeasurementRepository
    {
        Task<bool> Add(Measurement measurement);

        Task Update(Measurement measurement);

        Task<List<Measurement>> GetForAquarium(int userId, int aquariumId);

        Task<Measurement> Get(int Id);

        Task Delete(int Id);
    }
}

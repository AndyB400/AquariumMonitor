using AquariumMonitor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AquariumMonitor.DAL.Interfaces
{
    public interface IAquariumTypeRepository
    {
        Task<AquariumType> Get(string name);

        Task<List<AquariumType>> Get();
    }
}

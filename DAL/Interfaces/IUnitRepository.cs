using AquariumMonitor.Models;
using System.Threading.Tasks;

namespace AquariumMonitor.DAL.Interfaces
{
    public interface IUnitRepository
    {
        Task<Unit> GetUnitFromName(string name);
    }
}

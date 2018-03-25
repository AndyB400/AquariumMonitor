using AquariumMonitor.Models;
using System.Threading.Tasks;

namespace AquariumMonitor.BusinessLogic.Interfaces
{
    public interface IUnitManager
    {
        Task<Unit> LookUpByName(Unit unit);
    }
}

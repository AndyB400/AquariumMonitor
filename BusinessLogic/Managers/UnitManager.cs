using AquariumMonitor.BusinessLogic.Interfaces;
using AquariumMonitor.DAL.Interfaces;
using AquariumMonitor.Models;
using System.Threading.Tasks;

namespace AquariumMonitor.BusinessLogic
{
    public class UnitManager : IUnitManager
    {
        private readonly IUnitRepository _unitRepository;

        public UnitManager(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
        }

        public async Task<Unit> LookUpByName(Unit unit)
        {
            if (unit != null && unit.Id == 0 && unit.Name != null)
            {
                unit = await _unitRepository.GetUnitFromName(unit.Name);
            }
            else
            {
                unit = null;
            }

            return unit;
        }
    }
}

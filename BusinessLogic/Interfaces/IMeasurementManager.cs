using AquariumMonitor.Models;
using System.Threading.Tasks;

namespace AquariumMonitor.BusinessLogic.Interfaces
{
    public interface IMeasurementManager
    {
        MeasurementType LookupFromName(MeasurementType measurementType);
    }
}

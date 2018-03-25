using AquariumMonitor.BusinessLogic.Interfaces;
using AquariumMonitor.Models;
using AquariumMonitor.Models.Enums;
using System;

namespace AquariumMonitor.BusinessLogic
{
    public class MeasurementManager : IMeasurementManager
    {
        public MeasurementType LookupFromName(MeasurementType type)
        {
            if (type != null && type.Id == 0 && type.Name != null)
            {
                MeasurementTypes lookupType = (MeasurementTypes)Enum.Parse(typeof(MeasurementTypes), type.Name);

                type = new MeasurementType
                {
                    Id = (int)lookupType,
                    Name = lookupType.ToString()
                };
            }

            return type;
        }
    }
}

using AquariumMonitor.BusinessLogic.Interfaces;
using AquariumMonitor.Models;
using System;

namespace AquariumMonitor.BusinessLogic
{
    public class AquariumTypeManager : IAquariumTypeManager
    {
        public AquariumType LookupFromName(AquariumType type)
        {
            if (type != null && type.Id == 0 && type.Name != null)
            {
                AquariumTypes lookupType = (AquariumTypes)Enum.Parse(typeof(AquariumTypes), type.Name);

                type = new AquariumType
                {
                    Id = (int)lookupType,
                    Name = lookupType.ToString()
                };
            }

            return type;
        }
    }
}

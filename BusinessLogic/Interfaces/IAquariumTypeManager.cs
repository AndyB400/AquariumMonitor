using AquariumMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquariumMonitor.BusinessLogic.Interfaces
{
    public interface IAquariumTypeManager
    {
        AquariumType LookupFromName(AquariumType type);
    }
}

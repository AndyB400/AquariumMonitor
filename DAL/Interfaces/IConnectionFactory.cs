using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquariumMonitor.DAL.Interfaces
{
    public interface IConnectionFactory
    {
        DbConnection GetOpenConnection();
    }
}

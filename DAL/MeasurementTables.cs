using System.Collections;
using System.Collections.Generic;

namespace AquariumMonitor.DAL
{
    internal class MeasurementTables : IEnumerable<string>
    {
        private readonly List<string> _data = new List<string>
        {
            "MeasurementNH4",
            "MeasurementNO2",
            "MeasurementPH",
            "MeasurementTemp"
        };

        public IEnumerator<string> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

using System;

namespace AquariumMonitor.Models
{
    public class WaterChange
    {
        public int Id { get; set; }
        public int AquariumId { get; set; }
        public int UserId { get; set; }
        public short PercentageChanged { get; set; }
        public DateTimeOffset Changed { get; set; }
        public byte[] RowVersion { get; set; }
    }
}

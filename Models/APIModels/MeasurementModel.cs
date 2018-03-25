using System;
using System.ComponentModel.DataAnnotations;

namespace AquariumMonitor.APIModels
{
    public class MeasurementModel
    {
        public string Url { get; set; }
        public int AquariumId { get; set; }
        public int UserId { get; set; }

        [Required]
        public string UnitName { get; set; }

        [Required]
        public decimal Value { get; set; }
        public DateTimeOffset Taken { get; set; }

        [Required]
        public string MeasurementTypeName { get; set; }
    }
}

using AquariumAPI.Validation;
using System;
using System.ComponentModel.DataAnnotations;

namespace AquariumMonitor.Models.ViewModels
{
    public class WaterChangeModel : IViewModel
    {
        public string Url { get; set; }
        public int AquariumId { get; set; }
        public int UserId { get; set; }

        [Range(1, 100, ErrorMessage = "{0} must be between {1} and {2}")]
        public short PercentageChanged { get; set; }

        [DateRange(2016, 01, 01, 2032, 01, 01)]
        public DateTimeOffset Changed { get; set; }
    }
}

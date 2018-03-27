using System;
using System.ComponentModel.DataAnnotations;

namespace AquariumMonitor.APIModels
{
    public class AquariumModel : IAPIModel
    {
        public string Url { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "{0} must be between {1} and {2} characters")]
        public string Name { get; set; }
        public DateTimeOffset Created { get; set; }

        [MaxLength(4000, ErrorMessage = "{0} must be a maximum of {1} characters")]
        public string Notes { get; set; }

        [RegularExpression("^[0-9]{1,10}$", ErrorMessage = "{0} must be numeric")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Type Name is required")]
        public string TypeName { get; set; }

        [Range(1, 100000, ErrorMessage = "{0} must be between {1} and {2}")]
        public decimal? OfficalVolume { get; set; }

        [Range(1, 100000, ErrorMessage = "{0} must be between {1} and {2}")]
        public decimal? ActualVolume { get; set; }
        public string VolumeUnitName { get; set; }

        [Range(1, 100000, ErrorMessage = "{0} must be between {1} and {2}")]
        public decimal? Width { get; set; }

        [Range(1, 100000, ErrorMessage = "{0} must be between {1} and {2}")]
        public decimal? Height { get; set; }

        [Range(1, 100000, ErrorMessage = "{0} must be between {1} and {2}")]
        public decimal? Length { get; set; }
        public string DimensionUnitName { get; set; }
        public byte[] RowVersion { get; set; }
    }
}

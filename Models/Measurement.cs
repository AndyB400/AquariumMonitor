using AquariumMonitor.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AquariumMonitor.Models
{
    public class Measurement : IValidatableObject
    {
        public int Id { get; set; }
        public Unit Unit { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        public int AquariumId { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        public DateTimeOffset Taken { get; set; }
        public int UserId { get; set; }
        public MeasurementType MeasurementType { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        //[Range(0, 10, ErrorMessage = "{0} must be between {1} and {2}")]
        public decimal Value { get; set; }

        public byte[] RowVersion { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (MeasurementType.Name == MeasurementTypes.NH4.ToString() && (Value < 0 || Value > 10))
            {
                yield return new ValidationResult("Invalid Value. Value must be between 0 and 10");
            }

            if (MeasurementType.Name == MeasurementTypes.NO2.ToString() && (Value < 0 || Value > 1))
            {
                yield return new ValidationResult("Invalid Value. Value must be between 0 and 1");
            }

            if (MeasurementType.Name == MeasurementTypes.PH.ToString() && (Value < 0 || Value > 14))
            {
                yield return new ValidationResult("Invalid Value. Value must be between 0 and 14");
            }

            if (MeasurementType.Name == MeasurementTypes.Temp.ToString() && (Value < 0 || Value > 50))
            {
                yield return new ValidationResult("Invalid Value. Value must be between 0 and 50");
            }
        }
    }
}

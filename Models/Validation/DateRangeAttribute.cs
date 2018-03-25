using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AquariumAPI.Validation
{
    public sealed class DateRangeAttribute : ValidationAttribute
    {
        private DateTimeOffset StartDate { get; set; }
        private DateTimeOffset EndDate { get; set; }


        public DateRangeAttribute(object startYear, object startMonth, object startDay, object endYear, object endMonth, object endDay)
        {
            StartDate = new DateTimeOffset(new DateTime((int)startYear, (int)startMonth, (int)startDay));
            EndDate = new DateTimeOffset(new DateTime((int)endYear, (int)endMonth, (int)endDay));
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTimeOffset selectedDate = (DateTimeOffset)value;

            if (selectedDate >= StartDate && selectedDate <= EndDate)
                return ValidationResult.Success;

            string[] memberNames = new string[] { validationContext.MemberName };

            return new ValidationResult(
                string.Format("{0} value of '{1:dd-MMM-yyyy}' is invalid. Must be between {2:dd-MMM-yyyy} and {3:dd-MMM-yyyy}",
                validationContext.MemberName, selectedDate, StartDate, EndDate), memberNames);
        }
    }
}

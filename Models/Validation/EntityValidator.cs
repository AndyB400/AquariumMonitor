using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AquariumMonitor.Models.Validation
{
    public static class EntityValidator
    {
        public static List<ValidationResult> Validate(object instance, bool validateAllProperties = true)
        {
            var validationContext = new ValidationContext(instance, null, null);
            var validationResults = new List<ValidationResult>();

            Validator.TryValidateObject(instance, validationContext, validationResults, validateAllProperties);

            return validationResults;
        }
    }
}

using AquariumMonitor.BusinessLogic.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AquariumMonitor.BusinessLogic
{
    public class ValidationManager : IValidationManager
    {
        public List<ValidationResult> Validate(object instance, bool validateAllProperties = true)
        {
            var validationContext = new ValidationContext(instance, null, null);
            var validationResults = new List<ValidationResult>();

            Validator.TryValidateObject(instance, validationContext, validationResults, validateAllProperties);

            return validationResults;
        }
    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AquariumMonitor.BusinessLogic.Interfaces
{
    public interface IValidationManager
    {
        List<ValidationResult> Validate(object instance, bool validateAllProperties = true);
    }
}

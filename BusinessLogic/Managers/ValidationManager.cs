using AquariumMonitor.BusinessLogic.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AquariumMonitor.BusinessLogic
{
    public class ValidationManager : IValidationManager
    {
        public List<ValidationResult> Validate(object instance, bool validateAllProperties = true)
        {
            RegisterMetadataClass(instance);

            var validationContext = new ValidationContext(instance, null, null);
            var validationResults = new List<ValidationResult>();

            Validator.TryValidateObject(instance, validationContext, validationResults, validateAllProperties);

            return validationResults;
        }

        private void RegisterMetadataClass(object instance)
        {
            var modelType = instance.GetType();
            var metadataType = GetMetadataType(modelType);

            if (metadataType != null)
            {
                TypeDescriptor.AddProviderTransparent(new AssociatedMetadataTypeTypeDescriptionProvider(modelType, metadataType), modelType);
            }
        }

        private Type GetMetadataType(Type type)
        {
            var attribute = (MetadataTypeAttribute)type.GetCustomAttributes(typeof(MetadataTypeAttribute), true).FirstOrDefault();
            return attribute == null ? null : attribute.MetadataClassType;
        }
    }
}

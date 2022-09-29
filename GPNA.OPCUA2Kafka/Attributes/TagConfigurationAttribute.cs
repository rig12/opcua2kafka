namespace GPNA.OPCUA2Kafka.Attributes
{
    #region Using
    using Extensions;
    using GPNA.Templates.Constants;
    using Interfaces;
    using System;
    using System.ComponentModel.DataAnnotations;
    #endregion Using


    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TagConfigurationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var entity = value as ITagConfiguration;
            if (entity == default || !entity.IsValid())
            {
                return new ValidationResult(MessageConstants.ENTITY_INCORRECT_FORMAT_TEXT);
            }
            var tagConfigurationManager = validationContext.GetService(typeof(ITagConfigurationManager)) as ITagConfigurationManager;
            
            long.TryParse(entity.GetType().GetProperty("Id")?.GetValue(entity, null)?.ToString(), out long id);
            var previous = tagConfigurationManager?.GetByTagName(entity.ConvertToString());
            if (previous != default && (id == default || id != previous.Id))
            {
                return new ValidationResult(MessageConstants.ENTITY_ALREADY_EXISTS_TEXT);
            }
            return ValidationResult.Success;
        }
    }
}

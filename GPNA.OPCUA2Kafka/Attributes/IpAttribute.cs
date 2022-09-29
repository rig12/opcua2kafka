namespace GPNA.OPCUA2Kafka.Attributes
{
    using GPNA.Templates.Constants;
    #region Using
    using System;
    using System.ComponentModel.DataAnnotations;
    #endregion Using

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class IpAttribute : ValidationAttribute
    {
        public IpAttribute() { }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var ip = value as string;
            if (string.IsNullOrWhiteSpace(ip))
            {
                return new ValidationResult(MessageConstants.PROPERTY_INCORRECT_FORMAT_TEXT);
            }
            string[] arrOctets = ip.Split('.');
            if (arrOctets.Length != 4)
            {
                return new ValidationResult(MessageConstants.PROPERTY_INCORRECT_FORMAT_TEXT);
            }
            foreach (string strOctet in arrOctets)
            {
                if (!byte.TryParse(strOctet, out _))
                {
                    return new ValidationResult(MessageConstants.PROPERTY_INCORRECT_FORMAT_TEXT);
                }
            }
            return ValidationResult.Success;
        }
    }
}

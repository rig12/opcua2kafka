namespace GPNA.OPCUA2Kafka.Attributes
{
    #region Using
    using System;
    using System.ComponentModel.DataAnnotations;
    #endregion Using

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class StringIsLongIdAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if(value == default)
            {
                return false;
            }
            var isParsed = long.TryParse(value.ToString(), out var idLong);
            return isParsed && idLong >= 1;
        }
    }
}

using GPNA.OPCUA2Kafka.Interfaces;
using System.Text.RegularExpressions;

namespace GPNA.OPCUA2Kafka.Extensions
{
    public static class Extensions
    {
        public static string ConvertToString(this ITagConfiguration tagConfiguration)
        {
            return tagConfiguration.Validated() ?
                $"{tagConfiguration.Topic}://{tagConfiguration.Alias}" :
                string.Empty;
        }


        public static bool Validated(this ITagConfiguration tagConfiguration)
        {
            return !string.IsNullOrEmpty(tagConfiguration.ServerUrl)
                && !string.IsNullOrEmpty(tagConfiguration.Tagname)
                && !string.IsNullOrEmpty(tagConfiguration.Topic)
                && !string.IsNullOrEmpty(tagConfiguration.Alias);
        }


        private static bool ValidateTopic(this string topic)
        {
            if (!string.IsNullOrWhiteSpace(topic))
            {
                try
                {
                    var match = Regex.Match(topic, "[a-zA-Z0-9\\._\\-]");
                    return match.Success;
                }
                catch { }
            }
            return false;
        }
    }
}

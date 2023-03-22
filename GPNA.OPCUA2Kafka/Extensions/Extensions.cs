using GPNA.OPCUA2Kafka.Interfaces;
using System.Text.RegularExpressions;

namespace GPNA.OPCUA2Kafka.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagConfiguration"></param>
        /// <returns></returns>
        public static string ConvertToString(this ITagConfiguration tagConfiguration)
        {
            return tagConfiguration.Validated() ?
                $"{tagConfiguration.Topic}://{tagConfiguration.GetNodeTree()}/{tagConfiguration.Alias}" :
                string.Empty;
        }

        /// <summary>
        /// проверка корректности содержания конфигурации тега
        /// </summary>
        /// <param name="tagConfiguration"></param>
        /// <returns></returns>
        public static bool Validated(this ITagConfiguration tagConfiguration)
        {
            return !string.IsNullOrEmpty(tagConfiguration.ServerUrl)
                && !string.IsNullOrEmpty(tagConfiguration.Topic)
                && !string.IsNullOrEmpty(tagConfiguration.Alias);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagConfiguration"></param>
        /// <returns></returns>
        public static string GetNodeTree(this ITagConfiguration tagConfiguration)
        {
            var result = tagConfiguration.ServerUrl.Replace("opc.tcp://", "", System.StringComparison.OrdinalIgnoreCase);
            return result[result.Length - 1] == '/' ?
                string.IsNullOrEmpty(result[..(result.Length - 2)])
                ? result[..(result.Length - 2)]
                : string.Empty
                : result;
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

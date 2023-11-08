namespace OPCUA2Kafka.Extensions
{
    using Converters.TagValues;
    using DataModel.Integration;
    using System;

    public static class TagConfigurationExtensions
    {    
        /// <summary>
        /// копирование свойств из структуры <see cref="TagEvent"/>: <see cref="TagEvent.TimeStamp"/>  <see cref="TagEvent.Quality"/> <see cref="TagEvent.TimeStamp"/>
        /// установка свойства <see cref="TagValue.TagId"/>, <see cref="TagValue.Tagname"/>
        /// </summary>
        /// <param name="tagValue"></param>
        /// <param name="lmxTagEvent"></param>
        public static void FillTagValueData(this TagValue tagValue, TagEvent lmxTagEvent)
        {
            if (lmxTagEvent.TimeStamp is System.DateTime dateTime)
            {
                switch (dateTime.Kind)
                {
                    case System.DateTimeKind.Utc:
                        tagValue.DateTimeUtc = dateTime;
                        tagValue.DateTime = dateTime.ToLocalTime();
                        break;

                    case System.DateTimeKind.Local:
                        tagValue.DateTime = dateTime;
                        tagValue.DateTimeUtc = dateTime.ToUniversalTime();
                        break;

                    case System.DateTimeKind.Unspecified:
                    default:
                        tagValue.DateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
                        tagValue.DateTimeUtc = tagValue.DateTime?.ToUniversalTime();

                        break;
                }
            }
            tagValue.OpcQuality = lmxTagEvent.Quality;
            tagValue.TimeStampUtc = DateTime.Now.ToUniversalTime();
            tagValue.Tagname = lmxTagEvent.Name;
        }
    }
}

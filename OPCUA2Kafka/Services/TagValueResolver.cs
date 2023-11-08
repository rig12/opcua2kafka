using AutoMapper;
using Converters.TagValues;
using MessageQueue.Entities;
using OPCUA2Kafka.Model;

namespace OPCUA2Kafka.Services
{
    public class TagValueResolver : IValueResolver<GenericMessageQueueEntity<TagValue>, TagValueMessageEntity, object>
    {
        public object Resolve(GenericMessageQueueEntity<TagValue> source, TagValueMessageEntity destination, object destMember, ResolutionContext context)
        {
            var valueproperty = source.Payload?.GetType().GetProperty("Value");
            if (valueproperty != null && source.Payload != null)
            {
                return valueproperty.GetValue(source.Payload)?.ToString() ?? string.Empty;
            }
            return string.Empty;
        }

    }

    public class TagValueTopicResolver : IValueResolver<GenericMessageQueueEntity<TagValue>, TagValueMessageEntity, object>
    {

        public object Resolve(GenericMessageQueueEntity<TagValue> source, TagValueMessageEntity destination, object destMember, ResolutionContext context)
        {
            return source.Topic;
        }
    }
}

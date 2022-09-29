using AutoMapper;
using GPNA.Converters.TagValues;
using GPNA.MessageQueue.Entities;
using GPNA.OPCUA2Kafka.Model;

namespace GPNA.OPCUA2Kafka.Services
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

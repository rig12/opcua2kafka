using AutoMapper;
using GPNA.Converters.TagValues;
using GPNA.OPCUA2Kafka.Model;
using System;

namespace GPNA.OPCUA2Kafka.Services
{
    internal class TagValueMessageConverter : IValueConverter<TagValueMessageEntity, TagValue>
    {
        public TagValue Convert(TagValueMessageEntity sourceMember, ResolutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
using AutoMapper;
using Converters.TagValues;
using OPCUA2Kafka.Model;
using System;

namespace OPCUA2Kafka.Services
{
    internal class TagValueMessageConverter : IValueConverter<TagValueMessageEntity, TagValue>
    {
        public TagValue Convert(TagValueMessageEntity sourceMember, ResolutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
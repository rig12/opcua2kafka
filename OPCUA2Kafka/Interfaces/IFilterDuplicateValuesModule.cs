namespace OPCUA2Kafka.Interfaces
{
    using Converters.TagValues;
    using Templates.Interfaces;
    using System.Collections.Generic;

    /// <summary>
    /// Интерфейс модуля фильтрации дублированных данных
    /// </summary>
    public interface IFilterDuplicateValuesModule : IConveyorModule
    {

        /// <summary>
        /// Текущие значения тегов
        /// </summary>
        IReadOnlyDictionary<string, TagValue> CurrentValues { get; }

    }
}

using System.Collections.Generic;
using OPCUA2Kafka.Model;

namespace OPCUA2Kafka.Interfaces
{
    /// <summary>
    /// Интерфейс для управления конфигурациями тегов <see cref="TagConfiguration"/>
    /// </summary>
    public interface ITagConfigurationManager
    {

        /// <summary>
        /// Загрузить конфигурацию
        /// </summary>
        /// <returns>Возвращает true в случае успеха, иначе false</returns>
        bool Load();

        /// <summary>
        /// Конфигурации тегов
        /// </summary>
        IDictionary<string, TagConfigurationEntity> TagConfigurations { get; }

        /// <summary>
        /// получить конфигурацию по имени тега
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        TagConfigurationEntity? GetByTagName(string tagName);

        /// <summary>
        /// Получить конфигурацию по имени тега
        /// </summary>
        /// <param name="tagName">Имя тега</param>
        /// <returns>Возвращает конфигурацию</returns>
        TagConfigurationEntity? GetByAlias(string tagName);

        /// <summary>
        /// Удалить конфигурацию по идентификатору
        /// </summary>
        /// <returns>
        /// Возвращает true в случае успешного удаления, иначе false
        /// </returns>
        bool Delete(long id);

        /// <summary>
        /// Добавить конфигурации
        /// </summary>
        /// <returns>
        /// Возвращает коллекцию объектов <see cref="TagConfigurationEntity"/>
        /// </returns>
        IEnumerable<TagConfigurationEntity> Add(IEnumerable<TagConfiguration> tagConfigurations);

        /// <summary>
        /// Обновить конфигурацию
        /// </summary>
        /// <returns>
        /// Возвращает true в случае успешного обновления, иначе false
        /// </returns>
        bool Update(TagConfigurationEntity tagConfiguration);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagname"></param>
        /// <returns></returns>
        string? ShortenName(string tagname);
    }
}

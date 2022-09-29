namespace GPNA.OPCUA2Kafka.ModelDto
{
    #region Using
    using Interfaces;
    using Attributes;
    using AutoMapper;
    using System.ComponentModel.DataAnnotations;
    using GPNA.OPCUA2Kafka.Model.TagConfiguration;
    #endregion Using


    /// <summary>
    /// Класс Dto конфигурации тега
    /// </summary>
    [MessagePack.MessagePackObject(true)]
    [TagConfiguration]
    [AutoMap(typeof(TagConfiguration), ReverseMap = true)]
    [AutoMap(typeof(TagConfigurationAddDto), ReverseMap = true)]
    public class TagConfigurationDto : ITagConfiguration
    {
        public long Id { get; set; }

        /// <summary>
        /// Ip соединения (напр. 127.0.0.1)
        /// </summary>
        [Required]
        public string Ip { get; set; } = string.Empty;

        /// <summary>
        /// Имя процесса (напр. gateway)
        /// </summary>
        [Required]
        public string ProcessName { get; set; } = string.Empty;

        /// <summary>
        /// Тип источника (напр. system)
        /// </summary>
        [Required]
        public string SourceType { get; set; } = string.Empty;

        /// <summary>
        /// Имя тэга
        /// </summary>
        public string TagName { get; set; } = string.Empty;

        /// <summary>
        /// Топик кафки для публикации
        /// </summary>
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// период дискретности отправки тэга в кафку в секундах (0 - без дискретизации)
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// описание
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// единица измерения
        /// </summary>
        public string EngUnits { get; set; } = string.Empty;

        /// <summary>
        /// отправлять короткое имя
        /// </summary>
        public bool ShortName { get; set; }
        
        /// <summary>
        /// Регулярность принудительного запроса тега по SuiteLink
        /// </summary>
        [Range(0, int.MaxValue)]
        public int RequestPeriodMin { get; set; }
    }
}

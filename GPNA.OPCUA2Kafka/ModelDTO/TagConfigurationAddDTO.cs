namespace GPNA.OPCUA2Kafka.ModelDto
{
    #region Using
    using System.ComponentModel.DataAnnotations;
    using Interfaces;
    using Attributes;
    using AutoMapper;
    using Model;
    using GPNA.OPCUA2Kafka.Model.TagConfiguration;
    #endregion Using

    /// <summary>
    /// Класс Dto добавления конфигурации тега 
    /// </summary>
    [TagConfiguration]
    [AutoMap(typeof(TagConfiguration), ReverseMap = true)]
    public class TagConfigurationAddDto : ITagConfiguration
    {

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
        [Required]
        public string TagName { get; set; } = string.Empty;

        /// <summary>
        /// топик кафки куда отправлять тэг
        /// </summary>
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// период дискретности отправки тэга в кафку в секундах (0 - без дискретизации)
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// описание тэга
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// единица измерения
        /// </summary>
        public string EngUnits { get; set; } = string.Empty;

        /// <summary>
        /// отправлять короткое имя тэга (после !)
        /// </summary>
        public bool ShortName { get; set; }

        
        public int RequestPeriodMin { get; set; }

    }
}

namespace GPNA.OPCUA2Kafka.ModelDto
{
    #region Using
    using AutoMapper;
    using GPNA.Templates.Messages;
    using System;
    #endregion Using

    /// <summary>
    /// Класс Dto статуса объекта
    /// </summary>
    [AutoMap(typeof(MessageStatus))]
    public class MessageStatusDto
    {
        /// <summary>
        /// Сообщение
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Является ли последнее событие исключением
        /// </summary>
        public bool IsException { get; set; }

        /// <summary>
        /// Дата/время последнего события
        /// </summary>
        public DateTime? DateTime { get; set; }
    }
}

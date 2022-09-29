namespace GPNA.OPCUA2Kafka.ModelDto
{
    #region Using
    #endregion Using

    /// <summary>
    /// Класс Dto для передачи значения счетчика
    /// </summary>
    public class CountResultDto<TEntity> where TEntity : struct
    {
        /// <summary>
        /// Значение счетчика
        /// </summary>
        public TEntity Value { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace GPNA.OPCUA2Kafka.Configurations
{
    #region Using
    #endregion Using

    /// <summary>
    /// Конфигурация соединения с базой данных настроек
    /// </summary>
    public class DbConnectionConfiguration
    {
        /// <summary>
        /// Имя конфигурационной БД
        /// </summary>
        public string SettingsDBName { get; set; } = string.Empty;

        /// <summary>
        /// Строка соединения с регистром хранения
        /// </summary>
        public string RegisterConnectionString { get; set; } =
           "Server=localhost;Trusted_Connection=True;";

        /// <summary>
        /// Схема сохранения данных
        /// </summary> 
        public string RegisterSchemaName { get; set; } = "dbo";

        /// <summary>
        /// Таблица сохранения данных
        /// </summary>
        public string RegisterTableName { get; set; } = "ChangeTracking";
        
        /// <summary>
        /// Лимит сохранения данных
        /// </summary>
        [Range(1,1000)]
        public int RegisterSaveLimit { get; set; }
       
    }
}

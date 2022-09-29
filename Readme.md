# Сервис отправки значений тэгов, получаемых по SuiteLink, в kafka 
[GPNA.OPCUA2Kafka](https://dev.azure.com/GPNA/_git/GPNA-OPCUA2Kafka)

<hr />
Документация на Web API доступна на http://<serviceuri>:4444/swagger.

Подписывается на изменения значений тэгов по SuiteLink и производит его отправку в Kafka.
<hr />

## Порядок первоначальной настройки:

Данные параметры конфигурации используются для модулей-конвейеров, выполняющих многопоточную обработку данных. 
HandlersNumber - количество потоков
HandlersLimit - количество сообщений для обработки в потоке за раз
PauseIfFullQueue - пауза в случае полного буфера, размер которого определяется как HandlersNumber * HandlersLimit
ToTraceProcessedCount - ежесекундно вывод количества обработанных модулем сообщений

Внести параметры в appsettings.json:
    1. DbConnectionConfiguration - соединения с базами данных
    1.1 SettingsDBName - имя локальной LiteDB БД для хранения настроек сбора (перечень тэгов TagConfiguration)
    1.2 RegisterConnectionString - строка соединения с БД MS SQL Server промежуточного кэширования
    1.3 RegisterSchemaName - имя схемы к п.1.2
    1.4 RegisterTableName - имя таблицы к п.1.2
    1.5 RegisterSaveLimit - количество для вставки записей за раз к п.1.2

    2. InitializationModuleConfiguration - инициализация запуска модулей
    2.1 Выставить значения true для запуска модуля
    2.2 SuitelinkListenerModule - модуль сбора данных по SuiteLink
    2.3 PeriodizationStarted - модуль установки дискретности данных (поле Period в конфигурации тега)
    2.4 CacheStarted - модуль кэширования (Store&Forward)
    2.5 KafkaSendingModule - модуль отправки данных в kafka
    
    3. KafkaConfiguration - настройка работы с кафкой
    3.1 Brokers - брокеры кафки
    3.2 ClientId - Идентификатор клиент-сервиса
    3.3 MessageTimeoutMs - таймаут установки соединения с кафкой
    3.4 CompressionLevel - уровень сжатия
    3.5 CompressionType - тип сжатия
    3.5 EnableDeliveryReports - подтверждение доставки (false использовать для отправки больших объёмов данных)
    3.6 HandlersNumber
    3.7 HandlersLimit
    3.8 PauseIfFullQueue
    3.9 ToTraceProcessedCount

    4. ConvertConfiguration - Конфигурация конвертации значений

    4.1 ConvertValuesToDouble - задаёт использование типа конвертации - попытка преобразования данных в double (bool -> 0.0/1.0; int -> x.0; string -> null)
    4.2 DatetimeParseFormat - Формат даты для парсинга значения тэга типа "дата-время"
    4.3 ToSendNullValues - отправлять пустые значения в случае невозможности определения типа значения, поступившего из источника

    5. CacheConfiguration - настройка кэширования при недоступности кафки (Store&Forward)
    5.1 HandlersNumber
    5.2 HandlersLimit
    5.3 PauseIfFullQueue
    5.4 ToTraceProcessedCount
    5.5 CacheSendIntervalSec - периодичность сканирования и отправки данных из кэша в кафку, в секундах

    6. FilterConfiguration - Конфигурация фильтра дублированных значений
    6.1 IsEnabled - включить фильтр
    6.2 HandlersNumber
    6.3 HandlersLimit
    6.4 PauseIfFullQueue
    6.5 ToTraceProcessedCount

    7. PeriodizationConfiguration - конфигурация дискретности передачи данных (поле period конфигурации тега)
    7.1 HandlersNumber
    7.2 HandlersLimit
    7.3 PauseIfFullQueue
    7.4 ToTraceProcessedCount


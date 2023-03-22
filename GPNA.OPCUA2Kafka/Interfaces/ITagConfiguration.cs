namespace GPNA.OPCUA2Kafka.Interfaces
{
    /// <summary>
    /// конфигурация тега для сбора по OPC UA
    /// </summary>
    public interface ITagConfiguration
    {
        string ServerUrl { get; set; }
                
        string Topic { get; set; }

        string Alias { get; set; }

        string Node { get; set; }

        int Period { get; set; } 
    }
}

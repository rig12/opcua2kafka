using Opc.Ua;

namespace GPNA.OPCUA2Kafka.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class DataValueTagname
    {
        public DataValue? DataValue { get; set; }
        public string Tagname { get; set; } = string.Empty;
    }
}

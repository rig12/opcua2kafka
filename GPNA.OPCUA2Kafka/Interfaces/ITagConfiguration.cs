namespace GPNA.OPCUA2Kafka.Interfaces
{
    public interface ITagConfiguration
    {
        string ServerUrl { get; set; }

        string Tagname { get; set; } 

        string Topic { get; set; }

        string Alias { get; set; }

        string Node { get; set; }

        int Period { get; set; } 
    }
}

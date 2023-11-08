namespace OPCUA2Kafka.Interfaces
{
    public interface ISchedulerManager
    {
        void StartSchedulers();
        void StopSchedulers();
    }
}

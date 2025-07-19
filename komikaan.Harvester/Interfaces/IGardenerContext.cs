namespace komikaan.Harvester.Interfaces
{
    public interface IGardenerContext
    {
        Task StartAsync(CancellationToken token);
        void SendMessage(object item);
    }
}
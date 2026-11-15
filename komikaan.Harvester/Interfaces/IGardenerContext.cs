namespace komikaan.Harvester.Interfaces
{
    public interface IGardenerContext
    {
        Task StartAsync(CancellationToken token);
        Task SendMessageAsync(object item);
        Task StopAsync(CancellationToken token);
    }
}
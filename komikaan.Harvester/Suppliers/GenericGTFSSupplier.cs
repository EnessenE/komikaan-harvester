using GTFS;
using GTFS.Entities;
using JNogueira.Discord.Webhook.Client;
using komikaan.Harvester.Interfaces;
using komikaan.Harvester.Models;
using System.Collections.Concurrent;

namespace komikaan.Harvester.Suppliers;

public class GenericGTFSSupplier : ISupplier
{
    private readonly DiscordWebhookClient _discordWebHookClient;
    private readonly SupplierConfiguration _supplierConfig;

    public GenericGTFSSupplier(SupplierConfiguration supplierConfig, DiscordWebhookClient discordWebhookClient)
    {
        _discordWebHookClient = discordWebhookClient;
        _supplierConfig = supplierConfig;
    }

    public async Task<GTFSFeed> GetFeedAsync()
    {

        await SendMessageAsync("Started reading GTFS file");
        var reader = new GTFSReader<GTFSFeed>(false, _supplierConfig.Name);
        var feed = reader.Read(_supplierConfig.Url);
        await SendMessageAsync("Finished reading GTFS file");
        Console.WriteLine("Adjusting stops");
        await SendMessageAsync("Starting data adjustment for trips");

        await ChunkMapTripsAsync(feed);

        await SendMessageAsync("Started data adjustment stops");
        await ChunkMapStopsAsync(feed);
        foreach (var agency in feed.Agencies)
        {
            Console.WriteLine("An agency found in this data supplier: {0}", agency.Name);
        }
        Console.WriteLine($"Found a feed with {feed.Agencies.Count} agencies");
        return feed;
    }

    private async Task ChunkMapStopsAsync(GTFSFeed feed)
    {
        var amountPerChunk = 5000;
        var chunks = feed.StopTimes.ToList().Chunk(amountPerChunk);
        Console.WriteLine("Split into {0} chunks of {1}", chunks.Count(), amountPerChunk);
        var tasks = new List<Task>();
        foreach (var chunk in chunks)
        {
            var task = new Task(async () =>
            {
                await MapStops(feed, chunk.ToList());
                Console.WriteLine("FINISHED A STOPTIME TASK");
            });
            task.Start();
            tasks.Add(task);
        }
        Task.WaitAll(tasks.ToArray());
        await Task.CompletedTask;
    }


    private async Task ChunkMapTripsAsync(GTFSFeed feed)
    {
        var amountPerChunk = 5000;
        var chunks = feed.Trips.ToList().Chunk(amountPerChunk);
        Console.WriteLine("Split into {0} chunks of {1}", chunks.Count(), amountPerChunk);
        var tasks = new List<Task>();
        foreach (var chunk in chunks)
        {
            var task = new Task(async () =>
            {
                await MapTrips(feed, chunk.ToList());
            });
            task.Start();
            tasks.Add(task);
        }
        Task.WaitAll(tasks.ToArray());
        await Task.CompletedTask;
    }

    private static Task MapStops(GTFSFeed feed, List<StopTime> stopTimes)
    {
        foreach (var entity in stopTimes)
        {
            var key = entity.StopId.ToLowerInvariant();
            if (feed.Stop_StopTimes.ContainsKey(key))
            {
                feed.Stop_StopTimes[key].Add(entity);
            }
            else
            {
                var added = feed.Stop_StopTimes.TryAdd(key, new ConcurrentBag<StopTime>() { entity });
                if (!added)
                {
                    feed.Stop_StopTimes[key].Add(entity);
                }
            }
        }
        return Task.CompletedTask;
    }

    private static Task MapTrips(GTFSFeed feed, List<Trip> trips)
    {
        foreach (var entity in trips)
        {
            var key = entity.Id.ToLowerInvariant();
            if (feed.StopTime_Trips.ContainsKey(key))
            {
                feed.StopTime_Trips[key].Add(entity);
            }
            else
            {
                var added = feed.StopTime_Trips.TryAdd(key.ToLowerInvariant(), new ConcurrentBag<Trip>() { entity }); 
                if (!added)
                {
                    feed.StopTime_Trips[key].Add(entity);
                }
            }
        }
        return Task.CompletedTask;
    }

    public SupplierConfiguration GetConfiguration()
    {
        return _supplierConfig;
    }

    private async Task SendMessageAsync(string body)
    {
        var message = new DiscordMessage(            "**Import progress for " + _supplierConfig.Name + "**\n" + body,
            username: "Harvester",
            tts: false
        );
        await _discordWebHookClient.SendToDiscord(message);

    }
}
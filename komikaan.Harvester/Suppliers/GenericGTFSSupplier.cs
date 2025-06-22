using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using JNogueira.Discord.WebhookClient;
using komikaan.Common.Models;
using komikaan.GTFS.Models.Static.Models;
using RestSharp;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
namespace komikaan.Harvester.Suppliers;



public class GtfsTimeConverter : ITypeConverter
{
    public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            // Return default TimeSpan (00:00:00) if the field is empty.
            // Or throw an exception if time is always required.
            return default(TimeSpan);
        }

        var parts = text.Split(':');
        if (parts.Length != 3)
        {
            throw new TypeConverterException(this, memberMapData, text, row.Context, "Invalid time format. Expected HH:mm:ss.");
        }

        try
        {
            int hours = int.Parse(parts[0], CultureInfo.InvariantCulture);
            int minutes = int.Parse(parts[1], CultureInfo.InvariantCulture);
            int seconds = int.Parse(parts[2], CultureInfo.InvariantCulture);

            return new TimeSpan(hours, minutes, seconds);
        }
        catch (Exception ex)
        {
            throw new TypeConverterException(this, memberMapData, text, row.Context, "Error converting GTFS time.", ex);
        }
    }

    public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
    {
        if (value is TimeSpan ts)
        {
            // Format back to HH:mm:ss, ensuring hours includes the total hours.
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
        return string.Empty;
    }
}


public partial class GenericGTFSSupplier
{
    private readonly DiscordWebhookClient _discordWebHookClient;
    private ILogger<GenericGTFSSupplier> _logger;
    private static DirectoryInfo _rawPath;
    private static DirectoryInfo _dataPath;

    private Dictionary<string, Type> _filesToDetect = new Dictionary<string, Type>();

    public GenericGTFSSupplier(DiscordWebhookClient discordWebhookClient, ILogger<GenericGTFSSupplier> logger)
    {
        _discordWebHookClient = discordWebhookClient;
        _logger = logger;
    }

    public class AgencyMap : ClassMap<Agency>{
        public AgencyMap()
        {
            Map(m => m.AgencyName).Name("agency_name");
            Map(m => m.AgencyId).Name("agency_id");
        }
    }

    public sealed class StopTimeMap : ClassMap<StopTime>
    {
        public StopTimeMap()
        {
            Map(m => m.TripId).Name("trip_id");
            Map(m => m.ArrivalTime).Name("arrival_time").TypeConverter<GtfsTimeConverter>();
            Map(m => m.DepartureTime).Name("departure_time").TypeConverter<GtfsTimeConverter>();
            Map(m => m.StopId).Name("stop_id");
            Map(m => m.StopSequence).Name("stop_sequence");
            Map(m => m.StopHeadsign).Name("stop_headsign");
            Map(m => m.PickupType).Name("pickup_type");
            Map(m => m.DropOffType).Name("drop_off_type");
            Map(m => m.ContinuousPickup).Name("continuous_pickup");
            Map(m => m.ContinuousDropOff).Name("continuous_drop_off");
            Map(m => m.ShapeDistTraveled).Name("shape_dist_traveled");
            Map(m => m.Timepoint).Name("timepoint");
        }
    }

    public async Task<GTFSFeed> RetrieveFeed(SupplierConfiguration supplierConfig)
    {
        CreateClearDirectories();

        await DownloadFeed(supplierConfig);

        ZipFile.ExtractToDirectory(_rawPath.GetFiles().First().FullName, _dataPath.FullName);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            CacheFields = true,
            HeaderValidated = null,
            MissingFieldFound = null,
        };




        var feed = new GTFSFeed();
        await SendMessageAsync("Started reading GTFS file", supplierConfig);

        using (var reader = new StreamReader($@"{_dataPath.FullName}\agency.txt"))
        using (var csv = new CsvReader(reader, config))
        {
            csv.Context.RegisterClassMap<AgencyMap>();
            var records = csv.GetRecords<Agency>();
            feed.Agencies = records.ToList();
        }
        _logger.LogInformation("Started loading stoptimes");
        var stopwatch = Stopwatch.StartNew();
        using (var reader = new StreamReader($@"{_dataPath.FullName}\stop_times.txt"))
        using (var csv = new CsvReader(reader, config))
        {
            csv.Context.RegisterClassMap<StopTimeMap>();
            var records = csv.GetRecords<StopTime>();
            feed.StopTimes = records.ToList();
        }
        _logger.LogInformation("Finished loading {total} stoptimes in {elapsed}", feed.StopTimes.Count, stopwatch.Elapsed);

        //var feed = await DownloadFeed(reader, supplierConfig);

        //await SendMessageAsync("Finished reading GTFS file", supplierConfig);
        //_logger.LogInformation("Adjusting stops");
        //await SendMessageAsync("Starting data adjustment for trips", supplierConfig);

        //await ChunkMapTripsAsync(feed);

        //await SendMessageAsync("Started data adjustment stops", supplierConfig);
        //await ChunkMapStopsAsync(feed);
        foreach (var agency in feed.Agencies)
        {
            _logger.LogInformation("An agency found in this data supplier: {0}", agency.AgencyName);
        }
        _logger.LogInformation($"Found a feed with {feed.Agencies.Count} agencies");
        return feed;
    }

    private void CreateClearDirectories()
    {
        _rawPath = Directory.CreateDirectory("gtfs_raw");
        _dataPath = Directory.CreateDirectory("gtfs_data");
        _logger.LogInformation("Created {raw} and {data}", _rawPath.FullName, _dataPath.FullName);
        foreach (FileInfo file in _rawPath.GetFiles())
        {
            _logger.LogInformation("Deleting {file}", file.FullName);
            file.Delete();
        }

        foreach (FileInfo file in _dataPath.GetFiles())
        {
            _logger.LogInformation("Deleting {file}", file.FullName);
            file.Delete();
        }
        foreach (DirectoryInfo dir in _rawPath.GetDirectories())
        {
            dir.Delete(true);
        }
        foreach (DirectoryInfo dir in _dataPath.GetDirectories())
        {
            dir.Delete(true);
        }
        _logger.LogInformation("Created/Cleared working directories");
    }

    private async Task DownloadFeed(SupplierConfiguration supplier)
    {
        if (supplier.RetrievalType == RetrievalType.REST)
        {
            var options = new RestClientOptions(supplier.Url);
            var client = new RestClient(options);
            options.UserAgent = $"harvester/komikaan.nl/{GetType().Assembly.GetName().Version} (enes@reasulus.nl)";
            var request = new RestRequest() { Method = Method.Get };
            _logger.LogInformation("Request generated towards {url}", supplier.Url);

            // The cancellation token comes from the caller. You can still make a call without it.
            var response = await client.DownloadDataAsync(request);
            await File.WriteAllBytesAsync(_rawPath+@"\gtfs_file.zip", response);
        }
        else if (supplier.RetrievalType == RetrievalType.LOCAL)
        {
            File.Copy(supplier.Url, _rawPath + @"\gtfs_file.zip");
        }
        else
        {
            throw new NotImplementedException("Unsupported retrievaltype");
        }
    }

    private async Task ChunkMapStopsAsync(GTFSFeed feed)
    {
        //var amountPerChunk = 5000;
        //var chunks = feed.StopTimes.ToList().Chunk(amountPerChunk);
        //_logger.LogInformation("Split into {0} chunks of {1}", chunks.Count(), amountPerChunk);
        //var tasks = new List<Task>();
        //foreach (var chunk in chunks)
        //{
        //    var task = new Task(async () =>
        //    {
        //        await MapStops(feed, chunk.ToList());
        //    });
        //    task.Start();
        //    tasks.Add(task);
        //}
        //Task.WaitAll(tasks.ToArray());
        await Task.CompletedTask;
    }


    private async Task ChunkMapTripsAsync(GTFSFeed feed)
    {
        //var amountPerChunk = 5000;
        //var chunks = feed.Trips.ToList().Chunk(amountPerChunk);
        //_logger.LogInformation("Split into {0} chunks of {1}", chunks.Count(), amountPerChunk);
        //var tasks = new List<Task>();
        //foreach (var chunk in chunks)
        //{
        //    var task = new Task(async () =>
        //    {
        //        await MapTrips(feed, chunk.ToList());
        //    });
        //    task.Start();
        //    tasks.Add(task);
        //}
        //Task.WaitAll(tasks.ToArray());
        await Task.CompletedTask;
    }

    //private static Task MapStops(GTFSFeed feed, List<StopTime> stopTimes)
    //{
    //    foreach (var entity in stopTimes)
    //    {
    //        var key = entity.StopId.ToLowerInvariant();
    //        if (feed.Stop_StopTimes.ContainsKey(key))
    //        {
    //            feed.Stop_StopTimes[key].Add(entity);
    //        }
    //        else
    //        {
    //            var added = feed.Stop_StopTimes.TryAdd(key, new ConcurrentBag<StopTime>() { entity });
    //            if (!added)
    //            {
    //                feed.Stop_StopTimes[key].Add(entity);
    //            }
    //        }
    //    }
    //    return Task.CompletedTask;
    //}

    //private static Task MapTrips(GTFSFeed feed, List<Trip> trips)
    //{
    //    foreach (var entity in trips)
    //    {
    //        var key = entity.Id.ToLowerInvariant();
    //        if (feed.StopTime_Trips.ContainsKey(key))
    //        {
    //            feed.StopTime_Trips[key].Add(entity);
    //        }
    //        else
    //        {
    //            var added = feed.StopTime_Trips.TryAdd(key.ToLowerInvariant(), new ConcurrentBag<Trip>() { entity });
    //            if (!added)
    //            {
    //                feed.StopTime_Trips[key].Add(entity);
    //            }
    //        }
    //    }
    //    return Task.CompletedTask;
    //}

    private async Task SendMessageAsync(string body, SupplierConfiguration supplier)
    {
        _logger.LogInformation(body);
        var message = new DiscordMessage("**Import progress for " + supplier.Name + "**\n" + body,
            username: Environment.MachineName,
            tts: false
        );
        await _discordWebHookClient.SendToDiscordAsync(message);

    }
}
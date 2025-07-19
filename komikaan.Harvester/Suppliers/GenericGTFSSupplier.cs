using CsvHelper;
using CsvHelper.Configuration;
using JNogueira.Discord.WebhookClient;
using komikaan.Common.Models;
using komikaan.GTFS.Models.Static.Models;
using komikaan.Harvester.Adapters;
using komikaan.Harvester.Contexts;
using komikaan.Harvester.Interfaces;
using komikaan.Harvester.Managers;
using RestSharp;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
namespace komikaan.Harvester.Suppliers;


public partial class GenericGTFSSupplier
{
    private readonly DiscordWebhookClient _discordWebHookClient;
    private ILogger<GenericGTFSSupplier> _logger;
    private static DirectoryInfo _rawPath;
    private static DirectoryInfo _dataPath;
    private IDataContext _dataContext;
    private GTFSContext _gtfsContext;
    private IGardenerContext _gardenerContext;

    public GenericGTFSSupplier(DiscordWebhookClient discordWebhookClient, ILogger<GenericGTFSSupplier> logger, IDataContext dataContext, GTFSContext gtfsContext, IGardenerContext gardenerContext)
    {
        _discordWebHookClient = discordWebhookClient;
        _logger = logger;
        _dataContext = dataContext;
        _gtfsContext = gtfsContext;
        _gardenerContext = gardenerContext;
    }

    public async Task<GTFSFeed> RetrieveFeed(SupplierConfiguration supplierConfig)
    {
        await _dataContext.UpdateImportStatusAsync(supplierConfig, "Clearing directories");
        CreateClearDirectories();

        await DownloadFeed(supplierConfig);

        await _dataContext.UpdateImportStatusAsync(supplierConfig, "Extracting files");
        ZipFile.ExtractToDirectory(_rawPath.GetFiles().First().FullName, _dataPath.FullName);
        await _dataContext.UpdateImportStatusAsync(supplierConfig, "Extracting complete, starting reading");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            CacheFields = true,
            HeaderValidated = null,
            MissingFieldFound = null,
        };

        var feed = new GTFSFeed();
        await LogMessage(supplierConfig, "Reading agencies", false);

        var stopwatch = Stopwatch.StartNew();
        if (File.Exists($@"{_dataPath.FullName}\agencies.txt"))
        {
            using (var reader = new StreamReader($@"{_dataPath.FullName}\agency.txt"))
            {
                await LogMessage(supplierConfig, "Importing agencies", false);
                using (var csv = new CsvReader(reader, config))
                {
                    _logger.LogInformation($"Found a file with {csv?.ColumnCount} columns");
                    csv.Context.RegisterClassMap<AgencyMap>();
                    var records = csv.GetRecords<PSQLAgency>().ToList();
                    _logger.LogInformation($"Found a feed with {records?.Count()} agencies");
                    await _gtfsContext.UpsertAgenciesAsync(supplierConfig, records);
                }
            }
        }

        if (File.Exists($@"{_dataPath.FullName}\stop_times.txt"))
        {
            await LogMessage(supplierConfig, "Reading stop_times", false);
            using (var reader = new StreamReader($@"{_dataPath.FullName}\stop_times.txt"))
            {
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Context.RegisterClassMap<StopTimeMap>();
                    var records = csv.GetRecords<PSQLStopTime>();
                    await LogMessage(supplierConfig, "Importing stoptimes", false);
                    await _gtfsContext.UpsertStopTimesAsync(supplierConfig, records);
                }
            }
        }

        if (File.Exists($@"{_dataPath.FullName}\calendar_dates.txt"))
        {
            stopwatch.Restart();
            await LogMessage(supplierConfig, "Reading calendar_dates", false);
            using (var reader = new StreamReader($@"{_dataPath.FullName}\calendar_dates.txt"))
            {
                await LogMessage(supplierConfig, "Importing calendar_dates", false);
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Context.RegisterClassMap<CalendarDateMap>();
                    var records = csv.GetRecords<PSQLCalendarDate>();
                    await _gtfsContext.UpsertCalendarDatesAsync(supplierConfig, records.ToList());
                }
            }

        }


        stopwatch.Restart();
        await LogMessage(supplierConfig, "Reading routes", false);
        using (var reader = new StreamReader($@"{_dataPath.FullName}\routes.txt"))
        {
            await LogMessage(supplierConfig, "Importing routes", false);
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<RouteMap>();
                var records = csv.GetRecords<PSQLRoute>();
                await _gtfsContext.UpsertRoutesAsync(supplierConfig, records.ToList());
            }
        }
        stopwatch.Restart();
        await LogMessage(supplierConfig, "Reading trips", false);
        using (var reader = new StreamReader($@"{_dataPath.FullName}\trips.txt"))
        {
            await LogMessage(supplierConfig, "Importing trips", false);
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<TripMap>();
                var records = csv.GetRecords<PSQLTrip>();
                await _gtfsContext.UpsertTripsAsync(supplierConfig, records.ToList());
            }
        }



        //Stops should be after stoptimes, trips and routes as those are used to determine some on-the-fly data
        stopwatch.Restart();
        await LogMessage(supplierConfig, "Reading stops", false);
        using (var reader = new StreamReader($@"{_dataPath.FullName}\stops.txt"))
        {
            await LogMessage(supplierConfig, "Importing stops", false);
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<StopMap>();
                var records = csv.GetRecords<PSQLStop>();
                var stops = records.ToList();
                await _gtfsContext.UpsertStopsAsync(supplierConfig, stops);
                await NotifyAsync(supplierConfig, stops);
            }
        }

        stopwatch.Restart();
        if (File.Exists($@"{_dataPath.FullName}\shapes.txt"))
        {
            await LogMessage(supplierConfig, "Reading shapes", false);
            using (var reader = new StreamReader($@"{_dataPath.FullName}\shapes.txt"))
            {
                await LogMessage(supplierConfig, "Importing shapes", false);
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Context.RegisterClassMap<ShapeMap>();
                    var records = csv.GetRecords<PSQLShape>();
                    await _gtfsContext.UpsertShapesAsync(supplierConfig, records.ToList());
                }
            }
        }
        stopwatch.Restart();
        if (File.Exists($@"{_dataPath.FullName}\calendar.txt"))
        {
            await LogMessage(supplierConfig, "Reading calendar", false);

            using (var reader = new StreamReader($@"{_dataPath.FullName}\calendar.txt"))
            {
                await LogMessage(supplierConfig, "Importing calendar", false);
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Context.RegisterClassMap<CalendarMap>();
                    var records = csv.GetRecords<PSQLCalendar>();
                    await _gtfsContext.UpsertCalendarsAsync(supplierConfig, records);
                }
            }
        }
        else{
            _logger.LogInformation("No calendar.txt, next");
        }

        //await SendMessageAsync("Finished reading GTFS file", supplierConfig);

        await LogMessage(supplierConfig, "Success", false);
        return feed;
    }

    private async Task LogMessage(SupplierConfiguration supplierConfig, string message, bool discord)
    {
        _logger.LogInformation(message);
        await _dataContext.UpdateImportStatusAsync(supplierConfig, message);
        if (discord) { 
            await SendMessageAsync(message, supplierConfig); 
        }
    }

    private Task NotifyAsync(SupplierConfiguration supplier, List<PSQLStop> stops)
    {
        _logger.LogInformation("Notifying gardeners");
        foreach (var stop in stops)
        {
            _gardenerContext.SendMessage(new GardernerNotification() { Stop = stop, Supplier = supplier.Name });

        }
        return Task.CompletedTask;
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
            await _dataContext.UpdateImportStatusAsync(supplier, "Downloading feed");
            var options = new RestClientOptions(supplier.Url);
            var client = new RestClient(options);
            options.UserAgent = $"harvester/komikaan.nl/{GetType().Assembly.GetName().Version} (enes@reasulus.nl)";
            var request = new RestRequest() { Method = Method.Get };
            _logger.LogInformation("Request generated towards {url}", supplier.Url);

            // The cancellation token comes from the caller. You can still make a call without it.
            var response = await client.DownloadDataAsync(request);
            await File.WriteAllBytesAsync(_rawPath+@"\gtfs_file.zip", response);
            await _dataContext.UpdateImportStatusAsync(supplier, "Feed download complete feed");
        }
        else if (supplier.RetrievalType == RetrievalType.LOCAL)
        {
            await _dataContext.UpdateImportStatusAsync(supplier, "Copying feed");
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
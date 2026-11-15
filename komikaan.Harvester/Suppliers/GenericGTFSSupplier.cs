using CsvHelper;
using CsvHelper.Configuration;
using JNogueira.Discord.WebhookClient;
using komikaan.Common.Models;
using komikaan.GTFS.Models.Static.Models;
using komikaan.Harvester.Adapters;
using komikaan.Harvester.Contexts;
using komikaan.Harvester.Interfaces;
using komikaan.Harvester.Managers;
using komikaan.Harvester.Settings;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using komikaan.Harvester.Models;

namespace komikaan.Harvester.Suppliers;


public class GenericGTFSSupplier
{
    private readonly DiscordWebhookClient _discordWebHookClient;
    private readonly ILogger<GenericGTFSSupplier> _logger;
    private DirectoryInfo _rawPath = null!;
    private DirectoryInfo _dataPath = null!;
    private readonly IDataContext _dataContext;
    private readonly GTFSContext _gtfsContext;
    private readonly IGardenerContext _gardenerContext;
    private readonly HttpClient _httpClient;

    public GenericGTFSSupplier(DiscordWebhookClient discordWebhookClient, ILogger<GenericGTFSSupplier> logger, IDataContext dataContext, GTFSContext gtfsContext, IGardenerContext gardenerContext, HttpClient httpClient, IOptions<KomikaanSettings> komikaanSettings)
    {
        _discordWebHookClient = discordWebhookClient;
        _logger = logger;
        _dataContext = dataContext;
        _gtfsContext = gtfsContext;
        _gardenerContext = gardenerContext;
        _httpClient = httpClient;
        var contactPoint = komikaanSettings.Value.ContactPoint;
        if (string.IsNullOrWhiteSpace(contactPoint))
        {
            throw new ArgumentNullException("contactPoint", "Should be set for the useragent");
        }
        _httpClient.DefaultRequestHeaders.Add("User-Agent", $"detector/komikaan.nl {GetType().Assembly.GetName().Version} ({contactPoint})");
    }

    public async Task RetrieveFeed(ImportRequest supplierConfig, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        await _dataContext.UpdateImportStatusAsync(supplierConfig, "Clearing directories");
        CreateClearDirectories();

        await DownloadFeed(supplierConfig, token);

        var zipFile = _rawPath.GetFiles().FirstOrDefault();
        if (zipFile == null)
        {
            throw new InvalidDataException($"No GTFS zip file was downloaded for {supplierConfig.Name}, aborting import");
        }

        await _dataContext.UpdateImportStatusAsync(supplierConfig, "Extracting files");

        ZipFile.ExtractToDirectory(zipFile.FullName, _dataPath.FullName);
        ResolveDataPath();
        await _dataContext.UpdateImportStatusAsync(supplierConfig, "Extracting complete, starting reading");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            CacheFields = true,
            HeaderValidated = null,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim,
            BadDataFound = null,
            IgnoreBlankLines = true
        };

        await LogMessage(supplierConfig, "Reading agencies", false);

        var stopwatch = Stopwatch.StartNew();
        if (FeedFileExists("agency.txt"))
        {
            using (var reader = OpenFeedFile("agency.txt"))
            {
                await LogMessage(supplierConfig, "Importing agencies", false);
                using (var csv = new CsvReader(reader, config))
                {
                    _logger.LogInformation($"Found a file with {csv.ColumnCount} columns");
                    csv.Context.RegisterClassMap<AgencyMap>();
                    var records = csv.GetRecordsAsync<PSQLAgency>();
                    await _gtfsContext.UpsertAgenciesAsync(supplierConfig, records, token);
                }
            }
        }

        if (FeedFileExists("stop_times.txt"))
        {
            await LogMessage(supplierConfig, "Reading stop_times", false);
            using (var reader = OpenFeedFile("stop_times.txt"))
            {
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Context.RegisterClassMap<StopTimeMap>();
                    var records = csv.GetRecordsAsync<PSQLStopTime>();
                    await LogMessage(supplierConfig, "Importing stoptimes", false);
                    await _gtfsContext.UpsertStopTimesAsync(supplierConfig, records, token);
                }
            }
        }

        if (FeedFileExists("calendar_dates.txt"))
        {
            stopwatch.Restart();
            await LogMessage(supplierConfig, "Reading calendar_dates", false);
            using (var reader = OpenFeedFile("calendar_dates.txt"))
            {
                await LogMessage(supplierConfig, "Importing calendar_dates", false);
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Context.RegisterClassMap<CalendarDateMap>();
                    var records = csv.GetRecordsAsync<PSQLCalendarDate>();
                    await _gtfsContext.UpsertCalendarDatesAsync(supplierConfig, records, token);
                }
            }

        }


        stopwatch.Restart();
        await LogMessage(supplierConfig, "Reading routes", false);
        using (var reader = OpenRequiredFeedFile("routes.txt"))
        {
            await LogMessage(supplierConfig, "Importing routes", false);
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<RouteMap>();
                var records = csv.GetRecordsAsync<PSQLRoute>();
                await _gtfsContext.UpsertRoutesAsync(supplierConfig, records, token);
            }
        }
        stopwatch.Restart();
        await LogMessage(supplierConfig, "Reading trips", false);
        using (var reader = OpenRequiredFeedFile("trips.txt"))
        {
            await LogMessage(supplierConfig, "Importing trips", false);
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<TripMap>();
                var records = csv.GetRecordsAsync<PSQLTrip>();
                await _gtfsContext.UpsertTripsAsync(supplierConfig, records, token);
            }
        }



        //Stops should be after stoptimes, trips and routes as those are used by the gardener to determine on-the-fly data
        stopwatch.Restart();
        await LogMessage(supplierConfig, "Reading stops", false);
        using (var reader = OpenRequiredFeedFile("stops.txt"))
        {
            await LogMessage(supplierConfig, "Importing stops", false);
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<StopMap>();
                var stops = new List<PSQLStop>();
                await foreach (var stop in csv.GetRecordsAsync<PSQLStop>().WithCancellation(token))
                {
                    stops.Add(stop);
                }

                await _gtfsContext.UpsertStopsAsync(supplierConfig, ToAsyncEnumerable(stops, token), token);
                await NotifyAsync(supplierConfig, stops);
            }
        }

        stopwatch.Restart();
        if (FeedFileExists("shapes.txt"))
        {
            await LogMessage(supplierConfig, "Reading shapes", false);
            using (var reader = OpenFeedFile("shapes.txt"))
            {
                await LogMessage(supplierConfig, "Importing shapes", false);
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Context.RegisterClassMap<ShapeMap>();
                    var records = csv.GetRecordsAsync<PSQLShape>();
                    await _gtfsContext.UpsertShapesAsync(supplierConfig, records, token);
                }
            }
        }
        stopwatch.Restart();
        if (FeedFileExists("calendar.txt"))
        {
            await LogMessage(supplierConfig, "Reading calendar", false);

            using (var reader = OpenFeedFile("calendar.txt"))
            {
                await LogMessage(supplierConfig, "Importing calendar", false);
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Context.RegisterClassMap<CalendarMap>();
                    var records = csv.GetRecordsAsync<PSQLCalendar>();
                    await _gtfsContext.UpsertCalendarsAsync(supplierConfig, records, token);
                }
            }
        }
        else
        {
            _logger.LogInformation("No calendar.txt, next");
        }

        stopwatch.Restart();
        if (FeedFileExists("frequencies.txt"))
        {
            await LogMessage(supplierConfig, "Reading frequencies", false);
            using (var reader = OpenFeedFile("frequencies.txt"))
            {
                await LogMessage(supplierConfig, "Importing frequencies", false);
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Context.RegisterClassMap<FrequencyMap>();
                    var records = csv.GetRecordsAsync<PSQLFrequency>();
                    await _gtfsContext.UpsertFrequenciesAsync(supplierConfig, records, token);
                }
            }
        }

        await LogMessage(supplierConfig, "Success", false);
    }

    /// <summary>
    /// Some feeds wrap all txt files in a single top-level folder inside the zip; descend into it
    /// so the feed files are found at the root of <see cref="_dataPath"/>.
    /// </summary>
    private void ResolveDataPath()
    {
        var dir = _dataPath;
        while (dir.GetFiles("*.txt").Length == 0)
        {
            var subDirs = dir.GetDirectories();
            if (subDirs.Length != 1)
            {
                break;
            }
            dir = subDirs[0];
        }

        if (dir.GetFiles("*.txt").Length == 0)
        {
            throw new InvalidDataException($"No GTFS .txt files found in the zip of {StaticImportData.CurrentDataOrigin}");
        }

        _dataPath = dir;
    }

    private bool FeedFileExists(string fileName) => File.Exists(Path.Combine(_dataPath.FullName, fileName));

    private StreamReader OpenFeedFile(string fileName) => new(Path.Combine(_dataPath.FullName, fileName));

    private StreamReader OpenRequiredFeedFile(string fileName)
    {
        var path = Path.Combine(_dataPath.FullName, fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Required GTFS file '{fileName}' is missing in the feed of {StaticImportData.CurrentDataOrigin}", path);
        }
        return new StreamReader(path);
    }

    private async Task LogMessage(ImportRequest supplierConfig, string message, bool discord)
    {
        _logger.LogInformation(message);
        await _dataContext.UpdateImportStatusAsync(supplierConfig, message);
        if (discord)
        {
            await SendMessageAsync(message, supplierConfig);
        }
    }

    private async Task NotifyAsync(ImportRequest supplier, List<PSQLStop> stops)
    {
        _logger.LogInformation("Notifying gardeners");
        foreach (var stop in stops)
        {
            await _gardenerContext.SendMessageAsync(new GardernerNotification() { Stop = stop, Supplier = supplier.Name });

        }
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

    private async Task DownloadFeed(ImportRequest supplier, CancellationToken cancellationToken)
    {
        if (supplier.RetrievalType == RetrievalType.REST)
        {
            await _dataContext.UpdateImportStatusAsync(supplier, "Downloading feed");

            var request = new HttpRequestMessage(HttpMethod.Get, supplier.Url);

            if (!string.IsNullOrWhiteSpace(supplier.HeaderKey))
            {
                request.Headers.Add(supplier.HeaderKey, supplier.HeaderValue);
            }


            _logger.LogInformation("Request generated towards {url}", supplier.Url);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to download feed. Status code: {statusCode}, Error: {errorMessage}",
                    response.StatusCode, response.ReasonPhrase);
                await _dataContext.UpdateImportStatusAsync(supplier,
                    $"Download failed. Status code: {response.StatusCode}");
                throw new HttpRequestException($"Download of {supplier.Url} failed with status code {(int)response.StatusCode} ({response.ReasonPhrase})");
            }

            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            if (data.Length == 0)
            {
                await _dataContext.UpdateImportStatusAsync(supplier, $"Code: {response.StatusCode} but no data received from feed");
                throw new InvalidDataException($"Download of {supplier.Url} returned an empty body");
            }

            await File.WriteAllBytesAsync(Path.Combine(_rawPath.FullName, "gtfs_file.zip"), data, cancellationToken);
            await _dataContext.UpdateImportStatusAsync(supplier, "Feed download complete");
        }
        else if (supplier.RetrievalType == RetrievalType.LOCAL)
        {
            if (!File.Exists(supplier.Url))
            {
                throw new FileNotFoundException($"Local GTFS zip '{supplier.Url}' does not exist", supplier.Url);
            }
            await _dataContext.UpdateImportStatusAsync(supplier, "Copying feed");
            File.Copy(supplier.Url, Path.Combine(_rawPath.FullName, "gtfs_file.zip"), overwrite: true);
        }
        else
        {
            throw new NotImplementedException("Unsupported retrievaltype");
        }
    }
    
    private async Task SendMessageAsync(string body, ImportRequest supplier)
    {
        _logger.LogInformation(body);
        var message = new DiscordMessage("**Import progress for " + supplier.Name + "**\n" + body,
            username: Environment.MachineName,
            tts: false
        );
        await _discordWebHookClient.SendToDiscordAsync(message);

    }

    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> items, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }

        await Task.CompletedTask;
    }
}
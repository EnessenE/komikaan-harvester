using System.Reflection;
using JNogueira.Discord.WebhookClient;
using komikaan.Harvester.Contexts;
using komikaan.Harvester.Interfaces;
using komikaan.Harvester.Managers;
using komikaan.Harvester.Suppliers;
using Serilog;

namespace komikaan.Harvester
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddEnvironmentVariables();

            Log.Logger = new LoggerConfiguration()
                 .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            builder.Host.UseSerilog();
            Log.Logger.Information("Starting {app} {version} - {env}",
                Assembly.GetExecutingAssembly().GetName().Name,
                Assembly.GetExecutingAssembly().GetName().Version,
                builder.Environment.EnvironmentName);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();



            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            builder.Services.AddSingleton<HarvestingManager>();
            builder.Services.AddHostedService(x => x.GetRequiredService<HarvestingManager>());
            FixDiscordClient(builder.Services);

            var rmqFeatureFlag = builder.Configuration?.GetValue<bool?>("FeatureFlags:RabbitMQ");
            if (rmqFeatureFlag.HasValue && rmqFeatureFlag.Value == false)
            {
                builder.Services.AddSingleton<IGardenerContext, FakeGardener>();
            }
            else
            {
                builder.Services.AddSingleton<IGardenerContext, GardenerContext>();
            }


            builder.Services.AddSingleton<GenericGTFSSupplier>();
            builder.Services.AddHostedService<DetectorContext>();
            builder.Services.AddSingleton<IDataContext, PostgresContext>();
            builder.Services.AddSingleton<GTFSContext>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseSerilogRequestLogging();

            app.MapControllers();

            app.Run();
        }

        private static void FixDiscordClient(IServiceCollection services)
        {
            services.AddHttpClient<DiscordWebhookHttpClient>("DiscordWebhookHttpClient", delegate (HttpClient client)
            {
                client.BaseAddress = new Uri("https://discord.com/api/webhooks/1249326974883725343/hMojhofrsjrsY9Sl0kvaffMh6RGDltMe5W6sDrunND3zppONAI8Y00HaEUcfy7QumsOJ");
                client.Timeout = TimeSpan.FromSeconds(30.0);
            });
            services.AddSingleton<DiscordWebhookClient>();
        }
    }
}

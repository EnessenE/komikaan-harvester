using System.Reflection;
using JNogueira.Discord.Webhook.Client;
using komikaan.Harvester.Contexts;
using komikaan.Harvester.Helpers;
using komikaan.Harvester.Interfaces;
using komikaan.Harvester.Managers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

            builder.Services.AddHostedService<HarvestingManager>();
            builder.Services.AddSingleton(x => x.GetRequiredService<HarvestingManager>());
            var client = new DiscordWebhookClient("https://discord.com/api/webhooks/1249326974883725343/hMojhofrsjrsY9Sl0kvaffMh6RGDltMe5W6sDrunND3zppONAI8Y00HaEUcfy7QumsOJ");
            builder.Services.AddSingleton(client);

            builder.Services.AddSingleton<GardenerContext>();
            builder.Services.AddSingleton<DetectorContext>();
            builder.Services.AddSingleton<IDataContext, PostgresContext>();
            builder.Services.AddDbContext<GTFSContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("HarvestingTarget"), o => o.UseNetTopologySuite());
                options.UseSnakeCaseNamingConvention();
                options.ReplaceService<ISqlGenerationHelper, NpgsqlSqlGenerationLowercasingHelper>();
            }, optionsLifetime: ServiceLifetime.Singleton, contextLifetime: ServiceLifetime.Singleton); 
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
    }
}

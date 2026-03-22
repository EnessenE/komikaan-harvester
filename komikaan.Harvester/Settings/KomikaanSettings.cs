namespace komikaan.Harvester.Settings;

public class KomikaanSettings
{
    public const string SectionName = "Komikaan";

    public required string ContactPoint { get; set; }
    public required string DiscordWebhookUrl { get; set; }
}
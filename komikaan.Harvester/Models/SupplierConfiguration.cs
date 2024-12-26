namespace komikaan.Common.Models;

public class SupplierConfiguration
{
    public required RetrievalType RetrievalType { get; set; }
    public required SupplierType DataType { get; set; }
    public required TimeSpan PollingRate { get; set; }

    [System.ComponentModel.DataAnnotations.Key]
    public required string Name { get; set; }
    public required string Url { get; set; }
    public required string? ETag { get; set; }
    public required Guid ImportId { get; set; }
    public required Guid LatestSuccesfullImportId { get; set; }

    public DateTimeOffset LastUpdated { get; set; }
    public DateTimeOffset? LastAttempt { get; set; }
    public DateTimeOffset? LastChecked { get; set; }

    /// <summary>
    /// If marked as pending, a file detecter should ignore this supplier
    /// </summary>
    public bool DownloadPending { get; set; }
}

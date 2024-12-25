using System.ComponentModel.DataAnnotations;

namespace komikaan.Common.Models;

public class SupplierTypeMapping
{
    [Key]
    public string SupplierConfigurationName { get; set; }
    [Key]
    public int ListedType { get; set; }

    public int NewType { get; set; }
}

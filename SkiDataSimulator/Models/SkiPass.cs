using System.Data.SqlTypes;

namespace SkidataWpf.Models;

public class SkiPass
{
    public int Id { get; set; }
    public int? CardNumber { get; set; } // Till exempel e8e1761b-ef76-40f8-86e9-eed3c5865a2c
    public int SkierId { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public int DestinationId { get; set; }

}

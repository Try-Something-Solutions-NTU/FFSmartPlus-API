using System.ComponentModel.DataAnnotations;

namespace Domain;

public class DoorCode
{
    public long id { get; set; }
    public long Code { get; set; }
    public long SupplierId { get; set; }
    public Supplier Supplier { get; set; }

}
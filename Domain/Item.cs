namespace Domain;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string UnitDesc { get; set; }
    public double RestockTime { get; set; }
    public double DesiredStock { get; set; }
    public ICollection<Unit> Units { get; set; }
}
namespace Domain;

public class Item
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string UnitDesc { get; set; }

    public ICollection<Unit> Units { get; set; }
    public bool Active { get; set; } = true;

}
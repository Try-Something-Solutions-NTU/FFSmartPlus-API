namespace Domain;

public class Supplier
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string Email { get; set; }
    
    public ICollection<Item> Items { get; set; }
}
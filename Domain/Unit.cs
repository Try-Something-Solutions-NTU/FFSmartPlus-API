namespace Domain;

public class Unit
{
    public int Id { get; set; }
    public double Quantity { get; set; }
    public DateTime ExpiryDate { get; set; }
    
    public int ItemId { get; set; }
    public Item Item { get; set; }
    
    public int FridgeId { get; set; }
    public Fridge Fridge { get; set; }
}
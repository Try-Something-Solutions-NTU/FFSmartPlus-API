namespace Domain;

public class Unit
{
    public int ID { get; set; }
    public double Quantity { get; set; }
    public DateTime ExpiryDate { get; set; }
    public Item Item { get; set; }
    public Fridge Fridge { get; set; }
    
}
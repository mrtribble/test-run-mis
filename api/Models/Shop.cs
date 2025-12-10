namespace api.Models;

public class Shop
{
    public int ShopID { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public DateTime DateEntered { get; set; }
    public bool Favorited { get; set; }
    public bool Deleted { get; set; }
}


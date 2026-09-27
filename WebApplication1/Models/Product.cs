namespace WebApplication1.data
{
    public class Product
    {
        public int Id { get; set; } // Automatically treated as Primary Key
        public string rawmaterial { get; set; }
        public string quantity { get; set; }
        public string category { get; set; }
    }
}
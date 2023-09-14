namespace MyKendoApp.Models
{
    // Read from Master Database or some API call
    public class Order
    {
        public string OrderId { get; set; }
        public string OrderName { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public int OrderTotal { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}

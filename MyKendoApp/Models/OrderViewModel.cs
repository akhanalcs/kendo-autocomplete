namespace MyKendoApp.Models
{
    public class OrderViewModel
    {
        public string OrderId { get; set; }
        public string OrderName { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public int OrderTotal { get; set; }
        // Not set in the backend
        public string CustomerFullName { get; set; }
        public bool IsExpensive { get; set; }
        public string OrderDesc { get; set; }
    }
}

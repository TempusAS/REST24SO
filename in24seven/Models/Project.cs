namespace in24seven.Models
{
    public class Project
    {
        public string Id  { get; set; }
        public string Name { get; set; }
        public int CustomerId { get; set; }
        public bool MultiCustomer { get; set; }
    }
}
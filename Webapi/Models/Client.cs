namespace Webapi.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }

        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}

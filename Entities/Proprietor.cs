using LaunderManagerClient.Entities;
using System.Collections.Generic;

namespace LaunderManagerClient.Entities
{
    public class Proprietor : BaseMessageDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal TotalEarnings { get; set; }
        public ICollection<Laundry> Laundries { get; set; } = new List<Laundry>();
    }
}
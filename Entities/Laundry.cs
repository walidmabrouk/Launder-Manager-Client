using System.Collections.Generic;

namespace LaunderManagerClient.Entities
{
    public class Laundry
    {
        public int Id { get; set; }
        public int ProprietorId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Earnings { get; set; }
        public List<Machine> Machines { get; set; } = new List<Machine>();
    }
}
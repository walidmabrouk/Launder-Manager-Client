using LaunderManagerClient.Entities;
using System.Collections.Generic;

namespace LaunderManagerClient.Entities
{
    public class Machine
    {
        public int Id { get; set; }
        public int LaundryId { get; set; }
        public string SerialNumber { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
        public decimal Earnings { get; set; }
        public List<Cycle> Cycles { get; set; } = new List<Cycle>();
    }
}
using System;

namespace LaunderManagerClient.Entities
{
    public class Cycle
    {
        public int Id { get; set; }
        public int MachineId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Text.Json;
using LaunderManagerClient.Entities;
using LaunderManagerClient.Converters;
using System.Threading.Tasks;
using LaverieClient.Services;
using LaunderManagerClient.Entities;

namespace LaverieClient
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Initial Setup: Laundry Proprietors");

            var proprietor = new Proprietor
            {
                Name = ReadInput("Proprietor Name: "),
                Email = ReadInput("Proprietor Email: ")
            };

            proprietor.Laundries = CollectLaundries();

            if (!ValidationService.ValidateProprietor(proprietor))
            {
                Console.WriteLine("Invalid input data. Setup aborted.");
                return;
            }

            var notification = new
            {
                Type = "Configuration",
                proprietor.Id,
                proprietor.Name,
                proprietor.Email,
                TotalEarnings = 0,
                proprietor.Laundries
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new TimeSpanConverter() }
            };

            string jsonData = JsonSerializer.Serialize(notification, options);
            Console.WriteLine("\nSerialized Data:");
            Console.WriteLine(jsonData);

            string serverUrl = "ws://localhost:5049/api/configuration";
            var webSocketManager = new WebSocketManager(serverUrl);

            try
            {
                await webSocketManager.ConnectAsync();
                await webSocketManager.SendAsync(jsonData);
                await webSocketManager.ReceiveAsync();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            Console.ReadLine();
        }

        static string ReadInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine()?.Trim() ?? string.Empty;
        }

        static List<Laundry> CollectLaundries()
        {
            Console.Write("Number of Laundries: ");
            if (!int.TryParse(Console.ReadLine(), out int numLaundries) || numLaundries <= 0)
            {
                Console.WriteLine("Invalid input. Defaulting to 1 laundry.");
                numLaundries = 1;
            }

            var laundries = new List<Laundry>();
            for (int i = 0; i < numLaundries; i++)
            {
                Console.WriteLine($"\nLaundry {i + 1}");
                var laundry = new Laundry
                {
                    Name = ReadInput("Laundry Name: "),
                    Address = ReadInput("Laundry Address: "),
                    Machines = CollectMachines()
                };
                laundries.Add(laundry);
            }
            return laundries;
        }

        static List<Machine> CollectMachines()
        {
            Console.Write("Number of Machines: ");
            if (!int.TryParse(Console.ReadLine(), out int numMachines) || numMachines <= 0)
            {
                Console.WriteLine("Invalid input. Defaulting to 1 machine.");
                numMachines = 1;
            }

            var machines = new List<Machine>();
            for (int i = 0; i < numMachines; i++)
            {
                Console.WriteLine($"  Machine {i + 1}");
                var machine = new Machine
                {
                    Type = ReadInput("    Type: "),
                    SerialNumber = ReadInput("    Serial Number: "),
                    State = ReadInput("    State (Active/Inactive): "),
                    Cycles = CollectCycles()
                };
                machines.Add(machine);
            }
            return machines;
        }

        static List<Cycle> CollectCycles()
        {
            Console.Write("    Number of Cycles: ");
            if (!int.TryParse(Console.ReadLine(), out int numCycles) || numCycles <= 0)
            {
                Console.WriteLine("Invalid input. Defaulting to no cycles.");
                return new List<Cycle>();
            }

            var cycles = new List<Cycle>();
            for (int i = 0; i < numCycles; i++)
            {
                Console.WriteLine($"      Cycle {i + 1}");
                var cycle = new Cycle
                {
                    Name = ReadInput("        Name: "),
                    Duration = ParseDuration(ReadInput("        Duration (hh:mm:ss): ")),
                    Price = (decimal)(double.TryParse(ReadInput("        Price: "), out double price) ? price : 0)
                };
                cycles.Add(cycle);
            }
            return cycles;
        }

        static TimeSpan ParseDuration(string input)
        {
            if (TimeSpan.TryParse(input, out TimeSpan duration))
            {
                return duration;
            }
            else
            {
                Console.WriteLine("Invalid duration format. Defaulting to 00:00:00.");
                return TimeSpan.Zero;
            }
        }
    }
}
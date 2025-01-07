using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using LaunderManagerClient.Entities;
using LaunderManagerClient.Converters;
using LaverieClient.Services;

namespace LaverieClient
{
    public class Program
    {
        private static WebSocketManager _webSocketManager;
        private const string ServerUrl = "ws://localhost:5049/api/configuration";

        static async Task Main(string[] args)
        {
            _webSocketManager = new WebSocketManager(ServerUrl);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Laundry Management System");
                Console.WriteLine("1. Add Configuration");
                Console.WriteLine("2. View Proprietor Details");
                Console.WriteLine("0. Exit");

                Console.Write("\nSelect an option: ");
                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        await AddConfiguration();
                        break;

                    case "2":
                        ViewProprietorDetails();
                        break;

                    case "0":
                        Console.WriteLine("Exiting...");
                        return;

                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }

                Console.WriteLine("\nPress Enter to return to the main menu...");
                Console.ReadLine();
            }
        }

        static async Task AddConfiguration()
        {
            Console.Clear();
            Console.WriteLine("Add Configuration:");

            var proprietor = new Proprietor
            {
                Name = ReadInput("Proprietor Name: "),
                Email = ReadInput("Proprietor Email: ")
            };

            proprietor.Laundries = CollectLaundries();

            if (!ValidationService.ValidateProprietor(proprietor))
            {
                Console.WriteLine("Invalid input data. Configuration aborted.");
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

            try
            {
                await _webSocketManager.ConnectAsync();
                await _webSocketManager.SendAsync(jsonData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static async Task ViewProprietorDetails()
        {
            Console.Clear();
            Console.WriteLine("View Proprietor Details:");

            string serverUrl = "ws://localhost:5049/api/proprietor-details";
            var webSocketManager = new WebSocketManager(serverUrl);

            try
            {
                await webSocketManager.ConnectAsync();
                var proprietors = await webSocketManager.ReceiveAsync();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("[INFO] Received Proprietors Data:");
                Console.ResetColor();

                foreach (var proprietor in proprietors)
                {
                    Console.WriteLine("****************************************");
                    Console.WriteLine($"\n  ID Proprietor: {proprietor.Id} ");
                    Console.WriteLine($"  Proprietor Name: {proprietor.Name}");
                    Console.WriteLine($"    Total Earnings: {proprietor.TotalEarnings}");

                    foreach (var laundry in proprietor.Laundries)
                    {
                        Console.WriteLine($"      Laundry Name: {laundry.Name} - Laundry Address: {laundry.Address} - (Earnings: {laundry.Earnings})");

                        foreach (var machine in laundry.Machines)
                        {
                            Console.WriteLine($"       Machine ID: {machine.Id}  - Machine Type: {machine.Type} , Machine State: {machine.State} - Machine Earnings: {machine.Earnings}");
                        }
                    }
                }
                Console.WriteLine("==================================================================================================\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
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

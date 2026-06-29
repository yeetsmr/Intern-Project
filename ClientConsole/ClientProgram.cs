using Microsoft.AspNetCore.Builder;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ClientConsole
{
    public enum Priorty
    {
        low,
        mid,
        high
    }

    public class FilterDto
    {
        public string? TaskNameContains { get; set; }
        public double? MaxEstimatedTime { get; set; }
        public bool? IsCompleted { get; set; }
        public Priorty? SelectedPriority { get; set; }
        public DateTime? CreatedAfter { get; set; }
    }

    public class Tasks
    {
        public string? Id { get; set; }
        public string TaskName { get; set; } = null!;
        public double MaxTime { get; set; }
        public bool IsCompleted { get; set; }
        public Priorty Pri { get; set; }
        public DateTime CreatedAfter { get; set; }
    }



    class Code
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.File("Logs/api-log-.txt", rollingInterval: RollingInterval.Day);
            });
            string baseUrl = "https://localhost:7271/api/tasks";

            try
            {
                Console.WriteLine("Generating 50 mock tasks... Please wait.");

                Random rnd = new Random();

                string[] actions = { "Review", "Update", "Refactor", "Test", "Implement", "Design", "Fix bugs in", "Deploy" };
                string[] topics = { "MongoDB queries", "C# bindings", "API endpoints", "Git branches", "Unit tests", "WPF UI", "SQL tables" };

                int successCount = 0;

                for (int i = 1; i <= 50; i++)
                {

                    string randomAction = actions[rnd.Next(actions.Length)];
                    string randomTopic = topics[rnd.Next(topics.Length)];

                    var randomTask = new Tasks
                    {
                        TaskName = $"{randomAction} {randomTopic} (Auto-{i})",
                        MaxTime = Math.Round(rnd.NextDouble() * 10 + 0.5, 1),
                        IsCompleted = rnd.Next(2) == 0,
                        Pri = (Priorty)rnd.Next(3),
                        CreatedAfter = DateTime.Now.AddDays(-rnd.Next(1, 30))
                    };


                    var res = await client.PostAsJsonAsync(baseUrl, randomTask);

                    if (res.IsSuccessStatusCode)
                    {
                        successCount++;

                        Console.Write("\rAdding data: " + successCount + "/50");
                    }
                }

                Console.WriteLine($"\n\n-> {successCount} test datas added successfully!");

                Console.WriteLine("\n------------------------------------------------");
                Console.WriteLine("reading operations...");
                Console.WriteLine("Please choose a task to filter by (1, 2, or 3):");
                Console.WriteLine("\n1:Task name \n2:Max Estimated Time \n3:Priority");

                int choice = Convert.ToInt32(Console.ReadLine());

                ColumnDescriptor descriptor = new ColumnDescriptor();
                switch (choice)
                {
                    case 1:
                        Console.WriteLine("Please enter the task name to filter by:");
                        string taskName = Console.ReadLine();
                        descriptor.ColumnName = "TaskNameContains";
                        descriptor.DisplayName = "Task Name";
                        descriptor.DataType = typeof(string);
                        descriptor.SelectedOperator = "Contains";
                        descriptor.FilterValue = taskName;
                        break;

                    case 2:
                        Console.WriteLine("Please enter the maximum estimated time to filter by:");
                        if (double.TryParse(Console.ReadLine(), out double maxTime))
                        {
                            descriptor.ColumnName = "MaxEstimatedTime";
                            descriptor.DisplayName = "MaxTime";
                            descriptor.DataType = typeof(double);
                            descriptor.SelectedOperator = "Lte";
                            descriptor.FilterValue = maxTime;
                        }
                        else
                        {
                            Console.WriteLine("Invalid input for maximum estimated time.");
                            return;
                        }
                        break;

                    case 3:
                        Console.WriteLine("Please enter the priority to filter by (low, mid, high):");
                        string priorityInput = Console.ReadLine();
                        if (Enum.TryParse<Priorty>(priorityInput, true, out Priorty selectedPriority))
                        {
                            descriptor.ColumnName = "SelectedPriority";
                            descriptor.DisplayName = "Priority";
                            descriptor.DataType = typeof(Priorty);
                            descriptor.SelectedOperator = "Eq";
                            descriptor.FilterValue = selectedPriority;
                        }
                        else
                        {
                            Console.WriteLine("Invalid priority input.");
                            return;
                        }
                        break;

                    default:
                        Console.WriteLine("Please make a valid choice.");
                        return;
                }

                var filterMaker = new FilterDto
                {
                    TaskNameContains = descriptor.ColumnName == "TaskNameContains" ? descriptor.FilterValue.ToString() : null,
                    MaxEstimatedTime = descriptor.ColumnName == "MaxEstimatedTime" ? (double?)descriptor.FilterValue : null,
                    SelectedPriority = descriptor.ColumnName == "SelectedPriority" ? (Priorty?)descriptor.FilterValue : null
                };

                Console.WriteLine("\nSending request to API with dynamic filter...");

                HttpResponseMessage response = await client.PostAsJsonAsync($"{baseUrl}/filter", filterMaker);

                if (response.IsSuccessStatusCode)
                {
                    var gelenTasklar = await response.Content.ReadFromJsonAsync<List<Tasks>>();

                    Console.WriteLine("\n=== DYNAMIC FILTER RESULTS ===");
                    if (gelenTasklar != null && gelenTasklar.Count > 0)
                    {
                        foreach (var t in gelenTasklar)
                        {
                            Console.WriteLine($"- Task: {t.TaskName}\n  Time: {t.MaxTime} | Priority: {t.Pri} | Completed: {t.IsCompleted}");
                        }
                        Console.WriteLine($"\nTotal found: {gelenTasklar.Count}");
                    }
                    else
                    {
                        Console.WriteLine("No tasks found matching the criteria.");
                    }
                }
                else
                {
                    Console.WriteLine($"API Error: {response.StatusCode}");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection Error: {ex.Message}");
            }

            Console.WriteLine("\nProgram completed. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
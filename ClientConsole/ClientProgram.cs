using Microsoft.AspNetCore.Builder;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static BenchmarkDotNet.Engines.EngineEventSource;

namespace ClientConsole
{
    public enum Priority { low, mid, high }
    public enum Category { Work, Personal, Shopping, Health, Finance, Education, Travel, Entertainment, Other }

    public class Tasks
    {
        public string? Id { get; set; }
        public string TaskName { get; set; } = null!;
        public string EmployeeName { get; set; } = null!;
        public int Category { get; set; }
        public double EstimatedCost { get; set; }
        public int StoryPoints { get; set; }
        public int SprintNumber { get; set; }
        public double MaxTime { get; set; }
        public int pri { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAfter { get; set; }
        public DateTime? DueDate { get; set; }
    }

    class Code
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task Main(string[] args)
        {

            string baseUrl = "https://localhost:7271/api/tasks";

            try
            {
                Console.WriteLine("Generating 200 detailed mock tasks... Please wait.");

                Random rnd = new Random();

                string[] actions = { "Review", "Update", "Refactor", "Test", "Implement", "Design", "Fix bugs in", "Deploy", "Optimize" };
                string[] topics = { "MongoDB queries", "C# bindings", "API endpoints", "Git branches", "Unit tests", "WPF UI", "SQL tables", "Docker containers" };
                string[] employees = { "Yiğit", "Ahmet", "Ayşe", "Fatma", "Mehmet", "Can", "Zeynep", "Elif" };
                int[] storyPointsArr = { 1, 2, 3, 5, 8, 13, 21 };

                int successCount = 0;

                for (int i = 1; i <= 200; i++)
                {
                    string randomAction = actions[rnd.Next(actions.Length)];
                    string randomTopic = topics[rnd.Next(topics.Length)];

                    DateTime createdDate = DateTime.Now.AddDays(-rnd.Next(1, 60));

                    var randomTask = new Tasks
                    {
                        TaskName = $"{randomAction} {randomTopic} (Auto-{i})",
                        EmployeeName = employees[rnd.Next(employees.Length)],
                        Category = rnd.Next(9),
                        EstimatedCost = Math.Round(rnd.NextDouble() * 4900 + 100, 2),
                        StoryPoints = storyPointsArr[rnd.Next(storyPointsArr.Length)],
                        SprintNumber = rnd.Next(1, 11),
                        MaxTime = Math.Round(rnd.NextDouble() * 10 + 0.5, 1),
                        IsCompleted = rnd.Next(2) == 0,
                        IsActive = rnd.Next(2) == 0,
                        pri = rnd.Next(3),
                        CreatedAfter = createdDate,
                        DueDate = createdDate.AddDays(rnd.Next(3, 30))
                    };

                    var res = await client.PostAsJsonAsync(baseUrl, randomTask);

                    if (res.IsSuccessStatusCode)
                    {
                        successCount++;
                        Console.Write($"\rAdding data: {successCount}/200");
                    }
                    else
                    {
                        var errorDetail = await res.Content.ReadAsStringAsync(); 
                        Console.WriteLine($"Failed to add task {randomTask.TaskName}. Status: {res.StatusCode}");
                        Console.WriteLine($"Hata Detayı: {errorDetail}\n");
                    }
                }

                Console.WriteLine($"\n\n-> {successCount} detailed test datas added successfully to MongoDB!");
                Console.WriteLine("\nYou can now open your WPF Application and test all new filters.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection Error: {ex.Message}");
                Console.WriteLine("Make sure your Backend API project is running!");
            }

            Console.WriteLine("\nProgram completed. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
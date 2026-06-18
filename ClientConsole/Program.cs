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

    public class task
    {
        public string? Id { get; set; }
        public string TaskName { get; set; } = null!;
        public double MaxTime { get; set; }
        public bool IsCompleted { get; set; }
        public Priorty pri { get; set; }
        public DateTime CreatedAfter { get; set; }
    }



    class Code
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task Main(string[] args)
        {
            string baseUrl = "https://localhost:7271/api/tasks";

            try
            {
                var task1 = new task
                {
                    TaskName = "Complete MongoDB CRUD operations",
                    MaxTime = 2.5,
                    IsCompleted = true,
                    pri = Priorty.high
                };
                var res1 = await client.PostAsJsonAsync(baseUrl, task1);

                var task2 = new task
                {
                    TaskName = "Review C# code examples",
                    MaxTime = 4.0,
                    IsCompleted = false,
                    pri = Priorty.mid
                };
                var res2 = await client.PostAsJsonAsync(baseUrl, task2);

                var task3 = new task
                {
                    TaskName = "Pushing git changes of MongoDB operations",
                    MaxTime = 0.5,
                    IsCompleted = false,
                    pri = Priorty.high
                };
                var res3 = await client.PostAsJsonAsync(baseUrl, task3);

                if (res1.IsSuccessStatusCode && res2.IsSuccessStatusCode && res3.IsSuccessStatusCode)
                {
                    Console.WriteLine("-> Test datas added successfully.");
                }
                else
                {
                    Console.WriteLine("-> Error occurred while adding test data.");
                }

                Console.WriteLine("\nreading operations...");

                Console.WriteLine("Please choose a task to filter by (1, 2, or 3):");
                Console.WriteLine("\n1:Task name \n 2:Max Estimated Time \n 3:Priority");

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

                Console.WriteLine("Sending request to API with dynamic filter...");

                HttpResponseMessage response = await client.PostAsJsonAsync($"{baseUrl}/search", filterMaker);

                if (response.IsSuccessStatusCode)
                {
                    var gelenTasklar = await response.Content.ReadFromJsonAsync<List<task>>();

                    Console.WriteLine("\n=== DYNAMIC FILTER RESULTS ===");
                    if (gelenTasklar != null && gelenTasklar.Count > 0)
                    {
                        foreach (var t in gelenTasklar)
                        {
                            Console.WriteLine($"- Task: {t.TaskName}\n Time: {t.MaxTime} \n Priority: {t.pri} \n Completed: {t.IsCompleted}");
                        }
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

            Console.WriteLine("\nProgram completed.");
            Console.ReadLine();
        }
    }
}

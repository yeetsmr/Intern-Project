using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace ClientConsole
{
    class Program
    {
        private const int TEST_DURATION_SECONDS = 60;
        private const string BASE_API_URL = "https://localhost:7271/api/tasks/filter-dynamic";
        private const string JWT_TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFkbWluIiwicm9sZSI6IkFkbWluIiwibmJmIjoxNzgzNjkyNDgxLCJleHAiOjE3ODM2OTk2ODEsImlhdCI6MTc4MzY5MjQ4MSwiaXNzIjoiQmFja2VuZEFQSSIsImF1ZCI6IldQRkNsaWVudCJ9.AvXGGjYq2PPGaPYVaqI93SqYKGq_FiIqiaha_GTZFTw";

        private static int _successfulRequests = 0;
        private static int _failedRequests = 0;
        private static long _totalLatencyMs = 0;
        private static readonly HttpClient _httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            string cleanToken = JWT_TOKEN.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Replace("\n", "").Replace("\r", "").Trim();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cleanToken);


            Console.WriteLine("STRESS TEST FOR SPEED");


            Console.Write("Enter the number of concurrent users.(Ex: 10, 100, 500, 1000) : ");
            if (!int.TryParse(Console.ReadLine(), out int concurrentUsers)) concurrentUsers = 1000;

            Console.Write("Enter the number of filters to be sent. (Ex: 2, 4, 8, 10): ");
            if (!int.TryParse(Console.ReadLine(), out int filterCount)) filterCount = 10;

            var payload = GenerateDynamicPayload(filterCount);

            string[] mappingMethods = { "Reflection", "CET", "Direct" };

            Console.WriteLine($"\nTest: {concurrentUsers} Users | {filterCount} Filter");

            var warmupTasks = new List<Task>();
            for (int i = 0; i < 50; i++)
            {
                foreach (var method in mappingMethods)
                {
                    warmupTasks.Add(_httpClient.PostAsJsonAsync($"{BASE_API_URL}?mappingMethod={method.ToLower()}", payload));
                }
            }
            try { await Task.WhenAll(warmupTasks); } catch { }
            await Task.Delay(3000);

            foreach (var method in mappingMethods)
            {
                await RunBenchmarkForMethod(method, payload, concurrentUsers);

                if (method != mappingMethods.Last())
                {

                    await Task.Delay(3000);
                }
            }

            Console.WriteLine("All tests completed. Press Enter to exit.");
            Console.ReadLine();
        }

        private static async Task RunBenchmarkForMethod(string methodName, object payload, int concurrentUsers)
        {
            _successfulRequests = 0;
            _failedRequests = 0;
            _totalLatencyMs = 0;

            string targetUrl = $"{BASE_API_URL}?mappingMethod={methodName.ToLower()}";

            Console.WriteLine($"Starting benchmark for: [{methodName}] Mapping...");

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TEST_DURATION_SECONDS));
            var tasks = new List<Task>();
            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < concurrentUsers; i++)
            {
                tasks.Add(Task.Run(() => SendRequestsAsync(_httpClient, targetUrl, payload, cts.Token)));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Exception]: {ex.Message}");
            }

            stopwatch.Stop();
            PrintTestResults(methodName, stopwatch.Elapsed.TotalSeconds);
        }

        private static async Task SendRequestsAsync(HttpClient client, string url, object payload, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var reqStopwatch = Stopwatch.StartNew();
                try
                {
                    var response = await client.PostAsJsonAsync(url, payload, token);
                    reqStopwatch.Stop();

                    if (response.IsSuccessStatusCode)
                    {
                        Interlocked.Increment(ref _successfulRequests);
                        Interlocked.Add(ref _totalLatencyMs, reqStopwatch.ElapsedMilliseconds);
                    }
                    else
                    {
                        Interlocked.Increment(ref _failedRequests);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception) { Interlocked.Increment(ref _failedRequests); }
            }
        }

        private static void PrintTestResults(string methodName, double elapsedSeconds)
        {
            int totalReq = _successfulRequests;
            double rps = totalReq / elapsedSeconds;
            double avgLatency = totalReq > 0 ? (double)_totalLatencyMs / totalReq : 0;

            Console.WriteLine($"RESULTS FOR {methodName}");
            Console.WriteLine($"Successful Requests : {_successfulRequests}");
            Console.WriteLine($"Failed Requests     : {_failedRequests}");
            Console.WriteLine($"Throughput (RPS)    : {rps:F2} req/sec");
            Console.WriteLine($"Avg Latency         : {avgLatency:F2} ms");
            Console.WriteLine("-------------------------------------------------");
        }

        private static object GenerateDynamicPayload(int filterCount)
        {
            var allFilters = new List<object>
            {
                new { Member = "EstimatedCost", Operator = 0, Value = 1000 },
                new { Member = "TaskName", Operator = "contains", Value = "MongoDB" },
                new { Member = "IsCompleted", Operator = "IsEqualTo", Value = false },
                new { Member = "IsActive", Operator = "IsEqualTo", Value = true },
                new { Member = "StoryPoints", Operator = "IsEqualTo", Value = 5 },
                new { Member = "SprintNumber", Operator = "IsEqualTo", Value = 2 },
                new { Member = "Category", Operator = "IsEqualTo", Value = "Development" },
                new { Member = "DueDate", Operator = "IsGreaterThan", Value = DateTime.UtcNow.AddDays(7) },
                new { Member = "CreatedAfter", Operator = "IsLessThan", Value = DateTime.UtcNow.AddDays(-7) },
                new { Member = "Priority", Operator = "IsEqualTo", Value = "High" }
            };

            var selectedFilters = allFilters.Take(filterCount).ToList();

            return new { Take = 50, Skip = 0, Filter = new { Logic = "or", Filters = selectedFilters } };
        }
    }
}
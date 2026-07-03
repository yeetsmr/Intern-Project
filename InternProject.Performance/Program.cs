using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ClientConsole
{
    class Program
    {

        private const int CONCURRENT_USERS = 1000;
        private const int TEST_DURATION_SECONDS = 300; 
        private const string API_URL = "https://localhost:7271/api/tasks/filter-dynamic";
        private const string JWT_TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFkbWluIiwicm9sZSI6IkFkbWluIiwibmJmIjoxNzgzMDg0OTE5LCJleHAiOjE3ODMwOTIxMTksImlhdCI6MTc4MzA4NDkxOSwiaXNzIjoiQmFja2VuZEFQSSIsImF1ZCI6IldQRkNsaWVudCJ9.xy5asa06hOSnXBUrFL3fKstxT5P4EUIyKvaxmXwDfXQ";

        private static int _successfulRequests = 0;
        private static int _failedRequests = 0;
        private static readonly HttpClient _httpClient = new HttpClient();

        static async Task Main(string[] args)
        {

            Console.WriteLine($"Concurrent Users: {CONCURRENT_USERS}");
            Console.WriteLine($"Test Duration: {TEST_DURATION_SECONDS}");
            Console.WriteLine($"Target Endpoint: {API_URL}");

            string cleanToken = JWT_TOKEN.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Replace("\n", "").Replace("\r", "").Trim();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cleanToken);
            Console.WriteLine("Please wait for the payload...\n");

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TEST_DURATION_SECONDS));
            var payload = GenerateHeavyPayload();
            var tasks = new List<Task>();

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < CONCURRENT_USERS; i++)
            {
                tasks.Add(Task.Run(() => SendRequestsAsync(_httpClient, payload, cts.Token)));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Exception]: {ex.Message}\n");
            }

            stopwatch.Stop();
            PrintTestResults(stopwatch.Elapsed.TotalSeconds);
        }

        private static async Task SendRequestsAsync(HttpClient client, object payload, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var response = await client.PostAsJsonAsync(API_URL, payload, token);

                    if (response.IsSuccessStatusCode)
                    {
                        Interlocked.Increment(ref _successfulRequests);
                    }
                    else
                    {
                        int currentFails = Interlocked.Increment(ref _failedRequests);
                        if (currentFails == 1) 
                        {
                            var authHeader = response.Headers.WwwAuthenticate.ToString();
                            string errorDetail = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"\n[error occured] Status Code: {response.StatusCode}");
                            if (!string.IsNullOrEmpty(authHeader)) Console.WriteLine($"[JWT notification]: {authHeader}");
                            if (!string.IsNullOrEmpty(errorDetail)) Console.WriteLine($"[server respond]: {errorDetail}\n");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break; 
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref _failedRequests);
                }
            }
        }

        private static object GenerateHeavyPayload()
        {
            var filters = new List<object>();
            for (int i = 0; i < 45; i++)
            {
                filters.Add(new
                {
                    Member = "EstimatedCost",
                    Operator = 0, 
                    Value = 1000 + i
                });
            }

            return new
            {
                Take = 50,
                Skip = 0,
                Filter = new
                {
                    Logic = "or",
                    Filters = filters
                }
            };
        }

        private static void PrintTestResults(double elapsedSeconds)
        {
            int totalRequests = _successfulRequests + _failedRequests;
            double rps = totalRequests / elapsedSeconds;

 
            Console.WriteLine($"Elapsed Seconds   : {elapsedSeconds:F2} ");
            Console.WriteLine($"Requests : {totalRequests}");
            Console.WriteLine($"HTTP 200: {_successfulRequests}");
            Console.WriteLine($"HTTP 500/400: {_failedRequests}");
            Console.WriteLine($"Throughput : {rps:F2} req/sec (RPS)");
            Console.ReadLine(); 

        }
    }
}
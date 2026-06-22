using InternProject.WPF;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Windows;

namespace ClientWPF
{
    public class FilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;

        public string? TaskNameContains { get; set; }
        public double? MaxTime { get; set; }
        public int? pri { get; set; }
        public bool? IsCompleted { get; set; }
    }

    public class TaskViewModel
    {
        public string Id { get; set; } = null!;
        public string TaskName { get; set; } = null!;
        public double MaxTime { get; set; }
        public int pri { get; set; }
        public bool IsCompleted { get; set; }

        public string PriorityText
        {
            get
            {
                if (pri == 0) return "low";
                if (pri == 1) return "mid";
                if (pri == 2) return "high";
                return "Unknown";
            }
        }
    }

    public partial class MainWindow : Window
    {
        private const string ApiUrl = "https://localhost:7271/api/tasks/filter";
        private int currentPage = 1;

        public MainWindow()
        {
            InitializeComponent();

            // FetchDataWithCurrentPage(); 
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            currentPage = 1;
            FetchDataWithCurrentPage();
        }

        private void btnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                FetchDataWithCurrentPage();
            }
        }

        private void btnNextPage_Click(object sender, RoutedEventArgs e)
        {
            currentPage++;
            FetchDataWithCurrentPage();
        }

        private void FetchDataWithCurrentPage()
        {
            txtPageInfo.Text = $"Page: {currentPage}";
            btnPrevPage.IsEnabled = currentPage > 1;

            var filter = new FilterDto();

            if (!string.IsNullOrWhiteSpace(txtTaskName.Text))
                filter.TaskNameContains = txtTaskName.Text;

            if (double.TryParse(txtMaxTime.Text, out double parsedTime))
                filter.MaxTime = parsedTime;

            string priText = txtPriority.Text.Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(priText))
            {
                if (priText == "low") filter.pri = 0;
                else if (priText == "mid") filter.pri = 1;
                else if (priText == "high") filter.pri = 2;
                else
                {
                    MessageBox.Show("Please enter a valid priority value:\nlow, mid or high", "Invalid Filter");
                    return;
                }
            }

            filter.PageNumber = currentPage;
            filter.PageSize = 12;

            LoadDataFromApi(filter);
        }

        private async void LoadDataFromApi(FilterDto currentFilter)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    if (!string.IsNullOrEmpty(TokenStore.Token))
                    {
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", TokenStore.Token);
                    }
                    else
                    {
                        MessageBox.Show("First, you need to log in!", "Unauthorized Access");
                        return;
                    }

                    HttpResponseMessage response = await client.PostAsJsonAsync(ApiUrl, currentFilter);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonText = await response.Content.ReadAsStringAsync();

                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var taskList = System.Text.Json.JsonSerializer.Deserialize<List<TaskViewModel>>(jsonText, options);

                        TaskListView.ItemsSource = taskList;

                        if (taskList.Count < currentFilter.PageSize)
                        {
                            btnNextPage.IsEnabled = false;
                        }
                        else
                        {
                            btnNextPage.IsEnabled = true;
                        }

                        if (taskList.Count == 0 && currentPage > 1)
                        {
                            currentPage--;
                            FetchDataWithCurrentPage();
                            return;
                        }
                    }
                    else
                    {
                        string errorDetail = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"API connection error: {response.StatusCode}\n\nDetail:\n{errorDetail}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("API connection error! Please make sure the API project is running.\nError: " + ex.Message);
            }
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            using (HttpClient client = new HttpClient())
            {
                var loginDto = new
                {
                    Username = "admin",
                    Password = "123456"
                };

                var response = await client.PostAsJsonAsync("https://localhost:7271/api/auth/login", loginDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    TokenStore.Token = result.Token;

                    MessageBox.Show("Login successful! Access token obtained.", "Access Granted");

                    FetchDataWithCurrentPage();
                }
                else
                {
                    MessageBox.Show("Invalid username or password!", "Error");
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;

namespace ClientWPF
{
    public partial class MainWindow : Window
    {
        private const string ApiUrl = "https://localhost:7271/api/tasks/filter";

        public MainWindow()
        {
            InitializeComponent();

            LoadDataFromApi(new FilterDto());
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            var filter = new FilterDto();

            if (!string.IsNullOrWhiteSpace(txtTaskName.Text))
            {
                filter.TaskNameContains = txtTaskName.Text;
            }

            if (double.TryParse(txtMaxTime.Text, out double parsedTime))
            {
                filter.MaxTime = parsedTime;
            }

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

            LoadDataFromApi(filter);
        }

        private async void LoadDataFromApi(FilterDto currentFilter)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.PostAsJsonAsync(ApiUrl, currentFilter);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonText = await response.Content.ReadAsStringAsync();

                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var taskList = System.Text.Json.JsonSerializer.Deserialize<List<TaskViewModel>>(jsonText, options);

                        TaskListView.ItemsSource = taskList;
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
    }

    public class FilterDto
    {
        public string? TaskNameContains { get; set; } 
        public double? MaxTime { get; set; }
        public int? pri { get; set; }
        public bool? IsCompleted { get; set; }
    }

    public class TaskViewModel
    {
        public string Id { get; set; }
        public string TaskName { get; set; }
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
}
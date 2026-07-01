using InternProject.WPF;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;

namespace ClientWPF
{

    public class DataSourceRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public CompositeFilterDescriptor Filter { get; set; } = new CompositeFilterDescriptor();
        public List<SortDescriptor> Sorts { get; set; } = new List<SortDescriptor>();
    }

    public class CompositeFilterDescriptor
    {
        public List<FilterDescriptor> FilterDescriptors { get; set; } = new List<FilterDescriptor>();
        public FilterCompositionLogicalOperator LogicalOperator { get; set; }
    }

    public class FilterDescriptor
    {
        public string Member { get; set; } = null!;
        public object Value { get; set; } = null!;
        public FilterOperator Operator { get; set; }
    }

    public enum FilterCompositionLogicalOperator { And, Or }

    public enum FilterOperator
    {
        IsLessThan,
        IsLessThanOrEqualTo,
        IsEqualTo,
        IsNotEqualTo,
        IsGreaterThanOrEqualTo,
        IsGreaterThan,
        StartsWith,
        EndsWith,
        Contains,
        DoesNotContain,
        IsContainedIn,
        IsNotContainedIn,
        IsNull,
        IsNotNull,
        IsEmpty,
        IsNotEmpty
    }

    public class SortDescriptor
    {
        public string Member { get; set; } = null!;
        public string SortDirection { get; set; } = null!;
    }


    public class TaskViewModel
    {

        public string Id { get; set; } = null!;
        public string TaskName { get; set; } = null!;

        public double? MaxTime { get; set; }
        public int? pri { get; set; }
        public bool? IsCompleted { get; set; }
        public DateTime? CreatedAfter { get; set; }

        public string AssigneeName { get; set; }
        public int? Category { get; set; }
        public double? EstimatedCost { get; set; }
        public int? StoryPoints { get; set; }
        public int? SprintNumber { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? DueDate { get; set; }

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

        public string CategoryText
        {
            get
            {
              
                if (Category == 0) return "Work";
                if (Category == 1) return "Personal";
                if (Category == 2) return "Shopping";
                if (Category == 3) return "Health";
                if (Category == 4) return "Finance";
                if (Category == 5) return "Education";
                if (Category == 6) return "Travel";
                if (Category == 7) return "Entertainment";
                if (Category == 8) return "Other";
                return "Unknown";
            }
        }
    }

    public partial class MainWindow : Window
    {
        private const string ApiUrl = "https://localhost:7271/api/tasks/filter-dynamic";
        private int currentPage = 1;

        private string activeSortColumn = "";
        private string activeSortDirection = "Ascending";
        private int sortState = 0;
        private System.Windows.Controls.GridViewColumn? _activeColumn = null;

        public MainWindow()
        {
            InitializeComponent();
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

        private void AutoFilter_Changed(object sender, RoutedEventArgs e)
        {
            currentPage = 1;
            FetchDataWithCurrentPage();
        }

        private void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var header = e.OriginalSource as System.Windows.Controls.GridViewColumnHeader;

            if (header == null || header.Role == System.Windows.Controls.GridViewColumnHeaderRole.Padding || header.Column == null)
                return;

            string rawHeader = header.Column.Header?.ToString()?.Replace(" ▲", "")?.Replace(" ▼", "") ?? "";
            string clickedColumn = header.Tag?.ToString() ?? rawHeader.Replace(" ", "");

            switch (clickedColumn)
            {
                case "TaskID": clickedColumn = "Id"; break;
                case "TaskName": clickedColumn = "TaskName"; break;
                case "Assignee": clickedColumn = "AssigneeName"; break;
                case "Category": clickedColumn = "Category"; break;
                case "Cost": clickedColumn = "EstimatedCost"; break;
                case "SP": clickedColumn = "StoryPoints"; break;
                case "Sprint": clickedColumn = "SprintNumber"; break;
                case "MaxTime": clickedColumn = "MaxTime"; break;
                case "Priority": clickedColumn = "pri"; break;
                case "Active": clickedColumn = "IsActive"; break;
                case "Status": clickedColumn = "IsCompleted"; break;
                case "CreatedAt": clickedColumn = "CreatedAfter"; break;
                case "DueDate": clickedColumn = "DueDate"; break;
            }

            if (string.IsNullOrEmpty(clickedColumn)) return;

            if (activeSortColumn != clickedColumn)
            {
                activeSortColumn = clickedColumn;
                sortState = 1;
                activeSortDirection = "Ascending";
            }
            else
            {
                sortState++;
                if (sortState > 2) sortState = 0; 

                if (sortState == 1) activeSortDirection = "Ascending";
                else if (sortState == 2) activeSortDirection = "Descending";
                else activeSortColumn = "";
            }


            if (_activeColumn != null && _activeColumn.Header != null && _activeColumn != header.Column)
            {
                _activeColumn.Header = _activeColumn.Header.ToString()!.Replace(" ▲", "").Replace(" ▼", "");
            }

            _activeColumn = header.Column;
            if (_activeColumn.Header != null)
            {
                string cleanHeader = _activeColumn.Header.ToString()!.Replace(" ▲", "").Replace(" ▼", "");

           
                if (sortState == 1)
                {
                    _activeColumn.Header = cleanHeader + " ▲";
                }
                else if (sortState == 2)
                {
                    _activeColumn.Header = cleanHeader + " ▼";
                }
                else
                {
                    _activeColumn.Header = cleanHeader; 
                }
            }

            currentPage = 1;
            FetchDataWithCurrentPage();
        }

        private void FetchDataWithCurrentPage()
        {
            txtPageInfo.Text = $"Page: {currentPage}";
            btnPrevPage.IsEnabled = currentPage > 1;

            var request = new DataSourceRequest
            {
                PageNumber = currentPage,
                PageSize = 12,
                Filter = new CompositeFilterDescriptor { LogicalOperator = FilterCompositionLogicalOperator.And }
            };

            if (!string.IsNullOrEmpty(activeSortColumn))
            {
                request.Sorts.Add(new SortDescriptor { Member = activeSortColumn, SortDirection = activeSortDirection });
            }

            if (cmbPriority != null && cmbPriority.SelectedIndex > 0)
                FilterBuilder(request, "pri", (cmbPriority.SelectedIndex - 1).ToString(), FilterOperator.IsEqualTo);

            if (dpStartDate.SelectedDate.HasValue)
                FilterBuilder(request, "CreatedAfter", dpStartDate.SelectedDate.Value, FilterOperator.IsGreaterThanOrEqualTo);

            if (dpEndDate.SelectedDate.HasValue)
                FilterBuilder(request, "CreatedAfter", dpEndDate.SelectedDate.Value, FilterOperator.IsLessThanOrEqualTo);

            if (chkIsCompleted.IsChecked == true)
                FilterBuilder(request, "IsCompleted", true, FilterOperator.IsEqualTo);

            if (double.TryParse(txtMinTime.Text, out double minTime))
                FilterBuilder(request, "MaxTime", minTime, FilterOperator.IsGreaterThanOrEqualTo);

            if (double.TryParse(txtMaxTime.Text, out double maxTime))
                FilterBuilder(request, "MaxTime", maxTime, FilterOperator.IsLessThanOrEqualTo);

            if (!string.IsNullOrWhiteSpace(txtTaskName.Text))
            {
                FilterOperator op = cmbNameOperator.SelectedIndex switch { 1 => FilterOperator.StartsWith, 2 => FilterOperator.EndsWith, _ => FilterOperator.Contains };
                FilterBuilder(request, "TaskName", txtTaskName.Text, op);
            }

            if (!string.IsNullOrWhiteSpace(txtAssigneeName.Text))
                FilterBuilder(request, "AssigneeName", txtAssigneeName.Text, FilterOperator.Contains);

            if (cmbCategory != null && cmbCategory.SelectedIndex > 0)
                FilterBuilder(request, "Category", (cmbCategory.SelectedIndex - 1).ToString(), FilterOperator.IsEqualTo);

            if (double.TryParse(txtEstimatedCost.Text, out double estCost))
                FilterBuilder(request, "EstimatedCost", estCost, FilterOperator.IsLessThanOrEqualTo);

            if (int.TryParse(txtStoryPoints.Text, out int sPts))
                FilterBuilder(request, "StoryPoints", sPts, FilterOperator.IsEqualTo);

            if (dpDueDate.SelectedDate.HasValue)
                FilterBuilder(request, "DueDate", dpDueDate.SelectedDate.Value, FilterOperator.IsLessThanOrEqualTo);

            if (chkIsActive.IsChecked == true)
                FilterBuilder(request, "IsActive", true, FilterOperator.IsEqualTo);

            LoadDataFromApi(request);
        }

        private async void LoadDataFromApi(DataSourceRequest currentRequest)
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

                    HttpResponseMessage response = await client.PostAsJsonAsync(ApiUrl, currentRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonText = await response.Content.ReadAsStringAsync();

                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true,
                            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                        };

                        var taskList = System.Text.Json.JsonSerializer.Deserialize<List<TaskViewModel>>(jsonText, options);

                        if (taskList == null) taskList = new List<TaskViewModel>();

                        TaskListView.ItemsSource = taskList;

                        if (taskList.Count < currentRequest.PageSize)
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

        private void FilterBuilder(DataSourceRequest request, string memberName, object value, FilterOperator filterOperator)
        {
            request.Filter.FilterDescriptors.Add(new FilterDescriptor
            {
                Member = memberName,
                Value = value,
                Operator = filterOperator
            });
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
                    if (result != null)
                    {
                        TokenStore.Token = result.Token;
                        MessageBox.Show("Login successful! Access token obtained.", "Access Granted");
                        FetchDataWithCurrentPage();
                    }
                }
                else
                {
                    MessageBox.Show("Invalid username or password!", "Error");
                }
            }
        }
    }
}
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
        public double MaxTime { get; set; }
        public int pri { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CreatedAfter { get; set; }

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
        private const string ApiUrl = "https://localhost:7271/api/tasks/filter-dynamic";
        private int currentPage = 1;

        private string activeSortColumn = "";
        private string activeSortDirection = "Ascending";
        private int sortState = 0;

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
            var header = sender as System.Windows.Controls.GridViewColumnHeader;
            if (header == null || header.Tag == null) return;

            string clickedColumn = header.Tag.ToString();

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

                if (sortState == 1)
                {
                    activeSortDirection = "Ascending";
                }
                else if (sortState == 2)
                {
                    activeSortDirection = "Descending";
                }
                else
                {
                    activeSortColumn = "";
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
                Filter = new CompositeFilterDescriptor
                {
                    LogicalOperator = FilterCompositionLogicalOperator.And
                }
            };


            if (!string.IsNullOrEmpty(activeSortColumn))
            {
                request.Sorts.Add(new SortDescriptor
                {
                    Member = activeSortColumn,
                    SortDirection = activeSortDirection
                });
            }


            if (cmbPriority != null && cmbPriority.SelectedIndex > 0)
            {

                int selectedPriorityValue = cmbPriority.SelectedIndex - 1;

                request.Filter.FilterDescriptors.Add(new FilterDescriptor
                {
                    Member = "pri",
                    Value = selectedPriorityValue,
                    Operator = FilterOperator.IsEqualTo
                });
            }

            if (dpStartDate.SelectedDate.HasValue)
            {
                request.Filter.FilterDescriptors.Add(new FilterDescriptor
                {
                    Member = "CreatedAfter",
                    Value = dpStartDate.SelectedDate.Value,
                    Operator = FilterOperator.IsGreaterThanOrEqualTo
                });
            }

            if (dpEndDate.SelectedDate.HasValue)
            {
                request.Filter.FilterDescriptors.Add(new FilterDescriptor
                {
                    Member = "CreatedAfter",
                    Value = dpEndDate.SelectedDate.Value,
                    Operator = FilterOperator.IsLessThanOrEqualTo
                });
            }

            if (chkIsCompleted.IsChecked == true)
            {
                request.Filter.FilterDescriptors.Add(new FilterDescriptor
                {
                    Member = "IsCompleted",
                    Value = true,
                    Operator = FilterOperator.IsEqualTo
                });
            }

            if (double.TryParse(txtMinTime.Text, out double minTime))
            {
                request.Filter.FilterDescriptors.Add(new FilterDescriptor
                {
                    Member = "MaxTime",
                    Value = minTime,
                    Operator = FilterOperator.IsGreaterThanOrEqualTo
                });
            }

            if (double.TryParse(txtMaxTime.Text, out double maxTime))
            {
                request.Filter.FilterDescriptors.Add(new FilterDescriptor
                {
                    Member = "MaxTime",
                    Value = maxTime,
                    Operator = FilterOperator.IsLessThanOrEqualTo
                });
            }

            if (!string.IsNullOrWhiteSpace(txtTaskName.Text))
            {
                FilterOperator selectedOp = FilterOperator.Contains;
                if (cmbNameOperator.SelectedIndex == 1) selectedOp = FilterOperator.StartsWith;
                else if (cmbNameOperator.SelectedIndex == 2) selectedOp = FilterOperator.EndsWith;

                request.Filter.FilterDescriptors.Add(new FilterDescriptor
                {
                    Member = "TaskName",
                    Value = txtTaskName.Text,
                    Operator = selectedOp
                });
            }

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

                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
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
using ClientWPF;
using MVVM_WPF.Model;
using MVVM_WPF.MVVM;
using MVVM_WPF.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MVVM_WPF.Converters;

namespace MVVM_WPF.ViewModel
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FilterConfigAttribute : Attribute
    {
        public string Member { get; }
        public FilterOperator Operator { get; }

        public FilterConfigAttribute(string member, FilterOperator op)
        {
            Member = member;
            Operator = op;
        }
    }

    public class MainWindowViewModel : BaseViewModel
    {
        private const string ApiUrl = "https://localhost:7271/api/tasks/filter-dynamic";
        private const string AuthUrl = "https://localhost:7271/api/auth/login";
        private const string AddTaskUrl = "https://localhost:7271/api/tasks";

        private Visibility _loginVisibility = Visibility.Visible;
        public Visibility LoginVisibility
        {
            get => _loginVisibility;
            set { _loginVisibility = value; OnPropertyChanged(); }
        }

        private Visibility _mainAppVisibility = Visibility.Collapsed;
        public Visibility MainAppVisibility
        {
            get => _mainAppVisibility;
            set { _mainAppVisibility = value; OnPropertyChanged(); }
        }

        private ObservableCollection<TaskModel> _taskList = new ObservableCollection<TaskModel>();
        public ObservableCollection<TaskModel> TaskList
        {
            get => _taskList;
            set { _taskList = value; OnPropertyChanged(); }
        }

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set { _currentPage = value; OnPropertyChanged(); }
        }

        private string _username;
        public string Username { get => _username; set { _username = value; OnPropertyChanged(); } }

        private TaskModel _newTask = new TaskModel();
        public TaskModel NewTask
        {
            get => _newTask;
            set { _newTask = value; OnPropertyChanged(); }
        }

        private TaskModel _updateTask = new TaskModel();
        public TaskModel UpdateTask
        {
            get => _updateTask;
            set { _updateTask = value; OnPropertyChanged(); }
        }

        private TaskModel _deleteTask = new TaskModel();
        public TaskModel DeleteTask
        {
            get { return _deleteTask; }
            set { _deleteTask = value; OnPropertyChanged(); }
        }

        private string _taskNameFilter;
        public string TaskNameFilter
        {
            get => _taskNameFilter;
            set { if (_taskNameFilter != value) { _taskNameFilter = value; OnPropertyChanged(); } }
        }

        private int _nameOperatorIndex = 0;
        public int NameOperatorIndex
        {
            get => _nameOperatorIndex;
            set { if (_nameOperatorIndex != value) { _nameOperatorIndex = value; OnPropertyChanged(); } }
        }

        private string _employeeNameFilter;
        [FilterConfig("EmployeeName", FilterOperator.Contains)]
        public string EmployeeNameFilter
        {
            get => _employeeNameFilter;
            set { if (_employeeNameFilter != value) { _employeeNameFilter = value; OnPropertyChanged(); } }
        }

        private double? _minTimeFilter;
        [FilterConfig("MaxTime", FilterOperator.IsGreaterThanOrEqualTo)]
        public double? MinTimeFilter
        {
            get => _minTimeFilter;
            set { if (_minTimeFilter != value) { _minTimeFilter = value; OnPropertyChanged(); } }
        }

        private double? _maxTimeFilter;
        [FilterConfig("MaxTime", FilterOperator.IsLessThanOrEqualTo)]
        public double? MaxTimeFilter
        {
            get => _maxTimeFilter;
            set { if (_maxTimeFilter != value) { _maxTimeFilter = value; OnPropertyChanged(); } }
        }

        private DateTime? _startDateFilter;
        [FilterConfig("CreatedAfter", FilterOperator.IsGreaterThanOrEqualTo)]
        public DateTime? StartDateFilter
        {
            get => _startDateFilter;
            set { if (_startDateFilter != value) { _startDateFilter = value; OnPropertyChanged(); } }
        }

        private DateTime? _endDateFilter;
        [FilterConfig("CreatedAfter", FilterOperator.IsLessThanOrEqualTo)]
        public DateTime? EndDateFilter
        {
            get => _endDateFilter;
            set { if (_endDateFilter != value) { _endDateFilter = value; OnPropertyChanged(); } }
        }

        private int _selectedPriorityIndex = 0;
        public int SelectedPriorityIndex
        {
            get => _selectedPriorityIndex;
            set { if (_selectedPriorityIndex != value) { _selectedPriorityIndex = value; OnPropertyChanged(); } }
        }

        private double? _estimatedCostFilter;
        [FilterConfig("EstimatedCost", FilterOperator.IsLessThanOrEqualTo)]
        public double? EstimatedCostFilter
        {
            get => _estimatedCostFilter;
            set { if (_estimatedCostFilter != value) { _estimatedCostFilter = value; OnPropertyChanged(); } }
        }

        private DateTime? _dueDateFilter;
        [FilterConfig("DueDate", FilterOperator.IsLessThanOrEqualTo)]
        public DateTime? DueDateFilter
        {
            get => _dueDateFilter;
            set { if (_dueDateFilter != value) { _dueDateFilter = value; OnPropertyChanged(); } }
        }

        private int _selectedCategoryIndex = 0;
        public int SelectedCategoryIndex
        {
            get => _selectedCategoryIndex;
            set { if (_selectedCategoryIndex != value) { _selectedCategoryIndex = value; OnPropertyChanged(); } }
        }

        private int? _storyPointsFilter;
        [FilterConfig("StoryPoints", FilterOperator.IsEqualTo)]
        public int? StoryPointsFilter
        {
            get => _storyPointsFilter;
            set { if (_storyPointsFilter != value) { _storyPointsFilter = value; OnPropertyChanged(); } }
        }
        private int? _sprintNumberFilter;
        [FilterConfig("SprintNumber", FilterOperator.IsEqualTo)]
        public int? SprintNumberFilter
        {
            get => _sprintNumberFilter;
            set { if (_sprintNumberFilter != value) { _sprintNumberFilter = value; OnPropertyChanged(); } }
        }

        private bool _isCompletedFilter;
        [FilterConfig("IsCompleted", FilterOperator.IsEqualTo)]
        public bool IsCompletedFilter
        {
            get => _isCompletedFilter;
            set { if (_isCompletedFilter != value) { _isCompletedFilter = value; OnPropertyChanged(); ExecuteFilter(null); } }
        }

        private bool _isActiveFilter;
        [FilterConfig("IsActive", FilterOperator.IsEqualTo)]
        public bool IsActiveFilter
        {
            get => _isActiveFilter;
            set { if (_isActiveFilter != value) { _isActiveFilter = value; OnPropertyChanged(); ExecuteFilter(null); } }
        }

        public ICommand LoginCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand UpdateTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand CompleteTaskCommand { get; }

        public MainWindowViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            LogoutCommand = new RelayCommand(ExecuteLogout);
            FilterCommand = new RelayCommand(ExecuteFilter);
            NextPageCommand = new RelayCommand(ExecuteNextPage, CanExecuteNextPage);
            PrevPageCommand = new RelayCommand(ExecutePrevPage, CanExecutePrevPage);
            AddTaskCommand = new RelayCommand(ExecuteAddTask);
            UpdateTaskCommand = new RelayCommand(ExecuteUpdateTask);
            DeleteTaskCommand = new RelayCommand(ExecuteDeleteTask);
            CompleteTaskCommand = new RelayCommand(ExecuteCompleteTask);
        }

        public void ExecuteCompleteTask(object parameter)
        {
            if (UpdateTask != null)
            {
                UpdateTask.IsCompleted = true;
                ExecuteUpdateTask(null);
            }
        }

        private async void ExecuteAddTask(object parameter)
        {
            NewTask = new TaskModel();
            NewTask.EmployeeName = "Ahmet";
            NewTask.TaskName = "MongoDB Task";
            NewTask.pri = 0;
            NewTask.IsCompleted = false;
            NewTask.IsActive = true;
            NewTask.CreatedAfter = DateTime.Now;
            NewTask.DueDate = DateTime.Now.AddDays(7);
            NewTask.EstimatedCost = 0;
            NewTask.StoryPoints = 0;
            NewTask.MaxTime = 0;
            NewTask.SprintNumber = 0;

            var editor = new TaskEditorWindow { DataContext = NewTask };

            if (editor.ShowDialog() == true)
            {
                if (string.IsNullOrWhiteSpace(NewTask.TaskName))
                {
                    MessageBox.Show("Task name can not be null!", "Warning:", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        if (string.IsNullOrEmpty(TokenStore.Token)) return;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.Token);

                        HttpResponseMessage response = await client.PostAsJsonAsync(AddTaskUrl, NewTask);
                        if (!response.IsSuccessStatusCode)
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"API Hata Döndürdü: {errorContent}");
                            return;
                        }

                        if (response.IsSuccessStatusCode)
                        {
                            var createdTask = await response.Content.ReadFromJsonAsync<TaskModel>(new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            if (createdTask != null)
                            {
                                createdTask.IsNew = true;
                                TaskList.Insert(0, createdTask);

                                NewTask = new TaskModel();
                                MessageBox.Show("Task added successfully!", "Info:", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        else
                        {
                            string errorDetails = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Adding failed: {response.StatusCode}\n\nError Details:\n{errorDetails}",
                                            "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("API Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ExecuteUpdateTask(object parameter)
        {
            if (UpdateTask == null || string.IsNullOrEmpty(UpdateTask.Id))
            {
                MessageBox.Show("Please select a valid task to update.");
                return;
            }
            var editor = new TaskEditorWindow { DataContext = UpdateTask };

            if (editor.ShowDialog() == true)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        if (string.IsNullOrEmpty(TokenStore.Token)) return;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.Token);

                        string url = $"{AddTaskUrl}/{UpdateTask.Id}";
                        HttpResponseMessage response = await client.PutAsJsonAsync(url, UpdateTask);

                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Task updated successfully!");
                            string savedId = UpdateTask.Id; 
                            await FetchDataAsync(); 

                            var updatedItem = TaskList.FirstOrDefault(t => t.Id == savedId);
                            if (updatedItem != null)
                            {
                                updatedItem.IsUpdated = true;
                            }
                        }
                        else
                        {
                            string error = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Update failed: {response.StatusCode}\n{error}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("API Error: " + ex.Message);
                }
            }
        }

        private async void ExecuteDeleteTask(object parameter)
        {
            DeleteTask = UpdateTask;

            if (DeleteTask == null || string.IsNullOrEmpty(DeleteTask.Id))
            {
                MessageBox.Show("Please select a task to delete.");
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this task?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.Token);

                        string url = $"{AddTaskUrl}/{DeleteTask.Id}";
                        HttpResponseMessage response = await client.DeleteAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Task deleted!");
                            await FetchDataAsync();
                        }
                        else
                        {
                            MessageBox.Show($"Delete failed: {response.StatusCode}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("API Error: " + ex.Message);
                }
            }
        }

        private async void ExecuteLogin(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (string.IsNullOrWhiteSpace(Username) || passwordBox == null || string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                MessageBox.Show("Username or password can not be empty", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (HttpClient client = new HttpClient())
            {
                var loginDto = new { Username = Username, Password = passwordBox.Password };
                try
                {
                    HttpResponseMessage response = await client.PostAsJsonAsync(AuthUrl, loginDto);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                        if (result != null)
                        {
                            TokenStore.Token = result.Token;
                            LoginVisibility = Visibility.Collapsed;
                            MainAppVisibility = Visibility.Visible;
                            passwordBox.Clear();
                            await FetchDataAsync();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid username or password", "Try again", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occured: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteLogout(object parameter)
        {
            TokenStore.Token = null;
            MainAppVisibility = Visibility.Collapsed;
            LoginVisibility = Visibility.Visible;
            TaskList.Clear();
        }

        private async void ExecuteFilter(object parameter)
        {
            CurrentPage = 1;
            await FetchDataAsync();
        }

        private async void ExecuteNextPage(object parameter)
        {
            CurrentPage++;
            await FetchDataAsync();
        }
        private bool CanExecuteNextPage(object parameter) => TaskList.Count == 12;

        private async void ExecutePrevPage(object parameter)
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await FetchDataAsync();
            }
        }
        private bool CanExecutePrevPage(object parameter) => CurrentPage > 1;

        private async Task FetchDataAsync()
        {
            var request = new DataSourceRequest
            {
                PageNumber = CurrentPage,
                PageSize = 12,
                Filter = new CompositeFilterDescriptor { LogicalOperator = FilterCompositionLogicalOperator.And }
            };

            var properties = this.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var attr = (FilterConfigAttribute)Attribute.GetCustomAttribute(prop, typeof(FilterConfigAttribute));
                if (attr != null)
                {
                    var value = prop.GetValue(this);

                    if (value == null) continue;
                    if (value is string str && string.IsNullOrWhiteSpace(str)) continue;
                    if (value is bool b && b == false) continue;

                    request.Filter.FilterDescriptors.Add(new FilterDescriptor
                    {
                        Member = attr.Member,
                        Value = value,
                        Operator = attr.Operator
                    });
                }
            }

            if (!string.IsNullOrWhiteSpace(TaskNameFilter))
            {
                FilterOperator op = NameOperatorIndex switch { 1 => FilterOperator.StartsWith, 2 => FilterOperator.EndsWith, _ => FilterOperator.Contains };
                request.Filter.FilterDescriptors.Add(new FilterDescriptor { Member = "TaskName", Value = TaskNameFilter, Operator = op });
            }

            if (SelectedPriorityIndex > 0)
                request.Filter.FilterDescriptors.Add(new FilterDescriptor { Member = "pri", Value = (SelectedPriorityIndex - 1).ToString(), Operator = FilterOperator.IsEqualTo });

            if (SelectedCategoryIndex > 0)
                request.Filter.FilterDescriptors.Add(new FilterDescriptor { Member = "Category", Value = (SelectedCategoryIndex - 1).ToString(), Operator = FilterOperator.IsEqualTo });

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    if (string.IsNullOrEmpty(TokenStore.Token)) return;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.Token);

                    HttpResponseMessage response = await client.PostAsJsonAsync(ApiUrl, request);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonText = await response.Content.ReadAsStringAsync();
                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        List<TaskModel> fetchedTasks = null;
                        jsonText = jsonText.TrimStart();

                        if (jsonText.StartsWith("{"))
                        {
                            using (var doc = JsonDocument.Parse(jsonText))
                            {
                                JsonElement root = doc.RootElement;
                                JsonElement dataElement;

                                if (root.TryGetProperty("data", out dataElement) ||
                                    root.TryGetProperty("Data", out dataElement) ||
                                    root.TryGetProperty("items", out dataElement) ||
                                    root.TryGetProperty("Items", out dataElement))
                                {
                                    fetchedTasks = System.Text.Json.JsonSerializer.Deserialize<List<TaskModel>>(dataElement.GetRawText(), options);
                                }
                                else
                                {
                                    MessageBox.Show($"API başarılı döndü ancak beklenen liste formatı bulunamadı. Gelen Cevap:\n\n{jsonText}", "Format Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return;
                                }
                            }
                        }
                        else if (jsonText.StartsWith("["))
                        {
                            fetchedTasks = System.Text.Json.JsonSerializer.Deserialize<List<TaskModel>>(jsonText, options);
                        }

                        var activeNewIds = TaskList.Where(t => t.IsNew).Select(t => t.Id).ToList();
                        var activeUpdatedIds = TaskList.Where(t => t.IsUpdated).Select(t => t.Id).ToList();

                        TaskList.Clear();

                        if (fetchedTasks != null)
                        {
                            foreach (var task in fetchedTasks)
                            {
                                if (task.Id != null && activeNewIds.Contains(task.Id))
                                {
                                    task.IsNew = true;
                                }

                                if (task.Id != null && activeUpdatedIds.Contains(task.Id))
                                {
                                    task.IsUpdated = true;
                                }

                                TaskList.Add(task);
                            }
                        }
                    }
                    else
                    {
                        string errorText = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"API connection error: {response.StatusCode}\n\nDetay:\n{errorText}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("API Error: " + ex.Message);
            }
        }
    }
}
using MVVM_WPF.Model;
using MVVM_WPF.MVVM;
using MVVM_WPF.View;
using MVVM_WPF.Services;
using MVVM_WPF.Attributes;
using MVVM_WPF.Model.Enums;
using MVVM_WPF.Resources;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MVVM_WPF.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        #region Properties (Özellikler)
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
        #endregion

        #region Commands
        public ICommand LoginCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }
        public ICommand FilterCommand { get; private set; }
        public ICommand NextPageCommand { get; private set; }
        public ICommand PrevPageCommand { get; private set; }
        public ICommand AddTaskCommand { get; private set; }
        public ICommand UpdateTaskCommand { get; private set; }
        public ICommand DeleteTaskCommand { get; private set; }
        public ICommand CompleteTaskCommand { get; private set; }
        #endregion

        public MainWindowViewModel()
        {
            _apiService = new ApiService();
            InitializeCommands();
        }

        private void InitializeCommands()
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

        private void SetupInitialTask()
        {
            NewTask = new TaskModel
            {
                EmployeeName = "Ahmet",
                TaskName = "MongoDB Task",
                pri = 0,
                IsCompleted = false,
                IsActive = true,
                CreatedAfter = DateTime.Now,
                DueDate = DateTime.Now.AddDays(7),
                EstimatedCost = 0,
                StoryPoints = 0,
                MaxTime = 0,
                SprintNumber = 0
            };
        }

        private void InsertTaskToGrid(TaskModel task)
        {
            task.IsNew = true;
            TaskList.Insert(0, task);
            NewTask = new TaskModel();
        }

        private void MoveUpdatedTaskToTop(string savedId)
        {
            var updatedItem = TaskList.FirstOrDefault(t => t.Id == savedId);
            if (updatedItem != null)
            {
                updatedItem.IsNew = false;
                updatedItem.IsUpdated = true;
                TaskList.Remove(updatedItem);
                TaskList.Insert(0, updatedItem);
            }
        }

        private List<FilterDescriptor> BuildFilterList()
        {
            var filtersList = new List<FilterDescriptor>();

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

                    filtersList.Add(new FilterDescriptor { Member = attr.Member, Operator = attr.Operator, Value = value });
                }
            }

            if (!string.IsNullOrWhiteSpace(TaskNameFilter))
            {
                FilterOperator apiOp = NameOperatorIndex switch { 1 => FilterOperator.StartsWith, 2 => FilterOperator.EndsWith, _ => FilterOperator.Contains };
                filtersList.Add(new FilterDescriptor { Member = "TaskName", Operator = apiOp, Value = TaskNameFilter });
            }

            if (SelectedPriorityIndex > 0)
                filtersList.Add(new FilterDescriptor { Member = "pri", Operator = FilterOperator.IsEqualTo, Value = SelectedPriorityIndex - 1 });

            if (SelectedCategoryIndex > 0)
                filtersList.Add(new FilterDescriptor { Member = "Category", Operator = FilterOperator.IsEqualTo, Value = SelectedCategoryIndex - 1 });

            if (_activeGridFilters != null && _activeGridFilters.Count > 0)
                filtersList.AddRange(_activeGridFilters);

            return filtersList;
        }

        private void UpdateTaskListWithFetchedData(List<TaskModel> fetchedTasks)
        {
            var activeNewIds = TaskList.Where(t => t.IsNew).Select(t => t.Id).ToList();
            var activeUpdatedIds = TaskList.Where(t => t.IsUpdated).Select(t => t.Id).ToList();

            TaskList.Clear();

            if (fetchedTasks == null) return;

            var topItems = new List<TaskModel>();
            var normalItems = new List<TaskModel>();

            foreach (var task in fetchedTasks)
            {
                if (task.Id != null && activeUpdatedIds.Contains(task.Id))
                {
                    task.IsUpdated = true;
                    task.IsNew = false;
                    topItems.Add(task);
                }
                else if (task.Id != null && activeNewIds.Contains(task.Id))
                {
                    task.IsNew = true;
                    topItems.Add(task);
                }
                else
                {
                    normalItems.Add(task);
                }
            }

            foreach (var task in topItems) TaskList.Add(task);
            foreach (var task in normalItems) TaskList.Add(task);
        }

        private async void ExecuteLogin(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (string.IsNullOrWhiteSpace(Username) || passwordBox == null || string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                MessageBox.Show(MessageProvider.GetString(CustomMessages.LoginEmptyCredentials), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                TokenStore.Token = await _apiService.LoginAsync(Username, passwordBox.Password);

                LoginVisibility = Visibility.Collapsed;
                MainAppVisibility = Visibility.Visible;
                passwordBox.Clear();
                await FetchDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(MessageProvider.GetString(CustomMessages.ConnectionError, ex.Message), "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private List<FilterDescriptor> _activeGridFilters = new List<FilterDescriptor>();

        public async void ApplyGridFilters(List<FilterDescriptor> gridFilters)
        {
            _activeGridFilters = gridFilters;
            CurrentPage = 1;
            await FetchDataAsync();
        }

        private async Task FetchDataAsync()
        {
            var filtersList = BuildFilterList();

            var compositeFilter = new CompositeFilterDescriptor
            {
                LogicalOperator = FilterCompositionLogicalOperator.And,
                FilterDescriptors = filtersList
            };

            var requestPayload = new DataSourceRequest
            {
                PageNumber = CurrentPage,
                PageSize = 12,
                Filter = compositeFilter
            };

            try
            {
                var fetchedTasks = await _apiService.GetTasksAsync(requestPayload);
                UpdateTaskListWithFetchedData(fetchedTasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show(MessageProvider.GetString(CustomMessages.ApiGenericError, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExecuteAddTask(object parameter)
        {
            SetupInitialTask();

            var editor = new TaskEditorWindow { DataContext = NewTask };

            if (editor.ShowDialog() == true)
            {
                if (string.IsNullOrWhiteSpace(NewTask.TaskName))
                {
                    MessageBox.Show(MessageProvider.GetString(CustomMessages.TaskNameEmpty), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var createdTask = await _apiService.AddTaskAsync(NewTask);

                    if (createdTask != null)
                    {
                        InsertTaskToGrid(createdTask);
                        MessageBox.Show(MessageProvider.GetString(CustomMessages.TaskAddedSuccess), "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(MessageProvider.GetString(CustomMessages.ApiGenericError, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ExecuteUpdateTask(object parameter)
        {
            if (UpdateTask == null || string.IsNullOrEmpty(UpdateTask.Id))
            {
                MessageBox.Show(MessageProvider.GetString(CustomMessages.TaskUpdateNotSelected), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editor = new TaskEditorWindow { DataContext = UpdateTask };

            if (editor.ShowDialog() == true)
            {
                try
                {
                    await _apiService.UpdateTaskAsync(UpdateTask);

                    MessageBox.Show(MessageProvider.GetString(CustomMessages.TaskUpdatedSuccess), "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    string savedId = UpdateTask.Id;
                    await FetchDataAsync();

                    MoveUpdatedTaskToTop(savedId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(MessageProvider.GetString(CustomMessages.ApiGenericError, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ExecuteDeleteTask(object parameter)
        {
            DeleteTask = UpdateTask;

            if (DeleteTask == null || string.IsNullOrEmpty(DeleteTask.Id))
            {
                MessageBox.Show(MessageProvider.GetString(CustomMessages.TaskDeleteNotSelected), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show(MessageProvider.GetString(CustomMessages.TaskDeleteConfirm), "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    await _apiService.DeleteTaskAsync(DeleteTask.Id);

                    MessageBox.Show(MessageProvider.GetString(CustomMessages.TaskDeletedSuccess), "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await FetchDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(MessageProvider.GetString(CustomMessages.ApiGenericError, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void ExecuteCompleteTask(object parameter)
        {
            if (UpdateTask != null)
            {
                UpdateTask.IsCompleted = true;
                ExecuteUpdateTask(null);
            }
        }
    }
}
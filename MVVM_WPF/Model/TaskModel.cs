using MVVM_WPF.Model.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MVVM_WPF.Model
{
    public class TaskModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public int? pri { get; set; }
        public int? Category { get; set; }

        public string PriorityText
        {
            get
            {
                if (!pri.HasValue || !Enum.IsDefined(typeof(TaskPriority), pri.Value))
                    return "Unknown";
                return ((TaskPriority)pri.Value).ToString().ToLower();
            }
        }

        public string CategoryText
        {
            get
            {
                if (!Category.HasValue || !Enum.IsDefined(typeof(TaskCategory), Category.Value))
                    return "Unknown";

                return ((TaskCategory)Category.Value).ToString();
            }
        }

        public string? Id { get; set; }
        public string? TaskName { get; set; }
        public double? MaxTime { get; set; }
        public bool? IsCompleted { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public string? EmployeeName { get; set; }
        public double? EstimatedCost { get; set; }
        public int? StoryPoints { get; set; }
        public int? SprintNumber { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? DueDate { get; set; }

        

        private int _fadeCounter = 0;

        private bool _isNew;
        public bool IsNew
        {
            get => _isNew;
            set
            {
                _isNew = value;
                OnPropertyChanged();
                if (value) StartFadeTimer();
            }
        }

        private bool _isUpdated;
        public bool IsUpdated
        {
            get => _isUpdated;
            set
            {
                _isUpdated = value;
                OnPropertyChanged(); 
                if (value) StartFadeTimer();
            }
        }

        private async void StartFadeTimer()
        {
            int currentCounter = ++_fadeCounter;
            await Task.Delay(TimeSpan.FromMinutes(5));

            if (currentCounter == _fadeCounter)
            {
                if (IsNew) IsNew = false;
                if (IsUpdated) IsUpdated = false;
            }
        }
    }
}
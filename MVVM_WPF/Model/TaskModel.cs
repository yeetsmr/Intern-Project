using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MVVM_WPF.Model
{
    public class TaskModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string? Id { get; set; }
        public string? TaskName { get; set; }
        public double? MaxTime { get; set; }
        public int? pri { get; set; }
        public bool? IsCompleted { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public string? EmployeeName { get; set; }
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
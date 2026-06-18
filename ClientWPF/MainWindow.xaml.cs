using System;
using System.Collections.Generic;
using System.Windows;

namespace ClientWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            List<TaskViewModel> dummyTasks = new List<TaskViewModel>
            {
                new TaskViewModel { Id = "65f1a2b3c4d5e6f7a8b9c001", TaskName = "WPF Arayüz Altyapısının Kurulması", MaxTime = 4.5, pri = "high", IsCompleted = true },
                new TaskViewModel { Id = "65f1a2b3c4d5e6f7a8b9c002", TaskName = "API Bağlantısının Yapılması", MaxTime = 8.0, pri = "mid", IsCompleted = false }
            };

            TaskListView.ItemsSource = dummyTasks;
        }
    }

    public class TaskViewModel
    {
        public string Id { get; set; }
        public string TaskName { get; set; }
        public double MaxTime { get; set; }
        public string pri { get; set; }
        public bool IsCompleted { get; set; }
    }
}
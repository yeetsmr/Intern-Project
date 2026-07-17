using System.Windows;

namespace MVVM_WPF.View
{

    public partial class TaskEditorWindow : Window
    {
        public TaskEditorWindow()
        {
            InitializeComponent();
        }



        private void Save_Click(object sender, RoutedEventArgs e)
        {

            this.DialogResult = true;
            this.Close();
        }
    }
}



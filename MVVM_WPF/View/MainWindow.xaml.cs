using System;
using System.Collections.Generic;
using System.Windows;
using MVVM_WPF.ViewModel;
using MVVM_WPF.Model;

namespace MVVM_WPF.View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }

        private void TaskGridView_Filtered(object sender, Telerik.Windows.Controls.GridView.GridViewFilteredEventArgs e)
        {
            var filtersList = new List<FilterDescriptor>();

            foreach (var filterDescriptor in TaskGridView.FilterDescriptors)
            {
                if (filterDescriptor is Telerik.Windows.Controls.GridView.IColumnFilterDescriptor columnFilter)
                {
                    var activeFilter = columnFilter.FieldFilter.Filter1;
                    var dataColumn = columnFilter.Column as Telerik.Windows.Controls.GridViewDataColumn;

                    if (activeFilter.IsActive && dataColumn?.DataMemberBinding != null)
                    {
                        string memberName = dataColumn.DataMemberBinding.Path.Path;
                        string telerikOperatorStr = activeFilter.Operator.ToString();
                        object filterValue = activeFilter.Value;

                        FilterOperator apiOperator = FilterOperator.IsEqualTo;
                        Enum.TryParse(telerikOperatorStr, out apiOperator);

                        filtersList.Add(new FilterDescriptor
                        {
                            Member = memberName,
                            Operator = apiOperator,
                            Value = filterValue
                        });
                    }
                }
            }

            if (this.DataContext is MainWindowViewModel viewModel)
            {
                viewModel.ApplyGridFilters(filtersList);
            }
        }
    }
}
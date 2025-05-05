using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BudgetModel
{
    /// <summary>
    /// Interaction logic for AddCategory.xaml
    /// </summary>
    public partial class AddCategory : Window
    {
        private Presenter _presenter;
        public AddCategory(Presenter presenter)
        {
            InitializeComponent();
            _presenter = presenter;
        }

        /// <summary>
        /// Add new category (from Create Category area).
        /// </summary>
        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NewCategoryNameBox.Text.Trim();
            string selectedType = (NewCategoryTypeBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            bool success = _presenter.AddCategory(name, selectedType);

            NewCategoryNameBox.Text = "";
            NewCategoryTypeBox.SelectedIndex = -1;
        }
    }
}

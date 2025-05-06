using Budget;
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
using Views;

namespace BudgetModel
    //Display the old info
    //Get the new info
    //get id and categories id
    //Update the user
    //Show message
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        private Presenter _presenter;
        private Expense _expense;
        public UpdateWindow(Expense expenseToUpdate, Presenter presenter)
        {
            InitializeComponent();
            //Load the categories inside the combo box
            LoadCategories();

            _expense = expenseToUpdate;
            _presenter = presenter;

            //Displayy
            ExpenseDatePicker.SelectedDate = _expense.Date;
            CategoryComboBox.SelectedItem = _expense.Category;
            ExpenseName.Text =_expense.Description;
            Amount.Text = _expense.Amount.ToString("F2");
        }


        public void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowSucessMessage(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Loads all available categories from the database and updates the CategoryComboBox.
        /// </summary>
        private void LoadCategories()
        {
            CategoryComboBox.Items.Clear(); //reset

            var categoryList = _presenter.GetCategories();

            foreach (var category in categoryList) //add each category to the combobox
            {
                CategoryComboBox.Items.Add(category.Description);
            }
        }

        private void UpdateExpense_Click(object sender, RoutedEventArgs e)
        {
            string? expenseName = ExpenseName.Text;
            string? amount = Amount.Text;
            string? category = CategoryComboBox.SelectedItem.ToString();
            DateTime date = ExpenseDatePicker.SelectedDate.Value;

            bool success = _presenter.UpdateExistingExpense(
            _expense.Id, expenseName, amount, date, category, out string message);
            
            if(success)
            {
                ShowSucessMessage(message);
                this.Close();
            }
            else
            {
                ShowErrorMessage(message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            //Close the window and 
            //show success/ failing 
            try
            {
                this.Close();
                ShowSucessMessage("Successfully cancel the update.");
            }
            catch
            {
                ShowErrorMessage("Failed to cancel the update.");
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            //Call the presenter to delete
            //show success/ failing 
            bool success = _presenter.DeleteExpense(_expense.Id, out string message);

            if (success)
            {
                ShowSucessMessage(message);
                this.Close();
            }
            else
            {
                ShowErrorMessage(message);
            }
        }
    }
}

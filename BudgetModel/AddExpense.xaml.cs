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
{
    /// <summary>
    /// Interaction logic for AddExpense.xaml
    /// </summary>
    public partial class AddExpense : Window, IView
    {
        Presenter _presenter;
        public AddExpense()
        {
            InitializeComponent();
            LoadCategories();
        }

        /// <summary>
        /// Clears all input fields related to adding an expense, resetting the form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments associated with the button click.</param>
        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            //clear all input fields
            ExpenseNameTextBox.Clear();
            ExpenseAmountTextBox.Clear();
            CategoryComboBox.SelectedIndex = -1;
            CategoryComboBox.Text = string.Empty;
            ExpenseDatePicker.SelectedDate = DateTime.Today;
        }

        /// <summary>
        /// Handles adding a new expense entry after validating user input.
        /// Ensures that all required fields (name, amount, date, category, and category type) are properly filled.
        /// Adds the expense to the database and resets the input fields upon success.
        /// Shows error messages if any validation fails.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments associated with the button click.</param>
        private void AddExpenseClick(object sender, RoutedEventArgs e) // Renamed to avoid conflict with class name
        {
            try
            {
                DisplayAddExpense();

                OnCancelClick(sender, e); //clear
                LoadCategories();
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex.ToString());
            }
        }

        public void DisplayAddExpense()
        {
            try
            {
                if (!double.TryParse(ExpenseAmountTextBox.Text, out double amount))
                {
                    DisplayErrorMessage("The expense amount must be a valid number.");
                }

                string name = ExpenseNameTextBox.Text;
                DateTime date = ExpenseDatePicker.SelectedDate.Value; //crashing
                string? category;

                if (CategoryComboBox.SelectedItem != null)
                {
                    category = CategoryComboBox.SelectedItem.ToString(); //if category was selected via dropdown we get the content as string
                }
                else
                {
                    category = CategoryComboBox.Text; //get typed text from comboBox if nothing was selected 
                                                      // If it's a new category, make sure a type is selected
                }

                //check if the category is new/ get category type for new category input
                if (!_presenter.IsCategoryExisting(category))
                {
                    var categoryWindow = new SelectingCategoryType(); // This window allows the user to select a category type
                    categoryWindow.ShowDialog();

                    // Check if the user selected a category type or canceled
                    if (categoryWindow.DialogResult == true)
                    {
                        string categoryType = categoryWindow.SelectedCategoryType;
                        _presenter.AddCategory(category, categoryType); // Add new category only if confirmed
                    }
                    else
                    {
                        return; //if user canceled, go back
                    }
                }

                if (!string.IsNullOrEmpty(category)) //only add expense if category is valid and user didn't cancel
                {
                    _presenter.AddExpense(date, name, amount, category);
                    DisplaySuccessMessage($"Expense '{name}' added successfully.");
                }
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ex.ToString());
            }
        }

        /// <summary>
        /// Event handler triggered when the CategoryComboBox dropdown closes.
        /// Attempts to create a new category if the entered text does not match an existing category,
        /// then refreshes the list to immediately reflect any new categories.
        /// </summary>
        /// <param name="sender">The source of the event (CategoryComboBox).</param>
        /// <param name="e">Event arguments associated with the dropdown closing.</param>
        private void CategoryComboBox_DropDownClosed(object sender, EventArgs e)
        {
            string categoryName = CategoryComboBox.Text;

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                var category = _presenter.CreateOrGetCategory(categoryName);
                LoadCategories(category.Description);
            }
        }

        /// <summary>
        /// Refreshes the CategoryComboBox with the current list of categories.
        /// </summary>
        /// <param name="selectedCategory">Optional: Category name to select after refreshing.</param>
        private void LoadCategories(string selectedCategory = null)
        {
            CategoryComboBox.ItemsSource = null;
            CategoryComboBox.Items.Clear();

            List<string> categories = new List<string>();

            foreach (Category category in _presenter.GetCategories())
            {
                categories.Add(category.Description);
            }

            categories.Sort(); //sort the list

            CategoryComboBox.ItemsSource = categories; //bind to ItemsSource

            if (!string.IsNullOrEmpty(selectedCategory))
            {
                CategoryComboBox.SelectedItem = selectedCategory;
            }
        }

        /// <summary>
        /// Adds a new category by calling the Presenter's AddCategory method.
        /// Used by the Presenter to add a category through the View layer.
        /// </summary>
        /// <param name="name">The name of the category to add.</param>
        /// <param name="type">The type of the category ("Income", "Expense", "Credit", "Savings").</param>
        public void DisplayAddCategory(string name, string type)
        {
            _presenter.AddCategory(name, type);
        }

        public void DisplaySuccessMessage(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButton.OK);
        }

        /// <summary>
        /// Displays an error message to the user using a MessageBox.
        /// </summary>
        /// <param name="message">Error message to be shown.</param>
        public void DisplayErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            var categoryWindow = new SelectingCategoryType(); // Assuming this is your "Add Category" window
            categoryWindow.ShowDialog();
        }
    }
}

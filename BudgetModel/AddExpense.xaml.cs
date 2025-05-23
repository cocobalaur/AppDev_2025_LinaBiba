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
using static Budget.Category;

namespace BudgetModel
{
    /// <summary>
    /// Interaction logic for AddExpense.xaml
    /// </summary>
    public partial class AddExpense : Window
    {
        private AddCategory _categoryWindow; //for adding new categories
        Presenter _presenter;
        IView _view;
        public AddExpense(Presenter presenter, IView view)
        {
            
            InitializeComponent();
            _presenter = presenter;
            _view = view;
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
            this.Close();
        }

        /// <summary>
        /// Triggered by Add Expense button. Attempts to add a new expense.
        /// Validates and processes user input, then clears the form.
        /// </summary>
        private void AddExpenseClick(object sender, RoutedEventArgs e) 
        {
           GetExpense();
           OnCancelClick(sender, e); //clear
        }

        /// <summary>
        /// Handles adding a new expense entry after validating user input.
        /// Ensures that all required fields (name, amount, date, category, and category type) are properly filled.
        /// Adds the expense to the database and resets the input fields upon success.
        /// Shows error messages if any validation fails.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments associated with the button click.</param>
        /// 
        public void GetExpense() //Looks more like PRocessandMaybeAddAnExpense
        {
            string name = ExpenseNameTextBox.Text;
            DateTime date = ExpenseDatePicker.SelectedDate.Value; //crashing
            double amount = double.Parse(ExpenseAmountTextBox.Text); //parse amount to double
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
                if (!_presenter.FindCategory(category))
                {
                    var categoryWindow = new SelectingCategoryType(); //allows the user to select a category type in new window
                    categoryWindow.ShowDialog();

                    //check if the user selected a category type or canceled
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

            _presenter.ProcessNewAddExpense(date, name, amount, category);
        }


        /// <summary>
        /// Handles CategoryComboBox dropdown closing event.
        /// Checks if the entered text matches an existing category.
        /// If not, prompts for a category type and adds a new category.
        /// </summary>
        private void CategoryComboBox_DropDownClosed(object sender, EventArgs e)
        {
            string categoryName = CategoryComboBox.Text;

            if (string.IsNullOrEmpty(categoryName))
                return;

            if (!_presenter.FindCategory(categoryName))
            {
                string categoryType = PromptForCategoryType();

                if (!string.IsNullOrEmpty(categoryType))
                {
                    if (_presenter.AddCategory(categoryName, categoryType))
                    {
                        _presenter.RefreshCategoryList(categoryName);
                        return;
                    }
                }
            }
            else
            {
                _presenter.RefreshCategoryList(categoryName);
            }
        }

        /// <summary>
        /// Prompts user to select a category type for a new category with a dialog.
        /// </summary>
        /// <returns>Selected category type or null if canceled.</returns>
        public string PromptForCategoryType()
        {
            var categoryTypeWindow = new SelectingCategoryType();
            categoryTypeWindow.ShowDialog();

            if (categoryTypeWindow.DialogResult == true)
            {
                return categoryTypeWindow.SelectedCategoryType;
            }

            return null; //canceled
        }

        /// <summary>
        /// Opens the AddCategory window to add a category.
        /// </summary>
        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            _categoryWindow = new AddCategory(_presenter, _view);
            _categoryWindow.Show();
        }
    }
}

using Budget;
using System.Windows;
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
        private IView _view;


        /// <summary>
        /// Initializes a new instance of the update window class.
        /// Sets up the UI with the expense data that we want to update. 
        /// Initializes the presenter and the view, then display the exisiting 
        /// category and loads all available categories.
        /// </summary>
        /// <param name="expenseToUpdate">The expense object to update.</param>
        /// <param name="presenter">The presenter to talk with the model.</param>
        /// <param name="view">The view interface to display message to the user.</param>
        public UpdateWindow(Expense expenseToUpdate, Presenter presenter, IView view)
        {
            InitializeComponent();
   
            //Initializes the variables
            _expense = expenseToUpdate;
            _presenter = presenter;
            _view = view;

            //Get the category name with the category id
            string category = _presenter.GetCategoryName(expenseToUpdate.Category);
            
            //Displayy
            DataContext = _expense;
            CategoryComboBox.SelectedItem = category;

            //Load the categories inside the combo box
            LoadCategories();
        }

        /// <summary>
        /// Loads all available categories from the database and 
        /// updates the CategoryComboBox.
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

        /// <summary>
        /// Handles the update operation when the user clicks the "Update" button.
        /// It will retrieves the input value from the form, sends them to the presenter
        /// and then, depending on what happen, send a success or error message.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data associated with the click event.</param>
        private void UpdateExpense_Click(object sender, RoutedEventArgs e)
        {
            //Retrieves the input.
            string? expenseName = ExpenseName.Text;
            string? amount = Amount.Text;
            string? category = CategoryComboBox.SelectedItem.ToString();
            DateTime date = ExpenseDatePicker.SelectedDate.Value;

            //Call the presenter to try and update the expense.
            bool success = _presenter.UpdateExistingExpense(
            _expense.Id, expenseName, amount, date, category, out string message);

            //If the update is successful, display a success message
            if (success)
            {
                _view.DisplaySuccessMessage(message);
                this.Close();
            }
            else
            {
                _view.DisplayErrorMessage(message);
            }
        }

        /// <summary>
        /// Cancels the update operation and close the window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data associated with the click event.</param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            //Try to close the app, if it's successful, then display a success message.
            //Else, display an errror message
            try
            {
                this.Close();
                _view.DisplaySuccessMessage("Successfully cancel the update.");

            }
            catch
            {
                _view.DisplayErrorMessage("Failed to cancel the update.");
            }
        }

        /// <summary>
        /// Delete the current expense using the presenter. 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data associated with the click event.</param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            //Call the presenter to try and delete the expense.
            bool success = _presenter.DeleteExpense(_expense.Id, out string message);

            //If the delete was successfully return a success message; else an Error message
            if (success)
            {
                _view.DisplaySuccessMessage(message);
                this.Close();
            }
            else
            {
                _view.DisplayErrorMessage(message);
            }
        }
    }
}

using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using Views;
using Budget;
using System.Reflection;

namespace BudgetModel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// This window allows the user to apply different themes (Blush, Ocean, Lavender, Light)
    /// and toggle between light and dark mode.
    /// The background gradient is set programmatically to match the selected theme.
    /// </summary>
    public partial class MainWindow : Window, IView
    {
        private Presenter _presenter;
        private Filter _filterWindow;
        private AddExpense _expense;
        private UpdateWindow _updateWindow;
        private Expense _expenseToUpdate;

        /// <summary>
        /// Constructor: Initializes the main window and applies the default Light theme on startup.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            _presenter = new Presenter(this);
        }

        /// <summary>
        /// Opens a folder browser dialog and sets the selected fo lder path to the DirectoryTextBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments associated with the button click.</param>
        private void Browsefile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select Budget Database File",
                Filter = "Database Files (*.db;*.xml;*.json)|*.db;*.xml;*.json|All Files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                DirectoryTextBox.Text = openFileDialog.FileName;
            }
        }



        /// <summary>
        /// Handles the OK button click to load or create the selected budget database.
        /// If successful, it initializes and displays the Filter window,
        /// hooks the date filtering event, refreshes categories, and hides the main window.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        /// <param name="e">Event arguments related to the click.</param>
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            bool dbInitialized = FileDatabaseSelection();

            if (dbInitialized)
            {
                _filterWindow = new Filter(_presenter, this);

                // Runs every time the user changes either the start or end date.
                // It calls the Presenter's FilterByDate() method to update the displayed items.
                _filterWindow.DateRangeChanged += (s, e) => _presenter.FilterByDate();

                _filterWindow.Show();
                _presenter.RefreshCategoryList();
                this.Hide();
            }
        }

        /// <summary>
        /// Validates the file path entered by the user and attempts to initialize the database.
        /// Displays appropriate error messages if validation fails or initialization is unsuccessful.
        /// </summary>
        /// <returns>True if the database was successfully initialized; otherwise, false.</returns>
        public bool FileDatabaseSelection()
        {
            string input = DirectoryTextBox.Text;

            if (string.IsNullOrWhiteSpace(input))
            {
                DisplayErrorMessage("Please enter a valid file name or path.");
                return false;
            }

            string path = input;

            //if file doesnt exist, save it to desktop
            if (!System.IO.Path.IsPathRooted(path)) //check if the user entered a path or a filename
            {
                //if path is just a name, save it to the desktop
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); //get full path to desktop to place the new db file there
                path = System.IO.Path.Combine(desktopPath, input); //combine the desktop path with the file name
            }

            bool success = _presenter.GetDatabase(path);

            if (!success)
            {
                DisplayErrorMessage("Failed to initialize the database.");
            }

            return success;
        }

        /// <summary>
        /// Displays a success message in a MessageBox.
        /// </summary>
        /// <param name="message">The success message to display.</param>
        public void DisplaySuccessMessage(string message)
        {
            MessageBox.Show(message, "Success!", MessageBoxButton.OK);
        }

        /// <summary>
        /// Displays an error message to the user using a MessageBox.
        /// </summary>
        /// <param name="message">Error message to be shown.</param>
        public void DisplayErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Opens the AddExpense window and refreshes the category list.
        /// </summary>
        public void DisplayAddExpense()
        {
            _expense = new AddExpense(_presenter, this);
            _expense.Show();
            _presenter.RefreshCategoryList();
           
        }

        /// <summary>
        /// Updates the category ComboBox in the Filter window with the given list of categories.
        /// Optionally preselects a given category.
        /// </summary>
        /// <param name="categories">List of category names to display.</param>
        /// <param name="selectedCategory">Optional category to preselect.</param>
        public void DisplayCategoryFilterWindow(List<string> categories, string selectedCategory)
        {
            if (_filterWindow == null) return;
            _filterWindow.CategoryComboBox.ItemsSource = null;
            _filterWindow.CategoryComboBox.ItemsSource = categories;

            if (!string.IsNullOrEmpty(selectedCategory))
            {
                _filterWindow.CategoryComboBox.SelectedItem = selectedCategory;
            }
        }

        /// <summary>
        /// Updates the category ComboBox in the AddExpense window with the given list of categories.
        /// Optionally preselects a given category.
        /// </summary>
        /// <param name="categories">List of category names to display.</param>
        /// <param name="selectedCategory">Optional category to preselect.</param>
        public void DisplayCategoryExpense(List<string> categories, string selectedCategory)
        {
            if (_expense == null) return;
            _expense.CategoryComboBox.ItemsSource = null;
            _expense.CategoryComboBox.ItemsSource = categories;

            if (!string.IsNullOrEmpty(selectedCategory))
            {
                _expense.CategoryComboBox.SelectedItem = selectedCategory;
            }
        }

        /// <summary>
        /// Displays a list of budget items in the Filter window's DataGrid.
        /// </summary>
        /// <param name="items">List of budget items to display.</param>
        public void DisplayItems(List<BudgetItem> items)
        {
            if (_filterWindow != null)
            {
                _filterWindow.ExpenseDataGrid.ItemsSource = items;
            }
        }

        /// <summary>
        /// Retrieves the selected start date from the Filter window.
        /// </summary>
        /// <returns>The selected start date, or null if not selected.</returns>
        public DateTime? GetStartDate() => _filterWindow?.StartDate;

        /// <summary>
        /// Retrieves the selected end date from the Filter window.
        /// </summary>
        /// <returns>The selected end date, or null if not selected.</returns>
        public DateTime? GetEndDate() => _filterWindow?.EndDate;

        /// <summary>
        /// Gets the current state of the "By Month" summary checkbox from the filter window.
        /// </summary>
        /// <returns>True if "By Month" is selected; otherwise, false.</returns>
        public bool DisplayByMonthSummary() => _filterWindow?.ByMonthCheckBox.IsChecked == true;

        /// <summary>
        /// Gets the current state of the "By Category" summary checkbox from the filter window.
        /// </summary>
        /// <returns>True if "By Category" is selected; otherwise, false.</returns>
        public bool DisplayByCategorySummary() => _filterWindow?.ByCategoryCheckBox.IsChecked == true;

        /// <summary>
        /// Checks whether the "Filter By Category?" checkbox is currently enabled in the filter window.
        /// </summary>
        /// <returns>
        /// True if the checkbox is checked, indicating the user wants to filter by category;
        /// false otherwise.
        /// </returns>
        public bool DisplayIsCategoryFilter() => _filterWindow?.FilterByCategory.IsChecked == true;

        /// <summary>
        /// Retrieves the currently selected category from the category dropdown in the filter window.
        /// </summary>
        /// <returns>
        /// A string representing the selected category, or an empty string if no selection is made.
        /// </returns>
        public string RenameSelectedCategory() => _filterWindow?.CategoryComboBox.SelectedItem?.ToString() ?? "";

        /// <summary>
        /// Open the window to update expense.
        /// </summary>
        /// <param name="expense">The expense to update.</param>
        /// <param name="onCompleteUpdate">The action to complete when the update is successfull.</param>
        public void DisplayExpenseUpdate(Expense expense, Action onCompleteUpdate)
        {
            _expenseToUpdate = expense;
            _updateWindow = new UpdateWindow(_expenseToUpdate, _presenter, this, onCompleteUpdate);
            _updateWindow.Show();
        }
    }
}
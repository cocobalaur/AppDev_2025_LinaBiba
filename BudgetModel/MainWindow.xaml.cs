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
        /// Event handler for clicking the OK button to load or create a database file.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments associated with the button click.</param>
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            bool dbInitialized = FileDatabaseSelection();

            if (dbInitialized)
            {
                _filterWindow = new Filter(_presenter, this);
                _filterWindow.Show();
                _presenter.RefreshCategoryList();
                this.Hide(); 
            }
        }


        public bool FileDatabaseSelection()
        {
            string path = DirectoryTextBox.Text;

            if (string.IsNullOrWhiteSpace(path))
            {
                DisplayErrorMessage("Please select a valid database file.");
                return false;
            }

            bool success = _presenter.GetDatabase(path);

            if (!success)
            {
                DisplayErrorMessage("Failed to initialize the database.");
            }

            return success;
        }

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

        public void DisplayAddExpense()
        {
            _expense = new AddExpense(_presenter, this);
            _expense.Show();
            _presenter.RefreshCategoryList();
           
        }

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
    }
}
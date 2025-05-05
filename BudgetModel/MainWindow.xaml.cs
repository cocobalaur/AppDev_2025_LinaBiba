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
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Select Budget Database File";
            openFileDialog.Filter = "Database Files (*.db;*.xml;*.json)|*.db;*.xml;*.json|All Files (*.*)|*.*";
            openFileDialog.CheckFileExists = true;
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                DirectoryTextBox.Text = selectedFile;
            }
        }
        /// <summary>
        /// Event handler for clicking the OK button to load or create a database file.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments associated with the button click.</param>
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            FileDatabaseSelection();
            var filterWindow = new Filter(_presenter); 
            filterWindow.Show();
            this.Close();
        }

        public void FileDatabaseSelection()
        {
            // Combine directory and file name to create full path
            string directory = DirectoryTextBox.Text ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string fullPath = System.IO.Path.Combine(directory);

            // Check if file exists
            bool fileExists = File.Exists(fullPath);

            _presenter.GetDatabase(fullPath);
           
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

        public void DisplayAddExpense()
        {
            throw new NotImplementedException();
        }

        public void DisplayCategory(List<string> name, string type)
        {
            throw new NotImplementedException();
        }
    }
}
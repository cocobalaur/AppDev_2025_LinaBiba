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
        private bool _isDatabaseReady = false;

        /// <summary>
        /// Constructor: Initializes the main window and applies the default Light theme on startup.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            _presenter = new Presenter(this);
            
            // Load default Light theme on startup
            var lightTheme = new ResourceDictionary
            {
                Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative)
            };
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(lightTheme);

            // Apply light gradient on startup
            SetWindowGradient("Light", false);

        }

        /// <summary>
        /// Event handler for the Apply Theme button.
        /// Applies the selected theme and dark mode preference,
        /// and manually sets the matching background gradient.
        /// </summary>
        private void ApplyTheme_Click(object sender, RoutedEventArgs e)
        {
            // Get selected theme from ComboBox
            string baseTheme = ((ComboBoxItem)ColorComboBox.SelectedItem)?.Content?.ToString();

            // Determine if dark mode is checked
            bool isDark = DarkModeCheckBox.IsChecked == true;

            // Determine correct theme file path
            string themeFile;
            if (baseTheme == "Blush")
                themeFile = isDark ? "Themes/DarkBlushTheme.xaml" : "Themes/BlushTheme.xaml";
            else if (baseTheme == "Ocean")
                themeFile = isDark ? "Themes/DarkOceanTheme.xaml" : "Themes/OceanTheme.xaml";
            else if (baseTheme == "Lavender")
                themeFile = isDark ? "Themes/DarkLavenderTheme.xaml" : "Themes/LavenderTheme.xaml";
            else // Default to Light/DarkNeutral if unrecognized
                themeFile = isDark ? "Themes/DarkNeutralTheme.xaml" : "Themes/LightTheme.xaml";

            // Load and apply the selected theme resource dictionary
            var newTheme = new ResourceDictionary { Source = new Uri(themeFile, UriKind.Relative) };

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(newTheme);

            // Manually apply the matching gradient background
            SetWindowGradient(baseTheme, isDark);

            // FORCE update on stubborn controls
            ThemeLabel.Foreground = (Brush)Application.Current.Resources["TextBrush"];
            DarkModeCheckBox.Foreground = (Brush)Application.Current.Resources["TextBrush"];
        }

        /// <summary>
        /// Sets the gradient background of the window based on the selected theme and dark mode setting.
        /// </summary>
        /// <param name="themeName">The name of the base theme (e.g., Light, Blush, Ocean, Lavender).</param>
        /// <param name="isDark">True if dark mode is enabled; otherwise, false.</param>
        private void SetWindowGradient(string themeName, bool isDark)
        {
            // Create the gradient brush to be applied to the window background
            LinearGradientBrush gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };

            // Apply color stops based on theme and mode
            if (themeName == "Light")
            {
                if (isDark)
                {
                    // Dark Neutral fallback for Light + DarkMode
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF1E1B1A"), 0));
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF2A2421"), 1));
                }
                else
                {
                    // Light theme vanilla peach gradient
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FFFFFAF3"), 0));
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FFFFEDD7"), 1));
                }
            }
            else if (themeName == "Blush")
            {
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(isDark ? "#FF2D1E2A" : "#FFFCEFFB"), 0));
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(isDark ? "#FF3E2C38" : "#FFFFDDEE"), 1));
            }
            else if (themeName == "Ocean")
            {
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(isDark ? "#FF122529" : "#FFE6F9FA"), 0));
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(isDark ? "#FF1B3238" : "#FFD6F0F3"), 1));
            }
            else if (themeName == "Lavender")
            {
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(isDark ? "#FF2B2135" : "#FFF4E7FF"), 0));
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(isDark ? "#FF3A2E45" : "#FFE3D3FF"), 1));
            }
            else
            {
                // Fallback neutral gradient for unknown theme
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF1E1B1A"), 0));
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF2A2421"), 1));
            }

            // Apply the gradient to the window
            this.Background = gradient;
        }


        private void BrowseDirectory_Click(object sender, RoutedEventArgs e)
        {
            //setting up OpenFileDialog to open files in a folder
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = false;
            openFileDialog.CheckPathExists = true;
            openFileDialog.ValidateNames = false;
            openFileDialog.FileName = "Folder Selection.";

            openFileDialog.Filter = "Folders|*."; //filter to only show folders

            //check if a folder was selected
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFolder = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                DirectoryTextBox.Text = selectedFolder;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FileNameTextBox.Text))
            {
                ShowError("Please enter a database file name.");
            }

            // Combine directory and file name to create full path
            string directory = DirectoryTextBox.Text ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dbFileName = FileNameTextBox.Text;

            string fullPath = System.IO.Path.Combine(directory, dbFileName);

            // Check if file exists
            bool fileExists = File.Exists(fullPath);

            _presenter.GetDatabase(fullPath);

            // Enable expense controls
            _isDatabaseReady = true;
            SetExpenseControlsState(true);

            LoadCategories(); //load categories into ComboBox


            MessageBox.Show(fileExists ?
                $"Successfully opened existing database: {dbFileName}" :
                $"Successfully created new database: {dbFileName}");

        }

        private void LoadCategories()
        {
            CategoryComboBox.Items.Clear(); //reset

            var categoryList = _presenter.GetCategories();

            foreach (var category in categoryList) //add each category to the combobox
            {
                CategoryComboBox.Items.Add(category.Description);
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            //clear all input fields
            ExpenseNameTextBox.Clear();
            ExpenseAmountTextBox.Clear();
            CategoryComboBox.SelectedIndex = -1;
            CategoryComboBox.Text = string.Empty;
            ExpenseDatePicker.SelectedDate = DateTime.Today;

            //uncheck all radio buttons
            foreach (RadioButton categoryType in CategoryTypeRadioPanel.Children)
            {
                categoryType.IsChecked = false;
            }
        }
        /// <summary>
        /// Handles adding a new expense entry after validating user input.
        /// Ensures that all required fields (name, amount, date, category, and category type) are properly filled.
        /// Adds the expense to the database and resets the input fields upon success.
        /// Shows error messages if any validation fails.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments associated with the button click.</param>
        private void AddExpense(object sender, RoutedEventArgs e)
        {
            if (!_isDatabaseReady)
            {
                ShowError("Please set up a database first.");
            }

            try
            {
                //Specific validation message
                //Validate that the user added a name
                if (string.IsNullOrWhiteSpace(ExpenseNameTextBox.Text))
                {
                    throw new Exception("The name value cannot be empty.");
                }
                //Validate the amount
                if (!double.TryParse(ExpenseAmountTextBox.Text, out double amount))
                {
                    throw new Exception("The expense amount must be a valid number.");
                }
                //Validate the date
                if (!ExpenseDatePicker.SelectedDate.HasValue)
                {
                    throw new Exception("Please select a valid date.");
                }
                string name = ExpenseNameTextBox.Text;
                DateTime date = ExpenseDatePicker.SelectedDate.Value;
                string? category;

                if (CategoryComboBox.SelectedItem != null)
                {
                    category = CategoryComboBox.SelectedItem.ToString(); //if category was selected via dropdown we get the content as string
                }
                else
                {
                    //Validate that a category was entered.
                    if (string.IsNullOrWhiteSpace(CategoryComboBox.Text))
                    {
                        throw new Exception("Please enter a category.");
                    }
                    category = CategoryComboBox.Text; //get typed text from comboBox if nothing was selected 
                }

                //Verify that a category type was chosen
                if (!isCategoryTypeChecked())
                {
                    throw new Exception("Please select a category type.");
                }

                AddExpenseToDatabase(date, name, amount, category);

                OnCancelClick(sender, e); //clear

                RefreshCategoryComboBox();

                MessageBox.Show($"Expense '{name}' added successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error adding expense: {ex.Message}");
            }
        }

        private void CategoryComboBox_DropDownClosed(object sender, EventArgs e)
        {
            string categoryName = CategoryComboBox.Text;

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                try
                {
                    Category category = _presenter.CreateOrGetCategory(categoryName);
                    RefreshCategoryComboBox(category.Description); 
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to process category: {ex.Message}");
                }
            }
        }

        private void RefreshCategoryComboBox(string selectedCategory = null)
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

        private void SetExpenseControlsState(bool isEnabled)
        {
            ExpenseNameTextBox.IsEnabled = isEnabled;
            ExpenseAmountTextBox.IsEnabled = isEnabled;
            CategoryComboBox.IsEnabled = isEnabled;
            ExpenseDatePicker.IsEnabled = isEnabled;
            CategoryTypeRadioPanel.IsEnabled = isEnabled;
        }
        private void CategoryTypeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                int categoryType = Convert.ToInt32(radioButton.Tag);
                _presenter.SetCategoryType(categoryType);
            }
        }

        public void AddExpenseToDatabase(DateTime date, string name, double amount, string categoryName)
        {
            _presenter.AddExpense(date, name, amount, categoryName);
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void GetDatabase(string databasePath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add new category (from Create Category area).
        /// </summary>
        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NewCategoryNameBox.Text.Trim();
            string selectedType = (NewCategoryTypeBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(selectedType))
            {
                MessageBox.Show("Please enter a category name and select a type.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = _presenter.AddCategory(name, selectedType);

            if (success)
            {
                MessageBox.Show($"Category '{name}' created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshCategoryComboBox();
            }
            else
            {
                MessageBox.Show("Failed to create category. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            NewCategoryNameBox.Text = "";
            NewCategoryTypeBox.SelectedIndex = -1;
        }

        /// <summary>
        /// Checks if any radio button within the CategoryTypeRadioPanel is selected.
        /// </summary>
        /// <returns>
        /// True if at least one radio button is checked; otherwise, false.
        /// </returns>
        private bool isCategoryTypeChecked()
        {
            foreach (var categoryType in CategoryTypeRadioPanel.Children)
            {
                if (categoryType is RadioButton radioButton && radioButton.IsChecked == true)
                    return true;
            }
            return false;
        }

    }
}
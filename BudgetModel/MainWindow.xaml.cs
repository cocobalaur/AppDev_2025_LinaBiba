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
        /// Applies the selected theme and dark mode preferences.
        /// Updates the application's resource dictionaries and re-applies background gradients.
        /// </summary>
        private void ApplyTheme_Click(object sender, RoutedEventArgs e)
        {
            // Safely get the selected theme name
            if (ColorComboBox.SelectedItem is not ComboBoxItem selectedItem || selectedItem.Content == null)
            {
                DisplayErrorMessage("Please select a theme style first.");
                return;
            }

            string baseTheme = selectedItem.Content.ToString();

            // Determine if dark mode is checked
            bool isDark = DarkModeCheckBox.IsChecked == true;

            // Determine correct theme file path based on selection and dark mode
            string themeFile;
            switch (baseTheme)
            {
                case "Blush":
                    themeFile = isDark ? "Themes/DarkBlushTheme.xaml" : "Themes/BlushTheme.xaml";
                    break;
                case "Ocean":
                    themeFile = isDark ? "Themes/DarkOceanTheme.xaml" : "Themes/OceanTheme.xaml";
                    break;
                case "Lavender":
                    themeFile = isDark ? "Themes/DarkLavenderTheme.xaml" : "Themes/LavenderTheme.xaml";
                    break;
                case "Light":
                    themeFile = isDark ? "Themes/DarkNeutralTheme.xaml" : "Themes/LightTheme.xaml";
                    break;
                default:
                    DisplayErrorMessage("Unknown theme selected.");
                    return;
            }

            try
            {
                // Load and apply the selected theme
                var newTheme = new ResourceDictionary { Source = new Uri(themeFile, UriKind.Relative) };

                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(newTheme);

                // Update background gradient based on theme
                SetWindowGradient(baseTheme, isDark);

                // Force update text color on specific controls                
                DarkModeCheckBox.Foreground = (Brush)Application.Current.Resources["TextBrush"];
            }
            catch (Exception ex)
            {
                DisplayErrorMessage($"Failed to apply theme: {ex.Message}");
            }
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

        /// <summary>
        /// Opens a folder browser dialog and sets the selected folder path to the DirectoryTextBox.
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
        }

        public void FileDatabaseSelection()
        {
            // Combine directory and file name to create full path
            string directory = DirectoryTextBox.Text ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string fullPath = System.IO.Path.Combine(directory);

            // Check if file exists
            bool fileExists = File.Exists(fullPath);

            _presenter.GetDatabase(fullPath);
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
        private void AddExpense(object sender, RoutedEventArgs e)
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
                if (!_presenter.FindCategory(category))
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
    }
}
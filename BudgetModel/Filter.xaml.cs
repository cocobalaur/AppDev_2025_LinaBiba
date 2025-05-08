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
    /// Interaction logic for Filter.xaml
    /// </summary>
    public partial class Filter : Window
    {
        private Presenter _presenter;
        private IView _view;

        public event EventHandler DateRangeChanged;

        public Filter(Presenter presenter, IView view)
        {
            
            InitializeComponent();
            _presenter = presenter;
            _view = view;


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
                _view.DisplayErrorMessage("Please select a theme style first.");
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
                    _view.DisplayErrorMessage("Unknown theme selected.");
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
                _view.DisplayErrorMessage($"Failed to apply theme: {ex.Message}");
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
        /// Handles the click event for the "Add Expense" button.
        /// Delegates to the view to open the AddExpense window.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">Event arguments for the button click.</param>
        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            _view.DisplayAddExpense();
        }

        /// <summary>
        /// Handles the event when a category is selected from the ComboBox.
        /// Refreshes the list of categories in the view.
        /// </summary>
        /// <param name="sender">The ComboBox that triggered the event.</param>
        /// <param name="e">Event arguments associated with the selection.</param>
        private void CategoryComboBox_SelectionFilter(object sender, EventArgs e)
        {
           string categoryName = CategoryComboBox.Text;
            _presenter.RefreshCategoryList(categoryName);
            UpdateSummaryDisplay();
        }


        /// <summary>
        /// Handles changes in either the start or end date picker.
        /// Validates that both dates are selected and shows an error if not.
        /// </summary>
        /// <param name="sender">The DatePicker control that was changed.</param>
        /// <param name="e">Event arguments related to the selection change.</param>
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
 
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                _view.DisplayErrorMessage("Please select both a start and end date.");
                return;
            }

           
        }

        /// <summary>
        /// Event handler that raises the DateRangeChanged event when either date changes.
        /// This allows the MainWindow to trigger the Presenter to filter data.
        /// </summary>
        /// <param name="sender">The DatePicker that changed.</param>
        /// <param name="e">Event arguments for the date change.</param>
        private void OnDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateRangeChanged?.Invoke(this, EventArgs.Empty);
            UpdateSummaryDisplay();
        }

        /// <summary>
        /// Gets the currently selected start date from the StartDatePicker.
        /// </summary>
        public DateTime? StartDate => StartDatePicker?.SelectedDate;

        /// <summary>
        /// Gets the currently selected end date from the EndDatePicker.
        /// </summary>
        public DateTime? EndDate => EndDatePicker?.SelectedDate;

        /// <summary>
        /// Handles changes to any filter-related UI elements (e.g., checkboxes).
        /// Triggers the date filtering and summary update process by raising the DateRangeChanged event.
        /// </summary>
        /// <param name="sender">The checkbox or control that triggered the change.</param>
        /// <param name="e">Event arguments from the routed event.</param>
        private void OnFilterChanged(object sender, RoutedEventArgs e)
        {
            // Notify MainWindow to refresh data displa
            DateRangeChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Indicates whether the "By Month" checkbox is selected.
        /// </summary>
        public bool ByMonth => ByMonthCheckBox?.IsChecked == true;

        /// <summary>
        /// Indicates whether the "By Category" checkbox is selected.
        /// </summary>
        public bool ByCategory => ByCategoryCheckBox?.IsChecked == true;


        /// <summary>
        /// Event handler for changes to summary checkboxes ("By Month" or "By Category").
        /// Triggers an update of the DataGrid based on the current summary selections.
        /// </summary>
        /// <param name="sender">The checkbox that was checked/unchecked.</param>
        /// <param name="e">Event arguments associated with the change.</param>
        private void SummaryCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateSummaryDisplay();
        }

        /// <summary>
        /// Refreshes the DataGrid columns and rows to show a summary view
        /// based on the combination of "By Month" and/or "By Category" selections.
        /// </summary>
        private void UpdateSummaryDisplay()
        {
            if (_presenter == null) 
                return;
 
            // Determine summary mode
            bool byMonth = ByMonthCheckBox.IsChecked == true;
            bool byCategory = ByCategoryCheckBox.IsChecked == true;

            // Clear any existing columns before generating new ones
            ExpenseDataGrid.Columns.Clear();

            // Get the summary data from the Presenter
            var summary = _presenter.GetSummaryTable(byMonth, byCategory, StartDate, EndDate);

            // If no data found, clear the table
            if (summary == null || summary.Count == 0)
            {
                ExpenseDataGrid.ItemsSource = null;
                return;
            }

            var first = summary[0];

            // Handle dynamic dictionaries (Month + Category table with flexible headers)
            if (first is Dictionary<string, object> dictRow)
            {
                // Build columns from dictionary keys
                foreach (var key in dictRow.Keys)
                {
                    ExpenseDataGrid.Columns.Add(new DataGridTextColumn
                    {
                        Header = key,
                        Binding = new Binding($"[{key}]") // Bind dictionary key directly
                    });
                }

                ExpenseDataGrid.ItemsSource = summary;
            }
            else
            {                
                foreach (var prop in first.GetType().GetProperties())
                {
                    ExpenseDataGrid.Columns.Add(new DataGridTextColumn
                    {
                        Header = prop.Name,
                        Binding = new Binding(prop.Name)
                    });
                }

                ExpenseDataGrid.ItemsSource = summary;
            }
        }

    }
}

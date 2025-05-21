using Budget;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
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

        private List<object>? _lastFilteredItems; // Store the full list to reset search
        public event EventHandler DateRangeChanged;
        private int? _lastSelectedExpenseId = null;

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
        /// Handles a double-click on the ExpenseDataGrid row.
        /// Opens an update window for the selected expense.
        /// </summary>
        /// <param name="sender">The DataGrid control that detected the double-click.</param>
        /// <param name="e">Provides information about the double-click event, including mouse position and button state.</param>
        private void ExenseDataGrid_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (ExpenseDataGrid.SelectedItem is BudgetItem selectedItem)
            {
                _lastSelectedExpenseId = selectedItem.ExpenseID;
                var expense = new Expense(selectedItem.ExpenseID, selectedItem.Date, selectedItem.CategoryID, selectedItem.Amount, selectedItem.ShortDescription);
                _presenter.UpdateExpense(expense, UpdateSummaryDisplay);
            }
        }



        /// <summary>
        /// Handles the right click event on the expenseDataGrid.
        /// It ensures the row under the mouse cursor is selected
        /// and that the content menu to operate on the right items.
        /// </summary>
        /// <param name="sender">The ExpenseDataGrid control where the 
        /// right click occurred.</param>
        /// <param name="e">Mouse button even data, including the source
        /// element under the cursor.</param>
        private void ExpenseDataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var depObj = (DependencyObject)e.OriginalSource;
            while (depObj != null && !(depObj is DataGridRow))
            {
                depObj = VisualTreeHelper.GetParent(depObj);
            }


            if (depObj is DataGridRow row)
            {
                row.IsSelected = true;
                ExpenseDataGrid.SelectedItem = row.Item;
                ExpenseDataGrid.Focus();
            }
        }

        /// <summary>
        /// Opens the Update window for editing the selected expense.
        /// Triggered from the "Update" context menu item.
        /// </summary>
        /// <param name="sender">The MenuItem that triggered the event</param>
        /// <param name="e">Event data associated with the menu item click.</param>
        private void UpdateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ExpenseDataGrid.SelectedItem is BudgetItem selectedItem)
            {
                //For delete.
                int selectedIndex = ExpenseDataGrid.SelectedIndex;
                int deletedId = selectedItem.ExpenseID;

                //For update
                _lastSelectedExpenseId = selectedItem.ExpenseID;
                var expense = new Expense(selectedItem.ExpenseID, selectedItem.Date, selectedItem.CategoryID, selectedItem.Amount, selectedItem.ShortDescription);
                _presenter.UpdateExpense(expense, UpdateSummaryDisplay);
            }
        }


        /// <summary>
        /// Deletes the selected expense from the context menu and updates the view.
        /// </summary>
        /// <param name="sender">The MenuItem that triggered the event</param>
        /// <param name="e">Event data associated with the menu item click.</param>
        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ExpenseDataGrid.SelectedItem is BudgetItem selectedItem)
            {
                int selectedIndex = ExpenseDataGrid.SelectedIndex;
                int deletedId = selectedItem.ExpenseID;

                // Get a snapshot of the list
                var items = ExpenseDataGrid.ItemsSource.Cast<BudgetItem>().ToList();
                int? nextId = _presenter.GetNextOrPreviousExpenseId(items, deletedId);

                bool success = _presenter.DeleteExpense(deletedId, out string message, 
                    UpdateSummaryDisplay // Refresh the grid
                );


                if (success)
                {
                    _view.DisplaySuccessMessage(message);
                }
                else
                {
                    _view.DisplayErrorMessage(message);
                }

            }
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
                    var column = new DataGridTextColumn
                    {
                        Header = key,
                        Binding = new Binding($"[{key}]")   // Bind dictionary key directly
                    };
                    // Force right-alignment for any likely numeric category
                    string lowerKey = key.ToLower();
                    if (lowerKey != "month") // everything except 'Month'
                    {
                        column.ElementStyle = new Style(typeof(TextBlock))
                        {
                            Setters = { new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right) }
                        };
                    }

                    ExpenseDataGrid.Columns.Add(column);


                }

                ExpenseDataGrid.ItemsSource = summary;
            }
            else
            {
                foreach (var prop in first.GetType().GetProperties())
                {
                    // Skip displaying these columns (IDs)
                    if (prop.Name == "CategoryID" || prop.Name == "ExpenseID") continue;

                    var column = new DataGridTextColumn
                    {
                        Header = prop.Name,
                        Binding = new Binding(prop.Name)
                    };

                    if (prop.Name.Contains("Date") || prop.Name.Contains("Amount") || prop.Name.Contains("Total"))
                    {
                        column.ElementStyle = new Style(typeof(TextBlock))
                        {
                            Setters = { new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right) }
                        };
                    }

                    ExpenseDataGrid.Columns.Add(column);

                }

            }

            ExpenseDataGrid.ItemsSource = summary;
        }

        /// <summary>
        /// Handles the click event for the Search button.
        /// Performs a case-insensitive search through the currently visible (filtered) budget items
        /// based on the user's input in the SearchBox. Matches are found by comparing both the
        /// <c>ShortDescription</c> and <c>Amount</c> fields of each item.
        /// </summary>
        /// <param name="sender">The Search button that triggered the event.</param>
        /// <param name="e">The event arguments associated with the button click.</param>
        /// <remarks>
        /// If the search box is empty, the method re-applies the full filter logic by raising the
        /// <c>DateRangeChanged</c> event, which causes the Presenter to refresh the filtered data.
        /// If matches are found, the DataGrid displays only those matching entries and scrolls to the first.
        /// If no matches are found, a message box informs the user.
        /// </remarks>
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchBox.Text?.Trim().ToLower();

            // If the search box is empty, reapply the full filter from Presenter
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                DateRangeChanged?.Invoke(this, EventArgs.Empty); // triggers FilterByDate() in MainWindow
                ExpenseDataGrid.SelectedIndex = -1;
                return;
            }

            // Get currently visible filtered items
            var items = ExpenseDataGrid.ItemsSource?.Cast<object>().ToList();
            if (items == null || items.Count == 0) return;

            // Filter matching entries
            var filtered = items.Where(obj =>
            {
                string? description = obj.GetType().GetProperty("ShortDescription")?.GetValue(obj)?.ToString()?.ToLower();
                string? amount = obj.GetType().GetProperty("Amount")?.GetValue(obj)?.ToString()?.ToLower();

                return (description != null && description.Contains(searchTerm)) ||
                       (amount != null && amount.Contains(searchTerm));
            }).ToList();

            if (filtered.Any())
            {
                ExpenseDataGrid.ItemsSource = filtered;
                ExpenseDataGrid.SelectedIndex = 0;
                ExpenseDataGrid.ScrollIntoView(filtered[0]);
            }
            else
            {
                MessageBox.Show("No matching results found.", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Reselects a budget item in the data grid by its ID.
        /// If found, the item is selected, focused, and scrolled into view.
        /// </summary>
        /// <param name="expenseId">The ID of the expense to reselect.</param>
        public void ReselectExpenseById(int? expenseId)
        {
            if (expenseId == null)
                return;

            foreach (var item in ExpenseDataGrid.Items)
            {
                if (item is BudgetItem budget && budget.ExpenseID == expenseId)
                {
                    ExpenseDataGrid.SelectedItem = item;
                    ExpenseDataGrid.ScrollIntoView(item);
                    ExpenseDataGrid.Focus();
                    break;
                }
            }


        }
    }




}

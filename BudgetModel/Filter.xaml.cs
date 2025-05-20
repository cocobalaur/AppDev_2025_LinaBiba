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
        /// Handles a double click event on the ExpenseDataGrid row.
        /// It will open the updateWindow pre-filled with the selected expense data.
        /// </summary>
        /// <param name="sender">The DataGrid control that detected the double-click.</param>
        /// <param name="e">Provides information about the double-click event, including mouse position and button state.</param>
        private void ExenseDataGrid_MouseDoubleClick(object sender, RoutedEventArgs e) //not working???
        {
            //If the selected object is a Budget item, find the expense info and open the update window.
            if (ExpenseDataGrid.SelectedItem is BudgetItem selectedItem)
            {
                Expense theSelectedExpense = new Expense(selectedItem.ExpenseID, selectedItem.Date, selectedItem.CategoryID, selectedItem.Amount, selectedItem.ShortDescription);
                _presenter.UpdateExpense(theSelectedExpense, UpdateSummaryDisplay);

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
            }
        }
        /// <summary>
        /// Handles the click event for the "Update" context menu item.
        /// Opens the UpdateWindow with the selected expense for editing.
        /// </summary>
        /// <param name="sender">The MenuItem that triggered the event</param>
        /// <param name="e">Event data associated with the menu item click.</param>
        private void UpdateMenuItem_Click(object sender, RoutedEventArgs e)
        {

            //Make sure that the object is a Budget Item
            if (ExpenseDataGrid.SelectedItem is BudgetItem selectedItem)
            {
                //Get the expense information from the 
                Expense theSelectedExpense = new Expense(selectedItem.ExpenseID, selectedItem.Date, selectedItem.CategoryID, selectedItem.Amount, selectedItem.ShortDescription);
                _presenter.UpdateExpense(theSelectedExpense, UpdateSummaryDisplay);
                //refresh to load updated expense
            }
        }

        /// <summary>
        /// Hnaldes the click event for the "Delete" context menu item.
        /// Deletes the selected expense using the presenter and display
        /// a message.
        /// </summary>
        /// <param name="sender">The MenuItem that triggered the event</param>
        /// <param name="e">Event data associated with the menu item click.</param>
        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ExpenseDataGrid.SelectedItem is BudgetItem selectedItem)
            {
                bool success = _presenter.DeleteExpense(selectedItem.ExpenseID, out string message, UpdateSummaryDisplay);

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
        //private void UpdateSummaryDisplay()
        //{
        //    if (_presenter == null)
        //        return;

        //    // Determine summary mode
        //    bool byMonth = ByMonthCheckBox.IsChecked == true;
        //    bool byCategory = ByCategoryCheckBox.IsChecked == true;

        //    // Clear any existing columns before generating new ones
        //    ExpenseDataGrid.Columns.Clear();

        //    // Get the summary data from the Presenter
        //    var summary = _presenter.GetSummaryTable(byMonth, byCategory, StartDate, EndDate);

        //    // If no data found, clear the table
        //    if (summary == null || summary.Count == 0)
        //    {
        //        ExpenseDataGrid.ItemsSource = null;
        //        return;
        //    }

        //    var first = summary[0];

        //    // Handle dynamic dictionaries (Month + Category table with flexible headers)
        //    if (first is Dictionary<string, object> dictRow)
        //    {
        //        // Build columns from dictionary keys
        //        foreach (var key in dictRow.Keys)
        //        {
        //            ExpenseDataGrid.Columns.Add(new DataGridTextColumn
        //            {
        //                Header = key,
        //                Binding = new Binding($"[{key}]") // Bind dictionary key directly
        //            });
        //        }

        //        ExpenseDataGrid.ItemsSource = summary;
        //    }
        //    else
        //    {
        //        foreach (var prop in first.GetType().GetProperties())
        //        {
        //            // Skip displaying these columns (IDs)
        //            if (prop.Name == "CategoryID" || prop.Name == "ExpenseID") continue;

        //            ExpenseDataGrid.Columns.Add(new DataGridTextColumn
        //            {
        //                Header = prop.Name,
        //                Binding = new Binding(prop.Name)
        //            });
        //        }

        //        ExpenseDataGrid.ItemsSource = summary;
        //    }
        //}

        /// <summary>
        /// Refreshes the DataGrid columns and rows to show a summary view
        /// based on the combination of "By Month" and/or "By Category" selections.
        /// Also manages the visibility of the chart button and the chart control.
        /// </summary>
        private void UpdateSummaryDisplay()
        {
            if (_presenter == null)
                return;

            // Check if both filters are active
            bool byMonth = ByMonthCheckBox.IsChecked == true;
            bool byCategory = ByCategoryCheckBox.IsChecked == true;

            // Hide chart by default
            MyChartControl.Visibility = Visibility.Collapsed;
            NoResultLabel.Visibility = Visibility.Collapsed;

            // Clear DataGrid columns
            ExpenseDataGrid.Columns.Clear();

            // Get summary data
            var summary = _presenter.GetSummaryTable(byMonth, byCategory, StartDate, EndDate);

            if (summary == null || summary.Count == 0)
            {
                ExpenseDataGrid.ItemsSource = null;
                ChartButton.Visibility = Visibility.Collapsed;
                NoResultLabel.Text = "No results match your filters.";
                NoResultLabel.Visibility = Visibility.Visible;
                return;
            }

            var first = summary[0];

            if (first is Dictionary<string, object> dictRow)
            {
                foreach (var key in dictRow.Keys)
                {
                    var column = new DataGridTextColumn
                    {
                        Header = key,
                        Binding = new Binding($"[{key}]")
                    };

                    if (!key.Equals("Month", StringComparison.OrdinalIgnoreCase))
                    {
                        column.ElementStyle = new Style(typeof(TextBlock))
                        {
                            Setters = { new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right) }
                        };
                    }

                    ExpenseDataGrid.Columns.Add(column);
                }
            }
            else
            {
                foreach (var prop in first.GetType().GetProperties())
                {
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

            // Bind data
            ExpenseDataGrid.ItemsSource = summary;

            // Chart button logic
            ChartButton.Visibility = (byMonth && byCategory) ? Visibility.Visible : Visibility.Collapsed;

            // Let presenter decide whether to show the chart
            _presenter.DisplayChartIfEnabled();
        }


        private void ChartButton_Click(object sender, RoutedEventArgs e)
        {
            bool byMonth = ByMonthCheckBox?.IsChecked == true;
            bool byCategory = ByCategoryCheckBox?.IsChecked == true;

            if (byMonth && byCategory)
            {
                var summaryData = ExpenseDataGrid.ItemsSource?.OfType<Dictionary<string, object>>().ToList();

                if (summaryData == null || summaryData.Count == 0)
                {
                    MessageBox.Show("No data available to display in the chart.", "Chart Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Remove TOTALS row
                summaryData = summaryData.Where(row =>
                    !row.TryGetValue("Month", out object value) || value?.ToString() != "TOTALS").ToList();

                var categories = summaryData.FirstOrDefault()?.Keys
                    .Where(k => k != "Month")
                    .ToList() ?? new List<string>();

                MyChartControl.Visibility = Visibility.Visible;
                MyChartControl.SetData(summaryData.Cast<object>().ToList(), categories);
            }
            else
            {
                MessageBox.Show("Please check both 'By Month' and 'By Category' to view the chart.", "Chart Filter", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private int _lastSearchIndex = -1;

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchTextBox.Text.Trim().ToLower();
            var items = ExpenseDataGrid.ItemsSource?.Cast<object>().ToList();

            if (items == null || items.Count == 0 || string.IsNullOrWhiteSpace(searchTerm))
            {
                NoResultLabel.Text = "Nothing found.";
                NoResultLabel.Visibility = Visibility.Visible;
                return;
            }

            int start = (_lastSearchIndex + 1) % items.Count;
            for (int i = 0; i < items.Count; i++)
            {
                int index = (start + i) % items.Count;
                var item = items[index];
                var row = item as Dictionary<string, object>;

                if (row != null && row.Values.Any(val => val.ToString().ToLower().Contains(searchTerm)))
                {
                    ExpenseDataGrid.SelectedItem = item;
                    ExpenseDataGrid.ScrollIntoView(item);
                    _lastSearchIndex = index;
                    NoResultLabel.Visibility = Visibility.Collapsed;
                    return;
                }
            }

            NoResultLabel.Text = "Nothing found.";
            NoResultLabel.Visibility = Visibility.Visible;
            System.Media.SystemSounds.Beep.Play();
        }


        public void ShowChart(List<Dictionary<string, object>> groupedData, List<string> allCategories)
        {
            MyChartControl.Visibility = Visibility.Visible;
            MyChartControl.SetData(groupedData.Cast<object>().ToList(), allCategories);
        }


        public void HideChart()
        {
            MyChartControl.Visibility = Visibility.Collapsed;
            ExpenseDataGrid.Visibility = Visibility.Visible;
        }


        private void ViewModeSelector_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModeSelector.SelectedItem is ComboBoxItem selected)
            {
                string mode = selected.Content.ToString();

                // Safeguard in case components haven't initialized
                if (ExpenseDataGrid == null || MyChartControl == null)
                    return;

                else if (mode == "Pie Chart")
                {
                    if (ByMonthCheckBox.IsChecked == true && ByCategoryCheckBox.IsChecked == true)
                    {
                        ExpenseDataGrid.Visibility = Visibility.Collapsed;
                        MyChartControl.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MessageBox.Show("Chart view only works with both 'By Month' and 'By Category' checked.");
                        ViewModeSelector.SelectedIndex = 0;
                    }
                }

            }
        }


    }
}

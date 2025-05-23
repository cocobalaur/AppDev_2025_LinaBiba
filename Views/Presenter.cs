using Budget;
using System.IO;
using Views;
using static Budget.Category;

namespace BudgetModel
{
    /// <summary>
    /// The Presenter acts as a middle layer between the View (MainWindow) and the Model (Database, Categories, etc.).
    /// It processes user inputs and delegates actions to the appropriate model classes.
    /// </summary>
    public class Presenter //need to add implementation catch error if no db file or folder path entered
    {
        private IView _view;

        private HomeBudget? _budget;

        /// <summary>
        /// Gets or sets the associated view.
        /// </summary>
        public IView View
        {
            get { return _view; }
            set { _view = value; }
        }

        /// <summary>
        /// Initializes the Presenter and associates it with a View.
        /// </summary>
        /// <param name="view">The View (UI) to be managed.</param>
        public Presenter(IView view)
        {
            _view = view;
        }

        /// <summary>
        /// Creates a new HomeBudget database connection, or opens an existing one.
        /// </summary>
        /// <param name="databasePath">Full file path of the database.</param>
        public bool GetDatabase(string databasePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(databasePath))
                {
                    _view.DisplayErrorMessage("Please select a folder using the Browse button.");
                    return false; // STOP if no directory selected
                }

                string addedExtension = HomeBudget.VerifyFile(databasePath);

                bool IsNewDatabase = !System.IO.File.Exists(addedExtension); //does the db file exist -> to set up for homebudget boolean
               
                _budget = new HomeBudget(databasePath, IsNewDatabase);

                if (IsNewDatabase)
                {
                    _view.DisplaySuccessMessage("Successfully opened new database.");
                }
                else
                {
                    _view.DisplaySuccessMessage("Successfully opened database.");
                }

                return true;
            }
            catch (Exception ex)
            {
                _view.DisplayErrorMessage($"Error setting up database: {ex.Message}");
                return false; // Ensure a boolean is returned in all cases
            }
        }

        /// <summary>
        /// Adds a new expense record to the database.
        /// </summary>
        /// <param name="date">The date of the expense.</param>
        /// <param name="name">The name/description of the expense.</param>
        /// <param name="amount">The amount of the expense.</param>
        /// <param name="categoryName">The associated category name.</param>
        public void ProcessNewAddExpense(DateTime date, string name, double amount, string categoryName)
        {
            try
            {
                if (_budget == null)
                {
                    _view.DisplayErrorMessage("Database not initialized.");
                    return;
                }

                _view.DisplayAddExpense();              //open the add expense window
                Category category = GetCategory(categoryName);
        
                int categoryId = category.Id; //get category id when adding the expense 

                _budget.expenses.Add(date, amount, name, categoryId);
                _view.DisplaySuccessMessage($"Expense '{name}' added successfully.");
            }
            catch (Exception ex)
            {
                _view.DisplayErrorMessage($"Error adding expense: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all categories from the database.
        /// </summary>
        /// <returns>List of all available categories.</returns>
        public List<Category> GetCategories()
        {
            if (_budget == null)
            {
                _view.DisplayErrorMessage("Database not initialized.");
                return new List<Category>();
            }

            return _budget.categories.List();
        }

        /// <summary>
        /// Checks if a category with the given name exists in the list of categories.
        /// </summary>
        /// <param name="category">The category name to search for.</param>
        /// <returns>True if the category exists; otherwise, false.</returns>
        public bool FindCategory(string category)
        {
            List<Category> categories = GetCategories();

            foreach (Category categoryName in categories) //loop through each category and check if any category matches the given category
            {
                if (categoryName.Description.ToLower() == category.ToLower())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Retrieves a category by its description match.
        /// </summary>
        /// <param name="categoryDescription">The description of the category to retrieve.</param>
        /// <returns>The matching object.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no matching category is found or if an error occurs during retrieval.
        /// </exception>
        public Category GetCategory(string categoryDescription)
        {
            try
            {
                string inputName = categoryDescription.ToLower();

                List<Category> categories = _budget.categories.List();

                //find existing category
                for (int i = 0; i < categories.Count; i++)
                {
                    if (categories[i].Description.ToLower() == inputName)
                    {
                        return categories[i];
                    }
                }

                throw new InvalidOperationException("Failed to retrieve the category.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred retrieving the category.", ex);
            }
        }

        /// <summary>
        /// Refreshes the list of categories and updates the view components
        /// that display category filters and expense entries.
        /// </summary>
        /// <param name="selectedCategory">
        /// The category that should be pre-selected in the updated view, this is optional.
        /// </param>
        public void RefreshCategoryList(string selectedCategory = null)
        {
            if (_budget == null)
            {
                _view.DisplayErrorMessage("Database not initialized.");
            }

            List<string> categoryNames = new List<string>();

            foreach (Category category in GetCategories())
            {
                categoryNames.Add(category.Description);
            }

            categoryNames.Sort();
            _view.DisplayCategoryFilterWindow(categoryNames, selectedCategory);
            _view.DisplayCategoryExpense(categoryNames, selectedCategory);
        }

        /// <summary>
        /// Adds a new category based on user input (name and type string).
        /// </summary>
        /// <param name="name">The name of the new category.</param>
        /// <param name="typeString">The type selected ("Income", "Expense", "Credit", or "Savings").</param>
        /// <returns>True if category created successfully; false otherwise.</returns>
        public bool AddCategory(string name, string typeString)
        {
            if (_budget == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(typeString))
            {
                _view.DisplayErrorMessage("Please enter a category name and select a type.");
                return false;
            }

            Category.CategoryType type;

            if (typeString == "Income")
            {
                type = Category.CategoryType.Income;
            }
            else if (typeString == "Expense")
            {
                type = Category.CategoryType.Expense;
            }
            else if (typeString == "Credit")
            {
                type = Category.CategoryType.Credit;
            }
            else if (typeString == "Savings")
            {
                type = Category.CategoryType.Savings;
            }
            else
            {
                _view.DisplayErrorMessage("Invalid category type selected.");
                return false;
            }

            try
            {
                _budget.categories.Add(name, type);
                RefreshCategoryList();
                _view.DisplaySuccessMessage($"Category '{name}' created successfully.");
                return true;
            }
            catch (ArgumentException ex)
            {
                // Specific handling for already existing categories or bad input
                _view.DisplayErrorMessage("Failed to create category. Please try again.");
                return false;
            }
            catch (Exception ex)
            {
                // General unexpected error
                System.Diagnostics.Debug.WriteLine($"Unexpected error when adding category: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Filters and displays budget data based on the selected date range and summary options.
        /// It supports displaying full entries, summaries by month, summaries by category,
        /// and a combined summary by both month and category.
        /// </summary>
        public void FilterByDate()
        {
            // Retrieve selected date range from the UI
            DateTime? start = _view.GetStartDate();
            DateTime? end = _view.GetEndDate();

            // Ensure the dates and database are valid before proceeding
            if (start == null || end == null || _budget == null)
            {
                _view.DisplayErrorMessage("Please select a valid date range and ensure the database is initialized.");
                return;
            }

            try
            {
                // Fetch items within the date range
                var items = _budget.GetBudgetItems(start, end, false, -1);

                // Get list of all category names
                var allCategories = _budget.categories.List().Select(c => c.Description).Distinct().ToList();

                bool byMonth = _view.DisplayByMonthSummary();
                bool byCategory = _view.DisplayByCategorySummary();

                bool isCategoryFilter = _view.DisplayIsCategoryFilter();
                string selectedCategory = _view.RenameSelectedCategory();

                if (isCategoryFilter && !string.IsNullOrWhiteSpace(selectedCategory))
                {
                    items = items.Where(i => i.Category == selectedCategory).ToList();
                }


                if (byMonth && byCategory)
                {
                    // Summarize by month and category, including empty categories
                    var grouped = items
                        .GroupBy(i => new { i.Date.Year, i.Date.Month })
                        .SelectMany(g =>
                        {
                            var month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("yyyy-MM");
                            return allCategories.Select(cat => new BudgetItem
                            {
                                Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                                Category = cat,
                                Amount = g.Where(i => i.Category == cat).Sum(i => i.Amount)
                            });

                        }).ToList();

                    _view.DisplayItems(grouped);
                }
                else if (byMonth)
                {
                    // Summarize only by month
                    var grouped = items
                        .GroupBy(i => new { i.Date.Year, i.Date.Month })
                        .Select(g => new BudgetItem
                        {
                            Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                            Amount = g.Sum(i => i.Amount)
                        }).ToList();

                    _view.DisplayItems(grouped);
                }
                else if (byCategory)
                {
                    // Summarize only by category, including empty ones
                    var grouped = allCategories
                        .Select(cat => new BudgetItem
                        {
                            Category = cat,
                            Amount = items.Where(i => i.Category == cat).Sum(i => i.Amount)
                        }).ToList();

                    _view.DisplayItems(grouped);
                }
                else
                {
                    // Show all raw items if no summary selected
                    _view.DisplayItems(items);
                }
            }
            catch (Exception ex)
            {
                _view.DisplayErrorMessage($"Failed to filter items: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds a summary table for the DataGrid based on selected filters:
        /// by month, by category, or both. When both are selected, all categories are shown per month,
        /// including those with zero expenses. Adds a total row when both filters are active.
        /// </summary>
        /// <param name="byMonth">Whether to summarize by month.</param>
        /// <param name="byCategory">Whether to summarize by category.</param>
        /// <param name="startDate">Start date of the filter range.</param>
        /// <param name="endDate">End date of the filter range.</param>
        /// <returns>A list of summarized result rows (as anonymous objects or dictionaries).</returns>

        public List<object> GetSummaryTable(bool byMonth, bool byCategory, DateTime? startDate, DateTime? endDate)
        {
            if (_budget == null)
            {
                _view.DisplayErrorMessage("Database not initialized.");
                return new List<object>();
            }

            if (startDate == null || endDate == null)
            {
                _view.DisplayErrorMessage("Both start and end dates must be selected.");
                return new List<object>();
            }

            // List of all category names for consistency
            var allCategories = _budget.categories.List()
                .Select(c => c.Description)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            var items = _budget.GetBudgetItems(startDate, endDate, false, -1);

            if (byMonth && byCategory)
            {
                // Group by month and include all categories in each row
                var grouped = items
                    .GroupBy(i => new { i.Date.Year, i.Date.Month })
                    .OrderBy(g => new DateTime(g.Key.Year, g.Key.Month, 1))
                    .Select(g =>
                    {
                        var row = new Dictionary<string, object>();
                        string month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("yyyy-MM");
                        row["Month"] = month;

                        foreach (var cat in allCategories)
                        {
                            double total = g.Where(i => i.Category == cat).Sum(i => i.Amount);
                            row[cat] = Math.Round(total, 2); // Ensure double
                        }

                        return (object)row;
                    }).ToList();

                // Add a TOTAL row summing each category across all months
                var totalRow = new Dictionary<string, object>();
                totalRow["Month"] = "TOTALS";

                foreach (var cat in allCategories)
                {
                    double total = grouped
                        .Cast<Dictionary<string, object>>()
                        .Sum(row => row.ContainsKey(cat) ? Convert.ToDouble(row[cat]) : 0);

                    totalRow[cat] = Math.Round(total, 2);
                }

                grouped.Add(totalRow);
                return grouped;
            }
            else if (byMonth)
            {
                // Show month and total only
                return items
                    .GroupBy(i => new { i.Date.Year, i.Date.Month })
                    .OrderBy(g => new DateTime(g.Key.Year, g.Key.Month, 1))
                    .Select(g => new
                    {
                        Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("yyyy-MM"),
                        Total = Math.Round(g.Sum(i => i.Amount), 2)
                    })
                    .Cast<object>()
                    .ToList();
            }
            else if (byCategory)
            {
                // Show category totals, but omit zero entries
                return allCategories
                    .Select(cat => new
                    {
                        Category = cat,
                        Total = Math.Round(items.Where(i => i.Category == cat).Sum(i => i.Amount), 2)
                    })
                    .Where(entry => entry.Total != 0)
                    .Cast<object>()
                    .ToList();
            }
            else
            {
                // Return full items (raw data view)
                return items.Cast<object>().ToList();
            }
        }

        /// <summary>
        /// Deletes an expense from the budget database by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the expense to delete.</param>
        /// <param name="resultMessage">
        /// A message describing the result of the operation, either confirming deletion or detailing the error.
        /// </param>
        /// <param name="onDeleteComplete">The action to perform once the expense has been deleted.</param>
        /// <returns>True if the expense was successfully deleted; otherwise, false.</returns>
        public bool DeleteExpense(int id, out string resultMessage, Action onDeleteComplete)
        {
            if (_budget == null)
            {
                resultMessage = "Database not initialized.";
                return false;
            }

            try
            {
                var items = _view.GetAllItems();
                int nextId = GetNextOrPreviousExpenseId(items, id);
                _budget.expenses.Delete(id);
                onDeleteComplete.Invoke();
                _view.ReselectExpenseOnceUpdated(nextId);
                resultMessage = "Expense deleted successfully.";
                return true;
            }
            catch (Exception ex)
            {
                resultMessage = $"Error deleting expense: {ex.Message}";
                return false;
            }
        }
        /// <summary>
        /// Updates an existing expense in the database with the provided details.
        /// Validates the input, parses the amount, and creates or retrieves the corresponding category.
        /// </summary>
        /// <param name="id">The unique identifier of the expense to update.</param>
        /// <param name="name">The new description of the expense.</param>
        /// <param name="amountString">The amount as a string, which will be parsed into a numeric value.</param>
        /// <param name="date">The new date of the expense.</param>
        /// <param name="categoryName">The name of the category to associate with the expense.</param>
        /// <param name="resultMessage">
        /// A message describing the outcome of the operation, including success confirmation or validation errors.
        /// </param>
        /// <returns>True if the expense was updated successfully; otherwise, false.</returns>
        public bool UpdateExistingExpense(int id, string name, string amountString,
            DateTime date, string categoryName, out string resultMessage)
        {
            resultMessage = "";

            if (_budget == null)
            {
                resultMessage = "Database not initialized.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(categoryName) || string.IsNullOrWhiteSpace(amountString))
            {
                resultMessage = "Please fill in all fields.";
                return false;
            }
            if (!double.TryParse(amountString, out double amount))
            {
                resultMessage = "Invalid amount.";
                return false;
            }
            try
            {
                Category category = GetCategory(categoryName);
                int categoryId = category.Id;

                _budget.expenses.UpdateExpenses(id, date, amount, name, categoryId);
                resultMessage = "Expense updated successfullly!!";
                return true;
            }
            catch (Exception ex)
            {
                resultMessage = $"Failed to update expense: {ex.Message}";
                return false;
            }
        }


        /// <summary>
        /// Retrieves the description of a category based on its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the category.</param>
        /// <returns>The description of the category.</returns>
        public string GetCategoryName(int id)
        {
            Category category = _budget.categories.GetCategoryFromId(id);

            return category.Description;
        }
        /// <summary>
        /// Triggers the view to display the update UI for a given expense.
        /// </summary>
        /// <param name="expense">The expense to update.</param>
        /// <param name="onUpdateComplete">Callback to refresh the view once update is completed.</param>
        public void UpdateExpense(Expense expense, Action onUpdateComplete)
        {
            try
            {
                _view.DisplayExpenseUpdate(expense, onUpdateComplete);
            }
            catch (Exception ex)
            {
                _view.DisplayErrorMessage($"Error during update: {ex.Message}");
            }
        }


        /// <summary>
        /// Returns all category descriptions for chart labeling and to include zeroes.
        /// </summary>
        public List<string> GetAllCategoryNames()
        {
            return _budget.categories.List().Select(c => c.Description).Distinct().OrderBy(c => c).ToList();
        }

        /// <summary>
        /// Returns grouped expense data by month and category using existing summary logic.
        /// Filters out the "TOTAL" row if present.
        /// </summary>
        /// <param name="start">Start date of the range.</param>
        /// <param name="end">End date of the range.</param>
        /// <returns>A list of dictionaries, each representing a month's grouped expenses.</returns>
        public List<Dictionary<string, object>> GetGroupedExpensesByMonthAndCategory(DateTime? start, DateTime? end)
        {
            var categories = GetAllCategoryNames();
            var items = _budget.GetBudgetItems(start, end, false, -1);

            return items
                .GroupBy(i => new { i.Date.Year, i.Date.Month })
                .OrderBy(g => new DateTime(g.Key.Year, g.Key.Month, 1))
                .Select(g =>
                {
                    var row = new Dictionary<string, object>();
                    string month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("yyyy-MM");
                    row["Month"] = month;

                    foreach (var cat in categories)
                    {
                        double total = g.Where(i => i.Category == cat).Sum(i => i.Amount);
                        row[cat] = Math.Round(total, 2);
                    }
                    return row;
                }).ToList();
        }

        /// <summary>
        /// Determines whether the chart view should be displayed based on the current filter state.
        /// If both 'By Month' and 'By Category' are selected, it retrieves the grouped expense data
        /// and passes it to the view to render the pie chart.
        /// Otherwise, it hides the chart view.
        /// </summary>
        public void DisplayChartIfEnabled()
        {
            if (_budget == null) return;

            // Check if both summary filters are enabled
            bool byMonth = _view.DisplayByMonthSummary();
            bool byCategory = _view.DisplayByCategorySummary();

            if (byMonth && byCategory)
            {
                // Retrieve grouped data and category list for chart visualization
                var groupedData = GetGroupedExpensesByMonthAndCategory(_view.GetStartDate(), _view.GetEndDate());
                var allCategories = GetAllCategoryNames();

                // Display the chart with provided data
                _view.ShowChart(groupedData, allCategories);
            }
            else
            {
                // Hide chart if filter conditions are not satisfied
                _view.HideChart();
            }
        }

        /// <summary>
        /// Returns the next or previous expense ID relative to the deleted one, to maintain selection in the UI.
        /// </summary>
        /// <param name="items">The list of current budget items.</param>
        /// <param name="deletedId">The ID of the deleted expense.</param>
        /// <returns>The ID of the next or previous expense, or -1 if none found.</returns>
        public int GetNextOrPreviousExpenseId(List<BudgetItem> items, int deletedId)
        {
            if (items == null || items.Count == 0)
                return -1;

            int index = items.FindIndex(x => x.ExpenseID == deletedId);
            if (index == -1) return -1;

            // Try next
            if (index + 1 < items.Count)
                return items[index + 1].ExpenseID;

            // Try previous
            if (index - 1 >= 0)
                return items[index - 1].ExpenseID;

            return -1;
        }


    }
}


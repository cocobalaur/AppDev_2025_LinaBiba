using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Views;
using Budget;
using System.Data.Common;
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
        private CategoryType _selectedCategoryType = CategoryType.Expense; //default category type

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

                bool IsNewDatabase = !System.IO.File.Exists(databasePath); //does the db file exist -> to set up for homebudget boolean

                _budget = new HomeBudget(databasePath, IsNewDatabase);
                _view.DisplaySuccessMessage("Successfully opened database.");
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

                _view.DisplayAddExpense(); //open the add expense window
                Category category = CreateOrGetCategory(categoryName);

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
        /// Sets the currently selected category type for future category creation.
        /// </summary>
        /// <param name="categoryType">Integer representation of CategoryType (enum).</param>
        public void SetCategoryType(int categoryType)
        {
            _selectedCategoryType = (CategoryType)categoryType;
        }

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
        /// Searches for a category by description or creates a new one if it doesn't exist.
        /// </summary>
        /// <param name="categoryDescription">The category name to find or create.</param>
        /// <returns>The existing or newly created Category object.</returns>
        /// <exception cref="InvalidOperationException">Thrown if creation or retrieval fails.</exception>
        public Category CreateOrGetCategory(string categoryDescription)
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

                //if not found, try to create new category
                _budget.categories.Add(categoryDescription, _selectedCategoryType);

                //search again
                categories = _budget.categories.List();

                for (int i = 0; i < categories.Count; i++)
                {
                    if (categories[i].Description.ToLower() == inputName)
                    {
                        return categories[i];
                    }
                }

                throw new InvalidOperationException("Failed to create or retrieve the category.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while creating or retrieving the category.", ex);
            }
        }
      
        // <summary>
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

        //Method for testing purpose 
        /// <summary>
        /// Method to get the categoryType that was selected by the user.
        /// </summary>
        /// <returns>Return the selected type for the category.</returns>
        public CategoryType GetSelectedCategoryType()
        {
            return _selectedCategoryType;
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
                var allCategories = _budget.categories.List().Select(c => c.Description).Distinct().ToList();

                bool byMonth = _view.GetByMonthSummary();
                bool byCategory = _view.GetByCategorySummary();

                if (byMonth && byCategory)
                {
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
                    _view.DisplayItems(items);
                }
            }
            catch (Exception ex)
            {
                _view.DisplayErrorMessage($"Failed to filter items: {ex.Message}");
            }
        }

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

                // Add TOTAL row
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
                return items.Cast<object>().ToList();
            }
        }

    }
}


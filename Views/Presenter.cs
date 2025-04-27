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
        private IView _view; // Reference to the View (UI)

        private HomeBudget? _budget; // HomeBudget instance representing the database connection
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
        public void GetDatabase(string databasePath)
        {
            try
            {
                bool IsNewDatabase = !System.IO.File.Exists(databasePath); //does the db file exist -> to set up for homebudget boolean

                _budget = new HomeBudget(databasePath, IsNewDatabase);
            }
            catch (Exception ex)
            {
                _view.ShowError($"Error setting up database: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a new expense record to the database.
        /// </summary>
        /// <param name="date">The date of the expense.</param>
        /// <param name="name">The name/description of the expense.</param>
        /// <param name="amount">The amount of the expense.</param>
        /// <param name="categoryName">The associated category name.</param>
        public void AddExpense(DateTime date, string name, double amount, string categoryName)
        {
            try
            {
                if (_budget == null)
                {
                    _view.ShowError("Database not initialized.");
                    return;
    
                }

                Category category = CreateOrGetCategory(categoryName);

                int categoryId = category.Id; //get category id when adding the expense 

                _budget.expenses.Add(date, amount, name, categoryId);
            }
            catch (Exception ex)
            {
                _view.ShowError($"Error adding expense: {ex.Message}");
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
                _view.ShowError("Database not initialized.");
                return new List<Category>();
            }

            return _budget.categories.List();
        }

        /// <summary>
        /// Sets the currently selected category type for future category creation.
        /// </summary>
        /// <param name="categoryType">Integer representation of CategoryType (enum).</param>
        public void SetCategoryType(int categoryType)
        {
            _selectedCategoryType = (CategoryType)categoryType;
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
                string inputName = categoryDescription.ToLower(); // Lowercase for consistent comparison

                List<Category> categories = _budget.categories.List(); // Get current categories list

                // Search for an existing category
                for (int i = 0; i < categories.Count; i++)
                {
                    if (categories[i].Description.ToLower() == inputName)
                    {
                        return categories[i];
                    }
                }

                // Not found -> Try to create new category
                _budget.categories.Add(categoryDescription, _selectedCategoryType);

                // Refresh and search again
                categories = _budget.categories.List();

                for (int i = 0; i < categories.Count; i++)
                {
                    if (categories[i].Description.ToLower() == inputName)
                    {
                        return categories[i];
                    }
                }

                // If still not found after adding
                throw new InvalidOperationException("Failed to create or retrieve the category.");
            }
            catch (Exception ex)
            {
                // Catch any exception (e.g., database error, null pointer, etc.)
                System.Diagnostics.Debug.WriteLine($"Error in CreateOrGetCategory: {ex.Message}");
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
                // Default fallback to Expense if invalid input
                type = Category.CategoryType.Expense;
            }

            try
            {
                _budget.categories.Add(name, type);
                return true;
            }
            catch (ArgumentException ex)
            {
                // Specific handling for already existing categories or bad input
                System.Diagnostics.Debug.WriteLine($"Error adding category: {ex.Message}");
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
    }
}


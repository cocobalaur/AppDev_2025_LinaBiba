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
        private CategoryType _selectedCategoryType = CategoryType.Expense; //default
        
        
        public IView View
        {
            get { return _view; }
            set { _view = value; }
        }

        public Presenter(IView view)
        {
            _view = view;
        }

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

        public void AddExpense(DateTime date, string name, double amount, string categoryName)
        {
            try
            {
                if (_budget == null)
                {
                    _view.ShowError("Database not initialized.");
    
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
        public List<Category> GetCategories()
        {
            if (_budget == null)
            {
                _view.ShowError("Database not initialized.");
            }

            return _budget.categories.List();
        }

        public void SetCategoryType(int categoryType)
        {
            _selectedCategoryType = (CategoryType)categoryType;
        }

        public Category CreateOrGetCategory(string categoryDescription)
        {
            string inputName = categoryDescription.ToLower(); //setting to lower case to compare

            List<Category> categories = _budget.categories.List(); //find category

            for (int i = 0; i < categories.Count; i++) 
            {
                if (categories[i].Description.ToLower() == inputName)
                {
                    return categories[i];
                }
            }

            _budget.categories.Add(categoryDescription, _selectedCategoryType); //if not found create it 

            //reset categories and search again
            categories = _budget.categories.List();

            for (int i = 0; i < categories.Count; i++)
            {
                if (categories[i].Description.ToLower() == inputName)
                {
                    return categories[i];
                }
            }

            throw new InvalidOperationException("Failed to create or retrieve the category."); //throw error if not found (fix)
        }

        /// <summary>
        /// Adds a new category based on user input (name and type).
        /// </summary>
        /// <param name="name">The name of the new category.</param>
        /// <param name="typeString">The type selected (e.g., "Income" or "Expense").</param>
        /// <returns>True if created successfully, false if error.</returns>
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
                // Default fallback
                type = Category.CategoryType.Expense;
            }

            try
            {
                _budget.categories.Add(name, type);
                return true;
            }
            catch (ArgumentException ex)
            {
                // Likely a problem like category already exists or invalid argument
                System.Diagnostics.Debug.WriteLine($"Error adding category: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                // Unexpected error, log it for debugging
                System.Diagnostics.Debug.WriteLine($"Unexpected error when adding category: {ex.Message}");
                return false;
            }
        }

    }
}


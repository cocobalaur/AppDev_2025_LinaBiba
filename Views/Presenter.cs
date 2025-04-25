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
    }
}


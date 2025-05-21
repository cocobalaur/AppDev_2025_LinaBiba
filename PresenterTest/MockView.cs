using Budget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Views;

namespace PresenterTest
{
    class MockView : IView
    {
        public string LastError { get; private set; } = "";

        public void DisplayErrorMessage(string message)
        {
            LastError = message;
        }

        public void DisplayAddExpense()
        {
            //Not needed for presenter test
        }

        public void DisplaySuccessMessage(string message)
        {
            //Not needed for presenter test
        }

        public void DisplayCategoryFilterWindow(List<string> name, string type)
        {
            //Not needed for presenter test
        }

        public void DisplayCategoryExpense(List<string> categories, string selectedCategory)
        {
            //Not needed for presenter test
        }

        //add the implementation
        public DateTime? GetStartDate()
        {
            throw new NotImplementedException();
        }

        public DateTime? GetEndDate()
        {
            throw new NotImplementedException();
        }

        public void DisplayItems(List<BudgetItem> items)
        {
            //Not needed for presenter test
        }

        public bool DisplayByCategorySummary()
        {
            throw new NotImplementedException();
        }

        public bool DisplayByMonthSummary()
        {
            throw new NotImplementedException();
        }

        public bool DisplayIsCategoryFilter()
        {
            throw new NotImplementedException();
        }

        public string RenameSelectedCategory()
        {
            throw new NotImplementedException();
        }

        public void ShowSucessMessage(string message)
        {
            //Not needed for presenter test
        }

        public void ShowErrorMessage(string message)
        {
            //Not needed for presenter test
        }

        public void DisplayExpenseUpdate(Expense expense, Action onUpdateComplete)
        {
            throw new NotImplementedException();
        }

        public void ShowChart(List<Dictionary<string, object>> groupedData, List<string> allCategories)
        {
            throw new NotImplementedException();
        }

        public void HideChart()
        {
            throw new NotImplementedException();
        }

        public void ReselectExpenseOnceDeleted(int deleteId)
        {
            throw new NotImplementedException();
        }

        public void ReselectExpenseOnceUpdated(int id)
        {
            throw new NotImplementedException();
        }

        public List<BudgetItem> GetAllItems()
        {
            throw new NotImplementedException();
        }
    }
}

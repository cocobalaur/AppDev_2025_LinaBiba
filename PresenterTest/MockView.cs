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
            throw new NotImplementedException();
        }
    }
}

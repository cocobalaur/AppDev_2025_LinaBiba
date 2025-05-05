using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Views;

namespace PresenterTest
{
    class MockView //: IView
    {
        public string LastError { get; private set; } = "";

        public void AddCategory(string name, string type)
        {
            //Not needed for presenter test
        }

        public void AddExpenseToDatabase(DateTime date, string name, double amount, string categoryName)
        {
            // Not needed for Presenter tests

        }

        public void GetDatabase(string databasePath)
        {
            // Not needed for Presenter tests

        }

        public void DisplayErrorMessage(string message)
        {
            LastError = message;
        }
    }
}

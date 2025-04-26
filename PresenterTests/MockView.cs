using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Views;

namespace PresenterTests
{
    public class MockView : IView
    {
        public string LastError { get; private set; } = "";

        public void ShowError(string message)
        {
            LastError = message;
        }

        public void AddExpenseToDatabase(DateTime date, string name, double amount, string categoryName)
        {
            // Not needed for Presenter tests
        }

        public void GetDatabase(string databasePath)
        {
            // Not needed for Presenter tests
        }
    }
}

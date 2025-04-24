using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Views
{
    public interface IView
    {
        void AddExpenseToDatabase(DateTime date, string name, double amount, string categoryName);
        void ShowError(string message);

        //Get input from the user (e.g., enter expense name and amount)
        //string GetUserInput(string prompt);
        void GetDatabase(string databasePath);

    }
}

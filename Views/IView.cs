using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Views
{
    public interface IView
    {
      //  void DisplayMainMenu();
        //void DisplayAddExpense(string name, string amountStr);
       // void ShowError(string message, string title);

        //Get input from the user (e.g., enter expense name and amount)
      //  string GetUserInput(string prompt);
        //load database
        //use logic from database referenced
        //to create for wpf to work
        //for the database, contract assigned to budget interface 

        void getDatabase(string databasePath);
    }
}

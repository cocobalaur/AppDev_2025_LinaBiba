using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Views;
using Budget;

namespace BudgetModel
{
    public class Presenter : IView
    {
        private HomeBudget _budget;
        public bool IsNewDatabase { get; private set; }
        public Presenter(string databasePath)
        {
            getDatabase(databasePath);
        }

        public void getDatabase(string databasePath)
        {
            _budget = new HomeBudget(databasePath, IsNewDatabase);

        }
    }
}

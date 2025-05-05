using Budget;

namespace Views
{
    //Presener is the brain and talk with the model.
    public class PresenterUpdate
    {
        private HomeBudget? _budget;
        private IViewUpdate _view; // Reference to the View (UI)


        public PresenterUpdate(IViewUpdate view)
        {
            _view = view;
        }

        /// <summary>
        /// Gets or sets the associated view.
        /// </summary>
        public IViewUpdate View
        {
            get { return _view; }
            set { _view = value; }
        }
        public void UpdateExpense(string name, string amount, DateTime date, string? categories)
        {
            try
            {
                double goodAmount = VerificationOfData(name, amount, date, categories);
                _budget.expenses.UpdateExpenses(0, date, goodAmount, name, categories);
            }
            catch(Exception ex)
            {
                _view.ShowErrorMessage($"Error setting up database: {ex.Message}");
            }
        }

        private double VerificationOfData(string name, string amount, DateTime date, string categories)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new Exception("The name value cannot be empty.");
                }
                //Validate the amount
                if (!double.TryParse(amount, out double amountGood))
                {
                    throw new Exception("The expense amount must be a valid number.");
                }
                //Validate the date
                if (date == null)
                {
                    throw new Exception("Please select a valid date.");
                }
                //Validate that a category was entered.
                if (categories == null)
                {
                    throw new Exception("Please enter a category.");
                }
                return amountGood;
            }
            catch (Exception ex)
            {
                _view.ShowErrorMessage($"Error in the inputs: {ex.Message}");
            }
            return -1;
        }

        public List<Category> GetCategories()
        {
            return _budget.categories.List();
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
                _view.ShowErrorMessage($"Error setting up database: {ex.Message}");
            }
        }
    }

}

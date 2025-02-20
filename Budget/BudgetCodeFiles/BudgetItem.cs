// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================

namespace Budget
{
    // ====================================================================
    // CLASS: BudgetItem
    //        A single budget item, includes Category and Expense
    // ====================================================================
    /// <summary>
    /// Represent a single budget item, including the category, expense and financial details
    /// </summary>
    public class BudgetItem
    {
        /// <summary>
        /// Gets or sets the ID of the category related with the budget item.
        /// </summary>
        /// <value>
        /// An integer representing the unique identifier of the category.
        /// </value>
        public int CategoryID { get; set; }

        /// <summary>
        /// Gets or sets the ID of the expenses related with the budget item.
        /// </summary>
        /// <value>
        /// An integer representing the unique identifier of the expense.
        /// </value>
        public int ExpenseID { get; set; }

        /// <summary>
        /// Gets or sets the date of the budget item transaction.
        /// </summary>
        /// <value>
        /// A <see cref="DateTime"/> representing when the budget item was recorded.
        /// </value>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the category name of the budget item.
        /// </summary>
        /// <value>
        /// A string representing the name of the category.
        /// </value>
        public String Category { get; set; }

        /// <summary>
        /// Gets or sets a short description of the budget item.
        /// </summary>
        /// <value>
        /// A string providing a brief summary of the budget item.
        /// </value>
        public String ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets the financial amount of the budget item.
        /// </summary>
        /// <value>
        /// A double representing the monetary value of the budget item.
        /// </value>
        public Double Amount { get; set; }

        /// <summary>
        /// Gets or sets the balance of the budget item.
        /// </summary>
        /// <value>
        /// A double representing the remaining balance after transactions.
        /// </value>
        public Double Balance { get; set; }

    }

    /// <summary>
    /// Represents a group of budget items sorted by month.
    /// </summary>
    public class BudgetItemsByMonth
    {
        /// <summary>
        /// Gets or sets the month related with the budget items.
        /// </summary>
        /// <value>
        /// A string representing the name of the month (e.g., "January").
        /// </value>
        public String Month { get; set; }

        /// <summary>
        /// Gets or sets the list of budget items for the month.
        /// </summary>
        /// <value>
        /// A list of <see cref="BudgetItem"/> objects representing all items for the month.
        /// </value>
        public List<BudgetItem> Details { get; set; }

        /// <summary>
        /// Gets or sets the total amount spent in the month.
        /// </summary>
        /// <value>
        /// A double representing the sum of all expenses within the month.
        /// </value>
        public Double Total { get; set; }
    }

    /// <summary>
    /// Represents a group of budget items sorted by category.
    /// </summary>
    public class BudgetItemsByCategory
    {
        /// <summary>
        /// Gets or sets the category related with the budget items.
        /// </summary>
        /// <value>
        /// A string representing the name of the category.
        /// </value>
        public String Category { get; set; }

        /// <summary>
        /// Gets or sets the list of budget items in the category.
        /// </summary>
        /// <value>
        /// A list of <see cref="BudgetItem"/> objects representing all items under this category.
        /// </value>
        public List<BudgetItem> Details { get; set; }

        /// <summary>
        /// Gets or sets the total amount spent in the category.
        /// </summary>
        /// <value>
        /// A double representing the sum of all expenses under this category.
        /// </value>
        public Double Total { get; set; }

    }


}

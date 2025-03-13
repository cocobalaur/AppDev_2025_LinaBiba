using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================

namespace Budget
{
    // ====================================================================
    // CLASS: Expense
    //        - An individual expens for budget program
    // ====================================================================
    /// <summary>
    ///  Represents an individual expense for the budget program.
    /// </summary>
    public class Expense
    {
        // ====================================================================
        // Properties
        // ====================================================================
        /// <summary>
        /// Gets the unique identifier (id) for the expense.
        /// </summary>
        /// <value>
        /// An integer representing a unique number assigned to the expense.
        /// </value>
        public int Id { get; }

        /// <summary>
        /// Gets the date of the expense transaction.
        /// </summary>
        /// <value>
        /// A <see cref="DateTime"/> value representing when the expense was made.
        /// </value>
        public DateTime Date { get;  }

        /// <summary>
        /// Gets or sets the financial amount of the expense.
        /// </summary>
        /// <value>
        /// A <see cref="Double"/> value representing the financial amount of the expense.
        /// </value>
        public Double Amount { get; }

        /// <summary>
        /// Gets or sets the description of the expense.
        /// </summary>
        /// <value>
        /// A <see cref="String"/> containing a short text description explaining the expense.
        /// </value>
        public String Description { get; }

        /// <summary>
        /// Gets or sets the category ID associated with the expense.
        /// </summary>
        /// <value>
        /// An integer representing the identifier linking the expense to a category.
        /// </value>
        public int Category { get; }



        // ====================================================================
        // Constructor
        //    NB: there is no verification the expense category exists in the
        //        categories object
        // ====================================================================
        /// <summary>
        /// Initializes a new instance of the <see cref="Expense"/> class with specific details.
        /// This is a parameterized constructor that requires an ID, date, category, amount, and description.
        /// </summary>
        /// <param name="id">The unique identifier (id) for the expense.</param>
        /// <param name="date">The date of the expense.</param>
        /// <param name="category">The category ID related with the expense.</param>
        /// <param name="amount">The financial amount of the expense.</param>
        /// <param name="description">The description of the expense.</param>
        public Expense(int id, DateTime date, int category, Double amount, String description)
        {
            this.Id = id;
            this.Date = date;
            this.Category = category;
            this.Amount = amount < 0 ? amount : -amount;
            this.Description = description;
        }

        // ====================================================================
        // Copy constructor - does a deep copy
        // ====================================================================
        /// <summary>
        /// Initializes a new instance of the <see cref="Expense"/> class by copying an existing expense.
        /// This creates a new expense instance with the same values as the original but as a separate object.
        /// </summary>
        /// <param name="obj">The expense object to copy.</param>
        public Expense (Expense obj)
        {
            this.Id = obj.Id;
            this.Date = obj.Date;
            this.Category = obj.Category;
            this.Amount = obj.Amount < 0 ? obj.Amount : -Amount;
            this.Description = obj.Description;
           
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Views
{
    /// <summary>
    /// Defines the interface for the View layer in the MVP pattern.
    /// Allows the Presenter to communicate with the View without knowing its concrete implementation.
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// Adds a new expense entry into the database.
        /// </summary>
        /// <param name="date">The date associated with the expense.</param>
        /// <param name="name">The name or description of the expense.</param>
        /// <param name="amount">The amount of money for the expense.</param>
        /// <param name="categoryName">The category under which the expense falls.</param>
        void DisplayAddExpense();

        /// <summary>
        /// Displays an error message to the user (typically in a message box or alert).
        /// </summary>
        /// <param name="message">The error message text to show.</param>
        void DisplayErrorMessage(string message);       
        
        void DisplaySuccessMessage(string message);
        /// <summary>
        /// Connects to the specified database file or creates it if it does not exist.
        /// </summary>
        /// <param name="databasePath">The full path to the database file.</param>
        void FileDatabaseSelection();

        /// <summary>
        /// Adds a new category entry based on user input.
        /// </summary>
        /// <param name="name">The name of the new category.</param>
        /// <param name="type">The type of the category (e.g., "Income", "Expense", etc.).</param>
        void DisplayAddCategory(string name, string type);

    }
}

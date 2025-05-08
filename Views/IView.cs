using Budget;
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

        /// <summary>
        /// Displays a success message to the user
        /// </summary>
        /// <param name="message">The success message text to show</param>
        void DisplaySuccessMessage(string message);

        /// <summary>
        /// Adds a new category entry based on user input.
        /// </summary>
        /// <param name="name">The name of the new category.</param>
        /// <param name="type">The type of the category (e.g., "Income", "Expense", etc.).</param>
        void DisplayCategoryFilterWindow (List<string> name, string type);

        /// <summary>
        /// Displays a list of category names in the Add Expense window's ComboBox.
        /// Also optionally pre-selects a category if provided.
        /// </summary>
        /// <param name="categories">A list of category names to populate the ComboBox.</param>
        /// <param name="selectedCategory">The category to be pre-selected, if any.</param>
        void DisplayCategoryExpense(List<string> categories, string selectedCategory);

        /// <summary>
        /// Retrieves the selected start date from the UI.
        /// </summary>
        /// <returns>The selected start date, or null if not selected.</returns>
        DateTime? GetStartDate();

        /// <summary>
        /// Retrieves the selected end date from the UI.
        /// </summary>
        /// <returns>The selected end date, or null if not selected.</returns>
        DateTime? GetEndDate();

        /// <summary>
        /// Displays a list of budget items in the DataGrid.
        /// This method is used to show results of filtering or summaries.
        /// </summary>
        /// <param name="items">The list of budget items to display.</param>
        void DisplayItems(List<BudgetItem> items);

        /// <summary>
        /// Returns whether the "By Category" summary checkbox is selected.
        /// </summary>
        /// <returns>True if "By Category" is checked, false otherwise.</returns>
        bool GetByCategorySummary();

        /// <summary>
        /// Returns whether the "By Month" summary checkbox is selected.
        /// </summary>
        /// <returns>True if "By Month" is checked, false otherwise.</returns>
        bool GetByMonthSummary();

        /// <summary>
        /// Determines whether the category filter is enabled (i.e., checkbox is checked).
        /// </summary>
        /// <returns>True if category filter is active; otherwise, false.</returns>
        bool IsCategoryFilterEnabled();

        /// <summary>
        /// Returns the category selected in the filter dropdown.
        /// </summary>
        /// <returns>The selected category name as a string.</returns>
        string GetSelectedCategory();



    }
}

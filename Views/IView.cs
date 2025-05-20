using Budget;

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
        void DisplayCategoryFilterWindow(List<string> name, string type);

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
        bool DisplayByCategorySummary();

        /// <summary>
        /// Returns whether the "By Month" summary checkbox is selected.
        /// </summary>
        /// <returns>True if "By Month" is checked, false otherwise.</returns>
        bool DisplayByMonthSummary();

        /// <summary>
        /// Determines whether the category filter is enabled (i.e., checkbox is checked).
        /// </summary>
        /// <returns>True if category filter is active; otherwise, false.</returns>
        bool DisplayIsCategoryFilter();

        /// <summary>
        /// Returns the category selected in the filter dropdown.
        /// </summary>
        /// <returns>The selected category name as a string.</returns>
        string RenameSelectedCategory();

        /// <summary>
        /// Display the update window with the proper expense.
        /// </summary>
        /// <param name="expense"> The expense to update.</param>
        /// <param name="onUpdateComplete">The action to do once </param>
        void DisplayExpenseUpdate(Expense expense, Action onUpdateComplete);

        /// <summary>
        /// Enables the chart to be shown with grouped data and a list of all categories.
        /// Typically called when both 'By Month' and 'By Category' filters are active.
        /// </summary>
        /// <param name="groupedData">A list of dictionaries representing expense data grouped by month and category.</param>
        /// <param name="allCategories">A list of all category names used as chart segments.</param>
        void ShowChart(List<Dictionary<string, object>> groupedData, List<string> allCategories);

        /// <summary>
        /// Hides the chart when the current filter conditions do not support chart display
        /// (i.e., when either 'By Month' or 'By Category' is not selected).
        /// </summary>
        void HideChart();
    }
}

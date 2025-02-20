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
    // CLASS: Category
    //        - An individual category for budget program
    //        - Valid category types: Income, Expense, Credit, Saving
    // ====================================================================
    /// <summary>
    /// Represents an individual category for the budget program.
    /// Valid category types: Income, Expense, Credit, Savings.
    /// </summary>
    public class Category
    {
        // ====================================================================
        // Properties
        // ====================================================================
        /// <summary>
        /// Gets or sets the unique id for the category.
        /// </summary>
        /// <value>
        /// An integer representing the unique identifier for the category.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the description of the category.
        /// </summary>
        /// <value>
        /// A string representing the description of the category.
        /// </value>
        public String Description { get; set; }

        /// <summary>
        /// Gets or sets the type of category (Income, Expense, Credit, Savings).
        /// </summary>
        /// <value>
        /// A `CategoryType` enum that represents the type of the category. 
        /// The valid types are: Income, Expense, Credit, and Savings.
        /// </value>
        public CategoryType Type { get; set; }

        /// <summary>
        /// Defines valid category types.
        /// </summary>
        public enum CategoryType
        {
            /// <summary>
            /// Represents income-related categories, such as salary, bonuses, etc.
            /// </summary>
            Income,
            /// <summary>
            /// Represents expense-related categories, such as bills, rent, groceries, etc.
            /// </summary>
            Expense,
            /// <summary>
            /// Represents credit-related categories, such as credit card payments, loans, etc.
            /// </summary>
            Credit,
            /// <summary>
            /// Represents savings-related categories, such as retirement savings, emergency funds, etc.
            /// </summary>
            Savings
        };

        // ====================================================================
        // Constructor
        // ====================================================================
        /// <summary>
        /// Initializes a new instance of the <see cref="Category"/> class.
        /// </summary>
        /// <param name="id">The unique identifier (id) for the category.</param>
        /// <param name="description">The description of the category.</param>
        /// <param name="type">The type of category, default being "Expense".</param>
        /// <example>
        /// To create a new category, instantiate a 'Category` object with the required parameters:
        /// <code>
        /// Category cat = new Category(1, "Food", Category.CategoryType.Expense);
        /// </code>
        /// </example>
        public Category(int id, String description, CategoryType type = CategoryType.Expense)
        {
            this.Id = id;
            this.Description = description;
            this.Type = type;
        }

        // ====================================================================
        // Copy Constructor
        // ====================================================================
        /// <summary>
        ///  Initializes a new instance of the <see cref="Category"/> class by copying an existing category.
        /// </summary>
        /// <param name="category">The category to copy.</param>
        /// <example>
        /// To create a new category as a copy of an existing one, use the copy constructor:
        /// <code>
        /// Category originalCategory = new Category(1, "Food", Category.CategoryType.Expense);
        /// Category copiedCategory = new Category(originalCategory);
        /// </code>
        /// </example>
        public Category(Category category)
        {
            this.Id = category.Id;;
            this.Description = category.Description;
            this.Type = category.Type;
        }
        // ====================================================================
        // String version of object
        // ====================================================================
        /// <summary>
        ///  Returns a string representation of the category.
        /// </summary>
        /// <returns>The category description.</returns>
        /// <example>
        /// To get a string representation of a category, use the `ToString()` method:
        /// <code>
        /// Category cat = new Category(1, "Food", Category.CategoryType.Expense);
        /// string categoryDescription = cat.ToString();  // "Food"
        /// </code>
        /// </example>
        public override string ToString()
        {
            return Description;
        }

    }
}


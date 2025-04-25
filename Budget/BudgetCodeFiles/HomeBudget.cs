// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================

using System.Reflection.Metadata;
using System;
using System.Data.Entity;
using System.Data.SQLite;
using System.Data.Entity.Core.Mapping;
using System.IO;
using System.Globalization;

namespace Budget
{
    // ====================================================================
    // CLASS: HomeBudget
    //        - Combines categories Class and expenses Class
    //        - One File defines Category and Budget File
    //        - etc
    // ====================================================================
    /// <summary>
    /// Manages a home budget, including expenses and categories.
    /// </summary>
    public class HomeBudget
    {
        private Categories _categories;
        private Expenses _expenses;

        /// <summary>
        /// Gets the categories related with the budget.
        /// </summary>
        /// <value>
        /// A <see cref="Categories"/> object that holds the categories for the budget (e.g., "Groceries", "Utilities").
        /// </value>
        public Categories categories { get { return _categories; } }

        /// <summary>
        /// Gets the expenses related with the budget.
        /// </summary>
        /// <value>
        /// A <see cref="Expenses"/> object that holds the expenses for the budget (e.g., "Groceries: $200", "Utilities: $100").
        /// </value>
        public Expenses expenses { get { return _expenses; } }


        // -------------------------------------------------------------------
        // Constructor (existing budget ... must specify file)
        // -------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="HomeBudget"/> class with an existing budget file.
        /// </summary>
        /// <param name="budgetFileName">The name of the budget file.</param>

        public HomeBudget(String databaseFile, bool newDB)
        {

            if (string.IsNullOrWhiteSpace(databaseFile))
            {
                throw new ArgumentException("A valid database file must be specified.");
            }

            string fileIsGood = VerifyFile(databaseFile);

            bool isNewDB = !File.Exists(fileIsGood);

            // file did not exist, or user wants a new database, so open NEW DB
            if (isNewDB)
            {
                Database.newDatabase(fileIsGood);
                // create the category object and expenses object
                
            }
            else
            {
                // if database exists, and user doesn't want a new database, open existing DB
                Database.existingDatabase(fileIsGood);
            }
            _categories = new Categories(Database.dbConnection, newDB);
            _expenses = new Expenses(Database.dbConnection, newDB);
        }

        public static string VerifyFile(string databaseFile)
        {
            if (string.IsNullOrWhiteSpace(Path.GetExtension(databaseFile)))
            {
                databaseFile += ".db"; //has valid extension
            }

            return databaseFile;
        }

        #region GetList



        // ============================================================================
        // Get all expenses list
        // NOTE: VERY IMPORTANT... budget amount is the negative of the expense amount
        // Reasoning: an expense of $15 is -$15 from your bank account.
        // ============================================================================
        /// <summary>
        /// Retrieves a list of budget items (expenses) within a specified date range, 
        /// with an optional filter for a specific category.
        /// </summary>
        /// <param name="Start">The start date for filtering expenses. If null, defaults to 1900-01-01.</param>
        /// <param name="End">The end date for filtering expenses. If null, defaults to 2500-01-01.</param>
        /// <param name="FilterFlag">If true, filters expenses by the given CategoryID.</param>
        /// <param name="CategoryID">The ID of the category to filter by (if FilterFlag is true).</param>
        /// <returns>A list of <code>BudgetItem</code> objects representing the filtered expenses.</returns>
        /// <exception cref="Exception">Thrown if the database connection is not initialized or is closed.</exception>
        /// <example>
        /// Example usage:
        /// <code>
        /// HomeBudget budget = new HomeBudget("budget.db", false);
        /// DateTime startDate = new DateTime(2025, 1, 1);
        /// DateTime endDate = new DateTime(2025, 1, 31);
        /// bool filterByCategory = true;
        /// int categoryID = 3;
        ///
        /// List<BudgetItem> budgetItems = budget.GetBudgetItems(startDate, endDate, filterByCategory, categoryID);
        ///
        /// foreach (var item in budgetItems)
        /// {
        ///     Console.WriteLine($"{item.Date:yyyy-MM-dd} | {item.ShortDescription} | {item.Amount:C} | {item.Category} | {item.Balance:C}");
        /// }
        /// </code>
        ///
        /// <code>
        /// Sample Output:
        /// 2025-01-02 | Grocery Shopping | -$45.20 | Food | -$45.20
        /// 2025-01-05 | Restaurant Dinner | -$30.00 | Food | -$75.20
        /// 2025-01-12 | Coffee | -$5.50 | Food | -$80.70
        /// 2025-01-18 | Monthly Groceries | -$120.75 | Food | -$201.45
        /// 2025-01-25 | Snacks | -$12.30 | Food | -$213.75
        /// </code>
        /// </example>

        public List<BudgetItem> GetBudgetItems(DateTime? Start, DateTime? End, bool FilterFlag, int CategoryID)
        {
            List<BudgetItem> items = new List<BudgetItem>(); // Initialize the list of budget items
            double totalBalance = 0; // Initialize the balance variable to keep track of the running total

            if (Database.dbConnection == null)
            {
                throw new Exception("Database connection is not initialized. Call newDatabase() or existingDatabase() first.");
            }

            if (Database.dbConnection.State != System.Data.ConnectionState.Open)
            {
                throw new Exception("Database connection is closed. Ensure the database is opened before running queries.");
            }

            try
            {
                // Default start and end dates if they are not provided
                Start = Start ?? new DateTime(1900, 1, 1);
                End = End ?? new DateTime(2500, 1, 1);

                // Build the SQL query to fetch data from the database
                string query = @"
                                SELECT e.Id AS ExpenseId, e.Date, e.Description AS ExpenseDescription, 
                                       e.Amount, c.Id AS CategoryId, c.Description AS CategoryDescription
                                FROM expenses e
                                JOIN categories c ON e.CategoryId = c.Id
                                WHERE e.Date >= @StartDate AND e.Date <= @EndDate";

                // Modify query if filter flag is set
                if (FilterFlag)
                {
                    query += " AND e.CategoryId = @CategoryId";
                }

                // Execute the query
                using (SQLiteCommand cmd = new SQLiteCommand(query, Database.dbConnection))
                {
                    // Adding parameters to prevent SQL injection
                    cmd.Parameters.AddWithValue("@StartDate", Start.Value.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@EndDate", End.Value.ToString("yyyy-MM-dd"));

                    if (FilterFlag)
                    {
                        cmd.Parameters.AddWithValue("@CategoryId", CategoryID);
                    }

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                // Get data from the reader by column index
                                int expenseId = reader.GetInt32(reader.GetOrdinal("ExpenseId"));
                                DateTime date = reader.GetDateTime(reader.GetOrdinal("Date"));
                                string expenseDescription = reader.GetString(reader.GetOrdinal("ExpenseDescription"));
                                double amount = reader.GetDouble(reader.GetOrdinal("Amount"));
                                int categoryId = reader.GetInt32(reader.GetOrdinal("CategoryId"));
                                string categoryDescription = reader.GetString(reader.GetOrdinal("CategoryDescription"));


                                totalBalance += amount;

                                // Add the item to the list
                                items.Add(new BudgetItem
                                {
                                    ExpenseID = expenseId,
                                    Date = date,
                                    ShortDescription = expenseDescription,
                                    Amount = amount, // Make the amount negative as per business logic
                                    CategoryID = categoryId,
                                    Category = categoryDescription,
                                    Balance = totalBalance
                                });
                            }
                            catch (Exception ex)
                            {
                                // Log or handle individual row errors (e.g., data conversion issues)
                                Console.WriteLine($"Error processing row: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (SQLiteException sqlEx)
            {
                // Handle errors related to database operations
                Console.WriteLine($"Database error: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                // Handle any other unexpected errors
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

            return items;
        }


        // ============================================================================
        // Group all expenses month by month (sorted by year/month)
        // returns a list of BudgetItemsByMonth which is 
        // "year/month", list of budget items, and total for that month
        // ============================================================================
        /// <summary>
        /// Groups expenses month by month and returns a list of budget items by month.
        /// </summary>
        /// <param name="Start">The starting date for filtering (nullable). Defaults to 1900-01-01 if null.</param>
        /// <param name="End">The ending date for filtering (nullable). Defaults to 2500-01-01 if null.</param>
        /// <param name="FilterFlag">A flag indicating whether to apply category filtering.</param>
        /// <param name="CategoryID">The category ID to filter by (only applied if <paramref name="FilterFlag"/> is true).</param>
        /// <returns>A list of grouped budget items by month, including year/month, list of items, and monthly total.</returns>
        /// <example>
        /// For all examples below, assume the budget file contains the following elements:
        /// 
        /// <code>
        /// Cat_ID Expense_ID Date Description Cost
        /// 3 1 1/05/2025 12:00:00 AM Groceries Vegetables 30
        /// 3 2 1/10/2025 12:00:00 AM Groceries Milk 5
        /// 7 3 1/15/2025 12:00:00 AM Entertainment Concert -50
        /// 3 4 2/02/2025 12:00:00 AM Groceries Bread 3
        /// 3 5 2/10/2025 12:00:00 AM Groceries Fruits 10
        /// 7 6 2/15/2025 12:00:00 AM Entertainment Movie -15
        /// 5 7 2/25/2025 12:00:00 AM Transportation Gas 40
        /// 3 8 3/03/2025 12:00:00 AM Groceries Snacks 7
        /// 7 9 3/10/2025 12:00:00 AM Entertainment Subscription -12
        /// 5 10 3/20/2025 12:00:00 AM Transportation Train Ticket 25
        /// </code>
        ///
        /// Example usage for getting monthly budget items:
        /// <code>
        /// <![CDATA[
        /// DateTime startDate = new DateTime(2025, 1, 1);
        /// DateTime endDate = new DateTime(2025, 3, 31);
        /// bool filterByCategory = false;
        /// int categoryID = 0;
        ///
        /// List<BudgetItemsByMonth> budgetItems = budget.GetBudgetItemsByMonth(startDate, endDate, filterByCategory, categoryID);
        /// ]]>
        /// </code>
        ///
        /// Sample output:
        /// <code>
        /// 2025/Jan  Vegetables ($30.00) ($30.00)
        /// 2025/Jan  Milk ($5.00) ($35.00)
        /// 2025/Jan  Concert $50.00 $15.00
        /// 2025/Feb  Bread ($3.00) ($3.00)
        /// 2025/Feb  Fruits ($10.00) ($13.00)
        /// 2025/Feb  Movie $15.00 $2.00
        /// 2025/Feb  Gas ($40.00) ($38.00)
        /// 2025/Mar  Snacks ($7.00) ($7.00)
        /// 2025/Mar  Subscription $12.00 $5.00
        /// 2025/Mar  Train Ticket ($25.00) ($20.00)
        /// </code>
        /// </example>
        public List<BudgetItemsByMonth> GetBudgetItemsByMonth(DateTime? Start, DateTime? End, bool FilterFlag, int CategoryID)
        {
            try
            {
                // Check if filtering by category is enabled and if the category exists
                if (FilterFlag)
                {
                    var category = _categories.GetCategoryFromId(CategoryID);

                    // If the category does not exist, return an empty list
                    if (category == null)
                    {
                        return new List<BudgetItemsByMonth>();
                    }
                }

                // Initialize the list to store the grouped budget items by month
                List<BudgetItemsByMonth> itemsByMonth = new List<BudgetItemsByMonth>();

                // Set default start and end dates if they are not provided
                Start ??= new DateTime(1900, 1, 1);
                End ??= new DateTime(2500, 1, 1);

                // Define the SQL query to retrieve expenses and their associated categories
                string query = @"
                                SELECT e.Id AS ExpenseID, e.Date, e.Amount, e.Description, e.CategoryId, c.Description AS Category
                                FROM expenses e
                                INNER JOIN categories c ON e.CategoryId = c.Id
                                WHERE e.Date BETWEEN @StartDate AND @EndDate";

                if (FilterFlag)
                {
                    query += " AND c.Id = @CategoryID"; // Add category filter if FilterFlag is true
                }

                query += " ORDER BY e.Date;";

                // Execute the SQL query
                using (var cmd = new SQLiteCommand(query, Database.dbConnection))
                {
                    // Add parameters to the command
                    cmd.Parameters.AddWithValue("@StartDate", Start.Value.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@EndDate", End.Value.ToString("yyyy-MM-dd"));
                    if (FilterFlag)
                    {
                        // If filtering by category, add the categoryID parameter
                        cmd.Parameters.AddWithValue("@CategoryID", CategoryID);
                    }

                    // Execute the query and read the results
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        // Dictionary to group expenses by month (key: month, value: list of BudgetItem)
                        Dictionary<string, List<BudgetItem>> monthlyGroups = new Dictionary<string, List<BudgetItem>>();

                        // Process each row in the results
                        while (reader.Read())
                        {
                            // Extract data from current row
                            int expenseId = reader.GetInt32(0);
                            DateTime date = DateTime.Parse(reader.GetString(1));
                            double amount = reader.GetDouble(2);
                            string description = reader.GetString(3);
                            int categoryId = reader.GetInt32(4);
                            string category = reader.GetString(5);

                            // Create a BudgetItem object for the current expense
                            var budgetItem = new BudgetItem
                            {
                                ExpenseID = expenseId,
                                Date = date,
                                Amount = amount,
                                ShortDescription = description,
                                CategoryID = categoryId,
                                Category = category,
                                Balance = 0
                            };

                            // Generate a key for the month (YYYY/MM)
                            string monthKey = date.ToString("yyyy/MM", CultureInfo.InvariantCulture);

                            // Add the BudgetItem to the corresponding month group
                            if (!monthlyGroups.ContainsKey(monthKey))
                            {
                                monthlyGroups[monthKey] = new List<BudgetItem>();
                            }

                            monthlyGroups[monthKey].Add(budgetItem);
                        }

                        // Convert the grouped data into a list of BudgetItemsByMonth
                        foreach (var monthGroup in monthlyGroups)
                        {
                            string month = monthGroup.Key;
                            var details = monthGroup.Value;
                            double total = details.Sum(item => item.Amount);

                            // Create a new BudgetItemsByMonth object for the current month
                            var budgetItemsByMonth = new BudgetItemsByMonth
                            {
                                Month = month,
                                Details = details,
                                Total = total
                            };

                            // Add the BudgetItemsByMonth object to the list
                            itemsByMonth.Add(budgetItemsByMonth);
                        }
                    }
                }

                // Return the list of BudgetItemsByMonth
                return itemsByMonth;
            }
            catch (SQLiteException sqlEx)
            {
                Console.WriteLine($"Database error: {sqlEx.Message}");
                throw new Exception("A database error occurred while fetching budget items.", sqlEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw new Exception("An unexpected error occurred while processing budget data.", ex);
            }
        }



        // ============================================================================
        // Group all expenses by category (ordered by category name)
        // ============================================================================
        /// <summary>
        /// Groups expenses by category and returns a list of budget items grouped by category, including total expenses for each category.
        /// </summary>
        /// <param name="Start">The starting date for filtering (nullable). Defaults to 1900-01-01 if null.</param>
        /// <param name="End">The ending date for filtering (nullable). Defaults to 2500-01-01 if null.</param>
        /// <param name="FilterFlag">A flag indicating whether to apply category filtering.</param>
        /// <param name="CategoryID">The category ID to filter by (only applied if <paramref name="FilterFlag"/> is true).</param>
        /// <returns>A list of grouped budget items by category, including category name, list of items, and total for each category.</returns>
        /// <example>
        /// For all examples below, assume the budget file contains the following elements:
        /// 
        /// <code>
        /// Cat_ID Expense_ID Date Description Cost
        /// 3 1 1/05/2025 12:00:00 AM Groceries Vegetables 30
        /// 3 2 1/10/2025 12:00:00 AM Groceries Milk 5
        /// 7 3 1/15/2025 12:00:00 AM Entertainment Concert -50
        /// 3 4 2/02/2025 12:00:00 AM Groceries Bread 3
        /// 3 5 2/10/2025 12:00:00 AM Groceries Fruits 10
        /// 7 6 2/15/2025 12:00:00 AM Entertainment Movie -15
        /// 5 7 2/25/2025 12:00:00 AM Transportation Gas 40
        /// 3 8 3/03/2025 12:00:00 AM Groceries Snacks 7
        /// 7 9 3/10/2025 12:00:00 AM Entertainment Subscription -12
        /// 5 10 3/20/2025 12:00:00 AM Transportation Train Ticket 25
        /// </code>
        /// 
        /// Example usage for getting budget items by category:
        /// <code>
        /// <![CDATA[
        /// DateTime startDate = new DateTime(2025, 1, 1);
        /// DateTime endDate = new DateTime(2025, 3, 31);
        /// bool filterByCategory = false;
        /// int categoryID = 0;
        ///
        /// List<BudgetItemsByCategory> categoryItems = budget.GetBudgetItemsByCategory(startDate, endDate, filterByCategory, categoryID);
        /// ]]>
        /// </code>
        /// 
        /// Sample output:
        /// <code>
        /// Groceries  Vegetables ($30.00) ($30.00)
        /// Groceries  Milk ($5.00) ($35.00)
        /// Groceries  Bread ($3.00) ($38.00)
        /// Groceries  Fruits ($10.00) ($48.00)
        /// Entertainment Concert $50.00 $50.00
        /// Entertainment Movie $15.00 $65.00
        /// Transportation Gas ($40.00) ($40.00)
        /// Transportation Train Ticket ($25.00) ($65.00)
        /// Entertainment Subscription $12.00 ($12.00)
        /// </code>
        /// </example>
        public List<BudgetItemsByCategory> GetBudgetItemsByCategory(DateTime? Start, DateTime? End, bool FilterFlag, int CategoryID)
        {
            try
            {
                // Set default values for Start and End if they are not provided
                DateTime startDate = Start ?? new DateTime(1900, 1, 1);
                DateTime endDate = End ?? new DateTime(2500, 1, 1);

                // Initialize the list to store the final summary of grouped expenses
                List<BudgetItemsByCategory> summary = new List<BudgetItemsByCategory>();

                // Define the SQL query to retrieve the grouped expenses by category
                string query = @"
                                SELECT
                                    c.Description AS Category,e.Id AS ExpenseID,
                                    e.Date,e.Amount,
                                    e.Description AS ExpenseDescription,
                                    e.CategoryId
                                FROM expenses e
                                INNER JOIN categories c ON e.CategoryId = c.Id
                                WHERE e.Date BETWEEN @StartDate AND @EndDate";

                // If filtering by category is enabled, add the category filter to the query
                if (FilterFlag)
                {
                    query += " AND e.CategoryId = @CategoryID";
                }

                query += " ORDER BY c.Description, e.Date;";

                // Execute the SQL query
                using (var cmd = new SQLiteCommand(query, Database.dbConnection))
                {
                    // Add parameters to the command
                    cmd.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@EndDate", endDate.ToString("yyyy-MM-dd"));
                    if (FilterFlag)
                    {
                        // If filtering by category, add the categoryID parameter
                        cmd.Parameters.AddWithValue("@CategoryID", CategoryID);
                    }

                    // Execute the query and read the results
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        // Dictionary to group expenses by category (key: category name)
                        Dictionary<string, List<BudgetItem>> groupedByCategory = new Dictionary<string, List<BudgetItem>>();

                        // Process each row in the results
                        while (reader.Read())
                        {
                            string categoryName = reader.GetString(0); // Category description
                            int expenseId = reader.GetInt32(1); // Expense ID
                            DateTime date = reader.GetDateTime(2); // Expense date
                            double amount = reader.GetDouble(3); // Expense amount
                            string description = reader.GetString(4); // Expense description
                            int categoryId = reader.GetInt32(5); // Category ID

                            // Create a BudgetItem object for the current expense
                            var budgetItem = new BudgetItem
                            {
                                ExpenseID = expenseId,
                                Date = date,
                                Amount = amount,
                                ShortDescription = description,
                                CategoryID = categoryId,
                                Category = categoryName,  // This is the category for the current expense
                                Balance = 0
                            };

                            // Check if category already exists in the dictionary, if not, initialize it
                            if (!groupedByCategory.ContainsKey(categoryName))
                            {
                                groupedByCategory[categoryName] = new List<BudgetItem>();
                            }

                            // Add item to the corresponding category group
                            groupedByCategory[categoryName].Add(budgetItem);
                        }

                        // Convert the grouped data into a list of BudgetItemsByCategory
                        foreach (var categoryGroup in groupedByCategory.OrderBy(g => g.Key))
                        {
                            // Sort items within each category by Date (ascending order)
                            List<BudgetItem> sortedDetails = categoryGroup.Value.OrderBy(i => i.Date).ToList();

                            // Calculate total amount for the current category
                            double total = sortedDetails.Sum(i => i.Amount);

                            // Create a BudgetItemsByCategory object
                            var budgetCategorySummary = new BudgetItemsByCategory
                            {
                                Category = categoryGroup.Key, // Category description (e.g., Food)
                                Details = sortedDetails,      // List of budget items for this category
                                Total = total                 // Total expenses for this category
                            };

                            // Add to the summary list
                            summary.Add(budgetCategorySummary);
                        }
                    }
                }

                // Return the final sorted list of grouped budget items by category
                return summary;
            }
            catch (SQLiteException sqlEx)
            {
                // Handle SQL errors (e.g., constraints, invalid queries, database connectivity issues)
                Console.WriteLine($"Database error: {sqlEx.Message}");
                throw new Exception("A database error occurred while fetching budget items by category.", sqlEx);
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw new Exception("An unexpected error occurred while processing budget data by category.", ex);
            }
        }


        // ============================================================================
        // Group all events by category and Month
        // creates a list of Dictionary objects (which are objects that contain key value pairs).
        // The list of Dictionary objects includes:
        //          one dictionary object per month with expenses,
        //          and one dictionary object for the category totals
        // 
        // Each per month dictionary object has the following key value pairs:
        //           "Month", <the year/month for that month as a string>
        //           "Total", <the total amount for that month as a double>
        //            and for each category for which there is an expense in the month:
        //             "items:category", a List<BudgetItem> of all items in that category for the month
        //             "category", the total amount for that category for this month
        //
        // The one dictionary for the category totals has the following key value pairs:
        //             "Month", the string "TOTALS"
        //             for each category for which there is an expense in ANY month:
        //             "category", the total for that category for all the months
        // ============================================================================
        /// <summary>
        /// Retrieves a comprehensive list of budget items, grouped by both month and category, within a specified date range.
        /// </summary>
        /// <param name="Start">The starting date for filtering (nullable). If null, defaults to January 1, 1900.</param>
        /// <param name="End">The ending date for filtering (nullable). If null, defaults to January 1, 2500.</param>
        /// <param name="FilterFlag">A flag indicating whether to apply category filtering.</param>
        /// <param name="CategoryID">The category ID to filter by (only applied if <paramref name="FilterFlag"/> is true).</param>
        /// <returns>A list of dictionaries, each representing a month with its associated budget items and totals by category.</returns>
        /// <example>
        /// For all examples below, assume the budget file contains the following elements:
        /// 
        /// <code>
        /// Cat_ID Expense_ID Date Description Cost
        /// 10 1 1/10/2018 12:00:00 AM Clothes hat (on credit) 10
        /// 9 2 1/11/2018 12:00:00 AM Credit Card hat -10
        /// 10 3 1/10/2019 12:00:00 AM Clothes scarf(on credit) 15
        /// 9 4 1/10/2020 12:00:00 AM Credit Card scarf -15
        /// 14 5 1/11/2020 12:00:00 AM Eating Out McDonalds 45
        /// 14 7 1/12/2020 12:00:00 AM Eating Out Wendys 25
        /// 14 10 2/1/2020 12:00:00 AM Eating Out Pizza 33.33
        /// 9 13 2/10/2020 12:00:00 AM Credit Card mittens -15
        /// 9 12 2/25/2020 12:00:00 AM Credit Card Hat -25
        /// 14 11 2/27/2020 12:00:00 AM Eating Out Pizza 33.33
        /// 14 9 7/11/2020 12:00:00 AM Eating Out Cafeteria 11.11
        /// </code>
        /// 
        /// Example usage for getting a detailed budget dictionary by category and month:
        /// <code>
        /// <![CDATA[
        /// DateTime startDate = new DateTime(2020, 1, 1);
        /// DateTime endDate = new DateTime(2020, 12, 31);
        /// bool applyCategoryFilter = true;
        /// int categoryId = 14; // Example category ID
        /// List<Dictionary<string, object>> budgetSummary = budget.GetBudgetDictionaryByCategoryAndMonth(startDate, endDate, applyCategoryFilter, categoryId);
        /// ]]>
        /// </code>
        /// 
        /// Sample output:
        /// <code>
        /// Month: 2020/Jan
        /// Credit Card  hat ($10.00) ($10.00)
        /// Credit Card  scarf $15.00 $5.00
        /// Eating Out  McDonalds ($45.00) ($45.00)
        /// Eating Out  Wendys ($25.00) ($70.00)
        /// 
        /// Month: 2020/Feb
        /// Eating Out  Pizza ($33.33) ($103.33)
        /// Credit Card  mittens $15.00 ($88.33)
        /// Credit Card  Hat $25.00 ($63.33)
        /// Eating Out  Pizza ($33.33) ($96.66)
        /// 
        /// Month: 2020/Jul
        /// Eating Out  Cafeteria ($11.11) ($107.77)
        /// </code>
        /// </example>
        public List<Dictionary<string, object>> GetBudgetDictionaryByCategoryAndMonth(DateTime? Start, DateTime? End, bool FilterFlag, int CategoryID)
        {
            try
            {
                // -----------------------------------------------------------------------
                // Get all items by month
                // -----------------------------------------------------------------------
                List<BudgetItemsByMonth> GroupedByMonth = GetBudgetItemsByMonth(Start, End, FilterFlag, CategoryID);

                // -----------------------------------------------------------------------
                // Loop over each month
                // -----------------------------------------------------------------------
                var summary = new List<Dictionary<string, object>>();
                var totalsPerCategory = new Dictionary<string, double>();

                foreach (var MonthGroup in GroupedByMonth)
                {
                    // Create record object for this month
                    Dictionary<string, object> record = new Dictionary<string, object>();
                    record["Month"] = MonthGroup.Month;
                    record["Total"] = MonthGroup.Total;

                    // Break up the month details into categories
                    var GroupedByCategory = MonthGroup.Details.GroupBy(c => c.Category);

                    // -----------------------------------------------------------------------
                    // Loop over each category
                    // -----------------------------------------------------------------------
                    foreach (var CategoryGroup in GroupedByCategory.OrderBy(g => g.Key))
                    {
                        // Calculate totals for the category in this month and create list of details
                        double total = 0;
                        var details = new List<BudgetItem>();

                        foreach (var item in CategoryGroup)
                        {
                            total += item.Amount;
                            details.Add(item);
                        }

                        // Add new properties and values to our record object
                        record[$"details:{CategoryGroup.Key}"] = details; // Using string interpolation for consistency
                        record[CategoryGroup.Key] = total;

                        // Keep track of totals for each category across all months
                        if (totalsPerCategory.TryGetValue(CategoryGroup.Key, out double CurrentCatTotal))
                        {
                            totalsPerCategory[CategoryGroup.Key] = CurrentCatTotal + total;
                        }
                        else
                        {
                            totalsPerCategory[CategoryGroup.Key] = total;
                        }
                    }

                    // Add record to collection
                    summary.Add(record);
                }

                // ---------------------------------------------------------------------------
                // Add final record which is the totals for each category
                // ---------------------------------------------------------------------------
                Dictionary<string, object> totalsRecord = new Dictionary<string, object>();
                totalsRecord["Month"] = "TOTALS";

                // Adding totals for each category to the final record
                foreach (var category in totalsPerCategory)
                {
                    totalsRecord[category.Key] = category.Value;
                }

                // Add the totals record to the summary list
                summary.Add(totalsRecord);

                return summary;
            }
            catch (SQLiteException sqlEx)
            {
                // Handle SQL errors (e.g., constraints, invalid queries, database connectivity issues)
                Console.WriteLine($"Database error: {sqlEx.Message}");
                throw new Exception("A database error occurred while fetching the budget dictionary by category and month.", sqlEx);
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw new Exception("An unexpected error occurred while processing budget data by category and month.", ex);
            }
        }



        #endregion GetList

    }
}

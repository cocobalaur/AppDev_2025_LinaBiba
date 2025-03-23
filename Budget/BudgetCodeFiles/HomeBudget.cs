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

            bool isNewDB = !File.Exists(databaseFile);

            // file did not exist, or user wants a new database, so open NEW DB
            if (isNewDB)
            {
                Database.newDatabase(databaseFile);
            }
            else
            {
                // if database exists, and user doesn't want a new database, open existing DB
                Database.existingDatabase(databaseFile);
            }

            // create the category object and expenses object
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
        /// Retrieves a list of budget items from the database, allowing optional filtering by date range and category.  
        /// 
        /// <b>Functionality:</b>  
        /// - Users can specify a start and end date; if not provided, they default to January 1, 1900, and January 1, 2500.  
        /// - Users can filter results by category, but only if <paramref name="FilterFlag"/> is set to true.  
        /// - Each budget item includes:  
        ///   - <b>Category ID</b> (<paramref name="CategoryID"/>)  
        ///   - <b>Expense ID</b>  
        ///   - <b>Date</b>  
        ///   - <b>Expense Description</b>  
        ///   - <b>Category Description</b>  
        ///   - <b>Amount</b> (negative for expenses)  
        ///   - <b>Balance</b> (dynamically calculated running total).  
        /// 
        /// <b>Rules:</b>  
        /// - The balance should reflect a running total of the retrieved budget items.  
        /// - The category filter is applied only when <paramref name="FilterFlag"/> is set to true.  
        /// - The returned list must be sorted by date in ascending order.  
        /// - The method should use a SQL database query to ensure efficient data retrieval.  
        /// 
        /// <b>Acceptance Criteria:</b>  
        /// - <paramref name="Start"/> and <paramref name="End"/> default to January 1, 1900, and January 1, 2500, respectively.  
        /// - Category filtering is applied only when <paramref name="FilterFlag"/> is set to true.  
        /// - The balance should be computed as a running total across the returned results.  
        /// - The query should be database-driven rather than manually processing raw data.  
        /// - The resulting list of budget items must be sorted by date.  
        /// - Relevant unit tests should be created or updated to reflect these behaviors.  
        /// </summary>  
        /// <param name="Start">The start date for filtering. If null, defaults to January 1, 1900.</param>  
        /// <param name="End">The end date for filtering. If null, defaults to January 1, 2500.</param>  
        /// <param name="FilterFlag">If true, applies category filtering using <paramref name="CategoryID"/>.</param>  
        /// <param name="CategoryID">The category ID used for filtering, applied only if <paramref name="FilterFlag"/> is true.</param>  
        /// <returns>A list of <see cref="BudgetItem"/> objects, each representing a budget entry matching the filter criteria.</returns>  
        /// 
        /// <example>
        /// <b>Example: Retrieve all budget items without filtering</b>  
        /// This example fetches all budget items from the database without category filtering.  
        /// <code>
        /// <![CDATA[
        /// HomeBudget budget = new HomeBudget();
        /// budget.ConnectToDatabase("budget.db");
        ///
        /// // Get all budget items
        /// List<BudgetItem> budgetItems = budget.GetBudgetItems(null, null, false, 0);
        ///
        /// // Print results
        /// foreach (BudgetItem item in budgetItems)
        /// {
        ///     Console.WriteLine($"{item.Date:yyyy/MMM/dd} {item.ShortDescription} {item.Amount:C} {item.Balance:C}");
        /// }
        /// ]]>
        /// </code>
        /// <b>Expected Output:</b>
        /// <code>
        /// 2024/Jan/10 Clothes (Hat) ($10.00) ($10.00)
        /// 2024/Jan/11 Credit Card  $10.00 $0.00
        /// 2024/Jan/15 Eating Out   ($25.00) ($25.00)
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
        /// Retrieves and groups budget expenses by month within a specified date range, optionally filtering by category.
        /// Each result includes a month identifier, a list of budget items for that month, and the total expenses for the month.
        /// The data is fetched directly from the database, ensuring scalability and maintainability for large datasets.
        ///
        /// Acceptance Criteria:
        /// <ul>
        ///   <li>If no <paramref name="Start"/> date is provided, the system defaults to January 1, 1900.</li>
        ///   <li>If no <paramref name="End"/> date is provided, the system defaults to January 1, 2500.</li>
        ///   <li>If <paramref name="FilterFlag"/> is set to <c>false</c>, the <paramref name="CategoryID"/> is ignored, and no filtering by category is applied.</li>
        ///   <li>If <paramref name="FilterFlag"/> is set to <c>true</c>, only budget items that match the provided <paramref name="CategoryID"/> are included in the result.</li>
        ///   <li>The returned list of <see cref="BudgetItemsByMonth"/> objects must be ordered chronologically by year and month (from earliest to latest).</li>
        ///   <li>Each <see cref="BudgetItemsByMonth"/> object must contain the month identifier in the "YYYY/MM" format, a list of <see cref="BudgetItem"/> objects, and a total for the expenses in that month.</li>
        ///   <li>The total for each month in the <see cref="BudgetItemsByMonth"/> should be the sum of the <see cref="Cost"/> values for that month, including both positive and negative values.</li>
        ///   <li>The system must fetch data from the database efficiently, ensuring scalability and maintainability for large datasets.</li>
        /// </ul>
        /// </summary>
        /// <param name="Start">The start date for the expense filtering, with a default value of January 1, 1900, if unspecified.</param>
        /// <param name="End">The end date for the expense filtering, with a default value of January 1, 2500, if unspecified.</param>
        /// <param name="FilterFlag">A flag indicating whether to filter expenses by category. If set to true, the <paramref name="CategoryID"/> is applied. Otherwise, it is ignored.</param>
        /// <param name="CategoryID">The category ID to filter expenses by, applied only if <paramref name="FilterFlag"/> is true. If not provided, no filtering by category occurs.</param>
        /// <returns>
        /// A list of <see cref="BudgetItemsByMonth"/> objects, where:
        /// - The list is chronologically ordered by year and month.
        /// - Each object contains:
        ///   - A month identifier in "YYYY/MM" format.
        ///   - A list of <see cref="BudgetItem"/> objects for that month.
        ///   - A running total of all expenses for the month, which sums up the <see cref="Cost"/> values of the budget items.
        /// </returns>
        /// <example>
        /// Example dataset:
        /// <code>
        /// Cat_ID Expense_ID Date Description Cost
        /// 10 1 1/10/2018 Clothes hat (on credit) 10
        /// 9 2 1/11/2018 Credit Card hat -10
        /// 10 3 1/10/2019 Clothes scarf (on credit) 15
        /// 9 4 1/10/2020 Credit Card scarf -15
        /// 14 5 1/11/2020 Eating Out McDonalds 45
        /// 14 7 1/12/2020 Eating Out Wendys 25
        /// 14 10 2/1/2020 Eating Out Pizza 33.33
        /// 9 13 2/10/2020 Credit Card mittens -15
        /// 9 12 2/25/2020 Credit Card Hat -25
        /// 14 11 2/27/2020 Eating Out Pizza 33.33
        /// 14 9 7/11/2020 Eating Out Cafeteria 11.11
        /// </code>
        /// Example usage for retrieving grouped budget items by month:
        /// <code>
        /// <![CDATA[
        /// DateTime startDate = new DateTime(2024, 1, 1);
        /// DateTime endDate = new DateTime(2024, 1, 31);
        /// List<BudgetItemsByMonth> monthlyItems = budget.GetBudgetItemsByMonth(startDate, endDate, true, 1);
        /// ]]>
        /// </code>
        /// Sample output for the example dataset:
        /// <code>
        /// 2020/Jan  McDonalds ($45.00) ($45.00)
        /// 2020/Jan  Wendys ($25.00) ($70.00)
        /// 2020/Feb  Pizza ($33.33) ($103.33)
        /// 2020/Feb  mittens $15.00 ($88.33)
        /// 2020/Feb  Hat $25.00 ($63.33)
        /// 2020/Feb  Pizza ($33.33) ($96.66)
        /// 2020/Jul  Cafeteria ($11.11) ($107.77)
        /// </code>
        /// </example>

        public List<BudgetItemsByMonth> GetBudgetItemsByMonth(DateTime? Start, DateTime? End, bool FilterFlag, int CategoryID)
        {
            try
            {

                // Set default values for Start and End if they are not provided
                DateTime startDate = Start ?? new DateTime(1900, 1, 1);
                DateTime endDate = End ?? new DateTime(2500, 1, 1);

                //  Retrieve all budget items within the given date range and category filter
                // If FilterFlag is false, CategoryID is ignored
                List<BudgetItem> items = GetBudgetItems(startDate, endDate, FilterFlag, CategoryID);

                // -----------------------------------------------------------------------
                // Group by year/month
                // -----------------------------------------------------------------------
                var GroupedByMonth = items.GroupBy(c => c.Date.Year.ToString("D4") + "/" + c.Date.Month.ToString("D2"));

                // Initialize a list to store the final summary of grouped expenses
                List<BudgetItemsByMonth> summary = new List<BudgetItemsByMonth>();

                // Go through each group of expenses (one group per month)
                foreach (var MonthGroup in GroupedByMonth)
                {
                    // Initialize total amount for the current month
                    double total = 0;

                    // Create a new list to hold budget items for this month
                    List<BudgetItem> details = new List<BudgetItem>();

                    // Loop through all budget items in the current month group
                    // Add each item to the list and update the total amount
                    foreach (var item in MonthGroup)
                    {
                        total += item.Amount; // Accumulate the total for this month
                        details.Add(item);    // Add item to the monthly list
                    }

                    // Create a new BudgetItemsByMonth object and add it to the summary list
                    // This stores the month, total expenses, and list of budget items
                    summary.Add(new BudgetItemsByMonth
                    {
                        Month = MonthGroup.Key, // ex. "2024/01"
                        Details = details,      // List of budget items for this month
                        Total = total           // Total expenses for this month
                    });


                }
                // Return the final list of grouped budget items by month
                return summary;
            }
            catch (SQLiteException sqlEx)
            {
                // Handle SQL errors (e.g., constraints, invalid queries, database connectivity issues)
                Console.WriteLine($"Database error: {sqlEx.Message}");
                throw new Exception("A database error occurred while fetching budget items.", sqlEx);
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw new Exception("An unexpected error occurred while processing budget data.", ex);
            }
        }


        // ============================================================================
        // Group all expenses by category (ordered by category name)
        // ============================================================================
        /// <summary>
        /// Retrieves and groups budget expenses by category from the database within a specified date range, 
        /// optionally filtering by category. It returns a list of grouped budget items by category, sorted alphabetically by category name.
        /// Each entry includes the category description, a list of budget items for that category, and the total expenses for that category.
        /// The data is fetched directly from the database, ensuring scalability and maintainability for large datasets.
        ///
        /// Acceptance Criteria:
        /// <ul>
        ///   <li>If no <paramref name="Start"/> date is provided, the system defaults to January 1, 1900.</li>
        ///   <li>If no <paramref name="End"/> date is provided, the system defaults to January 1, 2500.</li>
        ///   <li>If <paramref name="FilterFlag"/> is set to <c>false</c>, the <paramref name="CategoryID"/> is ignored, and no filtering by category is applied.</li>
        ///   <li>If <paramref name="FilterFlag"/> is set to <c>true</c>, only budget items matching the provided <paramref name="CategoryID"/> are included in the result.</li>
        ///   <li>The returned list of <see cref="BudgetItemsByCategory"/> objects must be ordered alphabetically by category description (from A to Z).</li>
        ///   <li>Each <see cref="BudgetItemsByCategory"/> object must contain the category description, a list of <see cref="BudgetItem"/> objects, 
        ///       and a total for the expenses in that category.</li>
        ///   <li>Within each <see cref="BudgetItemsByCategory"/> object, the list of <see cref="BudgetItem"/> objects must be sorted by date in ascending order.</li>
        ///   <li>The system must retrieve data using SQL queries, ensuring scalability and maintainability for large data sets.</li>
        /// </ul>
        /// </summary>
        /// <param name="Start">The start date for filtering, with a default value of January 1, 1900, if unspecified.</param>
        /// <param name="End">The end date for filtering, with a default value of January 1, 2500, if unspecified.</param>
        /// <param name="FilterFlag">A flag indicating whether to filter by category. If set to true, the <paramref name="CategoryID"/> is applied.</param>
        /// <param name="CategoryID">The category ID to filter by, applied only if <paramref name="FilterFlag"/> is true. If not provided, no filtering by category occurs.</param>
        /// <returns>
        /// A list of <see cref="BudgetItemsByCategory"/> objects, where:
        /// - The list is sorted alphabetically by category description (A-Z).
        /// - Each object contains:
        ///   - The category description (e.g., "Eating Out", "Credit Card").
        ///   - A list of <see cref="BudgetItem"/> objects for that category, ordered by date.
        ///   - A running total of all expenses for that category.
        /// </returns>
        /// <example>
        /// Example dataset:
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
        /// Example usage for retrieving grouped budget items by category:
        /// <code>
        /// <![CDATA[
        /// DateTime startDate = new DateTime(2024, 1, 1);
        /// DateTime endDate = new DateTime(2024, 1, 31);
        /// List<BudgetItemsByCategory> categoryItems = budget.GetBudgetItemsByCategory(startDate, endDate, true, 1);
        /// ]]>
        /// </code>
        /// Sample output for the example dataset:
        /// <code>
        /// Credit Card  hat ($10.00) ($10.00)
        /// Credit Card  scarf $15.00 $5.00
        /// Eating Out  McDonalds ($45.00) ($45.00)
        /// Eating Out  Wendys ($25.00) ($70.00)
        /// Eating Out  Pizza ($33.33) ($103.33)
        /// </code>
        /// </example>

        public List<BudgetItemsByCategory> GetBudgetItemsByCategory(DateTime? Start, DateTime? End, bool FilterFlag, int CategoryID)
        {
            try
            {

                // Set default values for Start and End if they are not provided
                DateTime startDate = Start ?? new DateTime(1900, 1, 1);
                DateTime endDate = End ?? new DateTime(2500, 1, 1);

                // -----------------------------------------------------------------------
                // get all items first
                // If FilterFlag is false, CategoryID is ignored
                // -----------------------------------------------------------------------
                List<BudgetItem> items = GetBudgetItems(startDate, endDate, FilterFlag, CategoryID);

                // -----------------------------------------------------------------------
                // Group items by Category (Key: Category Description)
                // -----------------------------------------------------------------------
                Dictionary<string, List<BudgetItem>> groupedByCategory = new Dictionary<string, List<BudgetItem>>();


                foreach (BudgetItem item in items)
                {
                    string categoryName = item.Category; // Category description

                    // Check if category already exists in the dictionary, if not, initialize it
                    if (!groupedByCategory.ContainsKey(categoryName))
                    {
                        groupedByCategory[categoryName] = new List<BudgetItem>();
                    }

                    // Add item to the corresponding category group
                    groupedByCategory[categoryName].Add(item);
                }

                // Initialize a list to store the final summary of grouped expenses
                List<BudgetItemsByCategory> summary = new List<BudgetItemsByCategory>();

                // Go through each category group, sort, and calculate totals
                foreach (KeyValuePair<string, List<BudgetItem>> categoryGroup in groupedByCategory.OrderBy(g => g.Key))
                {
                    // Sort items within each category by Date (ascending order)
                    List<BudgetItem> sortedDetails = categoryGroup.Value.OrderBy(i => i.Date).ToList();

                    // Calculate total amount for the current category
                    double total = sortedDetails.Sum(i => i.Amount);

                    // Create a BudgetItemsByCategory object
                    BudgetItemsByCategory budgetCategorySummary = new BudgetItemsByCategory
                    {
                        Category = categoryGroup.Key, // Category description (ex Food)
                        Details = sortedDetails,      // List of budget items for this category
                        Total = total                 // Total expenses for this category
                    };

                    // Add to summary list
                    summary.Add(budgetCategorySummary);
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
        /// Groups expenses by category and month, and returns a list of dictionaries with detailed records from the database.
        /// </summary>
        /// <param name="Start">The starting date for filtering (nullable). Defaults to 1900-01-01 if not provided.</param>
        /// <param name="End">The ending date for filtering (nullable). Defaults to 2500-01-01 if not provided.</param>
        /// <param name="FilterFlag">A flag indicating whether to apply category filtering.</param>
        /// <param name="CategoryID">The category ID to filter by (only applied if <paramref name="FilterFlag"/> is true).</param>
        /// <returns>A list of dictionaries containing month-wise budget details, including totals per category and month, 
        /// and overall totals for each category.</returns>
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
        /// Example usage for grouping expenses by category and month:
        /// <code>
        /// <![CDATA[
        /// DateTime startDate = new DateTime(2024, 1, 1);
        /// DateTime endDate = new DateTime(2024, 1, 31);
        /// List<Dictionary<string, object>> budgetData = budget.GetBudgetDictionaryByCategoryAndMonth(startDate, endDate, true, 1);
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
                // GET ALL ITEMS BY MONTH
                // -----------------------------------------------------------------------
                List<BudgetItemsByMonth> GroupedByMonth = GetBudgetItemsByMonth(Start, End, FilterFlag, CategoryID);

                // -----------------------------------------------------------------------
                // LOOP OVER EACH MONTH
                // -----------------------------------------------------------------------
                var summary = new List<Dictionary<string, object>>();
                var totalsPerCategory = new Dictionary<string, double>();

                foreach (var MonthGroup in GroupedByMonth)
                {
                    // CREATE RECORD OBJECT FOR THIS MONTH
                    Dictionary<string, object> record = new Dictionary<string, object>();
                    record["Month"] = MonthGroup.Month;
                    record["Total"] = MonthGroup.Total;

                    // BREAK UP THE MONTH DETAILS INTO CATEGORIES
                    var GroupedByCategory = MonthGroup.Details.GroupBy(c => c.Category);

                    // -----------------------------------------------------------------------
                    // LOOP OVER EACH CATEGORY
                    // -----------------------------------------------------------------------
                    foreach (var CategoryGroup in GroupedByCategory.OrderBy(g => g.Key))
                    {
                        // CALCULATE TOTALS FOR THE CATEGORY IN THIS MONTH AND CREATE LIST OF DETAILS
                        double total = 0;
                        var details = new List<BudgetItem>();

                        foreach (var item in CategoryGroup)
                        {
                            total += item.Amount;
                            details.Add(item);
                        }

                        // ADD NEW PROPERTIES AND VALUES TO OUR RECORD OBJECT
                        record[$"details:{CategoryGroup.Key}"] = details; // CHANGED TO USE STRING INTERPOLATION FOR CONSISTENCY
                        record[CategoryGroup.Key] = total;

                        // KEEP TRACK OF TOTALS FOR EACH CATEGORY ACROSS ALL MONTHS
                        if (totalsPerCategory.TryGetValue(CategoryGroup.Key, out double CurrentCatTotal))
                        {
                            totalsPerCategory[CategoryGroup.Key] = CurrentCatTotal + total;
                        }
                        else
                        {
                            totalsPerCategory[CategoryGroup.Key] = total;
                        }
                    }

                    // ADD RECORD TO COLLECTION
                    summary.Add(record);
                }

                // ---------------------------------------------------------------------------
                // ADD FINAL RECORD WHICH IS THE TOTALS FOR EACH CATEGORY
                // ---------------------------------------------------------------------------
                Dictionary<string, object> totalsRecord = new Dictionary<string, object>();
                totalsRecord["Month"] = "TOTALS";

                foreach (var category in totalsPerCategory)
                {
                    totalsRecord[category.Key] = category.Value; // ADDED TO ENSURE TOTALS ARE INCLUDED IN FINAL RECORD
                }

                summary.Add(totalsRecord);
                return summary;
            }
            catch (SQLiteException sqlEx)
            {
                Console.WriteLine($"Database error: {sqlEx.Message}");
                throw new Exception("A database error occurred while fetching budget dictionary by category and month.", sqlEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw new Exception("An unexpected error occurred while processing budget data by category and month.", ex);
            }
        }




        #endregion GetList

    }
}

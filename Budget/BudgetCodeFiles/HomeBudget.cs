// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================

using System.Reflection.Metadata;
using System;
using System.Data.Entity;
using System.Data.SQLite;

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
        private string _FileName;
        private string _DirName;
        private Categories _categories;
        private Expenses _expenses;

        // ====================================================================
        // Properties
        // ===================================================================

        // Properties (location of files etc)
        /// <summary>
        /// Gets the budget file name.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the name of the budget file (e.g., "test.budget").
        /// </value>
        public String FileName { get { return _FileName; } }

        /// <summary>
        /// Gets the budget directory name.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the directory where the budget file is located (e.g., "C:\\Users\\Documents\\Budget").
        /// </value>
        public String DirName { get { return _DirName; } }

        /// <summary>
        /// Gets the full path of the budget file.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the full file path for the budget (e.g., "C:\\Users\\Documents\\Budget\\budget2024.txt").
        /// Returns null if the file name or directory name is not set.
        /// </value>
        public String PathName
        {
            get
            {
                if (_FileName != null && _DirName != null)
                {
                    return Path.GetFullPath(_DirName + "\\" + _FileName);
                }
                else
                {
                    return null;
                }
            }
        }

        // Properties (categories and expenses object)

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
        // Constructor (new... default categories, no expenses)
        // -------------------------------------------------------------------
        /// <summary>
        /// Initializing the categories and expenses fields using the default constructor of the <cref="Categories"/> class and 
        /// the <see cref="Expenses"/> class
        /// </summary>
        public HomeBudget()
        {
            _categories = new Categories();
            _expenses = new Expenses();
        }

        // -------------------------------------------------------------------
        // Constructor (existing budget ... must specify file)
        // -------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="HomeBudget"/> class with an existing budget file.
        /// </summary>
        /// <param name="budgetFileName">The name of the budget file.</param>
        public HomeBudget(String budgetFileName)
        {
            Database.newDatabase(budgetFileName);
            _categories = new Categories();
            //_categories = new Categories(Database.dbConnection, false);
            _expenses = new Expenses();
            ReadFromFile(budgetFileName);
        }

        //temp homebudget constructor
        public HomeBudget(String databaseFile, String expensesXMLFile, bool newDB = false)
        {
            // if database exists, and user doesn't want a new database, open existing DB
            if (!newDB && File.Exists(databaseFile))
            {
                Database.existingDatabase(databaseFile);
            }

            // file did not exist, or user wants a new database, so open NEW DB
            else
            {
                Database.newDatabase(databaseFile);
                newDB = true;
            }

            // create the category object
            _categories = new Categories(Database.dbConnection, newDB);

            // create the _expenses course
            _expenses = new Expenses();
            _expenses.ReadFromFile(expensesXMLFile);
        }

        #region OpenNewAndSave
        // ---------------------------------------------------------------
        // Read
        // Throws Exception if any problem reading this file
        // ---------------------------------------------------------------

        /// <summary>
        /// Reads budget data from a specified file, which contains references to category and expense files.
        /// </summary>
        /// <param name="budgetFileName">The name of the budget file containing references to category and expense files.</param>
        /// <exception cref="Exception">Thrown if there is an issue reading the file, such as if the file doesn't exist or can't be read.</exception>
        /// <example>
        /// Example usage:
        /// <code>
        /// HomeBudget budget = new HomeBudget();
        /// budget.ReadFromFile("budget_data.txt");
        /// </code>
        /// <para><b>Expected Budget File Format:</b></para>
        /// The budget file should contain two lines, each specifying the filename of the category and expense files, respectively.
        /// 
        /// <para><b>Example Budget File Content:</b></para>
        /// <code>
        /// categories.txt
        /// expenses.txt
        /// </code>
        /// 
        /// <para><b>Expected Category File Format (categories.xml):</b></para>
        /// The category file should be an XML file structured as follows:
        /// <code>
        /// <![CDATA[
        /// <Categories>
        ///   <Category ID="17" type="Expense">Non Standard</Category>
        ///   <Category ID="1" type="Expense">Utilities</Category>
        ///   <Category ID="2" type="Expense">Rent</Category>
        ///   <Category ID="3" type="Expense">Food</Category>
        ///   <Category ID="4" type="Expense">Entertainment</Category>
        /// </Categories>
        /// ]]>
        /// </code>
        /// 
        /// <para><b>Expected Expense File Format (expenses.xml):</b></para>
        /// The expense file should be an XML file structured as follows:
        /// <code>
        /// <![CDATA[
        /// <Expenses>
        ///   <Expense ID="1">
        ///     <Date>1/10/2018 12:00:00 AM</Date>
        ///     <Description>hat (on credit)</Description>
        ///     <Amount>12</Amount>
        ///     <Category>10</Category>
        ///   </Expense>
        ///   <Expense ID="2">
        ///     <Date>1/11/2018 12:00:00 AM</Date>
        ///     <Description>hat (on credit)</Description>
        ///     <Amount>-10</Amount>
        ///     <Category>9</Category>
        ///   </Expense>
        /// </Expenses>
        /// ]]>
        /// </code>
        /// </example>
        public void ReadFromFile(String budgetFileName)
        {
            // ---------------------------------------------------------------
            // read the budget file and process
            // ---------------------------------------------------------------
            try
            {
                // get filepath name (throws exception if it doesn't exist)
                budgetFileName = BudgetFiles.VerifyReadFromFileName(budgetFileName, "");

                // If file exists, read it
                string[] filenames = System.IO.File.ReadAllLines(budgetFileName);

                // ----------------------------------------------------------------
                // Save information about budget file
                // ----------------------------------------------------------------
                string folder = Path.GetDirectoryName(budgetFileName);
                _FileName = Path.GetFileName(budgetFileName);

                // read the expenses and categories from their respective files
                //_categories.ReadFromFile(folder + "\\" + filenames[0]);
                _expenses.ReadFromFile(folder + "\\" + filenames[1]);

                // Save information about budget file
                _DirName = Path.GetDirectoryName(budgetFileName);
                _FileName = Path.GetFileName(budgetFileName);

            }

            // ----------------------------------------------------------------
            // throw new exception if we cannot get the info that we need
            // ----------------------------------------------------------------
            catch (Exception e)
            {
                throw new Exception("Could not read budget info: \n" + e.Message);
            }

        }

        // ====================================================================
        // save to a file
        // saves the following files:
        //  filepath_expenses.exps  # expenses file
        //  filepath_categories.cats # categories files
        //  filepath # a file containing the names of the expenses and categories files.
        //  Throws exception if we cannot write to that file (ex: invalid dir, wrong permissions)
        // ====================================================================
        /// <summary>
        /// Saves the budget data to a file, creating separate files for expenses and categories.
        /// </summary>
        /// <param name="filepath">The base file path where budget data will be saved.</param>
        /// <exception cref="Exception">Thrown if the file cannot be written (e.g., invalid directory, insufficient permissions).</exception>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// HomeBudget budget = new HomeBudget();
        /// try
        /// {
        ///     budget.SaveToFile("C:\\Users\\Alaa\\source\\repos\\Budget\\BudgetTesting\\test.budget");
        ///     Console.WriteLine("Budget saved successfully.");
        /// }
        /// catch (Exception ex)
        /// {
        ///     Console.WriteLine($"Error saving budget: {ex.Message}");
        /// }
        /// ]]>
        /// </code>
        /// <para><b>Files Created:</b></para>
        /// When this method runs, it creates three files:
        /// <ul>
        ///   <li><b>Budget file:</b> Contains the names of the category and expense files.</li>
        ///   <li><b>Category file:</b> Stores all budget categories.</li>
        ///   <li><b>Expense file:</b> Stores all recorded expenses.</li>
        /// </ul>
        ///
        /// <para><b>Expected Budget File Format (test.budget):</b></para>
        /// This file lists the filenames of the category and expense files:
        /// <code>
        /// test_categories.cats
        /// test_expenses.exps
        /// </code>
        ///
        /// <para><b>Expected Category File Format (test_categories.cats):</b></para>
        /// The category file is a serialized format of budget categories:
        /// <code>
        /// <![CDATA[
        /// <Categories>
        ///   <Category ID="1" type="Expense">Utilities</Category>
        ///   <Category ID="2" type="Expense">Rent</Category>
        ///   <Category ID="3" type="Expense">Food</Category>
        ///   <Category ID="4" type="Expense">Entertainment</Category>
        /// </Categories>
        /// ]]>
        /// </code>
        ///
        /// <para><b>Expected Expense File Format (test_expenses.exps):</b></para>
        /// The expense file contains recorded expenses:
        /// <code>
        /// <![CDATA[
        /// <Expenses>
        ///   <Expense ID="1">
        ///     <Date>1/10/2018 12:00:00 AM</Date>
        ///     <Description>hat (on credit)</Description>
        ///     <Amount>12</Amount>
        ///     <Category>10</Category>
        ///   </Expense>
        ///   <Expense ID="2">
        ///     <Date>1/11/2018 12:00:00 AM</Date>
        ///     <Description>hat (on credit)</Description>
        ///     <Amount>-10</Amount>
        ///     <Category>9</Category>
        ///   </Expense>
        /// </Expenses>
        /// ]]>
        /// </code>
        /// </example>
        public void SaveToFile(String filepath)
        {

            // ---------------------------------------------------------------
            // just in case filepath doesn't exist, reset path info
            // ---------------------------------------------------------------
            _DirName = null;
            _FileName = null;

            // ---------------------------------------------------------------
            // get filepath name (throws exception if we can't write to the file)
            // ---------------------------------------------------------------
            filepath = BudgetFiles.VerifyWriteToFileName(filepath, "");

            String path = Path.GetDirectoryName(Path.GetFullPath(filepath));
            String file = Path.GetFileNameWithoutExtension(filepath);
            String ext = Path.GetExtension(filepath);

            // ---------------------------------------------------------------
            // construct file names for expenses and categories
            // ---------------------------------------------------------------
            String expensepath = path + "\\" + file + "_expenses" + ".exps";
            String categorypath = path + "\\" + file + "_categories" + ".cats";

            // ---------------------------------------------------------------
            // save the expenses and categories into their own files
            // ---------------------------------------------------------------
            _expenses.SaveToFile(expensepath);
            //_categories.SaveToFile(categorypath);

            // ---------------------------------------------------------------
            // save filenames of expenses and categories to budget file
            // ---------------------------------------------------------------
            string[] files = { Path.GetFileName(categorypath), Path.GetFileName(expensepath) };
            System.IO.File.WriteAllLines(filepath, files);

            // ----------------------------------------------------------------
            // save filename info for later use
            // ----------------------------------------------------------------
            _DirName = path;
            _FileName = Path.GetFileName(filepath);
        }
        #endregion OpenNewAndSave

        #region GetList



        // ============================================================================
        // Get all expenses list
        // NOTE: VERY IMPORTANT... budget amount is the negative of the expense amount
        // Reasoning: an expense of $15 is -$15 from your bank account.
        // ============================================================================
        /// <summary>
        /// Retrieves a list of budget items based on the provided filters. 
        /// The amount for expenses is negative (e.g., $15 expense = -$15).
        /// </summary>
        /// <param name="Start">The start date for filtering. If not provided, defaults to 1900-01-01.</param>
        /// <param name="End">The end date for filtering. If not provided, defaults to 2500-01-01.</param>
        /// <param name="FilterFlag">A boolean indicating whether to apply category filtering. If true, the <paramref name="CategoryID"/> parameter is used to filter by category.</param>
        /// <param name="CategoryID">The category ID to filter by. Only applied if <paramref name="FilterFlag"/> is true.</param>
        /// <returns>A list of <see cref="BudgetItem"/> objects, each representing a budget item within the specified date range and filter criteria.</returns>
        /// <example>
        /// For all examples below, assume the budget file contains the following data:
        /// <code>
        /// Cat_ID Expense_ID Date Description Cost
        /// 10 1 1/10/2018 12:00:00 AM Clothes hat (on credit) 10
        /// 9 2 1/11/2018 12:00:00 AM Credit Card hat -10
        /// 10 3 1/10/2019 12:00:00 AM Clothes scarf (on credit) 15
        /// 9 4 1/10/2020 12:00:00 AM Credit Card scarf -15
        /// 14 5 1/11/2020 12:00:00 AM Eating Out McDonalds 45
        /// 14 7 1/12/2020 12:00:00 AM Eating Out Wendys 25
        /// 14 10 2/1/2020 12:00:00 AM Eating Out Pizza 33.33
        /// 9 13 2/10/2020 12:00:00 AM Credit Card mittens -15
        /// 9 12 2/25/2020 12:00:00 AM Credit Card Hat -25
        /// 14 11 2/27/2020 12:00:00 AM Eating Out Pizza 33.33
        /// 14 9 7/11/2020 12:00:00 AM Eating Out Cafeteria 11.11
        /// </code>
        /// <b>Example: Get all budget items without filtering</b>
        /// This will retrieve all budget items without applying category filtering.
        /// <code>
        /// <![CDATA[
        /// HomeBudget budget = new HomeBudget();
        /// budget.ReadFromFile("budget_data.txt");
        ///
        /// // Get a list of all budget items
        /// List<BudgetItem> budgetItems = budget.GetBudgetItems(null, null, false, 0);
        ///
        /// // Print results
        /// foreach (BudgetItem  item in budgetItems)
        /// {
        ///     Console.WriteLine($"{item.Date:yyyy/MMM/dd} {item.ShortDescription} {item.Amount:C} {item.Balance:C}");
        /// }
        /// ]]>
        /// </code>
        /// Sample output:
        /// <code>
        /// 2018/Jan/10 hat (on credit) ($10.00) ($10.00)
        /// 2018/Jan/11 hat $10.00 $0.00
        /// 2019/Jan/10 scarf (on credit) ($15.00) ($15.00)
        /// 2020/Jan/10 scarf $15.00 $0.00
        /// 2020/Jan/11 McDonalds ($45.00) ($45.00)
        /// 2020/Jan/12 Wendys ($25.00) ($70.00)
        /// 2020/Feb/01 Pizza ($33.33) ($103.33)
        /// 2020/Feb/10 mittens $15.00 ($88.33)
        /// 2020/Feb/25 Hat $25.00 ($63.33)
        /// 2020/Feb/27 Pizza ($33.33) ($96.66)
        /// 2020/Jul/11 Cafeteria ($11.11) ($107.77)
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
        /// <param name="Start">The starting date for filtering (nullable).</param>
        /// <param name="End">The ending date for filtering (nullable).</param>
        /// <param name="FilterFlag">A flag indicating whether to apply category filtering.</param>
        /// <param name="CategoryID">The category ID to filter by (only applied if <paramref name="FilterFlag"/> is true).</param>
        /// <returns>A list of grouped budget items by month, including year/month, list of items, and monthly total.</returns>
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
        /// Example usage for getting monthly budget items:
        /// <code>
        /// <![CDATA[
        /// DateTime startDate = new DateTime(2024, 1, 1);
        /// DateTime endDate = new DateTime(2024, 1, 31);
        /// List<BudgetItemsByMonth> monthlyItems = budget.GetBudgetItemsByMonth(startDate, endDate, true, 1);
        /// ]]>
        /// </code>
        /// 
        /// Sample output:
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
        /// Groups expenses by category and returns a list of budget items by category.
        /// </summary>
        /// <param name="Start">The starting date for filtering (nullable).</param>
        /// <param name="End">The ending date for filtering (nullable).</param>
        /// <param name="FilterFlag">A flag indicating whether to apply category filtering.</param>
        /// <param name="CategoryID">The category ID to filter by (only applied if <paramref name="FilterFlag"/> is true).</param>
        /// <returns>A list of grouped budget items by category, including category name, list of items, and total per category.</returns>
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
        /// Example usage for getting category-wise budget items:
        /// <code>
        /// <![CDATA[
        /// DateTime startDate = new DateTime(2024, 1, 1);
        /// DateTime endDate = new DateTime(2024, 1, 31);
        /// List<BudgetItemsByCategory> categoryItems = budget.GetBudgetItemsByCategory(startDate, endDate, true, 1);
        /// ]]>
        /// </code>
        /// 
        /// Sample output:
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
        /// Groups expenses by category and month, and returns a list of dictionaries with detailed records.
        /// </summary>
        /// <param name="Start">The starting date for filtering (nullable). Defaults to 1900-01-01 if not provided.</param>
        /// <param name="End">The ending date for filtering (nullable). Defaults to 2500-01-01 if not provided.</param>
        /// <param name="FilterFlag">A flag indicating whether to apply category filtering.</param>
        /// <param name="CategoryID">The category ID to filter by (only applied if <paramref name="FilterFlag"/> is true).</param>
        /// <returns>A list of dictionaries containing month-wise budget details, including total per category and month, 
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

using System.Data;
using System.Data.SQLite;
using System.Xml;

// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================

namespace Budget
{
    // ====================================================================
    // CLASS: expenses
    //        - A collection of expense items,
    //        - Read / write to file
    //        - etc
    // ====================================================================
    /// <summary>
    /// Represents a collection of expense items with functionality to read from and write to XML files.
    /// </summary>
    public class Expenses
    {

        //Database connection 
        private SQLiteConnection _connection;
        public SQLiteConnection Connection { get { return _connection; } set { _connection = value; } }
        public Expenses()
        {
            //default????
            SetExpensesToDefaults();
            
        }
        public Expenses(SQLiteConnection conn, bool useDefault)
        {
            Connection = conn;
            
            if (useDefault)
            {
                SetExpensesToDefaults();
            }
            else
            {
                List();
            }
        }
        private List<Expense> _Expenses = new List<Expense>();



        //
        public void SetExpensesToDefaults()
        {
            //heheh
            DeleteAll();
            
            InsertIntoExpenses(new DateTime(2018 - 01 - 10), 12, "hat (on credit)", 10);
            InsertIntoExpenses(new DateTime(2018 - 01 - 11), -10, "hat (on credit)", 9);
            InsertIntoExpenses(new DateTime(2019 - 01 - 10), 15, "scarf (on credit)", 10);
            InsertIntoExpenses(new DateTime(2020 - 01 - 10), -15, "scarf (on credit)", 9);
            InsertIntoExpenses(new DateTime(2020 - 01 - 11), 45, "McDonalds", 14);
            InsertIntoExpenses(new DateTime(2020 - 01 - 12), 25, "Wendys", 14);
        }

        public void InsertIntoExpenses(DateTime date, Double amount, String description, int category)
        {
            //is the id automatic??
            string queryInsertNewExpense = "INSERT INTO expenses (Date, CategoryId, Amount, Description) " +
                "VALUES (@date, @idCategory, @amount, @desc)";

            using SQLiteCommand cmd = new SQLiteCommand(queryInsertNewExpense, Connection);

            //Add the parameters

            cmd.Parameters.AddWithValue("@date", date);
            cmd.Parameters.AddWithValue("@idCategory", category);
            cmd.Parameters.AddWithValue("@amount", amount);
            cmd.Parameters.AddWithValue("@desc", description);
            cmd.ExecuteNonQuery();
        }
        public void DeleteAll()
        {
            try
            {
                string deleteQuery = "DELETE FROM expenses;";
                using var deleteCmd = new SQLiteCommand(deleteQuery, Connection);
                deleteCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting all expenses: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a new expense to the list of expenses.
        /// </summary>
        /// <param name="date">The date of the expense.</param>
        /// <param name="category">The ID of the category associated with the expense.</param>
        /// <param name="amount">The amount of the expense.</param>
        /// <param name="description">The description of the expense.</param>
        /// <example>
        /// To add a new expense:
        /// <code>
        ///<![CDATA[
        /// Expenses expenses = new Expenses();
        /// expenses.Add(new DateTime(2025, 2, 1), 1, 50.00, "Lunch at a café");
        /// ]]>
        /// </code>
        /// </example>
        public void Add(DateTime date, Double amount, String description, int category)
        {

            //Using System.DataSqlite
            try
            {
                //is the id automatic??
                string queryInsertNewExpense = "INSERT INTO expenses (Date, CategoryId, Amount, Description) " +
                    "VALUES (@date, @idCategory, @amount, @desc)";

                using SQLiteCommand cmd = new SQLiteCommand(queryInsertNewExpense, Connection);

                //Add the parameters
                //cmd.Parameters.AddWithValue("@date", date);
                cmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@idCategory", category);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@desc", description);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding the expense: " + ex.Message);
            }
        }

        // ====================================================================
        // Delete expense
        // ====================================================================
        /// <summary>
        /// Deletes an expense by its id.
        /// </summary>
        /// <param name="Id">The Id of the expense to be deleted.</param>
        /// <example>
        /// To delete an expense by Id:
        /// <code>
        /// <![CDATA[
        /// Expenses expenses = new Expenses();
        /// expenses.Delete(1); // Delete the expense with Id 1
        /// ]]>
        /// </code>
        /// </example>
        public void Delete(int Id)
        {
            //int i = _Expenses.FindIndex(x => x.Id == Id);
            //if (i < 0)
            //    return;
            //_Expenses.RemoveAt(i);
            //Throws exception if not allowed to delete in database(foreign key constraint).
            try
            {
                string query = "DELETE FROM expenses WHERE Id = @id;";
                using var cmd = new SQLiteCommand(query, Connection);
                cmd.Parameters.AddWithValue("@id", Id);

                //if the number of row affected is zero, then nothing was deleted.
                int rowsChanged = cmd.ExecuteNonQuery();
                if (rowsChanged == 0)
                {
                    throw new Exception($"Error: No expenses with Id {Id} found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting the expenses with Id {Id}: {ex.Message}");
            }

        }

        // ====================================================================
        // Return list of expenses
        // Note:  make new copy of list, so user cannot modify what is part of
        //        this instance
        // ====================================================================

        /// <summary>
        /// Returns a new list of all expenses, ensuring that the original list is not modified.
        /// </summary>
        /// <returns>A list of expense objects.</returns>
        /// <example>
        /// To get a list of all expenses:
        /// <code>
        /// <![CDATA[
        /// Expenses expenses = new Expenses();
        /// List<Expense> allExpenses = expenses.List();
        /// foreach (Expense exp in allExpenses)
        /// {
        ///     Console.WriteLine(exp.Description); // Output each expense's description
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public List<Expense> List()
        {
            List<Expense> newList = new List<Expense>();
            string retrieves = "SELECT Id, Date, Amount, CategoryId, Description FROM expenses ORDER BY Id";

            try
            {
                int colId = 0, colDate = 1, colAmount = 2, colCategory = 3, colDescription = 4;

                using var cmd = new SQLiteCommand(retrieves, Connection);
                using SQLiteDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    int id = rdr.GetInt32(colId);
                    DateTime dateTime = DateTime.Parse(rdr.GetString(colDate));
                    double amount = rdr.GetDouble(colAmount);
                    int category = rdr.GetInt32(colCategory);
                    String description = rdr.GetString(colDescription);
                    newList.Add(new Expense(id, dateTime, category, amount, description));
                }
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Error in retrieve Expense.");
            }
            return newList;
        }
        //
        public Expense GetExpenseFromId(int id)
        {
            try
            {
                string retrieves = "SELECT Id, Date, Amount, CategoryId, Description FROM expenses WHERE Id = @Id";

                using var cmd = new SQLiteCommand(retrieves, Connection);

                if (Connection.State != ConnectionState.Open)
                {
                    Connection.Open();
                }

                cmd.Parameters.AddWithValue("@Id", id);

                using SQLiteDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    int colId = 0, colDate = 1, colAmount = 2, colCategory = 3, colDescription = 4;

                    DateTime dateTime = DateTime.Parse(rdr.GetString(colDate));
                    double amount = rdr.GetDouble(colAmount);
                    int category = rdr.GetInt32(colCategory);
                    string description = rdr.GetString(colDescription);

                    return new Expense(id, dateTime, category, amount, description);
                }
                else
                {
                    throw new Exception("Cannot find expense with id " + id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetExpenseFromId." + ex.Message);
            }
            return null;
        }
        //
        public void UpdateExpenses(int id, DateTime date, Double amount, String description, int category)
        {
            try
            {
                //is the id automatic??
                string queryInsertNewExpense = "UPDATE expenses SET Date = @date, CategoryId = @idCategory" +
                    ", Amount = @amount, Description = @desc WHERE Id = @Id";

                using SQLiteCommand cmd = new SQLiteCommand(queryInsertNewExpense, Connection);

                //Add the parameters

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@date", date);
                cmd.Parameters.AddWithValue("@idCategory", category);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@desc", description);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating the expense: " + ex.Message);
            }

        }

    }
}


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
        private bool _useDefaults;
        public SQLiteConnection Connection { get { return _connection; } set { _connection = value; } }
        public bool UseDefaults { get { return _useDefaults; } set { _useDefaults = value; } }
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
        private static String DefaultFileName = "budget.txt";
        private List<Expense> _Expenses = new List<Expense>();
        private string _FileName;
        private string _DirName;

        // ====================================================================
        // Properties
        // ====================================================================
        /// <summary>
        /// Gets the filename of the current expenses file.
        /// </summary>
        /// <value>
        /// The name of the file where expense data is stored.
        /// </value>
        public String FileName { get { return _FileName; } }

        /// <summary>
        /// Gets the directory name of the current expenses file.
        /// </summary>
        /// <value>
        /// The full path of the directory where the expenses file is stored.
        /// </value>
        public String DirName { get { return _DirName; } }

        // ====================================================================
        // populate categories from a file
        // if filepath is not specified, read/save in AppData file
        // Throws System.IO.FileNotFoundException if file does not exist
        // Throws System.Exception if cannot read the file correctly (parsing XML)
        // ====================================================================
        /// <summary>
        /// Reads expenses from an XML file and adds them to the current expenses list.
        /// </summary>
        /// <param name="filepath">The path to the XML file containing expense data. If null, the default file path is used.</param>
        /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
        /// <exception cref="Exception">Thrown if there is an error parsing the XML file.</exception>
        /// <example>
        /// To read expenses from a file:
        /// <code>
        /// <![CDATA[
        /// Expenses expenses = new Expenses();
        /// expenses.ReadFromFile("expenses.xml"); // Read expenses from the specified XML file
        /// ]]>
        /// </code>
        /// </example>
        public void ReadFromFile(String? filepath = null)
        {

            // ---------------------------------------------------------------
            // reading from file resets all the current expenses,
            // so clear out any old definitions
            // ---------------------------------------------------------------
            _Expenses.Clear();

            // ---------------------------------------------------------------
            // reset default dir/filename to null 
            // ... filepath may not be valid, 
            // ---------------------------------------------------------------
            _DirName = null;
            _FileName = null;

            // ---------------------------------------------------------------
            // get filepath name (throws exception if it doesn't exist)
            // ---------------------------------------------------------------
            filepath = BudgetFiles.VerifyReadFromFileName(filepath, DefaultFileName);

            // ---------------------------------------------------------------
            // read the expenses from the xml file
            // ---------------------------------------------------------------
            _ReadXMLFile(filepath);

            // ----------------------------------------------------------------
            // save filename info for later use?
            // ----------------------------------------------------------------
            _DirName = Path.GetDirectoryName(filepath);
            _FileName = Path.GetFileName(filepath);


        }

        // ====================================================================
        // save to a file
        // if filepath is not specified, read/save in AppData file
        // ====================================================================
        /// <summary>
        /// Saves the current list of expenses to an XML file.
        /// </summary>
        /// <param name="filepath">The path to the XML file where the expenses should be saved. If null, the last used file path is used.</param>
        /// <exception cref="Exception">Thrown if there is an error writing the XML file.</exception>
        /// <example>
        /// To save expenses to a file:
        /// <code>
        /// <![CDATA[
        /// Expenses expenses = new Expenses();
        /// expenses.SaveToFile("expenses_updated.xml"); // Save expenses to the specified file
        /// ]]>
        /// </code>
        /// </example>
        public void SaveToFile(String filepath = null)
        {
            // ---------------------------------------------------------------
            // if file path not specified, set to last read file
            // ---------------------------------------------------------------
            if (filepath == null && DirName != null && FileName != null)
            {
                filepath = DirName + "\\" + FileName;
            }

            // ---------------------------------------------------------------
            // just in case filepath doesn't exist, reset path info
            // ---------------------------------------------------------------
            _DirName = null;
            _FileName = null;

            // ---------------------------------------------------------------
            // get filepath name (throws exception if it doesn't exist)
            // ---------------------------------------------------------------
            filepath = BudgetFiles.VerifyWriteToFileName(filepath, DefaultFileName);

            // ---------------------------------------------------------------
            // save as XML
            // ---------------------------------------------------------------
            _WriteXMLFile(filepath);

            // ----------------------------------------------------------------
            // save filename info for later use
            // ----------------------------------------------------------------
            _DirName = Path.GetDirectoryName(filepath);
            _FileName = Path.GetFileName(filepath);
        }


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
        // ====================================================================
        // Add expense
        // ====================================================================
        private void Add(Expense exp)
        {
            _Expenses.Add(exp);
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
                    Console.WriteLine($"Error: No expenses with Id {Id} found.");
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
            catch (ArgumentException)
            {
                Console.WriteLine("Error in GetExpenseFromId.");
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

        // ====================================================================
        // read from an XML file and add categories to our categories list
        // ====================================================================
        private void _ReadXMLFile(String filepath)
        {


            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filepath);

                // Loop over each Expense
                foreach (XmlNode expense in doc.DocumentElement.ChildNodes)
                {
                    // set default expense parameters
                    int id = int.Parse((((XmlElement)expense).GetAttributeNode("ID")).InnerText);
                    String description = "";
                    DateTime date = DateTime.Parse("2000-01-01");
                    int category = 0;
                    Double amount = 0.0;

                    // get expense parameters
                    foreach (XmlNode info in expense.ChildNodes)
                    {
                        switch (info.Name)
                        {
                            case "Date":
                                date = DateTime.Parse(info.InnerText);
                                break;
                            case "Amount":
                                amount = Double.Parse(info.InnerText);
                                break;
                            case "Description":
                                description = info.InnerText;
                                break;
                            case "Category":
                                category = int.Parse(info.InnerText);
                                break;
                        }
                    }

                    // have all info for expense, so create new one
                    this.Add(new Expense(id, date, category, amount, description));

                }

            }
            catch (Exception e)
            {
                throw new Exception("ReadFromFileException: Reading XML " + e.Message);
            }
        }


        // ====================================================================
        // write to an XML file
        // if filepath is not specified, read/save in AppData file
        // ====================================================================
        private void _WriteXMLFile(String filepath)
        {
            // ---------------------------------------------------------------
            // loop over all categories and write them out as XML
            // ---------------------------------------------------------------
            try
            {
                // create top level element of expenses
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<Expenses></Expenses>");

                // foreach Category, create an new xml element
                foreach (Expense exp in _Expenses)
                {
                    // main element 'Expense' with attribute ID
                    XmlElement ele = doc.CreateElement("Expense");
                    XmlAttribute attr = doc.CreateAttribute("ID");
                    attr.Value = exp.Id.ToString();
                    ele.SetAttributeNode(attr);
                    doc.DocumentElement.AppendChild(ele);

                    // child attributes (date, description, amount, category)
                    XmlElement d = doc.CreateElement("Date");
                    XmlText dText = doc.CreateTextNode(exp.Date.ToString("M/dd/yyyy hh:mm:ss tt"));
                    ele.AppendChild(d);
                    d.AppendChild(dText);

                    XmlElement de = doc.CreateElement("Description");
                    XmlText deText = doc.CreateTextNode(exp.Description);
                    ele.AppendChild(de);
                    de.AppendChild(deText);

                    XmlElement a = doc.CreateElement("Amount");
                    XmlText aText = doc.CreateTextNode(exp.Amount.ToString());
                    ele.AppendChild(a);
                    a.AppendChild(aText);

                    XmlElement c = doc.CreateElement("Category");
                    XmlText cText = doc.CreateTextNode(exp.Category.ToString());
                    ele.AppendChild(c);
                    c.AppendChild(cText);

                }

                // write the xml to FilePath
                doc.Save(filepath);

            }
            catch (Exception e)
            {
                throw new Exception("SaveToFileException: Reading XML " + e.Message);
            }
        }


    }
}


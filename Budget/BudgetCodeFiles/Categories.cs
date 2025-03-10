using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Reflection.PortableExecutable;
using System.Xml;

// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================

namespace Budget
{
    // ====================================================================
    // CLASS: categories
    //        - A collection of category items,
    //        - Read / write to file
    //        - etc
    // ====================================================================
    /// <summary>
    /// Represents a collection of budget categories with functionality to read from and write to files.
    /// </summary>
    public class Categories
    {
        private static String DefaultFileName = "budgetCategories.txt";
        private List<Category> _Cats = new List<Category>();
        private string _FileName;
        private string _DirName;

        private string connectionString;
        private SQLiteConnection _connection;
        private bool _useDefaults;

        // Properties
        // ====================================================================

        /// <summary>
        /// Gets the filename of the current categories file.
        /// </summary>
        /// <value>
        /// A string representing the filename of the current categories file.
        /// </value>
        public String FileName { get { return _FileName; } }

        /// <summary>
        ///     Gets the directory name of the current categories file.
        /// </summary>
        /// <value>
        /// A string representing the directory where the current categories file is stored.
        /// </value>
        public String DirName { get { return _DirName; } }

        // ====================================================================
        // Constructor
        // ====================================================================
        /// <summary>
        /// Initializes a new instance of the Categories class with default values.
        /// </summary>
        /// <example>
        /// To create a new `Categories` object with default categories:
        /// <code>
        /// Categories categories = new Categories();
        /// </code>
        /// </example>
        public Categories()
        {
            SetCategoriesToDefaults();
        }

        public Categories(SQLiteConnection conn, bool useDefaults)
        {
            _connection = conn;
            _useDefaults = useDefaults;
            connectionString = conn.ConnectionString;

            if (useDefaults)
            {
                SetCategoriesToDefaults();
            }
            else
            {
                List();
            }
        }

        // ====================================================================
        // get a specific category from the list where the id is the one specified
        // ====================================================================

        /// <summary>
        /// Retrieves a category by its ID.
        /// </summary>
        /// <param name="i">The ID of the category to retrieve.</param>
        /// <returns>The matching category.</returns>
        /// <exception cref="Exception">Thrown if the category is not found.</exception>
        /// <example>
        /// To retrieve a category by its ID, use the `GetCategoryFromId` method:
        /// <code>
        /// Categories categories = new Categories();
        /// Category cat = categories.GetCategoryFromId(1);
        /// </code>
        /// </example>
        public Category GetCategoryFromId(int id)
        {
            string query = "SELECT Id, Description, TypeId FROM categories WHERE Id = @id;";
            using var cmd = new SQLiteCommand(query, _connection);
            cmd.Parameters.AddWithValue("@id", id); //avoid sql injection 

            using var reader = cmd.ExecuteReader();
            if (reader.Read()) //if a record was found
            {
                int indexCategoryId = 0;
                int indexDescription = 1;
                int indexTypeId = 2;

                //get id, description and typeId from the reader
                int categoryId = reader.GetInt32(indexCategoryId);
                string description = reader.GetString(indexDescription);
                int typeId = reader.GetInt32(indexTypeId);

                Category.CategoryType type = (Category.CategoryType)typeId - 1;

                return new Category(categoryId, description, type);
            }
            else
            {
                throw new Exception("Cannot find category with id " + id);
            }
        }

        // ====================================================================
        // populate categories from a file
        // if filepath is not specified, read/save in AppData file
        // Throws System.IO.FileNotFoundException if file does not exist
        // Throws System.Exception if cannot read the file correctly (parsing XML)
        // ====================================================================
        /// <summary>
        /// Reads categories from a file.
        /// </summary>
        /// <param name="filepath">The path of the file to read from. If null, a default path is used.</param>
        /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
        /// <exception cref="Exception">Thrown if there is an error parsing the XML file.</exception>
        /// <example>
        /// To read categories from a file:
        /// <code>
        /// Categories categories = new Categories();
        /// categories.ReadFromFile("categories.txt");
        /// </code>
        /// </example>
        public void ReadFromFile(String filepath = null)
        {

            // ---------------------------------------------------------------
            // reading from file resets all the current categories,
            // ---------------------------------------------------------------
            _Cats.Clear();

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
            // If file exists, read it
            // ---------------------------------------------------------------
            _ReadXMLFile(filepath);
            _DirName = Path.GetDirectoryName(filepath);
            _FileName = Path.GetFileName(filepath);
        }

        //// ====================================================================
        //// save to a file
        //// if filepath is not specified, read/save in AppData file
        //// ====================================================================
        ///// <summary>
        ///// Saves categories to a file.
        ///// </summary>
        ///// <param name="filepath">The path of the file to write to. If null, the last used file path is used.</param>
        ///// <example>
        ///// To save categories to a file:
        ///// <code>
        ///// Categories categories = new Categories();
        ///// categories.SaveToFile("categories.txt");
        ///// </code>
        ///// </example>
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

        // ====================================================================
        // set categories to default
        // ====================================================================
        /// <summary>
        /// Resets categories to default values.
        /// </summary>
        /// <example>
        /// To reset categories to the default values:
        /// <code>
        /// Categories categories = new Categories();
        /// categories.SetCategoriesToDefaults();
        /// </code>
        /// </example>


        // ===================================================================
        // INSERT DATA USING PARAMETERIZED QUERIES TO PREVENT SQL INJECTION
        // ===================================================================
        // This method ensures that category types are inserted and then sets the default categories.
        public void SetCategoriesToDefaults()
        {
             DeleteAll();

            // Insert category types if they don't already exist
            InsertCategoryTypeIfNotExists("Income");
            InsertCategoryTypeIfNotExists("Savings");
            InsertCategoryTypeIfNotExists("Expense");
            InsertCategoryTypeIfNotExists("Credit");


           
            // Get the IDs of the predefined category types
            int incomeTypeId = GetCategoryTypeId("Income");
            int savingsTypeId = GetCategoryTypeId("Savings");
            int expenseTypeId = GetCategoryTypeId("Expense");
            int creditTypeId = GetCategoryTypeId("Credit");

            // Insert default categories for each category type
            InsertCategory("Utilities", expenseTypeId);
            InsertCategory("Rent", expenseTypeId);
            InsertCategory("Food", expenseTypeId);
            InsertCategory("Entertainment", expenseTypeId);
            InsertCategory("Education", expenseTypeId);
            InsertCategory("Medical Expenses", expenseTypeId);
            InsertCategory("Vacation", expenseTypeId);
            InsertCategory("Credit Card", creditTypeId);
            InsertCategory("Clothes", expenseTypeId);
            InsertCategory("Gifts", expenseTypeId);
            InsertCategory("Insurance", expenseTypeId);
            InsertCategory("Transportation", expenseTypeId);
            InsertCategory("Eating Out", expenseTypeId);
            InsertCategory("Savings", savingsTypeId);
            InsertCategory("Income", incomeTypeId);

        }

        private int GetCategoryTypeId(string categoryTypeDescription)
        {
            switch (categoryTypeDescription)
            {
                case "Income":
                    return 1;
                case "Expense":
                    return 2;
                case "Credit":
                    return 3;
                case "Savings":
                    return 4;
                default:
                    throw new Exception("Unknown category type: " + categoryTypeDescription);
            }
        }
        // Inserts a category type into the categoryTypes table if it doesn't already exist.
        private void InsertCategoryTypeIfNotExists(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Description cannot be empty or null.");
            }

            try
            {
                // Ensure the connection is open
                if (_connection.State != System.Data.ConnectionState.Open)
                {
                    _connection.Open();
                }

                // Check if the category type already exists
                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM categoryTypes WHERE Description = @desc;", _connection))
                {
                    cmd.Parameters.AddWithValue("@desc", description);
                    var result = cmd.ExecuteScalar();

                    Console.WriteLine($"Checking if category type '{description}' exists. Result: {result}");

                    if (Convert.ToInt32(result) == 0) // If not exists
                    {
                        // Insert the new category type
                        using (var insertCmd = new SQLiteCommand("INSERT INTO categoryTypes (Description) VALUES (@desc);", _connection))
                        {
                            insertCmd.Parameters.AddWithValue("@desc", description);
                            int rowsAffected = insertCmd.ExecuteNonQuery();
                            Console.WriteLine($"Inserted category type '{description}', rows affected: {rowsAffected}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Category type '{description}' already exists.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting category type '{description}': {ex.Message}");
            }
        }


        // Inserts a category into the categories table with a given type ID
        private void InsertCategory(string description, int typeId)
        {
            using (var cmd = new SQLiteCommand("INSERT INTO categories (Description, TypeId) VALUES (@desc, @typeId);", _connection))
            {
                cmd.Parameters.AddWithValue("@desc", description);
                cmd.Parameters.AddWithValue("@typeId", typeId);
                cmd.ExecuteNonQuery();
            }
        }

        // ====================================================================
        // Add category
        // ====================================================================
        private void Add(Category cat)
        {
            _Cats.Add(cat);
        }

        /// <summary>
        /// Adds a new category with a description and type.
        /// </summary>
        /// <param name="desc">The description of the category.</param>
        /// <param name="type">The type of the category (Income, Expense, Credit, Saving)</param>
        /// <example>
        /// To add a new category:
        /// <code>
        /// Categories categories = new Categories();
        /// categories.Add("Travel", Category.CategoryType.Expense);
        /// </code>
        /// </example>
        public void Add(string desc, Category.CategoryType type)
        {
            try
            {
                int typeId = (int)type + 1;
                // SQL query to insert the new category into the categories table
                string query = "INSERT INTO categories (Description, TypeId) VALUES (@desc, @typeId)";

                using (SQLiteCommand cmd = new SQLiteCommand(query, _connection))
                {
                    // Add parameters to the query to prevent SQL injection
                    cmd.Parameters.AddWithValue("@desc", desc);
                    cmd.Parameters.AddWithValue("@typeId", typeId); // Use the typeId directly from the switch expression

                    // Execute the query to insert the new category
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding category: " + ex.Message);
                throw;
            }
        }

        // ====================================================================
        // Delete category
        // ====================================================================
        /// <summary>
        /// Deletes a category by its Id.
        /// </summary>
        /// <param name="Id">The Id of the category to delete.</param>
        /// <example>
        /// To delete a category by Id:
        /// <code>
        /// Categories categories = new Categories();
        /// categories.Delete(1); // Deletes the category with Id 1
        /// </code>
        /// </example>
        public void Delete(int id) //throw error 
        {
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();
            string query = "DELETE FROM categories WHERE Id = @id;";
            using var cmd = new SQLiteCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void DeleteAll()
        {
            using var conn = new SQLiteConnection(connectionString);
            try
            {
                conn.Open();

                string deleteQuery = "DELETE FROM categories;";
                using var deleteCmd = new SQLiteCommand(deleteQuery, conn);
                int rowsAffected = deleteCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting all categories: {ex.Message}");
                throw;
            }
        }

        // ====================================================================
        // Return list of categories
        // Note:  make new copy of list, so user cannot modify what is part of
        //        this instance
        // ====================================================================
        /// <summary>
        /// Returns a copy of the list of categories.
        /// </summary>
        /// <returns>A new list of categories.</returns>
        /// <example>
        /// To get a list of categories:
        /// <code>
        /// <![CDATA[
        /// Categories categories = new Categories();
        /// List<Category> categoryList = categories.List();
        /// ]]>
        /// </code>
        /// </example>
        /// 
        public List<Category> List()
        {
            List<Category> categoriesList = new List<Category>();  

            string query = "SELECT Id, Description, TypeId FROM categories ORDER BY Id";

            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(query, _connection))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);          // Read Id (Index 0)
                            string description = reader.GetString(1);  // Read Description (Index 1)
                            int typeId = reader.GetInt32(2);  // Read TypeId (Index 2)

                            Category.CategoryType type = (Category.CategoryType)typeId ;  // If TypeId is an int in DB
                            // Add the category to the list
                            categoriesList.Add(new Category(id, description, type));
                        }
                    }
                    else
                    {
                        Console.WriteLine("No categories found in the database.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading categories: " + ex.Message);
                throw;
            }

            return categoriesList;  // Return new list instead of modifying _Cats
        }



        // ====================================================================
        // read from an XML file and add categories to our categories list
        // ====================================================================
        private void _ReadXMLFile(String filepath)
        {

            // ---------------------------------------------------------------
            // read the categories from the xml file, and add to this instance
            // ---------------------------------------------------------------
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filepath);

                foreach (XmlNode category in doc.DocumentElement.ChildNodes)
                {
                    String id = (((XmlElement)category).GetAttributeNode("ID")).InnerText;
                    String typestring = (((XmlElement)category).GetAttributeNode("type")).InnerText;
                    String desc = ((XmlElement)category).InnerText;

                    Category.CategoryType type;
                    switch (typestring.ToLower())
                    {
                        case "income":
                            type = Category.CategoryType.Income;
                            break;
                        case "expense":
                            type = Category.CategoryType.Expense;
                            break;
                        case "credit":
                            type = Category.CategoryType.Credit;
                            break;
                        default:
                            type = Category.CategoryType.Savings;
                            break;
                    }
                    this.Add(new Category(int.Parse(id), desc, type));
                }

            }
            catch (Exception e)
            {
                throw new Exception("ReadXMLFile: Reading XML " + e.Message);
            }

        }


        //// ====================================================================
        //// write all categories in our list to XML file
        //// ====================================================================
        private void _WriteXMLFile(String filepath)
        {
            try
            {
                // create top level element of categories
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<Categories></Categories>");

                // foreach Category, create an new xml element
                foreach (Category cat in _Cats)
                {
                    XmlElement ele = doc.CreateElement("Category");
                    XmlAttribute attr = doc.CreateAttribute("ID");
                    attr.Value = cat.Id.ToString();
                    ele.SetAttributeNode(attr);
                    XmlAttribute type = doc.CreateAttribute("type");
                    type.Value = cat.Type.ToString();
                    ele.SetAttributeNode(type);

                    XmlText text = doc.CreateTextNode(cat.Description);
                    doc.DocumentElement.AppendChild(ele);
                    doc.DocumentElement.LastChild.AppendChild(text);

                }

                // write the xml to FilePath
                doc.Save(filepath);

            }
            catch (Exception e)
            {
                throw new Exception("_WriteXMLFile: Reading XML " + e.Message);
            }

        }

        // ====================================================================
        // Method: UpdateCategory
        // Purpose: Allows API users to update a category's Description and Type 
        // based on the provided Id.
        // ====================================================================
        public void UpdateProperties(int id, string newDescription, Category.CategoryType newType)
        {
            try
            {
                // Get the typeId directly from the enum value (no need to add 1)
                int typeId = (int)newType + 1; 

                string query = "UPDATE categories SET Description = @desc, TypeId = @typeId WHERE Id = @id";

                using (SQLiteCommand cmd = new SQLiteCommand(query, _connection))
                {
                    // Add parameters to the query
                    cmd.Parameters.AddWithValue("@desc", newDescription);
                    cmd.Parameters.AddWithValue("@typeId", typeId);
                    cmd.Parameters.AddWithValue("@id", id); // The category Id to be updated

                    // Execute the query
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        throw new Exception($"Category with Id {id} not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating category: {ex.Message}");
                throw;
            }
        }
    }
}


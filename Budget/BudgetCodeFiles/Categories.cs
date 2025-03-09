using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data.SQLite;
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
                LoadCategoriesFromDatabase();
            }
        }

        // Method to load categories from the database
        private void LoadCategoriesFromDatabase()
        {
            string query = "SELECT Id, Description FROM categories";

            try
            {
                Console.WriteLine("Opening connection...");
                if (_connection.State != System.Data.ConnectionState.Open)
                {
                    _connection.Open();
                }

                Console.WriteLine("Executing query...");
                using (SQLiteCommand cmd = new SQLiteCommand(query, _connection))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            Console.WriteLine("Rows found. Reading data...");
                            while (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string description = reader.GetString(1);
                                Console.WriteLine($"Id: {id}, Description: {description}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("No categories found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading categories: {ex.Message}");
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
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT Description, TypeId FROM categories WHERE Id = @id;", _connection))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Category(id, reader.GetString(0), (Category.CategoryType)reader.GetInt32(1));
                    }
                    else
                    {
                        Console.WriteLine($"Category with Id {id} not found.");
                        return null;  // Ensure this part is correct.
                    }
                }
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
            // Insert category types if they don't already exist
            InsertCategoryTypeIfNotExists(Category.CategoryType.Income.ToString());
            InsertCategoryTypeIfNotExists(Category.CategoryType.Savings.ToString());
            InsertCategoryTypeIfNotExists(Category.CategoryType.Expense.ToString());
            InsertCategoryTypeIfNotExists(Category.CategoryType.Credit.ToString());

            // Get the IDs of the predefined category types
            int incomeTypeId = GetCategoryTypeId(Category.CategoryType.Income);
            int savingsTypeId = GetCategoryTypeId(Category.CategoryType.Savings);
            int expenseTypeId = GetCategoryTypeId(Category.CategoryType.Expense);
            int creditTypeId = GetCategoryTypeId(Category.CategoryType.Credit);

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

        // Gets the category type ID based on the description

        private int GetCategoryTypeId(Category.CategoryType type)
        {
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT Id FROM categoryTypes WHERE Description = @desc", _connection))
                {
                    cmd.Parameters.AddWithValue("@desc", type.ToString());
                    object result = cmd.ExecuteScalar();

                    if (result != null && int.TryParse(result.ToString(), out int typeId))
                    {
                        return typeId;  // Return the valid TypeId
                    }
                    else
                    {
                        // If category type is not found, insert the new category type and fetch its Id
                        using (SQLiteCommand insertCmd = new SQLiteCommand("INSERT INTO categoryTypes (Description) VALUES (@desc)", _connection))
                        {
                            insertCmd.Parameters.AddWithValue("@desc", type.ToString());
                            insertCmd.ExecuteNonQuery();  // Insert the new category type
                        }

                        // Now fetch the Id of the newly inserted category type
                        using (SQLiteCommand getIdCmd = new SQLiteCommand("SELECT Id FROM categoryTypes WHERE Description = @desc", _connection))
                        {
                            getIdCmd.Parameters.AddWithValue("@desc", type.ToString());
                            object newResult = getIdCmd.ExecuteScalar();
                            if (newResult != null && int.TryParse(newResult.ToString(), out int newTypeId))
                            {
                                return newTypeId;  // Return the newly inserted Id
                            }
                            else
                            {
                                throw new Exception($"Failed to fetch Id for newly added category type '{type}'");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching CategoryType ID: {ex.Message}");
                throw;  // Rethrow to ensure proper error handling in the calling method
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
        //private void Add(Category cat)
        //{
        //    _Cats.Add(cat);
        //}

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
        public void Add(string description, Category.CategoryType type)
        {
            try
            {
                // Get the TypeId for the given category type
                int typeId = GetCategoryTypeId(type);

                // Insert a new category into the database
                string query = "INSERT INTO categories (Description, TypeId) VALUES (@desc, @typeId)";

                using (SQLiteCommand cmd = new SQLiteCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@desc", description); ///////////////////
                    cmd.Parameters.AddWithValue("@typeId", typeId);

                    cmd.ExecuteNonQuery();  // Execute the query to insert the new category
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding category: " + ex.Message);
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
        public void Delete(int id)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM categories WHERE Id = @id;";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
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
        public List<Category> List()
        {
            List<Category> categoriesList = new List<Category>();

            try
            {
                // Ensure the connection is open
                if (_connection.State != System.Data.ConnectionState.Open)
                {
                    _connection.Open();
                }

                // Query to fetch categories along with type descriptions
                string query = @"
                    SELECT c.Id, c.Description, ct.Description AS Type 
                    FROM categories c
                    JOIN categoryTypes ct ON c.TypeId = ct.Id";

                // Use 'using' for SQLiteCommand and SQLiteDataReader to ensure proper disposal
                using (SQLiteCommand cmd = new SQLiteCommand(query, _connection))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    // Check if there are any rows returned
                    while (reader.Read())
                    {
                        try
                        {
                            // Read and map the data
                            int id = reader.GetInt32(0);
                            string description = reader.GetString(1);
                            string typeString = reader.GetString(2);

                            // Parse the CategoryType enum
                            if (Enum.TryParse(typeString, out Category.CategoryType type))
                            {
                                // Create category and add to list
                                categoriesList.Add(new Category(id, description, type));
                            }
                            else
                            {
                                Console.WriteLine($"Warning: Invalid CategoryType '{typeString}' for category ID {id}. Skipping...");
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log any unexpected errors for each row
                            Console.WriteLine($"Error processing row: {ex.Message}");
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Database error while retrieving categories: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error: " + ex.Message);
            }

            return categoriesList;
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
                    //this.Add(new Category(int.Parse(id), desc, type));
                    this.Add(desc, type);
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
            string updateQuery = "UPDATE categories SET Description = @desc WHERE Id = @id";

            try
            {
                // Step 1: Check if category exists before updating
                using (SQLiteCommand checkCmd = new SQLiteCommand(_connection))
                {
                    checkCmd.Parameters.AddWithValue("@id", id);
                    string checkQuery = "SELECT COUNT(*) FROM categories WHERE Id = @id";
                    checkCmd.CommandText = checkQuery;
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count == 0)
                    {
                        Console.WriteLine($"No category found with Id {id}. Update aborted.");
                        return;
                    }
                }

                // Step 2: Execute the update if the category exists
                SQLiteCommand cmd = new SQLiteCommand(updateQuery, _connection);

                cmd.Parameters.AddWithValue("@desc", newDescription);
                cmd.Parameters.AddWithValue("@type", newType);

                int updated = cmd.ExecuteNonQuery();

                if (updated == 0)
                {
                    Console.WriteLine($"Category with Id {id} exists but was not updated.");
                }
                else
                {
                    Console.WriteLine($"Category with Id {id} updated successfully.");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating category: " + ex.Message);
            }
        }




    }
}


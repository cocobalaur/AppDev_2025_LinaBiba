using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Reflection.PortableExecutable;
using System.Xml;
using static Budget.Category;

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
        //private static String DefaultFileName = "budgetCategories.txt";
        //private List<Category> _Cats = new List<Category>();
        //private string _FileName;
        //private string _DirName;

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
        //public String FileName { get { return _FileName; } }

        /// <summary>
        ///     Gets the directory name of the current categories file.
        /// </summary>
        /// <value>
        /// A string representing the directory where the current categories file is stored.
        /// </value>
        //public String DirName { get { return _DirName; } }

        public SQLiteConnection Connection { get { return _connection; } set { _connection = value; } }

        public bool UseDefaults { get { return _useDefaults; } set { _useDefaults = value; } }

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
            Connection = conn;
            UseDefaults = useDefaults;

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
            using var cmd = new SQLiteCommand(query, Connection);
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

                Category.CategoryType type = (Category.CategoryType)typeId;

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
        //public void ReadFromFile(String filepath = null)
        //{

        //    // ---------------------------------------------------------------
        //    // reading from file resets all the current categories,
        //    // ---------------------------------------------------------------
        //    _Cats.Clear();

        //    // ---------------------------------------------------------------
        //    // reset default dir/filename to null 
        //    // ... filepath may not be valid, 
        //    // ---------------------------------------------------------------
        //    _DirName = null;
        //    _FileName = null;

        //    // ---------------------------------------------------------------
        //    // get filepath name (throws exception if it doesn't exist)
        //    // ---------------------------------------------------------------
        //    filepath = BudgetFiles.VerifyReadFromFileName(filepath, DefaultFileName);

        //    // ---------------------------------------------------------------
        //    // If file exists, read it
        //    // ---------------------------------------------------------------
        //    _ReadXMLFile(filepath);
        //    _DirName = Path.GetDirectoryName(filepath);
        //    _FileName = Path.GetFileName(filepath);
        //}

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
        //public void SaveToFile(String filepath = null)
        //{
        //    // ---------------------------------------------------------------
        //    // if file path not specified, set to last read file
        //    // ---------------------------------------------------------------
        //    if (filepath == null && DirName != null && FileName != null)
        //    {
        //        filepath = DirName + "\\" + FileName;
        //    }

        //    // ---------------------------------------------------------------
        //    // just in case filepath doesn't exist, reset path info
        //    // ---------------------------------------------------------------
        //    _DirName = null;
        //    _FileName = null;

        //    // ---------------------------------------------------------------
        //    // get filepath name (throws exception if it doesn't exist)
        //    // ---------------------------------------------------------------
        //    filepath = BudgetFiles.VerifyWriteToFileName(filepath, DefaultFileName);

        //    // ---------------------------------------------------------------
        //    // save as XML
        //    // ---------------------------------------------------------------
        //    _WriteXMLFile(filepath);

        //    // ----------------------------------------------------------------
        //    // save filename info for later use
        //    // ----------------------------------------------------------------
        //    _DirName = Path.GetDirectoryName(filepath);
        //    _FileName = Path.GetFileName(filepath);
        //}

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

            foreach (CategoryType type in Enum.GetValues(typeof(CategoryType)))
            {
                InsertCategoryType(type);
            }

            // Insert default categories for each category type
            InsertIntoCategories("Income", CategoryType.Income);
            InsertIntoCategories("Utilities", CategoryType.Expense);
            InsertIntoCategories("Rent", CategoryType.Expense);
            InsertIntoCategories("Food", CategoryType.Expense);
            InsertIntoCategories("Entertainment", CategoryType.Expense);
            InsertIntoCategories("Education", CategoryType.Expense);
            InsertIntoCategories("Medical Expenses", CategoryType.Expense);
            InsertIntoCategories("Vacation", CategoryType.Expense);
            InsertIntoCategories("Credit Card", CategoryType.Credit);
            InsertIntoCategories("Clothes", CategoryType.Expense);
            InsertIntoCategories("Gifts", CategoryType.Expense);
            InsertIntoCategories("Insurance", CategoryType.Expense);
            InsertIntoCategories("Transportation", CategoryType.Expense);
            InsertIntoCategories("Eating Out",CategoryType.Expense);
            InsertIntoCategories("Savings", CategoryType.Savings);

        }

        private void InsertCategoryType(CategoryType categoryType)
        {
            try
            {
                int id = (int)categoryType; //Get categoryType enum 

                string description = categoryType.ToString(); //Get description

                string queryInsertCategory = "INSERT INTO categoryTypes (Id, Description) VALUES (@id, @desc);";

                using var insertCmd = new SQLiteCommand(queryInsertCategory, Connection);

                insertCmd.Parameters.AddWithValue("@id", id);
                insertCmd.Parameters.AddWithValue("@desc", description);
                insertCmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting category type: " + ex);
            }
        }

        private void InsertIntoCategories(string description, Category.CategoryType type)
        {
            int id = (int)type;
            string query = "INSERT INTO categories (Description, TypeId) VALUES (@desc, @typeId);";
            using var cmd = new SQLiteCommand(query, Connection);
            cmd.Parameters.AddWithValue("@desc", description);
            cmd.Parameters.AddWithValue("@typeId", id);
            cmd.ExecuteNonQuery();
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
        public void Add(string desc, Category.CategoryType type)
        {
            try
            {
                int typeId = (int)type; //explicitily convert enum to int for typeId
                string queryInsertNewCategory = "INSERT INTO categories (Description, TypeId) VALUES (@desc, @typeId)";

                using SQLiteCommand cmd = new SQLiteCommand(queryInsertNewCategory, Connection);

                // Add parameters to the query to prevent SQL injection
                cmd.Parameters.AddWithValue("@desc", desc);
                cmd.Parameters.AddWithValue("@typeId", typeId);
                cmd.ExecuteNonQuery();
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
            try
            {
                string query = "DELETE FROM categories WHERE Id = @id;";
                using var cmd = new SQLiteCommand(query, Connection);
                cmd.Parameters.AddWithValue("@id", id);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    Console.WriteLine($"Error: No category found with Id {id}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting category with Id {id}: {ex.Message}");
            }
        }

        public void DeleteAll()
        {
            try
            {
                string deleteQuery = "DELETE FROM categories;";
                using var deleteCmd = new SQLiteCommand(deleteQuery, Connection);
                deleteCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting all categories: {ex.Message}");
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
                using var cmd = new SQLiteCommand(query, Connection);
                using var reader = cmd.ExecuteReader();
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            int indexCategoryId = 0;
                            int indexDescription = 1;
                            int indexTypeId = 2;

                            int id = reader.GetInt32(indexCategoryId);
                            string description = reader.GetString(indexDescription);
                            int typeId = reader.GetInt32(indexTypeId);

                            Category.CategoryType type = (Category.CategoryType)typeId; // Convert int to enum because we want to view the category type as a string from the list not the number

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
            }

            return categoriesList;
        }



        // ====================================================================
        // read from an XML file and add categories to our categories list
        // ====================================================================
        //private void _ReadXMLFile(String filepath)
        //{

        //    // ---------------------------------------------------------------
        //    // read the categories from the xml file, and add to this instance
        //    // ---------------------------------------------------------------
        //    try
        //    {
        //        XmlDocument doc = new XmlDocument();
        //        doc.Load(filepath);

        //        foreach (XmlNode category in doc.DocumentElement.ChildNodes)
        //        {
        //            String id = (((XmlElement)category).GetAttributeNode("ID")).InnerText;
        //            String typestring = (((XmlElement)category).GetAttributeNode("type")).InnerText;
        //            String desc = ((XmlElement)category).InnerText;

        //            Category.CategoryType type;
        //            switch (typestring.ToLower())
        //            {
        //                case "income":
        //                    type = Category.CategoryType.Income;
        //                    break;
        //                case "expense":
        //                    type = Category.CategoryType.Expense;
        //                    break;
        //                case "credit":
        //                    type = Category.CategoryType.Credit;
        //                    break;
        //                default:
        //                    type = Category.CategoryType.Savings;
        //                    break;
        //            }
        //            this.Add(new Category(int.Parse(id), desc, type));
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("ReadXMLFile: Reading XML " + e.Message);
        //    }

        //}


        //// ====================================================================
        //// write all categories in our list to XML file
        //// ====================================================================
        //private void _WriteXMLFile(String filepath)
        //{
        //    try
        //    {
        //        // create top level element of categories
        //        XmlDocument doc = new XmlDocument();
        //        doc.LoadXml("<Categories></Categories>");

        //        // foreach Category, create an new xml element
        //        foreach (Category cat in _Cats)
        //        {
        //            XmlElement ele = doc.CreateElement("Category");
        //            XmlAttribute attr = doc.CreateAttribute("ID");
        //            attr.Value = cat.Id.ToString();
        //            ele.SetAttributeNode(attr);
        //            XmlAttribute type = doc.CreateAttribute("type");
        //            type.Value = cat.Type.ToString();
        //            ele.SetAttributeNode(type);

        //            XmlText text = doc.CreateTextNode(cat.Description);
        //            doc.DocumentElement.AppendChild(ele);
        //            doc.DocumentElement.LastChild.AppendChild(text);

        //        }

        //        // write the xml to FilePath
        //        doc.Save(filepath);

        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("_WriteXMLFile: Reading XML " + e.Message);
        //    }

        //}

        // ====================================================================
        // Method: UpdateCategory
        // Purpose: Allows API users to update a category's Description and Type 
        // based on the provided Id.
        // ====================================================================
        public void UpdateProperties(int id, string newDescription, Category.CategoryType newType)
        {
            try
            {
                // Get the typeId directly from the enum value 
                Category.CategoryType typeId = newType;

                string query = "UPDATE categories SET Description = @desc, TypeId = @typeId WHERE Id = @id";

                using SQLiteCommand cmd = new SQLiteCommand(query, Connection);
                cmd.Parameters.AddWithValue("@desc", newDescription);
                cmd.Parameters.AddWithValue("@typeId", typeId);
                cmd.Parameters.AddWithValue("@id", id); 

                int rowsAffected = cmd.ExecuteNonQuery();

                int nothindModified = 0;
                if (rowsAffected == nothindModified)
                {
                    throw new Exception($"Category with Id {id} not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating category: {ex.Message}");
            }
        }
    }
}


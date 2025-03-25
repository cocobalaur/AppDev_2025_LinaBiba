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
    //        - Use a database to retrive a list of categories
    // ====================================================================
    /// <summary>
    /// Represents a collection of budget categories with functionality to retrieve a list of categories and to manipulate that list.
    /// </summary>
    public class Categories
    {

        private SQLiteConnection _connection;
        private bool _useDefaults;

        // Properties
        // ====================================================================

        /// <summary>
        /// Gets and sets the SQLiteConnection instance used for database operations.
        /// </summary>
        public SQLiteConnection Connection { get { return _connection; } set { _connection = value; } }

        /// <summary>
        /// Gets and sets a boolean indicating whether a default categores list should be used.
        /// </summary>
        public bool UseDefaults { get { return _useDefaults; } set { _useDefaults = value; } }

        // ====================================================================
        // Constructor
        // ====================================================================
        /// <summary>
        /// Initializes a new instance of the Categories class with connection value and what categories list do we use.
        /// </summary>
        /// <param name="conn">The SQLite database connection.</param>
        /// <param name="useDefaults">The boolean value to determine if we use default categories or not/ the database is empty or not.
        /// If true it uses defaultCategories which is adding new categories to the empty database. If false is uses list and retrives a list of the categories
        /// that's in the database.</param>
        /// <example>
        /// To create a new `Categories` object with default categories:
        /// <code>
        /// Categories categories = new Categories(conn, True);
        /// </code>
        /// </example>

        public Categories(SQLiteConnection conn, bool useDefaults)
        {
            Connection = conn;
            UseDefaults = useDefaults;

            if (useDefaults)
            {

                // Insert category types into the database
                foreach (CategoryType type in Enum.GetValues(typeof(CategoryType)))
                {
                    InsertCategoryType(type);
                }

                SetCategoriesToDefaults(); // Insert default categories
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
        /// Retrieves a category by its ID using a query.
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

        // ===================================================================
        // INSERT DATA INTO DATABASE
        // ===================================================================

        /// <summary>
        /// Insert default categories into the database if the database.
        /// It deletes all previous information before inserting the default categories.
        /// </summary>
        public void SetCategoriesToDefaults()
        {
            DeleteAll();

            // Insert default categories for each category type
            InsertIntoCategories("Income", CategoryType.Income, 1);
            InsertIntoCategories("Utilities", CategoryType.Expense, 2);
            InsertIntoCategories("Rent", CategoryType.Expense, 3);
            InsertIntoCategories("Food", CategoryType.Expense, 4);
            InsertIntoCategories("Entertainment", CategoryType.Expense, 5);
            InsertIntoCategories("Education", CategoryType.Expense, 6);
            InsertIntoCategories("Medical Expenses", CategoryType.Expense, 7);
            InsertIntoCategories("Vacation", CategoryType.Expense, 8);
            InsertIntoCategories("Credit Card", CategoryType.Credit, 9);
            InsertIntoCategories("Clothes", CategoryType.Expense, 10);
            InsertIntoCategories("Gifts", CategoryType.Expense, 11);
            InsertIntoCategories("Insurance", CategoryType.Expense, 12);
            InsertIntoCategories("Transportation", CategoryType.Expense, 13);
            InsertIntoCategories("Eating Out", CategoryType.Expense, 14);
            InsertIntoCategories("Savings", CategoryType.Savings, 15);

        }

        /// <summary>
        /// Inserts a category type into the database using parameterized queries to prevent sql injecttion.
        /// </summary>
        /// <param name="categoryType">The category type to insert.</param>
        /// <exception cref="Exception">Thrown if invalid category type to insert.</exception>
        private void InsertCategoryType(CategoryType categoryType)
        {
            try
            {
                string description = categoryType.ToString(); //Get description

                string queryInsertCategory = "INSERT INTO categoryTypes (Description) VALUES (@desc);";

                using var insertCmd = new SQLiteCommand(queryInsertCategory, Connection);

                insertCmd.Parameters.AddWithValue("@desc", description);
                insertCmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting category type: " + ex);
            }
        }

        /// <summary>
        /// Inserts a new category with category name and category type into the database.
        /// </summary>
        /// <param name="description">The description of the category.</param>
        /// <param name="type">The category type.</param>
        /// <example>
        /// To insert default categories into the database:
        /// <code>
        /// InsertIntoCategories("Savings", CategoryType.Savings);
        /// </code>
        /// </example
        private void InsertIntoCategories(string description, Category.CategoryType type, int id)
        {
            //verify category types is in database
            string getIdQuery = "SELECT Id FROM categoryTypes WHERE Description = @desc;";
            int typeId;

            using (var getIdCmd = new SQLiteCommand(getIdQuery, Connection))
            {
                getIdCmd.Parameters.AddWithValue("@desc", type.ToString());

                //execute the query and get the id of the category type
                using var reader = getIdCmd.ExecuteReader();

                //check if any rows were returned
                if (!reader.Read())
                {
                    Console.WriteLine($"CategoryType " + type + " does not exist in categoryTypes table.");
                }

                //get the id
                typeId = reader.GetInt32(0);
            }

            string insertQuery = "INSERT INTO categories (Id, Description, TypeId) VALUES (@id, @desc, @typeId);";

            using var insertCmd = new SQLiteCommand(insertQuery, Connection);
            insertCmd.Parameters.AddWithValue("@id", id);  //manually add the id 
            insertCmd.Parameters.AddWithValue("@desc", description);
            insertCmd.Parameters.AddWithValue("@typeId", typeId);
            insertCmd.ExecuteNonQuery();
        }

        // ====================================================================
        // Add category
        // ====================================================================

        /// <summary>
        /// Adds a new category with a description and type.
        /// </summary>
        /// <param name="desc">The description of the category.</param>
        /// <param name="type">The type of the category (Income, Expense, Credit, Saving)</param>
        /// <exception cref="Exception">Thrown if invalid category type or category description to add.</exception>
        /// <exception cref="ArgumentException">Thrown if invalid category type enum or category description.</exception>
        /// <example>
        /// To add a new category:
        /// <code>
        /// string descr = "New Category";
        /// Category.CategoryType type = Category.CategoryType.Income;
        /// categories.Add(descr, type);
        /// </code>
        /// </example>
        public void Add(string desc, Category.CategoryType type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(desc))
                {
                    throw new ArgumentException("Category description cannot be empty.");
                }

                if (!Enum.IsDefined(typeof(Category.CategoryType), type)) //check if type is a valid enum, isDefined verifies if the value is defined in the enum
                {
                    throw new ArgumentException("Invalid category type.");
                }

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
        /// Deletes a category from the database by its id.
        /// </summary>
        /// <param name="id">The Id of the category to delete.</param>
        /// <exception cref="Exception">Thrown if invalid category id to delete.</exception>
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

        /// <summary>
        /// Deletes all categories from the database.
        /// </summary>
        /// <exception cref="Exception">Thrown if it can't delete the tables in the database.</exception>
        public void DeleteAll()
        {
            try
            {
                using var deleteExpenses = new SQLiteCommand("DELETE FROM expenses;", Connection);
                deleteExpenses.ExecuteNonQuery();
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
        /// Returns a copy of the list of categories from the database.
        /// </summary>
        /// <returns>A new list of categories.</returns>
        /// <exception cref="Exception">Thrown if there is no categories in the database to retrieve a list from.</exception>
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
        // Update Categories
        // ====================================================================
        /// <summary>
        /// Updates the description and type of a category in the database based on its Id.
        /// </summary>
        /// <param name="id">The Id of the category to update.</param>
        /// <param name="newDescription">The new description for the category.</param>
        /// <param name="newType">The new category type for the category (from CategoryType enum).</param>
        /// <exception cref="Exception">Thrown if cannot update the category.</exception>
        /// <exception cref="ArgumentException">Thrown if invalid category type enum or category description.</exception>
        /// <example>
        /// To update a category using an Id:
        /// <code>
        /// <![CDATA[
        /// String newDescr = "Presents";
        /// int id = 11;
        /// categories.UpdateProperties(id, newDescr, Category.CategoryType.Income);
        /// Category category = categories.GetCategoryFromId(id);
        /// ]]>
        /// </code>
        /// </example>
        public void UpdateProperties(int id, string newDescription, Category.CategoryType newType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newDescription))
                {
                    throw new ArgumentException("Category description cannot be empty.");
                }

                if (!Enum.IsDefined(typeof(Category.CategoryType), newType)) //check if type is a valid enum, isDefined verifies if the value is defined in the enum
                {
                    throw new ArgumentException("Invalid category type.");
                }

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


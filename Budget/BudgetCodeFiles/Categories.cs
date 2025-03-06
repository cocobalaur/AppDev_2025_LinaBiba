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

        // ====================================================================
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
        public Category GetCategoryFromId(int i)
        {
            Category c = _Cats.Find(x => x.Id == i);
            if (c == null)
            {
                throw new Exception("Cannot find category with id " + i.ToString());
            }
            return c;
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

        // ====================================================================
        // save to a file
        // if filepath is not specified, read/save in AppData file
        // ====================================================================
        /// <summary>
        /// Saves categories to a file.
        /// </summary>
        /// <param name="filepath">The path of the file to write to. If null, the last used file path is used.</param>
        /// <example>
        /// To save categories to a file:
        /// <code>
        /// Categories categories = new Categories();
        /// categories.SaveToFile("categories.txt");
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
        public void SetCategoriesToDefaults()
        {
            // ---------------------------------------------------------------
            // reset any current categories,
            // ---------------------------------------------------------------
            _Cats.Clear();

            // ---------------------------------------------------------------
            // Add Defaults
            // ---------------------------------------------------------------
            Add("Utilities", Category.CategoryType.Expense);
            Add("Rent", Category.CategoryType.Expense);
            Add("Food", Category.CategoryType.Expense);
            Add("Entertainment", Category.CategoryType.Expense);
            Add("Education", Category.CategoryType.Expense);
            Add("Miscellaneous", Category.CategoryType.Expense);
            Add("Medical Expenses", Category.CategoryType.Expense);
            Add("Vacation", Category.CategoryType.Expense);
            Add("Credit Card", Category.CategoryType.Credit);
            Add("Clothes", Category.CategoryType.Expense);
            Add("Gifts", Category.CategoryType.Expense);
            Add("Insurance", Category.CategoryType.Expense);
            Add("Transportation", Category.CategoryType.Expense);
            Add("Eating Out", Category.CategoryType.Expense);
            Add("Savings", Category.CategoryType.Savings);
            Add("Income", Category.CategoryType.Income);

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
        public void Add(String desc, Category.CategoryType type)
        {
            int new_num = 1;
            if (_Cats.Count > 0)
            {
                new_num = (from c in _Cats select c.Id).Max();
                new_num++;
            }
            _Cats.Add(new Category(new_num, desc, type));
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
        public void Delete(int Id)
        {
            int i = _Cats.FindIndex(x => x.Id == Id);
            if (i < 0)
                return;
            _Cats.RemoveAt(i);
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
            List<Category> newList = new List<Category>();
            foreach (Category category in _Cats)
            {
                newList.Add(new Category(category));
            }
            return newList;
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


        // ====================================================================
        // write all categories in our list to XML file
        // ====================================================================
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

    }
}


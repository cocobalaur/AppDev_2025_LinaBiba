using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Threading;

// ===================================================================
// Very important notes:
// ... To keep everything working smoothly, you should always
//     dispose of EVERY SQLiteCommand even if you recycle a 
//     SQLiteCommand variable later on.
//     EXAMPLE:
//            Database.newDatabase(GetSolutionDir() + "\\" + filename);
//            var cmd = new SQLiteCommand(Database.dbConnection);
//            cmd.CommandText = "INSERT INTO categoryTypes(Description) VALUES('Whatever')";
//            cmd.ExecuteNonQuery();
//            cmd.Dispose();
//
// ... also dispose of reader objects
//
// ... by default, SQLite does not impose Foreign Key Restraints
//     so to add these constraints, connect to SQLite something like this:
//            string cs = $"Data Source=abc.sqlite; Foreign Keys=1";
//            var con = new SQLiteConnection(cs);
//
// ===================================================================


namespace Budget
{
    public class Database
    {

        public static SQLiteConnection dbConnection { get { return _connection; } }
        private static SQLiteConnection _connection;

        // ===================================================================
        // create and open a new database
        // ===================================================================
        public static void newDatabase(string filename)
        {

            // If there was a database open before, close it and release file
            CloseDatabaseAndReleaseFile();
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), filename); //using GetCurrentDirectory() instead of using \\..\\..\\, source: https://stackoverflow.com/questions/40994534/get-relative-path-of-a-file-c-sharp

            try
            {
                // Define the database file path, fixed to use foreign keys
                string databasePath = $"Data Source={filePath}; Foreign Keys=1;";

                // Create and open the new database connection
                _connection = new SQLiteConnection(databasePath);
                _connection.Open();

                CreateTables();

            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create and open the new Database...", ex);
            }
        }

        // ===================================================================
        // open an existing database
        // ===================================================================
        public static void existingDatabase(string filename)
        {
            // Build the full path to the database file
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), filename);
            Console.WriteLine($"Attempting to open database from: {filePath}");  // Debugging log to see the full path

            // Check if the file exists at the specified path
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified database file does not exist.", filename);
            }

            try
            {
                // Define the database connection string
                string databasePath = $"Data Source={filePath}; Foreign Keys=1;";

                // Log the connection string (optional, remove in production)
                Console.WriteLine($"Database connection string: {databasePath}");

                // Create and open the database connection
                _connection = new SQLiteConnection(databasePath);
                _connection.Open();

                // Log successful connection
                Console.WriteLine("Database connection successfully opened.");
            }
            catch (Exception ex)
            {
                // If there's an issue opening the database, log the error
                Console.WriteLine($"Error opening database: {ex.Message}");
                throw new Exception("Failed to create and open the existing database...", ex);
            }
            //CloseDatabaseAndReleaseFile();

            //string filePath = Path.Combine(Directory.GetCurrentDirectory(), filename); //get the file path with GetCurrentDirectory

            //if (!File.Exists(filePath))
            //{
            //    throw new FileNotFoundException("The specified database file does not exist.", filename);
            //}

            //try
            //{
            //    // Define the database connection string with the URI prefix, fixed to use foreign keys
            //    string databasePath = $"Data Source={filePath}; Foreign Keys=1;";

            //    // Create and open the existing database connection
            //    _connection = new SQLiteConnection(databasePath);
            //    _connection.Open();

            //}
            //catch (Exception ex)
            //{
            //    throw new Exception("Failed to create and open the existing database...", ex);
            //}
        }

        // ===================================================================
        // close existing database, wait for garbage collector to
        // release the lock before continuing
        // ===================================================================
        static public void CloseDatabaseAndReleaseFile()
        {
            if (Database.dbConnection != null)
            {
                // close the database connection
                Database.dbConnection.Close();


                // wait for the garbage collector to remove the
                // lock from the database file
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

        }

        private static void CreateTables()
        {
            DropTables();

            using var cmd = new SQLiteCommand(_connection);

            //categoryTypes
            cmd.CommandText = @"
                CREATE TABLE categoryTypes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                    Description TEXT NOT NULL
                );";
            cmd.ExecuteNonQuery();

            //categories
            cmd.CommandText = @"
                CREATE TABLE categories (
                    Id INTEGER PRIMARY KEY NOT NULL,
                    Description TEXT NOT NULL,
                    TypeId INTEGER NOT NULL,
                    FOREIGN KEY (TypeId) REFERENCES categoryTypes(Id)
                );";
            cmd.ExecuteNonQuery();

            //expenses
            cmd.CommandText = @"
                CREATE TABLE expenses (
                    Id INTEGER PRIMARY KEY NOT NULL,
                    Date TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    Amount DOUBLE NOT NULL,
                    CategoryId INTEGER NOT NULL,
                    FOREIGN KEY (CategoryId) REFERENCES categories(Id) 
                );";
            cmd.ExecuteNonQuery();

        }

        private static void DropTables()
        {
            using var cmd = new SQLiteCommand(_connection);
            cmd.CommandText = @"
                DROP TABLE IF EXISTS expenses;
                DROP TABLE IF EXISTS categories;
                DROP TABLE IF EXISTS categoryTypes;";
            cmd.ExecuteNonQuery();
        }
    }
}

using System;
using Xunit;
using System.IO;
using System.Collections.Generic;
using Budget;
using System.Data.SQLite;

namespace BudgetCodeTests
{
    [Collection("Sequential")]
    public class TestExpenses
    {
        int numberOfExpensesInFile = TestConstants.numberOfExpensesInFile;
        String testInputFile = TestConstants.testExpensesInputFile;
        int maxIDInExpenseFile = TestConstants.maxIDInExpenseFile;
        Expense firstExpenseInFile = new Expense(1, new DateTime(2021, 1, 10), 10, 12, "hat (on credit)");


        // ========================================================================

        [Fact]
        public void ExpensesObject_New()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\newDB.db";
            Database.newDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;

            // Act
            Categories categories = new Categories(conn, true);
            Expenses expenses = new Expenses(conn, true);

            // Assert 
            Assert.IsType<Expenses>(expenses);

        }

        // ========================================================================

        [Fact]
        public void ExpensesMethod_ReadFromDatabase_ValidateCorrectDataWasRead()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String existingDB = $"{folder}\\{TestConstants.testDBInputFileExpenses}";
            Database.existingDatabase(existingDB);
            SQLiteConnection conn = Database.dbConnection;

            // Act
            Expenses expenses = new Expenses(conn, false);
            List<Expense> list = expenses.List();
            Expense firstExpense = list[0];

            // Assert
            Assert.Equal(numberOfExpensesInFile, list.Count);
            Assert.Equal(firstExpenseInFile.Id, firstExpense.Id);
            Assert.Equal(firstExpenseInFile.Amount, firstExpense.Amount);
            Assert.Equal(firstExpenseInFile.Description, firstExpense.Description);
            Assert.Equal(firstExpenseInFile.Category, firstExpense.Category);
        }

        // ========================================================================

        [Fact]
        public void ExpensesMethod_List_ReturnsListOfExpenses()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\{TestConstants.testDBInputFileExpenses}";
            Database.existingDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses(conn, false);

            // Act
            List<Expense> list = expenses.List();

            // Assert
            Assert.Equal(numberOfExpensesInFile, list.Count);

        }

        // ========================================================================

        [Fact]
        public void ExpensesMethod_List_DatabaseClose()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\{TestConstants.testDBInputFileExpenses}";
            Database.existingDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses(conn, false);

            // Act
            conn.Close();
            List<Expense> list = expenses.List();

            // Assert
            Assert.Empty(list);

        }
        // ========================================================================

        [Fact]
        public void ExpensesMethod_List_WhenDataIsCorrupt()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String corruptDB = $"{folder}\\corruptDB.db";
            Database.newDatabase(corruptDB);
            SQLiteConnection conn = Database.dbConnection;
            Categories categories = new Categories(conn, true);
            Expenses expenses = new Expenses(conn, false);

            // Manually corrupt the database by inserting a non-numeric value in the Amount column
            using (var command = new SQLiteCommand("INSERT INTO Expenses (Id, Date, Amount, Description, CategoryId) VALUES (1, '2025-01-01', 'INVALID_AMOUNT', 'Corrupt Data', 1)", conn))
            {
                command.ExecuteNonQuery();
            }

            // Act & Assert
            List<Expense> list = expenses.List();
            Assert.Empty(list);

        }

        // ========================================================================

        [Fact]
        //public void ExpensesMethod_Add()
        //{
        //    // Arrange
        //    String dir = TestConstants.GetSolutionDir();
        //    String goodDB = $"{dir}\\{TestConstants.testDBInputFileExpenses}";
        //    String messyDB = $"{dir}\\messy.db";
        //    System.IO.File.Copy(goodDB, messyDB, true);
        //    SQLiteConnection conn = new SQLiteConnection($"Data Source={messyDB};Version=3;");
        //    conn.Open();
        //    Expenses expenses = new Expenses(conn, false);

        //    int category = 57;
        //    double amount = 98.1;

        //    // Act
        //    expenses.Add(DateTime.Now, amount, "new expense", category);
        //    List<Expense> expensesList = expenses.List();
        //    int sizeOfList = expenses.List().Count;

        //    // Assert
        //    Assert.Equal(numberOfExpensesInFile + 1, sizeOfList);
        //    Assert.Equal(maxIDInExpenseFile + 1, expensesList[sizeOfList - 1].Id);
        //    Assert.Equal(amount, expensesList[sizeOfList - 1].Amount);
        //}
        public void ExpensesMethod_Add()
        {
            // Arrange
            String dir = TestConstants.GetSolutionDir();
            String goodDB = $"{dir}\\{TestConstants.testDBInputFileExpenses}";
            String messyDB = $"{dir}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            SQLiteConnection conn = new SQLiteConnection($"Data Source={messyDB};Version=3;");
            conn.Open();
            Expenses expenses = new Expenses(conn, false);
            List<Expense> initalExpenses = expenses.List();

            int category = 57;
            double amount = 98.1;

            // Act
            expenses.Add(DateTime.Now, amount, "new expense", category);
            List<Expense> expensesList = expenses.List();
            int sizeOfList = expenses.List().Count;

            // Assert
            Assert.Equal(numberOfExpensesInFile + 1, sizeOfList);
            Assert.Equal(maxIDInExpenseFile + 1, expensesList[sizeOfList - 1].Id);
            Assert.Equal(amount, expensesList[sizeOfList - 1].Amount);
        }

        // ========================================================================
        [Fact]
        public void ExpensesMethod_Add_NonExistentCategoryId()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\newDB.db";
            Database.newDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses(conn, false);

            int invalidCategoryId = -99;
            double amount = 98.1;

            // Act
            List<Expense> expensesList = expenses.List();
            int initialCount = expenses.List().Count;
            expenses.Add(DateTime.Now, amount, "expense with category id", invalidCategoryId);
            int finalCount = expenses.List().Count;

            // Assert
            Assert.Equal(finalCount, initialCount);

        }
        // ========================================================================

        [Fact]
        public void ExpensesMethod_Delete()
        {
            // Arrange
            String dir = TestConstants.GetSolutionDir();
            String goodDB = $"{dir}\\{TestConstants.testDBInputFileExpenses}";
            String messyDB = $"{dir}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses(conn, false);
            int IdToDelete = 3;

            // Act
            expenses.Delete(IdToDelete);
            List<Expense> expensesList = expenses.List();
            int sizeOfList = expensesList.Count;

            // Assert
            Assert.Equal(numberOfExpensesInFile - 1, sizeOfList);
            Assert.False(expensesList.Exists(e => e.Id == IdToDelete), "correct expense item deleted");

        }

        // ========================================================================

        [Fact]
        public void ExpensesMethod_Delete_InvalidIDDoesntCrash()
        {
            // Arrange
            String dir = TestConstants.GetSolutionDir();
            String goodDB = $"{dir}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{dir}\\messyDB";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses(conn, false);

            int IdToDelete = 1006;
            int sizeOfList = expenses.List().Count;

            // Act
            try
            {
                expenses.Delete(IdToDelete);
                Assert.Equal(sizeOfList, expenses.List().Count);
            }

            // Assert
            catch
            {
                Assert.True(false, "Invalid ID causes Delete to break");
            }
        }

        // ========================================================================

        [Fact]
        public void ExpensesMethod_DeleteAll()
        {
            // Arrange
            String dir = TestConstants.GetSolutionDir();
            String goodDB = $"{dir}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{dir}\\messyDB";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses(conn, false);

            int startSize = expenses.List().Count;

            // Act
            expenses.DeleteAll();
            int endSize = expenses.List().Count;

            // Assert
            Assert.NotEqual(startSize, endSize);
            Assert.Equal(0, endSize);
        }

        // ========================================================================

        [Fact]
        public void ExpensesMethod_GetExpenseFromId()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\{TestConstants.testDBInputFileExpenses}";
            Database.existingDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses(conn, false);
            int expID = 2;

            // Act
            Expense expense = expenses.GetExpenseFromId(expID);

            // Assert
            Assert.Equal(expID, expense.Id);

        }
        // ========================================================================

        [Fact]
        public void ExpensesMethod_GetExpenseFromId_InexistantID()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\{TestConstants.testDBInputFileExpenses}";
            Database.existingDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses(conn, false);
            int expID = -1;

            // Act
            Expense expense = expenses.GetExpenseFromId(expID);


            // Assert
            Assert.Null(expense);
            
        }
        // ========================================================================

        [Fact]
        public void ExpensesMethod_GetExpenseFromId_DatabaseClose()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\{TestConstants.testDBInputFileExpenses}";
            Database.existingDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;
            Expenses expenses = new Expenses(conn, false);
            int expID = 1;

            // Act
            conn.Close();
            Expense expense = expenses.GetExpenseFromId(expID);


            // Assert
            Assert.Equal(expID, expense.Id);

        }

        // ========================================================================


        [Fact]
        public void ExpensesMethod_SetExpensesToDefaults()
        {

            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\newDB.db";
            Database.newDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;

            // Act
            Categories categories = new Categories(conn, true);
            Expenses expenses = new Expenses(conn, true);
            List<Expense> originalList = expenses.List();

            // modify list of categories
            expenses.Delete(1);
            expenses.Delete(2);
            expenses.Delete(3);

            double amount = 12.0;
            string description = "shirt (on credit)";
            int categoriesId = 1;
            expenses.Add(new DateTime(2023, 1, 10), amount, description, categoriesId);

            //"just double check that initial conditions are correct");
            Assert.NotEqual(originalList.Count, expenses.List().Count);

            // Act
            expenses.SetExpensesToDefaults();

            // Assert
            Assert.Equal(originalList.Count, expenses.List().Count);
            foreach (Expense defaultExp in originalList)
            {
                Assert.True(expenses.List().Exists(c => c.Description == defaultExp.Description && c.Date == defaultExp.Date && c.Amount == defaultExp.Amount && c.Id == defaultExp.Id));
            }

        }

        // ========================================================================

        [Fact]
        public void ExpensesMethod_UpdateExpense()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\newDB.db";
            Database.newDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;
            Categories categories = new Categories(conn, true);
            Expenses expenses = new Expenses(conn, true);

            String newDescr = "Presents";
            int id = 3;
            DateTime date = new DateTime(2025, 1, 10);
            double amount = 20.0;
            int categoriesId = 1;


            // Act
            expenses.UpdateExpenses(id, date, amount,newDescr, categoriesId); //id must not be updated
            Expense expense = expenses.GetExpenseFromId(id);

            // Assert 
            Assert.Equal(newDescr, expense.Description);
            Assert.Equal(id, expense.Id);
            Assert.Equal(date, expense.Date);
            Assert.Equal(amount, expense.Amount);
            Assert.Equal(categoriesId, expense.Category);
        }
        // ========================================================================

        [Fact]
        public void ExpensesMethod_UpdateExpense_InvalidCategoryId()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\newDB.db";
            Database.newDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;
            Categories categories = new Categories(conn, true);
            Expenses expenses = new Expenses(conn, true);

            String newDescr = "Presents";
            int id = 3;
            DateTime date = new DateTime(2025, 1, 10);
            double amount = 20.0;
            int categoriesId = -1;


            // Act
            Expense oldExpense = expenses.GetExpenseFromId(id);
            expenses.UpdateExpenses(id, date, amount, newDescr, categoriesId); 
            Expense expense = expenses.GetExpenseFromId(id);

            // Assert 
            Assert.Equal(oldExpense.Description, expense.Description);
            Assert.Equal(oldExpense.Id, expense.Id);
            Assert.Equal(oldExpense.Date, expense.Date);
            Assert.Equal(oldExpense.Amount, expense.Amount);
            Assert.Equal(oldExpense.Category, expense.Category);
        }
        // ========================================================================

        [Fact]
        public void ExpensesMethod_UpdateExpense_InexistantExpense()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\newDB.db";
            Database.newDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;
            Categories categories = new Categories(conn, true);
            Expenses expenses = new Expenses(conn, true);

            String newDescr = "Presents";
            int id = 99;
            DateTime date = new DateTime(2025, 1, 10);
            double amount = 20.0;
            int categoriesId = 1;


            // Act
            expenses.UpdateExpenses(id, date, amount, newDescr, categoriesId);
            Expense expense = expenses.GetExpenseFromId(id);

            // Assert 
            Assert.Null(expense);
        }
        
        // ========================================================================



        // -------------------------------------------------------
        // helpful functions, ... they are not tests
        // -------------------------------------------------------

        // source taken from: https://www.dotnetperls.com/file-equals

        private bool FileEquals(string path1, string path2)
        {
            byte[] file1 = File.ReadAllBytes(path1);
            byte[] file2 = File.ReadAllBytes(path2);
            if (file1.Length == file2.Length)
            {
                for (int i = 0; i < file1.Length; i++)
                {
                    if (file1[i] != file2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}

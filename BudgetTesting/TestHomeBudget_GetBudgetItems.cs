using System;
using Xunit;
using System.IO;
using System.Collections.Generic;
using Budget;

namespace BudgetCodeTests
{
    public class TestHomeBudget_GetBudgetItems
    {
        string testInputFile = TestConstants.testBudgetFile;
        

        // ========================================================================
        // Get Expenses Method tests
        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItems_NoStartEnd_NoFilter()
        {
            // Arrange
            string inFile = GetSolutionDir() + "\\" + testInputFile;
            HomeBudget homeBudget = new HomeBudget(inFile);
            List<Expense> listExpenses = homeBudget.expenses.List();
            List<Category> listCategories = homeBudget.categories.List();

            // Act
            List<BudgetItem> budgetItems =  homeBudget.GetBudgetItems(null,null,false,9);

            // Assert
            Assert.Equal(listExpenses.Count, budgetItems.Count);
            foreach (Expense expense in listExpenses)
            {
                BudgetItem budgetItem = budgetItems.Find(b => b.ExpenseID == expense.Id);
                Category category = listCategories.Find(c => c.Id == expense.Category);
                Assert.Equal(budgetItem.Category, category.Description);
                Assert.Equal(budgetItem.CategoryID, expense.Category);
                Assert.Equal(budgetItem.Amount, 0 - expense.Amount);
                Assert.Equal(budgetItem.ShortDescription, expense.Description);
            }
       }

        [Fact]
        public void HomeBudgetMethod_GetBudgetItems_NoStartEnd_NoFilter_VerifyBalanceProperty()
        {
            // Arrange
            string inFile = GetSolutionDir() + "\\" + testInputFile;
            HomeBudget homeBudget = new HomeBudget(inFile);

            // Act
            List<BudgetItem> budgetItems = homeBudget.GetBudgetItems(null, null, false, 9);

            // Assert
            double balance = 0;
            foreach (BudgetItem budgetItem in budgetItems)
            {
                balance = balance + budgetItem.Amount;
                Assert.Equal(balance, budgetItem.Balance);
            }

        }

        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItems_NoStartEnd_FilterbyCategory()
        {
            // Arrange
            string inFile = GetSolutionDir() + "\\" + testInputFile;
            HomeBudget homeBudget = new HomeBudget(inFile);
            int filterCategory = 9;
            List<Expense> listExpenses = TestConstants.filteredbyCat9();
            List<Category> listCategories = homeBudget.categories.List();

            // Act
            List<BudgetItem> budgetItems = homeBudget.GetBudgetItems(null, null, true, filterCategory);

            // Assert
            Assert.Equal(listExpenses.Count, budgetItems.Count);
            foreach (Expense expense in listExpenses)
            {
                BudgetItem budgetItem = budgetItems.Find(b => b.ExpenseID == expense.Id);
                Category category = listCategories.Find(c => c.Id == expense.Category);
                Assert.Equal(budgetItem.Category, category.Description);
                Assert.Equal(budgetItem.CategoryID, expense.Category);
                Assert.Equal(budgetItem.Amount, 0 - expense.Amount);
                Assert.Equal(budgetItem.ShortDescription, expense.Description);
            }
        }

        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItems_2018_filterDate()
        {
            // Arrange
            string inFile = GetSolutionDir() + "\\" + testInputFile;
            HomeBudget homeBudget = new HomeBudget(inFile);
            List<Expense> listExpenses = TestConstants.filteredbyYear2018();
            List<Category> listCategories = homeBudget.categories.List();

            // Act
            List<BudgetItem> budgetItems = homeBudget.GetBudgetItems(new DateTime(2018, 1, 1), new DateTime(2018, 12, 31), false, 0);

            // Assert
            Assert.Equal(listExpenses.Count, budgetItems.Count);
            foreach (Expense expense in listExpenses)
            {
                BudgetItem budgetItem = budgetItems.Find(b => b.ExpenseID == expense.Id);
                Category category = listCategories.Find(c => c.Id == expense.Category);
                Assert.Equal(budgetItem.Category, category.Description);
                Assert.Equal(budgetItem.CategoryID, expense.Category);
                Assert.Equal(budgetItem.Amount, 0 - expense.Amount);
                Assert.Equal(budgetItem.ShortDescription, expense.Description);
            }
        }

        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItems_2018_filterDate_verifyBalance()
        {
            // Arrange
            string inFile = GetSolutionDir() + "\\" + testInputFile;
            HomeBudget homeBudget = new HomeBudget(inFile);
            List<Expense> listExpenses = TestConstants.filteredbyCat9();
            List<Category> listCategories = homeBudget.categories.List();

            // Act
            List<BudgetItem> budgetItems = homeBudget.GetBudgetItems(null, null,  true, 9);
            double total = budgetItems[budgetItems.Count-1].Balance;
            

            // Assert
            Assert.Equal(0-TestConstants.filteredbyCat9Total, total);
        }

        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItems_2018_filterDateAndCat10()
        {
            // Arrange
            string inFile = GetSolutionDir() + "\\" + testInputFile;
            HomeBudget homeBudget = new HomeBudget(inFile);
            List<Expense> listExpenses = TestConstants.filteredbyYear2018AndCategory10();
            List<Category> listCategories = homeBudget.categories.List();

            // Act
            List<BudgetItem> budgetItems = homeBudget.GetBudgetItems(new DateTime(2018, 1, 1), new DateTime(2018, 12, 31), true, 10);

            // Assert
            Assert.Equal(listExpenses.Count, budgetItems.Count);
            foreach (Expense expense in listExpenses)
            {
                BudgetItem budgetItem = budgetItems.Find(b => b.ExpenseID == expense.Id);
                Category category = listCategories.Find(c => c.Id == expense.Category);
                Assert.Equal(budgetItem.Category, category.Description);
                Assert.Equal(budgetItem.CategoryID, expense.Category);
                Assert.Equal(budgetItem.Amount, 0 - expense.Amount);
                Assert.Equal(budgetItem.ShortDescription, expense.Description);
            }
        }




        // ========================================================================

        // -------------------------------------------------------
        // helpful functions, ... they are not tests
        // -------------------------------------------------------

        private String GetSolutionDir()
        {

            // this is valid for C# .Net Foundation (not for C# .Net Core)
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\"));
        }

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

        private bool FileSameSize(string path1, string path2)
        {
            byte[] file1 = File.ReadAllBytes(path1);
            byte[] file2 = File.ReadAllBytes(path2);
            return (file1.Length == file2.Length);
        }

    }
}


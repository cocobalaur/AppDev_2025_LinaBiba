using System;
using Xunit;
using System.IO;
using System.Collections.Generic;
using Budget;

namespace BudgetCodeTests
{
    [Collection("Sequential")]
    public class TestHomeBudget_GetBudgetItems
    {
        string testInputFile = TestConstants.testExpensesInputFile;
        

        // ========================================================================
        // Get Expenses Method tests
        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItems_NoStartEnd_NoFilter()
        {
            // Arrange
            string folder = TestConstants.GetSolutionDir();
            string inFile = TestConstants.GetSolutionDir() + "\\" + testInputFile;
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            HomeBudget homeBudget = new HomeBudget(messyDB,inFile,false);
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
                Assert.Equal(budgetItem.Amount, expense.Amount);
                Assert.Equal(budgetItem.ShortDescription, expense.Description);
            }
       }

        [Fact]
        public void HomeBudgetMethod_GetBudgetItems_NoStartEnd_NoFilter_VerifyBalanceProperty()
        {
            // Arrange
            string folder = TestConstants.GetSolutionDir();
            string inFile = TestConstants.GetSolutionDir() + "\\" + testInputFile;
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            HomeBudget homeBudget = new HomeBudget(messyDB, inFile, false);

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
            string folder = TestConstants.GetSolutionDir();
            string inFile = TestConstants.GetSolutionDir() + "\\" + testInputFile;
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            HomeBudget homeBudget = new HomeBudget(messyDB, inFile, false);
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
                Assert.Equal(budgetItem.Amount, expense.Amount);
                Assert.Equal(budgetItem.ShortDescription, expense.Description);
            }
        }

        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItems_2018_filterDate()
        {
            // Arrange
            string folder = TestConstants.GetSolutionDir();
            string inFile = TestConstants.GetSolutionDir() + "\\" + testInputFile;
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            HomeBudget homeBudget = new HomeBudget(messyDB, inFile, false);
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
                Assert.Equal(budgetItem.Amount, expense.Amount);
                Assert.Equal(budgetItem.ShortDescription, expense.Description);
            }
        }

        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItems_2018_filterDate_verifyBalance()
        {
            // Arrange
            string folder = TestConstants.GetSolutionDir();
            string inFile = TestConstants.GetSolutionDir() + "\\" + testInputFile;
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            HomeBudget homeBudget = new HomeBudget(messyDB, inFile, false);
            List<Expense> listExpenses = TestConstants.filteredbyCat9();
            List<Category> listCategories = homeBudget.categories.List();

            // Act
            List<BudgetItem> budgetItems = homeBudget.GetBudgetItems(null, null,  true, 9);
            double total = budgetItems[budgetItems.Count-1].Balance;
            

            // Assert
            Assert.Equal(TestConstants.filteredbyCat9Total, total);
        }

        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItems_2018_filterDateAndCat10()
        {
            // Arrange
            string folder = TestConstants.GetSolutionDir();
            string inFile = TestConstants.GetSolutionDir() + "\\" + testInputFile;
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            HomeBudget homeBudget = new HomeBudget(messyDB, inFile, false);
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
                Assert.Equal(budgetItem.Amount, expense.Amount);
                Assert.Equal(budgetItem.ShortDescription, expense.Description);
            }
        }
    }
}


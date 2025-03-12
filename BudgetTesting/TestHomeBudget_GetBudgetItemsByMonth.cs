using System;
using Xunit;
using System.IO;
using System.Collections.Generic;
using Budget;

namespace BudgetCodeTests
{
    [Collection("Sequential")]
    public class TestHomeBudget_GetBudgetItemsByMonth
    {
        string testInputFile = TestConstants.testExpensesInputFile;
        

        // ========================================================================
        // Get Expenses By Month Method tests
        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItemsByMonth_NoStartEnd_NoFilter()
        {
            // Arrange
            string folder = TestConstants.GetSolutionDir();
            string inFile = TestConstants.GetSolutionDir() + "\\" + testInputFile;
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            HomeBudget homeBudget = new HomeBudget(messyDB, inFile, false);
            int maxRecords = TestConstants.budgetItemsByMonth_MaxRecords;
            BudgetItemsByMonth firstRecord = TestConstants.budgetItemsByMonth_FirstRecord;

            // Act
            List<BudgetItemsByMonth> budgetItemsByMonth = homeBudget.GetBudgetItemsByMonth(null, null, false, 9);
            BudgetItemsByMonth firstRecordTest = budgetItemsByMonth[0];

            // Assert
            Assert.Equal(maxRecords, budgetItemsByMonth.Count);

            // verify 1st record
            Assert.Equal(firstRecord.Month, firstRecordTest.Month);
            Assert.Equal(firstRecord.Total, firstRecordTest.Total);
            Assert.Equal(firstRecord.Details.Count, firstRecordTest.Details.Count);
            for (int record = 0; record < firstRecord.Details.Count; record++)
            {
                BudgetItem validItem = firstRecord.Details[record];
                BudgetItem testItem = firstRecordTest.Details[record];
                Assert.Equal(validItem.Amount, testItem.Amount);
                Assert.Equal(validItem.CategoryID, testItem.CategoryID);
                Assert.Equal(validItem.ExpenseID, testItem.ExpenseID);

            }
        }

        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItemsByMonth_NoStartEnd_FilterbyCategory()
        {
            // Arrange
            string folder = TestConstants.GetSolutionDir();
            string inFile = TestConstants.GetSolutionDir() + "\\" + testInputFile;
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            HomeBudget homeBudget = new HomeBudget(messyDB, inFile, false);
            int maxRecords = TestConstants.budgetItemsByMonth_FilteredByCat9_number;
            BudgetItemsByMonth firstRecord = TestConstants.budgetItemsByMonth_FirstRecord_FilteredCat9;

            // Act
            List<BudgetItemsByMonth> budgetItemsByMonth = homeBudget.GetBudgetItemsByMonth(null, null, true, 9);
            BudgetItemsByMonth firstRecordTest = budgetItemsByMonth[0];

            // Assert
            Assert.Equal(maxRecords, budgetItemsByMonth.Count);

            // verify 1st record
            Assert.Equal(firstRecord.Month, firstRecordTest.Month);
            Assert.Equal(firstRecord.Total, firstRecordTest.Total);
            Assert.Equal(firstRecord.Details.Count, firstRecordTest.Details.Count);
            for (int record = 0; record < firstRecord.Details.Count; record++)
            {
                BudgetItem validItem = firstRecord.Details[record];
                BudgetItem testItem = firstRecordTest.Details[record];
                Assert.Equal(validItem.Amount, testItem.Amount);
                Assert.Equal(validItem.CategoryID, testItem.CategoryID);
                Assert.Equal(validItem.ExpenseID, testItem.ExpenseID);

            }
        }
        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItemsByMonth_2018_filterDateAndCat9()
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
            List<BudgetItemsByMonth> validBudgetItemsByMonth = TestConstants.getBudgetItemsBy2018_01_filteredByCat9();
            BudgetItemsByMonth firstRecord = TestConstants.budgetItemsByMonth_FirstRecord_FilteredCat9;

            // Act
            List<BudgetItemsByMonth> budgetItemsByMonth = homeBudget.GetBudgetItemsByMonth(new DateTime(2018, 1, 1), new DateTime(2018, 12, 31), true, 9);
            BudgetItemsByMonth firstRecordTest = budgetItemsByMonth[0];

            // Assert
            Assert.Equal(validBudgetItemsByMonth.Count, budgetItemsByMonth.Count);

            // verify 1st record
            Assert.Equal(firstRecord.Month, firstRecordTest.Month);
            Assert.Equal(firstRecord.Total, firstRecordTest.Total);
            Assert.Equal(firstRecord.Details.Count, firstRecordTest.Details.Count);
            for (int record = 0; record < firstRecord.Details.Count; record++)
            {
                BudgetItem validItem = firstRecord.Details[record];
                BudgetItem testItem = firstRecordTest.Details[record];
                Assert.Equal(validItem.Amount, testItem.Amount);
                Assert.Equal(validItem.CategoryID, testItem.CategoryID);
                Assert.Equal(validItem.ExpenseID, testItem.ExpenseID);

            }
        }

        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItemsByMonth_2018_filterDate()
        {
            // Arrange
            string folder = TestConstants.GetSolutionDir();
            string inFile = TestConstants.GetSolutionDir() + "\\" + testInputFile;
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            HomeBudget homeBudget = new HomeBudget(messyDB, inFile, false);

            List<BudgetItemsByMonth> validBudgetItemsByMonth = TestConstants.getBudgetItemsBy2018_01();
            BudgetItemsByMonth firstRecord = validBudgetItemsByMonth[0];


            // Act
            List<BudgetItemsByMonth> budgetItemsByMonth = homeBudget.GetBudgetItemsByMonth(new DateTime(2018, 1, 1), new DateTime(2018, 12, 31), false, 9);
            BudgetItemsByMonth firstRecordTest = budgetItemsByMonth[0];

            // Assert
            Assert.Equal(validBudgetItemsByMonth.Count, budgetItemsByMonth.Count);

            // verify 1st record
            Assert.Equal(firstRecord.Month, firstRecordTest.Month);
            Assert.Equal(firstRecord.Total, firstRecordTest.Total);
            Assert.Equal(firstRecord.Details.Count, firstRecordTest.Details.Count);
            for (int record = 0; record < firstRecord.Details.Count; record++)
            {
                BudgetItem validItem = firstRecord.Details[record];
                BudgetItem testItem = firstRecordTest.Details[record];
                Assert.Equal(validItem.Amount, testItem.Amount);
                Assert.Equal(validItem.CategoryID, testItem.CategoryID);
                Assert.Equal(validItem.ExpenseID, testItem.ExpenseID);

            }
        }
    }
}


using System;
using Xunit;
using System.IO;
using System.Collections.Generic;
using Budget;

namespace BudgetCodeTests
{
    public class TestHomeBudget_GetBudgetItemsByCategory
    {
        string testInputFile = TestConstants.testBudgetFile;

        // ========================================================================
        // Get Expenses By Month Method tests
        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItemsByCategory_NoStartEnd_NoFilter()
        {
            // Arrange
            string inFile = GetSolutionDir() + "\\" + testInputFile;
            HomeBudget homeBudget = new HomeBudget(inFile);
            int maxRecords = TestConstants.budgetItemsByCategory_MaxRecords; 
            BudgetItemsByCategory firstRecord = TestConstants.budgetItemsByCategory_FirstRecord;

            // Act
            List<BudgetItemsByCategory> budgetItemsByCategory = homeBudget.GetBudgetItemsByCategory(null, null, false, 9);
            BudgetItemsByCategory firstRecordTest = budgetItemsByCategory[0];

            // Assert
            Assert.Equal(maxRecords, budgetItemsByCategory.Count);

            // verify 1st record
            Assert.Equal(firstRecord.Category, firstRecordTest.Category);
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
        public void HomeBudgetMethod_GetBudgetItemsByCategory_NoStartEnd_FilterbyCategory()
        {
            // Arrange
            string inFile = GetSolutionDir() + "\\" + testInputFile;
            HomeBudget homeBudget = new HomeBudget(inFile);
            int maxRecords14 = TestConstants.budgetItemsByCategory14;
            int maxRecords20 = TestConstants.budgetItemsByCategory20;

            // Act
            List<BudgetItemsByMonth> budgetItemsByCategory = homeBudget.GetBudgetItemsByMonth(null, null, true, 14);

            // Assert
            Assert.Equal(maxRecords14, budgetItemsByCategory.Count);


            // Act
            budgetItemsByCategory = homeBudget.GetBudgetItemsByMonth(null, null, true, 20);

            // Assert
            Assert.Equal(maxRecords20, budgetItemsByCategory.Count);

        }
        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItemsByCategory_2018_filterDateAndCat9()
        {
            // Arrange
            string inFile = GetSolutionDir() + "\\" + testInputFile;
            HomeBudget homeBudget = new HomeBudget(inFile);
            List<BudgetItemsByCategory> validBudgetItemsByCategory = TestConstants.getBudgetItemsByCategory2018_Cat9();
            BudgetItemsByCategory firstRecord = validBudgetItemsByCategory[0];

            // Act
            List<BudgetItemsByCategory> budgetItemsByCategory = homeBudget.GetBudgetItemsByCategory(new DateTime(2018, 1, 1), new DateTime(2018, 12, 31), true, 9);
            BudgetItemsByCategory firstRecordTest = budgetItemsByCategory[0];

            // Assert
            Assert.Equal(validBudgetItemsByCategory.Count, budgetItemsByCategory.Count);

            // verify 1st record
            Assert.Equal(firstRecord.Category, firstRecordTest.Category);
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
        public void HomeBudgetMethod_GetBudgetItemsByCategory_2018_filterDate()
        {
            // Arrange
            string inFile = GetSolutionDir() + "\\" + testInputFile;
            HomeBudget homeBudget = new HomeBudget(inFile);
            List<BudgetItemsByCategory> validBudgetItemsByCategory = TestConstants.getBudgetItemsByCategory2018();
            BudgetItemsByCategory firstRecord = validBudgetItemsByCategory[0];


            // Act
            List<BudgetItemsByCategory> budgetItemsByCategory = homeBudget.GetBudgetItemsByCategory(new DateTime(2018, 1, 1), new DateTime(2018, 12, 31), false, 9);
            BudgetItemsByCategory firstRecordTest = budgetItemsByCategory[0];

            // Assert
            Assert.Equal(validBudgetItemsByCategory.Count, budgetItemsByCategory.Count);

            // verify 1st record
            Assert.Equal(firstRecord.Category, firstRecordTest.Category);
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


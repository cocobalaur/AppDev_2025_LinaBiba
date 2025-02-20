using System;
using Xunit;
using System.IO;
using System.Collections.Generic;
using Budget;
using System.Dynamic;

namespace BudgetCodeTests
{
    public class TestHomeBudget_GetBudgetDictionaryByCategoryAndMonth
    {
        string testInputFile = TestConstants.testBudgetFile;



        // ========================================================================
        // Get Expenses By Month Method tests
        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetDictionaryByCategoryAndMonth_NoStartEnd_NoFilter_VerifyNumberOfRecords()
        {
            // Arrange
            string inFile = GetSolutionDir() + "\\" + testInputFile;
            HomeBudget homeBudget = new HomeBudget(inFile);
            int maxRecords = TestConstants.budgetItemsByCategoryAndMonth_MaxRecords;
            Dictionary<string, object> firstRecord = TestConstants.getBudgetItemsByCategoryAndMonthFirstRecord();

            // Act
            List<Dictionary<string, object>> budgetItemsByCategoryAndMonth = homeBudget.GetBudgetDictionaryByCategoryAndMonth(null, null, false, 9);

            // Assert
            Assert.Equal(maxRecords+1,budgetItemsByCategoryAndMonth.Count);

        }

        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetDictionaryByCategoryAndMonth_NoStartEnd_NoFilter_VerifyFirstRecord()
        {
            // Arrange
            string inFile = GetSolutionDir() + "\\" + testInputFile;
            HomeBudget homeBudget = new HomeBudget(inFile);
            int maxRecords = TestConstants.budgetItemsByCategoryAndMonth_MaxRecords; 
            Dictionary<string,object> firstRecord = TestConstants.getBudgetItemsByCategoryAndMonthFirstRecord();

            // Act
            List<Dictionary<string,object>> budgetItemsByCategoryAndMonth = homeBudget.GetBudgetDictionaryByCategoryAndMonth(null, null, false, 9);
            Dictionary<string,object> firstRecordTest = budgetItemsByCategoryAndMonth[0];

            // Assert
            Assert.True(AssertDictionaryForExpenseByCategoryAndMonthIsOK(firstRecord,firstRecordTest));
            
        }

        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetDictionaryByCategoryAndMonth_NoStartEnd_NoFilter_VerifyTotalsRecord()
        {
            // Arrange
            string inFile = GetSolutionDir() + "\\" + testInputFile;
            HomeBudget homeBudget = new HomeBudget(inFile);
            int maxRecords = TestConstants.budgetItemsByCategoryAndMonth_MaxRecords;
            Dictionary<string, object> totalsRecord = TestConstants.getBudgetItemsByCategoryAndMonthTotalsRecord();

            // Act
            List<Dictionary<string, object>> budgetItemsByCategoryAndMonth = homeBudget.GetBudgetDictionaryByCategoryAndMonth(null, null, false, 9);
            Dictionary<string, object> totalsRecordTest = budgetItemsByCategoryAndMonth[budgetItemsByCategoryAndMonth.Count - 1];

            // Assert
            // ... loop over all key/value pairs 
            Assert.True(AssertDictionaryForExpenseByCategoryAndMonthIsOK(totalsRecord, totalsRecordTest), "Totals Record is Valid");

        }

        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetDictionaryByCategoryAndMonth_NoStartEnd_FilterbyCategory()
        {
            // Arrange
            string inFile = GetSolutionDir() + "\\" + testInputFile;
            HomeBudget homeBudget = new HomeBudget(inFile);
            List<Dictionary<string, object>> expectedResults =TestConstants.getBudgetItemsByCategoryAndMonthCat10();

            // Act
            List<Dictionary<string, object>> gotResults = homeBudget.GetBudgetDictionaryByCategoryAndMonth(null, null, true, 10);

            // Assert
            Assert.Equal(expectedResults.Count, gotResults.Count);
            for (int record = 0; record < expectedResults.Count; record++)
            {
                Assert.True(AssertDictionaryForExpenseByCategoryAndMonthIsOK(expectedResults[record],
                    gotResults[record]), "Record:" + record + " is Valid");

            }
        }

        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetDictionaryByCategoryAndMonth_2020()
        {
            // Arrange
            string inFile = GetSolutionDir() + "\\" + testInputFile;
            HomeBudget homeBudget = new HomeBudget(inFile);
            List<Dictionary<string, object>> expectedResults = TestConstants.getBudgetItemsByCategoryAndMonth2020();

            // Act
            List<Dictionary<string, object>> gotResults = homeBudget.GetBudgetDictionaryByCategoryAndMonth(new DateTime(2020,1,1), new DateTime(2020,12,31), false, 10);

            // Assert
            Assert.Equal(expectedResults.Count, gotResults.Count);
            for (int record = 0; record < expectedResults.Count; record++)
            {
                Assert.True(AssertDictionaryForExpenseByCategoryAndMonthIsOK(expectedResults[record],
                    gotResults[record]), "Record:" + record + " is Valid");

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

        Boolean AssertDictionaryForExpenseByCategoryAndMonthIsOK(Dictionary<string,object> recordExpeted, Dictionary<string,object> recordGot)
        {
            try
            {
                foreach (var kvp in recordExpeted)
                {
                    String key = kvp.Key as String;
                    Object recordExpectedValue = kvp.Value;
                    Object recordGotValue = recordGot[key];


                    // ... validate the budget items
                    if (recordExpectedValue != null && recordExpectedValue.GetType() == typeof(List<BudgetItem>))
                    {
                        List<BudgetItem> expectedItems = recordExpectedValue as List<BudgetItem>;
                        List<BudgetItem> gotItems = recordGotValue as List<BudgetItem>;
                        for (int budgetItemNumber = 0; budgetItemNumber < expectedItems.Count; budgetItemNumber++)
                        {
                            Assert.Equal(expectedItems[budgetItemNumber].Amount, gotItems[budgetItemNumber].Amount);
                            Assert.Equal(expectedItems[budgetItemNumber].CategoryID, gotItems[budgetItemNumber].CategoryID);
                            Assert.Equal(expectedItems[budgetItemNumber].ExpenseID, gotItems[budgetItemNumber].ExpenseID);
                        }
                    }

                    // else ... validate the value for the specified key
                    else
                    {
                        Assert.Equal(recordExpectedValue, recordGotValue);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


    }
}


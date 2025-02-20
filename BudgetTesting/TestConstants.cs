using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Budget;

namespace BudgetCodeTests
{
    public class TestConstants
    {

        private static Expense expense1 = new Expense(1, new DateTime(2018, 1, 10), 10, 12, "hat (on credit)");
        private static BudgetItem budgetItem1 = new BudgetItem
        {
            CategoryID = expense1.Category,
            ExpenseID = expense1.Id,
            Amount = -expense1.Amount
        };

        private static Expense expense2 = new Expense(2, new DateTime(2018, 1, 11), 9, -10, "hat (on credit)");
        private static BudgetItem budgetItem2 = new BudgetItem
        {
            CategoryID = expense2.Category,
            ExpenseID = expense2.Id,
            Amount = -expense2.Amount
        };


        private static BudgetItem budgetItem3 = new BudgetItem
        {
            CategoryID = 10,
            ExpenseID = 3,
            Amount = -15
        };

        private static Expense expense4 = new Expense(4, new DateTime(2020, 1, 10), 9, -15, "scarf (on credit)");
        private static BudgetItem budgetItem4 = new BudgetItem
        {
            CategoryID = expense4.Category,
            ExpenseID = expense4.Id,
            Amount = -expense4.Amount
        };


        private static Expense expense5 = new Expense(5, new DateTime(2020, 1, 11), 14, 45, "McDonalds");
        private static BudgetItem budgetItem5 = new BudgetItem
        {
            CategoryID = expense5.Category,
            ExpenseID = expense5.Id,
            Amount = -expense5.Amount
        };

        private static Expense expense7 = new Expense(7, new DateTime(2020, 1, 12), 14, 25, "Wendys");
        private static BudgetItem budgetItem7 = new BudgetItem
        {
            CategoryID = expense7.Category,
            ExpenseID = expense7.Id,
            Amount = -expense7.Amount
        };







        public static int numberOfCategoriesInFile = 17;
        public static String testCategoriesInputFile = "test_categories.cats";
        public static int maxIDInCategoryInFile = 17;
        public static Category firstCategoryInFile = new Category(17, "Non Standard", Category.CategoryType.Expense);
        public static int CategoryIDWithSaveType = 15;
        public static string CategoriesOutputTestFile = "test_output.cats";

        public static int numberOfExpensesInFile = 6;
        public static String testExpensesInputFile = "test_expenses.exps";
        public static int maxIDInExpenseFile = 7;
        public static Expense firstExpenseInFile { get { return expense1; } }
        public static string ExpenseOutputTestFile = "test_output.exps";

        public static string testBudgetFile = "test.budget";
        public static string outputTestBudgetFile = "output_test.budget";

        public static List<Expense> filteredbyCat14()
        {
            List<Expense> filtered = new List<Expense>();
            filtered.Add(expense5);
            return filtered;
        }
        public static double filteredbyCat9Total = expense2.Amount + expense4.Amount;
        public static List<Expense> filteredbyCat9()
        {
            List<Expense> filtered = new List<Expense>();
            filtered.Add(expense2);
            filtered.Add(expense4);
            return filtered;
        }
        public static List<Expense> filteredbyYear2018AndCategory10()
        {
            List<Expense> filtered = new List<Expense>();
            filtered.Add(expense1);
            return filtered;
        }

        public static List<Expense> filteredbyYear2018()
        {
            List<Expense> filtered = new List<Expense>();
            filtered.Add(expense1);
            filtered.Add(expense2);
            return filtered;
        }


        // LIST EXPENSES BY MONTH
        public static int budgetItemsByMonth_MaxRecords = 3;
        public static BudgetItemsByMonth budgetItemsByMonth_FirstRecord = getBudgetItemsBy2018_01()[0];
        public static int budgetItemsByMonth_FilteredByCat9_number = 2;
        public static BudgetItemsByMonth budgetItemsByMonth_FirstRecord_FilteredCat9 = getBudgetItemsBy2018_01_filteredByCat9()[0];
        public static int budgetItemsByMonth_2018_FilteredByCat9_number = 1;


        public static List<BudgetItemsByMonth> getBudgetItemsBy2018_01()
        {
            List<BudgetItemsByMonth> list = new List<BudgetItemsByMonth>();
            List<BudgetItem> budgetItems = new List<BudgetItem>();

            budgetItems.Add(budgetItem1);
            budgetItems.Add(budgetItem2);


            list.Add(new BudgetItemsByMonth
            {
                Month = "2018/01",
                Details = budgetItems,
                Total = budgetItem1.Amount + budgetItem2.Amount
            });
            return list;
        }

        public static List<BudgetItemsByMonth> getBudgetItemsBy2018_01_filteredByCat9()
        {
            List<BudgetItemsByMonth> list = new List<BudgetItemsByMonth>();
            List<BudgetItem> budgetItems = new List<BudgetItem>();

            budgetItems.Add(budgetItem2);

            list.Add(new BudgetItemsByMonth
            {
                Month = "2018/01",
                Details = budgetItems,
                Total = budgetItem2.Amount
            });
            return list;
        }



        // LIST EXPENSES BY CATEGORY
        public static int budgetItemsByCategory_MaxRecords = 3;
        public static BudgetItemsByCategory budgetItemsByCategory_FirstRecord = getBudgetItemsByCategoryCat10()[0];
        public static int budgetItemsByCategory_FilteredByCat10_number = 2;
        public static int budgetItemsByCategory14 = 1;
        public static int budgetItemsByCategory20 = 0;


        public static List<BudgetItemsByCategory> getBudgetItemsByCategoryCat10()
        {
            List<BudgetItemsByCategory> list = new List<BudgetItemsByCategory>();
            List<BudgetItem> budgetItems = new List<BudgetItem>();

            budgetItems.Add(budgetItem1);
            budgetItems.Add(budgetItem3);


            list.Add(new BudgetItemsByCategory
            {
                Category = "Clothes",
                Details = budgetItems,
                Total = budgetItem1.Amount+ budgetItem3.Amount
            });
            return list;
        }

        public static List<BudgetItemsByCategory> getBudgetItemsByCategory2018_Cat9()
        {
            List<BudgetItemsByCategory> list = new List<BudgetItemsByCategory>();
            List<BudgetItem> budgetItems = new List<BudgetItem>();

            budgetItems.Add(budgetItem2);

            list.Add(new BudgetItemsByCategory
            {
                Category = "Credit Card",
                Details = budgetItems,
                Total = budgetItem2.Amount
            });
            return list;
        }

        public static List<BudgetItemsByCategory> getBudgetItemsByCategory2018()
        {
            List<BudgetItemsByCategory> list = new List<BudgetItemsByCategory>();
            List<BudgetItem> budgetItems = new List<BudgetItem>();

            budgetItems.Add(budgetItem1);

            list.Add(new BudgetItemsByCategory
            {
                Category = "Clothes",
                Details = budgetItems,
                Total = budgetItem1.Amount
            });


            budgetItems = new List<BudgetItem>();
            budgetItems.Add(budgetItem2);

            list.Add(new BudgetItemsByCategory
            {
                Category = "Credit Card",
                Details = budgetItems,
                Total = budgetItem2.Amount
            });
            return list;
        }




        // LIST EXPENSES BY CATEGORY AND MONTH
        public static int budgetItemsByCategoryAndMonth_MaxRecords = 3; // 3 months

        public static Dictionary<string, object> getBudgetItemsByCategoryAndMonthFirstRecord()
        {
            List<BudgetItem> budgetItems;

            Dictionary<string, object> dict = new Dictionary<string, object> {
                { "Month","2018/01" },{"Total", budgetItem1.Amount+budgetItem2.Amount }  };


            budgetItems = new List<BudgetItem>();
            budgetItems.Add(budgetItem1);

            dict.Add("details:Clothes", budgetItems);
            dict.Add("Clothes", budgetItem1.Amount);


            budgetItems = new List<BudgetItem>();

            budgetItems.Add(budgetItem2);

            dict.Add("details:Credit Card", budgetItems);
            dict.Add("Credit Card", budgetItem2.Amount);



            return dict;
        }

        public static Dictionary<string, object> getBudgetItemsByCategoryAndMonthTotalsRecord()
        {
            Dictionary<string, object> dict = new Dictionary<string, object> {
                { "Month","TOTALS" }  };
            dict.Add("Clothes", budgetItem1.Amount + budgetItem3.Amount);
            dict.Add("Credit Card", budgetItem4.Amount + budgetItem2.Amount);
            dict.Add("Eating Out", budgetItem5.Amount + budgetItem7.Amount);

            return dict;
        }

        public static List<Dictionary<string,object>> getBudgetItemsByCategoryAndMonthCat10()
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            List<BudgetItem> budgetItems = new List<BudgetItem>();

            budgetItems.Add(budgetItem1);

            list.Add(new Dictionary<string, object> {
                {"Month","2018/01" },
                { "Clothes",budgetItem1.Amount},
                {"details:Clothes",budgetItems },
                }
            );

            budgetItems = new List<BudgetItem>();

            budgetItems.Add(budgetItem3);

            list.Add(new Dictionary<string, object> {
                {"Month","2019/01" },
                { "Clothes",budgetItem3.Amount},
                {"details:Clothes",budgetItems },
                }
            );

            list.Add(new Dictionary<string, object> {
                {"Month","TOTALS" },
                { "Clothes",budgetItem1.Amount + budgetItem3.Amount},
                }
            );

            return list;
        }

 
        public static List<Dictionary<string,object>> getBudgetItemsByCategoryAndMonth2020()
        {
            List< Dictionary<string, object> > list = new List<Dictionary<string, object>>();

            list.Add(new Dictionary<string, object> {
                {"Month","2020/01" },
                { "Credit Card",budgetItem4.Amount},
                {"details:Credit Card",new List<BudgetItem>{budgetItem4 } },
                {"Eating Out",budgetItem5.Amount + budgetItem7.Amount },
                {"details:Eating Out", new List<BudgetItem>{budgetItem5, budgetItem7} }
                }
            );

           list.Add(new Dictionary<string, object> {
                {"Month","TOTALS" },
                { "Credit Card",budgetItem4.Amount},
                { "Eating Out",budgetItem5.Amount + budgetItem7.Amount},
                }
            );



            return list;
        }
    }


}





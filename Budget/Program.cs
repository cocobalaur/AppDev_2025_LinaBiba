// See https://aka.ms/new-console-template for more information
namespace Budget
{
    class Program
    {
        //global variables
        public static HomeBudget budget;

        //constants for formatting
        public const int Width = -27;
        public const string HeaderFormat = "{0,-15}  {1,-25}  {2,-10}  {3,-10}";
        public const string DetailFormat = "{0,-15:yyyy/MMM/dd}  {1,-25}  {2,-10:C}  {3,-10:C}";

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            //budget = new HomeBudget("..\\..\\..\\..\\BudgetTesting\\test.budget");


            // Menu loop
            while (true)
            {
                DisplayMenu();

                Console.Write("Choose an option (1-4): ");

                int choice;
                if (!int.TryParse(Console.ReadLine(), out choice))
                {
                    PrintInvalidInputMessage();
                    //skips any code after the "continue" and move directly to the next iteration of thr loop
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        GetBudgetItems();
                        break;
                    case 2:
                        GetBudgetItemsByMonth();
                        break;
                    case 3:
                        GetBudgetItemsByCategory();
                        break;
                    case 4:
                        GetBudgetDictionaryByCategoryAndMonth();
                        break;
                    case 5:
                        Console.Clear();
                        Console.WriteLine("\nExiting the application. Goodbye!");
                        return;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid choice. Please enter a number from 1 to 4.\n");
                        Console.ResetColor();
                        break;
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                Console.Clear();

                //keeps on looping until valid choice is set to true
            }
        }

        public static void DisplayMenu()
        {
            Console.WriteLine("Budget Menu:");
            Console.WriteLine("1. Get Budget Items");
            Console.WriteLine("2. Get Budget Items By Month");
            Console.WriteLine("3. Get Budget Items By Category");
            Console.WriteLine("4. Get Budget Dictionary By Category And Month");
            Console.WriteLine("5. Exit");
        }

        public static void PrintInvalidInputMessage()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid input. Press any key to continue...");
            Console.ResetColor();
            Console.ReadKey();
            Console.Clear();
        }



        public static void GetBudgetItems()
        {
            DateTime? startDate = GetDateInput("Enter start date (mm/dd/yyyy): ");
            DateTime? endDate = GetDateInput("Enter end date (mm/dd/yyyy): ");
            bool filterFlag = GetBooleanInput();
            int categoryId = filterFlag ? GetIntInput() : 0;

            List<BudgetItem> items = budget.GetBudgetItems(startDate, endDate, filterFlag, categoryId);
            PrintBudgetItems(items);
        }

        public static void GetBudgetItemsByMonth()
        {
            DateTime? startDate = GetDateInput("Enter start date (mm/dd/yyyy): ");
            DateTime? endDate = GetDateInput("Enter end date (mm/dd/yyyy): ");
            bool filterFlag = GetBooleanInput();
            int categoryId = filterFlag ? GetIntInput() : 0;

            List<BudgetItemsByMonth> monthlyItems = budget.GetBudgetItemsByMonth(startDate, endDate, filterFlag, categoryId);
            PrintBudgetItemsByMonth(monthlyItems);
        }

        public static void GetBudgetItemsByCategory()
        {
            DateTime? startDate = GetDateInput("Enter start date (mm/dd/yyyy): ");
            DateTime? endDate = GetDateInput("Enter end date (mm/dd/yyyy): ");
            bool filterFlag = GetBooleanInput();
            int categoryId = filterFlag ? GetIntInput() : 0;

            List<BudgetItemsByCategory> categoryItems = budget.GetBudgetItemsByCategory(startDate, endDate, filterFlag, categoryId);
            PrintBudgetItemsByCategory(categoryItems);
        }

        public static void GetBudgetDictionaryByCategoryAndMonth()
        {
            DateTime? startDate = GetDateInput("Enter start date (mm/dd/yyyy): ");
            DateTime? endDate = GetDateInput("Enter end date (mm/dd/yyyy): ");
            bool filterFlag = GetBooleanInput();
            int categoryId = filterFlag ? GetIntInput() : 0;

            List<Dictionary<string, object>> budgetDictionary = budget.GetBudgetDictionaryByCategoryAndMonth(startDate, endDate, filterFlag, categoryId);
            PrintBudgetDictionary(budgetDictionary);
        }



        //verifying input
        public static DateTime? GetDateInput(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine();

                // Check if the input is empty
                if (string.IsNullOrWhiteSpace(input))
                {
                    return null; // Return null for empty input
                }

                // Try to parse the date
                if (DateTime.TryParse(input, out DateTime date))
                {
                    return date; // Return the valid date
                }

                // If input is invalid, print the message
                Console.WriteLine("Invalid date format. Please try again.");


            }
        }

        public static bool GetBooleanInput()
        {
            while (true)
            {
                Console.Write("Apply category filter? (y/n): ");
                string input = Console.ReadLine().ToLower();

                if (input == "y")
                    return true;
                if (input == "n")
                    return false;

                Console.WriteLine("Invalid input. Please enter 'y' or 'n'.");
            }
        }

        public static int GetIntInput()
        {
            while (true)
            {
                Console.Write("Enter category ID: ");
                if (int.TryParse(Console.ReadLine(), out int number))
                {
                    return number;
                }
                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
        }





        //printing content
        public static void PrintBudgetItems(List<BudgetItem> items)
        {
            Console.WriteLine("\nBudget Items:");

            Console.WriteLine(HeaderFormat, "Date", "Description", "Amount", "Balance");
            foreach (BudgetItem item in items)
            {
                // Print each budget item's details
                Console.WriteLine(DetailFormat, item.Date, item.ShortDescription, item.Amount, item.Balance);
            }


        }

        public static void PrintBudgetItemsByMonth(List<BudgetItemsByMonth> monthlyItems)
        {
            Console.WriteLine("\nBudget Items by Month:\n");


            // Loop through each BudgetItemsByMonth object
            foreach (BudgetItemsByMonth monthItem in monthlyItems)
            {
                // Print the header using the HeaderFormat constant
                Console.WriteLine(HeaderFormat, "Month", "Description", "Total", "Balance");

                // Print the month and total amount
                Console.WriteLine(DetailFormat, monthItem.Month, "", monthItem.Total.ToString("C"), "");

                // Loop through each BudgetItem for the month and print its details
                foreach (BudgetItem detail in monthItem.Details)
                {
                    // Print the description and amount for each BudgetItem
                    Console.WriteLine(string.Format(DetailFormat, "", detail.ShortDescription, detail.Amount.ToString("C"), detail.Balance.ToString("C")));
                }

                // Add a blank line between months for better readability
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        public static void PrintBudgetItemsByCategory(List<BudgetItemsByCategory> categoryItems)
        {
            Console.WriteLine("\nBudget Items by Category:");
            foreach (BudgetItemsByCategory categoryItem in categoryItems)
            {
                // Print category name and total for the category
                Console.WriteLine($"\n{"Category: " + categoryItem.Category, Width}   Total: {categoryItem.Total:C}");
                // loop through each BudgetItem for the category and print its details
                foreach (BudgetItem detail in categoryItem.Details)
                {
                    Console.WriteLine($"{detail.ShortDescription,Width}  {detail.Amount,Width:C}");
                }
            }
        }

        public static void PrintBudgetDictionary(List<Dictionary<string, object>> budgetDictionary)
        {
            Console.WriteLine("\nBudget Dictionary by Category and Month:");

            foreach (Dictionary<string, object> record in budgetDictionary)
            {
                // Safely print the month and total for the record
                if (record.TryGetValue("Month", out object month) && record.TryGetValue("Total", out object total))
                {
                    Console.WriteLine($"\n{"Month:"+ month,Width} Total: {Convert.ToDecimal(total):C}");
                }

                // Check for category details (starts with "details:")
                foreach (KeyValuePair<string, object> item in record)
                {
                    if (item.Key.StartsWith("details:"))
                    {
                        List<BudgetItem> details = item.Value as List<BudgetItem>;
                        if (details != null)
                        {
                            foreach (BudgetItem detail in details)
                            {
                                Console.WriteLine($"{detail.ShortDescription,Width} {detail.Amount:C}");
                            }
                        }
                    }
                    // Print the category totals for the month
                    else if (!item.Key.StartsWith("Month") && !item.Key.StartsWith("Total") && item.Value is double categoryTotal)
                    {
                        Console.WriteLine($"{"Category: " + item.Key,Width} {categoryTotal:C}");
                    }
                }
            }
        }




    }
}
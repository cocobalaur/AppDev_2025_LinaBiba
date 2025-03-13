using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Budget
{
    internal class DemoSprint1
    {
        public static void InsertDemo()
        {

            Console.Write("Enter db file:");
            string dbFile = Console.ReadLine();
            Database.newDatabase(dbFile);

            Categories categories = new Categories(Database.dbConnection, true);

            while (true)
            {

                Console.WriteLine("\n--- Categories Demo ---");
                Console.WriteLine("1. View Categories");
                Console.WriteLine("2. Add a Category");
                Console.WriteLine("3. Update a Category");
                Console.WriteLine("4. Delete a Category");
                Console.WriteLine("5. Get Category");
                Console.WriteLine("6. Exit");
                Console.Write("Choose an option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        List<Category> allCategories = categories.List();
                        Console.WriteLine("\nCurrent Categories:");

                        foreach (Category cat in allCategories)
                        {
                            Console.WriteLine($"ID: {cat.Id}, Description: {cat.Description}, Type: {cat.Type}");
                        }

                        break;

                    case "2":
                        Console.Write("Enter category name: ");
                        string name = Console.ReadLine();

                        Console.Write("Enter category type (1 = Income, 2 = Expense, 3 = Credit, 4 = Savings): ");
                        if (Enum.TryParse(Console.ReadLine(), out Category.CategoryType type))
                        {
                            categories.Add(name, type);
                            Console.WriteLine("Category added successfully.");
                        }
                        break;

                    case "3":
                        Console.Write("Enter the ID of the category to update: ");
                        if (int.TryParse(Console.ReadLine(), out int updateId))
                        {
                            Console.Write("Enter new category name: ");
                            string newName = Console.ReadLine();

                            Console.Write("Enter new category type (1 = Income, 2 = Expense, 3 = Credit, 4 = Savings): ");
                            if (Enum.TryParse(Console.ReadLine(), out Category.CategoryType newType))
                            {
                                categories.UpdateProperties(updateId, newName, newType);
                                Console.WriteLine("Category updated successfully.");
                            }
                        }
                        break;

                    case "4":
                        Console.Write("Enter the ID of the category to delete: ");
                        if (int.TryParse(Console.ReadLine(), out int deleteId))
                        {
                            categories.Delete(deleteId);
                            Console.WriteLine("Category deleted successfully.");
                        }
                        break;

                    case "5":
                        Console.Write("Enter ID to get specific Category: ");
                        if (int.TryParse(Console.ReadLine(), out int id))
                        {
                            Category retrieved = categories.GetCategoryFromId(id);
                            Console.WriteLine($"ID: {retrieved.Id}, Description: {retrieved.Description}, Type: {retrieved.Type}");
                        }
                        break;

                    case "6":
                        Console.Write("Bye!");
                        return;

                    default:
                        Console.WriteLine("Invalid choice. Please select a valid option.");
                        break;
                }
            }
        }

        public static void NoUserInsertDemo()
        {
            string testDbFile = "test.db";
            Database.newDatabase(testDbFile);
            Console.WriteLine("New database file create: " + testDbFile);

            Categories categories = new Categories(Database.dbConnection, true);

            Console.WriteLine("\nAdding Default Categories to Database:");
            List<Category> allCategories = categories.List();

            foreach (Category category in allCategories)
            {
                Console.WriteLine($"ID: {category.Id}, Description: {category.Description}, Type: {category.Type}");

            }

            Console.WriteLine("\nAdd method:");
            categories.Add("Test Category 1", Category.CategoryType.Expense);
            categories.Add("Test Category 2", Category.CategoryType.Income);
            Console.WriteLine("Added Test Category 1 and Test Category 2 to the database\n");

            Console.WriteLine("Default categories with 2 new added categories:");
            if (allCategories.Count > 0)
            {
                int categoryId = allCategories[0].Id;
                categories.UpdateProperties(categoryId, "Updated Category", Category.CategoryType.Savings);
                Console.WriteLine("Category updated successfully.");
            }

            Console.WriteLine("\nCategories after update:");
            allCategories = categories.List();
            foreach (Category cat in allCategories)
            {
                Console.WriteLine($"ID: {cat.Id}, Description: {cat.Description}, Type: {cat.Type}");
            }

            Console.WriteLine("\nDeleting category 1");
            if (allCategories.Count > 0)
            {
                int categoryId = allCategories[0].Id;
                categories.Delete(categoryId);
                Console.WriteLine("Category deleted successfully.");
            }

            Console.WriteLine("\nFinal category list:");
            allCategories = categories.List();
            foreach (Category cat in allCategories)
            {
                Console.WriteLine($"ID: {cat.Id}, Description: {cat.Description}, Type: {cat.Type}");
            }


        }

        public static void Option()
        {
            while (true)
            {
                Console.WriteLine("\n1. No insert demo");
                Console.WriteLine("2. Insert Demo");
                Console.WriteLine("3. Exit");
                Console.Write("Choose an option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine("\nRunning DemoSprint1...");
                        DemoSprint1.NoUserInsertDemo();
                        break;

                    case "2":
                        Console.WriteLine("\nRunning InsertDemo...");
                        DemoSprint1.InsertDemo();
                        break;

                    case "3":
                        return;

                    default:
                        Console.WriteLine("Invalid option. Please enter 1, 2, or 3.");
                        break;
                }
            }
        }
    }
}


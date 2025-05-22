using Budget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Views;

namespace PresenterTest
{
    class MockView : IView
    {
        // Tracking logs
        public List<string> ErrorMessages { get; } = new();
        public List<string> SuccessMessages { get; } = new();
        public List<string> CalledMethods { get; } = new();

        public List<(List<string> names, string type)> CategoryFilterCalls { get; } = new();
        public List<(List<string> categories, string selected)> CategoryExpenseCalls { get; } = new();
        public List<List<BudgetItem>> DisplayedItems { get; } = new();
        public List<Expense> ExpenseUpdateCalls { get; } = new();
        public List<int> DeletedExpenseIds { get; } = new();
        public List<int> UpdatedExpenseIds { get; } = new();
        public List<(List<Dictionary<string, object>> groupedData, List<string> allCategories)> ChartCalls { get; } = new();

        // Stubs for return values (can be set during test)
        public DateTime? StubStartDate { get; set; }
        public DateTime? StubEndDate { get; set; }
        public bool StubDisplayByCategorySummary { get; set; }
        public bool StubDisplayByMonthSummary { get; set; }
        public bool StubIsCategoryFilter { get; set; }
        public string StubRenameSelectedCategory { get; set; } = "";
        public List<BudgetItem> StubAllItems { get; set; } = new();

        public void DisplayErrorMessage(string message)
        {
            ErrorMessages.Add(message);
            CalledMethods.Add(nameof(DisplayErrorMessage));
        }

        public void DisplayAddExpense()
        {
            CalledMethods.Add(nameof(DisplayAddExpense));
        }

        public void DisplaySuccessMessage(string message)
        {
            SuccessMessages.Add(message);
            CalledMethods.Add(nameof(DisplaySuccessMessage));
        }

        public void DisplayCategoryFilterWindow(List<string> name, string type)
        {
            CategoryFilterCalls.Add((new List<string>(name), type));
            CalledMethods.Add(nameof(DisplayCategoryFilterWindow));
        }

        public void DisplayCategoryExpense(List<string> categories, string selectedCategory)
        {
            CategoryExpenseCalls.Add((new List<string>(categories), selectedCategory));
            CalledMethods.Add(nameof(DisplayCategoryExpense));
        }

        public DateTime? GetStartDate()
        {
            CalledMethods.Add(nameof(GetStartDate));
            return StubStartDate;
        }

        public DateTime? GetEndDate()
        {
            CalledMethods.Add(nameof(GetEndDate));
            return StubEndDate;
        }

        public void DisplayItems(List<BudgetItem> items)
        {
            DisplayedItems.Add(new List<BudgetItem>(items));
            CalledMethods.Add(nameof(DisplayItems));
        }

        public bool DisplayByCategorySummary()
        {
            CalledMethods.Add(nameof(DisplayByCategorySummary));
            return StubDisplayByCategorySummary;
        }

        public bool DisplayByMonthSummary()
        {
            CalledMethods.Add(nameof(DisplayByMonthSummary));
            return StubDisplayByMonthSummary;
        }

        public bool DisplayIsCategoryFilter()
        {
            CalledMethods.Add(nameof(DisplayIsCategoryFilter));
            return StubIsCategoryFilter;
        }

        public string RenameSelectedCategory()
        {
            CalledMethods.Add(nameof(RenameSelectedCategory));
            return StubRenameSelectedCategory;
        }

        public void ShowSucessMessage(string message)
        {
            SuccessMessages.Add(message);
            CalledMethods.Add(nameof(ShowSucessMessage));
        }

        public void ShowErrorMessage(string message)
        {
            ErrorMessages.Add(message);
            CalledMethods.Add(nameof(ShowErrorMessage));
        }

        public void DisplayExpenseUpdate(Expense expense, Action onUpdateComplete)
        {
            ExpenseUpdateCalls.Add(expense);
            CalledMethods.Add(nameof(DisplayExpenseUpdate));
            onUpdateComplete?.Invoke();
        }

        public void ShowChart(List<Dictionary<string, object>> groupedData, List<string> allCategories)
        {
            ChartCalls.Add((groupedData, allCategories));
            CalledMethods.Add(nameof(ShowChart));
        }

        public void HideChart()
        {
            CalledMethods.Add(nameof(HideChart));
        }

        public void ReselectExpenseOnceDeleted(int deleteId)
        {
            DeletedExpenseIds.Add(deleteId);
            CalledMethods.Add(nameof(ReselectExpenseOnceDeleted));
        }

        public void ReselectExpenseOnceUpdated(int id)
        {
            UpdatedExpenseIds.Add(id);
            CalledMethods.Add(nameof(ReselectExpenseOnceUpdated));
        }

        public List<BudgetItem> GetAllItems()
        {
            CalledMethods.Add(nameof(GetAllItems));
            return StubAllItems;
        }
    }
}

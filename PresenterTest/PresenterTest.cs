using Budget;
using BudgetModel;

namespace PresenterTest
{
    public class PresenterTest
    {
        private readonly MockView _mockView;
        private readonly Presenter _presenter;
        private HomeBudget _budget;

        //Constructor
        public PresenterTest()
        {
            _mockView = new MockView();
            _presenter = new Presenter(_mockView);
        }

        //Testing SetAndGet
        [Fact]
        public void SetAndGetView_PropertyWorksAsExpected()
        {
            // Arrange
            var mockView = new MockView(); // Create an instance of MockView
            var presenter = new Presenter(mockView); // Pass the mockView into the Presenter

            // Act
            var viewFromPresenter = presenter.View;  // Get the current View (should be the mockView)
            presenter.View = mockView;  // Set the View to the mockView again

            // Assert
            Assert.Equal(mockView, viewFromPresenter);  // Check if the View property returns the correct mockView instance
        }

        //Testing GetDatabase
        [Fact]
        public void GetDatabase_ShouldInitializeBudget_WhenDatabasePathIsValid()
        {
            // Arrange
            string databasePath = "testingdb.db";

            // Act
            bool result = _presenter.GetDatabase(databasePath);

            // Assert
            Assert.True(result);
            Assert.Empty(_mockView.ErrorMessages); // No errors expected
            Assert.Contains("Successfully opened", _mockView.SuccessMessages[0]);
        }

        [Fact]
        public void GetDatabase_InvalidPath_ShowsError()
        {
            // Arrange
            string invalidPath = "Z:\\nonexistent\\invalidfile.db";

            // Act
            bool result = _presenter.GetDatabase(invalidPath);

            // Assert
            Assert.False(result);
            Assert.NotEmpty(_mockView.ErrorMessages);
            Assert.Contains("Error setting up database", _mockView.ErrorMessages[0]);
        }



        //Testing AddExpense
        [Fact]
        public void AddExpense_ShouldAddExpense_WhenBudgetIsInitialized()
        {
            // Arrange
            _presenter.GetDatabase("testingdb.db"); // Ensures _budget is initialized

            DateTime date = new DateTime(2025, 4, 26);
            string name = "Coffee";
            double amount = 4.50;
            string categoryName = "Food";

            // Act
            _presenter.ProcessNewAddExpense(date, name, amount, categoryName);

            // Assert
            Assert.Empty(_mockView.ErrorMessages); // No errors should be reported
            Assert.Contains("Expense 'Coffee' added successfully.", _mockView.SuccessMessages);
            Assert.Contains(nameof(_mockView.DisplayAddExpense), _mockView.CalledMethods);
        }
        [Fact]
        public void AddExpense_ShouldShowError_WhenBudgetIsNotInitialized()
        {
            // Arrange – intentionally not calling GetDatabase

            DateTime date = new DateTime(2025, 4, 26);
            string name = "Coffee";
            double amount = 4.50;
            string categoryName = "Food";

            // Act
            _presenter.ProcessNewAddExpense(date, name, amount, categoryName);

            // Assert
            Assert.Single(_mockView.ErrorMessages);
            Assert.Equal("Database not initialized.", _mockView.ErrorMessages[0]);
            Assert.DoesNotContain(nameof(_mockView.DisplaySuccessMessage), _mockView.CalledMethods);
        }

        [Fact]
        public void AddExpense_ShouldThrowError_WhenInformationIsWrong()
        {
            // Arrange
            _presenter.GetDatabase("testDatabase.db"); // Ensure initialized

            DateTime date = new DateTime(2025, 4, 26);
            string name = "Coffee";
            double amount = 4.50;
            string categoryName = ""; // Invalid (empty category name)

            // Act
            _presenter.ProcessNewAddExpense(date, name, amount, categoryName);

            // Assert
            Assert.NotEmpty(_mockView.ErrorMessages);
            Assert.Contains("Error adding expense:", _mockView.ErrorMessages[0]);
            Assert.Contains(nameof(_mockView.DisplayErrorMessage), _mockView.CalledMethods);

        }



        //Testing GetCategories
        [Fact]
        public void GetCategories_ShouldReturnCategories_WhenBudgetIsInitialized()
        {
            // Arrange
            _presenter.GetDatabase("testDatabase.db"); // This initializes _budget

            // Act
            List<Category> categories = _presenter.GetCategories();

            // Assert
            Assert.NotNull(categories);                      // Should return a list, even if it's empty
            Assert.Empty(_mockView.ErrorMessages);           // No error message should have been shown
            Assert.DoesNotContain(nameof(_mockView.DisplayErrorMessage), _mockView.CalledMethods);
        }


        [Fact]
        public void GetCategories_ShouldShowError_WhenBudgetIsNotInitialized()
        {
            // Act – no call to GetDatabase
            List<Category> categories = _presenter.GetCategories();

            // Assert
            Assert.Empty(categories);                                       // Return should still be a valid empty list
            Assert.Single(_mockView.ErrorMessages);                         // One error should be shown
            Assert.Equal("Database not initialized.", _mockView.ErrorMessages[0]);
            Assert.Contains(nameof(_mockView.DisplayErrorMessage), _mockView.CalledMethods);
        }


        //Testing CreateOrGetCategory
        [Fact]
        public void CreateOrGetCategory_CategoryExists_ReturnCategory()
        {
            // Arrange
            _presenter.GetDatabase("testingdb.db");
            string categoryDescription = "Income";

            // Act
            Category category = _presenter.GetCategory(categoryDescription);

            // Assert
            Assert.Equal(categoryDescription, category.Description);
            Assert.Empty(_mockView.ErrorMessages); // No error expected
            Assert.DoesNotContain(nameof(_mockView.DisplayErrorMessage), _mockView.CalledMethods);
        }

        [Fact]
        public void CreateOrGetCategory_WithoutDatabase_ThrowsException()
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _presenter.GetCategory("Groceries"));

            // Optional: Verify that no view methods were incorrectly called
            Assert.Empty(_mockView.CalledMethods);
        }

        //Testing AddCategory
        [Fact]
        public void AddCategory_HomeBudgetNotInitialize_ShouldReturnFalse()
        {

            // Arrange
            string categoryName = "WOah";
            string categoryType = "Income";

            // Act
            bool result = _presenter.AddCategory(categoryName, categoryType);

            // Assert
            Assert.False(result);
            Assert.DoesNotContain(nameof(_mockView.DisplaySuccessMessage), _mockView.CalledMethods);
        }
        [Fact]
        public void AddCategory_CategoryDescriptionIsEmpty_ShouldReturnFalse()
        {
            // Arrange
            _presenter.GetDatabase("testDatabase.db");
            string categoryName = "";
            string categoryType = "Expense";

            // Act
            bool result = _presenter.AddCategory(categoryName, categoryType);

            // Assert
            Assert.False(result);
            Assert.Contains(nameof(_mockView.DisplayErrorMessage), _mockView.CalledMethods);
            Assert.Contains("Please enter a category name", _mockView.ErrorMessages.Last());
        }

        [Fact]
        public void AddCategory_CategoryTypeDoesntExist_ShouldReturnFalse()
        {

            // Arrange
            string goodDB = "testingdb.db";
            string messyDB = "MessDB.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            _presenter.GetDatabase(messyDB);
            string categoryName = "ProperName";
            string categoryType = "TypeDoesntExist";

            // Act
            bool result = _presenter.AddCategory(categoryName, categoryType);

            // Assert
            Assert.False(result);
            Assert.Contains(nameof(_mockView.DisplayErrorMessage), _mockView.CalledMethods);
            Assert.Contains("Invalid category type", _mockView.ErrorMessages.Last());

        }

        [Fact]
        public void AddCategory_CategoryAlreadyExist_ShouldReturnFalse()
        {
            // Arrange
            string goodDB = "testingdb.db";
            string messyDB = "MessDB.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            _presenter.GetDatabase(messyDB);

            // Act
            _presenter.AddCategory("YapYap", "Credit");
            bool result = _presenter.AddCategory("yapyap", "Credit");

            // Assert
            Assert.False(result);
            Assert.Contains(nameof(_mockView.DisplayErrorMessage), _mockView.CalledMethods);
            Assert.Contains("Failed to create category", _mockView.ErrorMessages.Last());

        }
        [Fact]
        public void AddCategory_VerificationTheCategoryIsCreated_ShouldReturnTrue()
        {
            // Arrange
            string goodDB = "testingdb.db";
            string messyDB = "MessDB.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            _presenter.GetDatabase(messyDB);
            string categoryName = "Travel";
            string categoryType = "Savings";

            // Act
            bool result = _presenter.AddCategory(categoryName, categoryType);
            Category category = _presenter.GetCategories().FirstOrDefault(c => c.Description == categoryName);

            // Assert
            Assert.True(result);
            Assert.NotNull(category);
            Assert.Equal(categoryName, category.Description);
            Assert.Equal(Category.CategoryType.Savings, category.Type);
            Assert.Contains(nameof(_mockView.DisplaySuccessMessage), _mockView.CalledMethods);
            Assert.Contains($"Category '{categoryName}' created successfully.", _mockView.SuccessMessages.Last());
        }

        //Test Delete
        [Fact]
        public void DeleteExpense_ValidId_ReturnsTrueAndSuccessMessage()
        {
            // Arrange
            string goodDB = "testingdb.db";
            string messyDB = "MessDB.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            _presenter.GetDatabase(messyDB);

            // Simulate items with at least one valid ID (e.g., 1, 2)
            _mockView.StubAllItems = new List<BudgetItem>
            {
                new BudgetItem { ExpenseID = 1 },
                new BudgetItem { ExpenseID = 2 }
            };

            // Act
            bool result = _presenter.DeleteExpense(1, out string message, () => { });

            // Assert
            Assert.True(result);
            Assert.Equal("Expense deleted successfully.", message);
            Assert.Contains(nameof(_mockView.GetAllItems), _mockView.CalledMethods);
            Assert.Contains(nameof(_mockView.ReselectExpenseOnceUpdated), _mockView.CalledMethods);
            Assert.Contains(2, _mockView.UpdatedExpenseIds); // Next ID should be selected
        }

        [Fact]
        public void DeleteExpense_InvalidDatabase_ReturnsFalseAndErrorMessage()
        {
            // Arrange
            string message;

            // Act
            bool result = _presenter.DeleteExpense(1, out message, () => { }); // No DB initialized

            // Assert
            Assert.False(result);
            Assert.Equal("Database not initialized.", message);
            Assert.DoesNotContain(nameof(_mockView.ReselectExpenseOnceUpdated), _mockView.CalledMethods);
        }

        //Test UpdateExpense
        [Fact]
        public void UpdateExistingExpense_ValidInputs_ReturnsTrueAndSuccessMessage()
        {
            // Arrange
            string goodDB = "testingdb.db";
            string messyDB = "MessDB.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            _presenter.GetDatabase(messyDB);
            int expenseId = 1;
            string name = "Updated name";
            string amount = "99,99";
            DateTime date = DateTime.Today;

            _presenter.ProcessNewAddExpense(date, "woah", 12, "Clothes");       //Making sure there is not nothing

            string categories = _presenter.GetAllCategoryNames()[0];

            // Act
            bool result = _presenter.UpdateExistingExpense(expenseId, name, amount, date, categories, out string message);

            // Assert
            Assert.True(result);
            Assert.Equal("Expense updated successfullly!!", message);
            Assert.DoesNotContain(nameof(_mockView.DisplayErrorMessage), _mockView.CalledMethods);
        }
        [Fact]
        public void UpdateExistingExpense_MissingFields_ReturnsFalse()
        {
            // Arrange
            _presenter.GetDatabase("testingdb.db");
            string name = "Updated name";
            string amount = "99.99";
            DateTime date = DateTime.Today;
            string category = null;

            // Act
            bool result = _presenter.UpdateExistingExpense(2, name, amount, DateTime.Today, category, out string message);

            // Assert
            Assert.False(result);
            Assert.Equal("Please fill in all fields.", message);
        }

        [Fact]
        public void UpdateExistingExpense_InvalidAmount_ReturnsFalse()
        {
            // Arrange
            _presenter.GetDatabase("testingdb.db");

            // Act
            bool result = _presenter.UpdateExistingExpense(1, "Lunch", "notanumber", DateTime.Today, "Food", out string message);

            // Assert
            Assert.False(result);
            Assert.Equal("Invalid amount.", message);
        }

        //Test GetCategoryName
        [Fact]
        public void GetCategoryName_ValidId_ReturnsCorrectName()
        {
            // Arrange
            _presenter.GetDatabase("testingdb.db");
            Category category;
            _presenter.AddCategory("newCat", "Income");
            category = _presenter.GetCategory("newCat");

            // Act
            string name = _presenter.GetCategoryName(category.Id);

            // Assert
            Assert.Equal("newCat", name);
        }
        [Fact]
        public void GetCategoryName_InvalidId_ThrowsException()
        {
            // Arrange
            _presenter.GetDatabase("testingdb.db");

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _presenter.GetCategoryName(-1));
            Assert.Contains("category", ex.Message, StringComparison.OrdinalIgnoreCase);
        }


        //Test FindCategory
        [Fact]
        public void FindCategory_CategoryExists_ReturnsTrue()
        {
            // Arrange
            _presenter.GetDatabase("testDatabase.db");
            string categories = _presenter.GetAllCategoryNames()[0].ToLower();

            // Act
            bool result = _presenter.FindCategory(categories); // case-insensitive

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void FindCategory_CategoryDoesNotExist_ReturnsFalse()
        {
            // Arrange
            _presenter.GetDatabase("testDatabase.db");

            // Act
            bool result = _presenter.FindCategory("Nonexistent");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void FindCategory_EmptyString_ReturnsFalse()
        {
            // Arrange
            _presenter.GetDatabase("testDatabase.db");

            // Act
            bool result = _presenter.FindCategory("");

            // Assert
            Assert.False(result);
        }

        //Test FilterByDate
        [Fact]
        public void FilterByDate_ShouldShowError_WhenDateRangeIsInvalid()
        {
            // Arrange
            _mockView.StubStartDate = null;
            _mockView.StubEndDate = DateTime.Now;
            _presenter.GetDatabase("testDatabase.db");

            // Act
            _presenter.FilterByDate();

            // Assert
            Assert.Contains(nameof(_mockView.DisplayErrorMessage), _mockView.CalledMethods);
            Assert.Contains("Please select a valid date range", _mockView.ErrorMessages.Last());
        }
        [Fact]
        public void FilterByDate_ShouldDisplayRawItems_WhenNoSummaryIsSelected()
        {
            // Arrange
            _mockView.StubStartDate = new DateTime(2025, 1, 1);
            _mockView.StubEndDate = new DateTime(2025, 12, 31);
            _mockView.StubDisplayByCategorySummary = false;
            _mockView.StubDisplayByMonthSummary = false;
            _mockView.StubIsCategoryFilter = false;
            _mockView.StubRenameSelectedCategory = "";
            _budget = new HomeBudget("testDatabase.db", true);

            _presenter.GetDatabase("testDatabase.db");


            // Add test data
            _presenter.AddCategory("TEST", "Debit");
            _budget.expenses.Add(new DateTime(2025, 5, 1), 100, "TestItem", _budget.categories.List().First().Id);

            // Act
            _presenter.FilterByDate();

            // Assert
            Assert.Contains(nameof(_mockView.DisplayItems), _mockView.CalledMethods);
            Assert.NotEmpty(_mockView.DisplayedItems.Last());
        }
        [Fact]
        public void FilterByDate_ShouldFilterByCategory_WhenCategoryFilterIsOn()
        {
            // Arrange
            _mockView.StubStartDate = new DateTime(2025, 1, 1);
            _mockView.StubEndDate = new DateTime(2025, 12, 31);
            _mockView.StubDisplayByCategorySummary = false;
            _mockView.StubDisplayByMonthSummary = false;
            _mockView.StubIsCategoryFilter = true;
            _mockView.StubRenameSelectedCategory = "TEST";
            _budget = new HomeBudget("testDatabase.db", true);

            _presenter.GetDatabase("testDatabase.db");
            _presenter.AddCategory("TEST", "Debit");
            var catId = _budget.categories.List().First().Id;
            _budget.expenses.Add(new DateTime(2025, 5, 1), 100, "FilteredItem", catId);

            // Act
            _presenter.FilterByDate();

            // Assert
            var displayed = _mockView.DisplayedItems.Last();
            Assert.All(displayed, item => Assert.Equal("TEST", item.Category));
        }

        [Fact]
        public void FilterByDate_ShouldDisplayRawItems_ByCategoriesAndMonth()
        {
            // Arrange
            _mockView.StubStartDate = new DateTime(2024, 1, 1);
            _mockView.StubEndDate = new DateTime(2024, 1, 31);
            _mockView.StubDisplayByCategorySummary = true;
            _mockView.StubDisplayByMonthSummary = true;
            _mockView.StubIsCategoryFilter = false;
            _mockView.StubRenameSelectedCategory = "TEST";

            _budget = new HomeBudget("testDatabase.db", true); 
            _presenter.GetDatabase("testDatabase.db");

            _presenter.AddCategory("TEST", "Income");
            int testCatId = _budget.categories.List().First(c => c.Description == "TEST").Id;

            _presenter.FilterByDate();
            int numberOfItems = _mockView.DisplayedItems.Last().Count;

            // Add 2 new expenses that should appear in the filter
            _budget.expenses.Add(new DateTime(2024, 1, 2), 100, "TestItem", testCatId);
            _budget.expenses.Add(new DateTime(2024, 1, 6), 100, "TestItem2", testCatId);
            _budget.expenses.Add(new DateTime(2024, 2, 6), 100, "TestItem3", testCatId);

            // Act
            _presenter.FilterByDate();

            // Assert
            Assert.Contains(nameof(_mockView.DisplayItems), _mockView.CalledMethods);
            Assert.NotEmpty(_mockView.DisplayedItems.Last());
            Assert.True(_mockView.DisplayedItems.Last().Count >= numberOfItems);
        }

        [Fact]
        public void FilterByDate_ShouldDisplayRawItems_ByCategories()
        {
            // Arrange
            _mockView.StubStartDate = new DateTime(2024, 1, 1);
            _mockView.StubEndDate = new DateTime(2024, 12, 31);
            _mockView.StubDisplayByCategorySummary = true;
            _mockView.StubDisplayByMonthSummary = false;
            _mockView.StubIsCategoryFilter = false;
            _mockView.StubRenameSelectedCategory = "TEST";

            _budget = new HomeBudget("testDatabase.db", true);
            _presenter.GetDatabase("testDatabase.db");
            _presenter.FilterByDate();
            int numberOfItems = _mockView.DisplayedItems.Last().Count;

            // Add test data
            _presenter.AddCategory("TEST", "Income");
            int testCatId = _budget.categories.List().First().Id;
            _budget.expenses.Add(new DateTime(2024, 5, 1), 100, "TestItem", testCatId);

            // Act
            _presenter.FilterByDate();

            // Assert
            Assert.Contains(nameof(_mockView.DisplayItems), _mockView.CalledMethods);
            Assert.NotEmpty(_mockView.DisplayedItems.Last());
            Assert.Equal(_mockView.DisplayedItems.Last().Count, numberOfItems + 1);
        }

        [Fact]
        public void FilterByDate_ShouldDisplayRawItems_ByMonth()
        {
            // Arrange
            _mockView.StubStartDate = new DateTime(2024, 1, 1);
            _mockView.StubEndDate = new DateTime(2024, 12, 31);
            _mockView.StubDisplayByCategorySummary = false;
            _mockView.StubDisplayByMonthSummary = true;
            _mockView.StubIsCategoryFilter = false;
            _mockView.StubRenameSelectedCategory = "TEST";

            _budget = new HomeBudget("testDatabase.db", true);
            _presenter.GetDatabase("testDatabase.db");

            _presenter.FilterByDate();
            int numberOfItems = _mockView.DisplayedItems.Last().Count;

            // Add test data
            _presenter.AddCategory("TEST", "Income");
            int testCatId = _budget.categories.List().First().Id;
            _budget.expenses.Add(new DateTime(2024, 5, 1), 100, "TestItem", testCatId);

            // Act
            _presenter.FilterByDate();

            // Assert
            Assert.Contains(nameof(_mockView.DisplayItems), _mockView.CalledMethods);
            Assert.NotEmpty(_mockView.DisplayedItems.Last());
            Assert.True(_mockView.DisplayedItems.Last().Count >= numberOfItems);
        }
        //Test GetSummaryTable
        [Fact]
        public void GetSummaryTable_ShouldShowError_WhenBudgetIsNull()
        {
            // Arrange
            Presenter falsePresenter = new Presenter(_mockView);        // no database initialized

            // Act
            var result = falsePresenter.GetSummaryTable(true, true, DateTime.Now, DateTime.Now);

            // Assert
            Assert.Empty(result);
            Assert.Contains("Database not initialized", _mockView.ErrorMessages.Last());
            Assert.Contains(nameof(_mockView.DisplayErrorMessage), _mockView.CalledMethods);
        }
        [Fact]
        public void GetSummaryTable_ShouldShowError_WhenDatesAreNull()
        {
            // Arrange
            _presenter.GetDatabase("test.db");

            // Act
            var result = _presenter.GetSummaryTable(true, true, null, DateTime.Now);

            // Assert
            Assert.Empty(result);
            Assert.Contains("Both start and end dates must be selected", _mockView.ErrorMessages.Last());
        }

        [Fact]
        public void GetSummaryTable_ShouldReturnRawItems_WhenNoSummaryFlagsSet()
        {
            // Arrange
            _budget = new HomeBudget("test.db", true);
            _presenter.GetDatabase("test.db");
            _presenter.AddCategory("Groceries", "Debit");
            var catId = _budget.categories.List().First().Id;
            _budget.expenses.Add(new DateTime(2025, 5, 5), 40, "Milk", catId);

            // Act
            var result = _presenter.GetSummaryTable(false, false, new DateTime(2025, 1, 1), new DateTime(2025, 12, 31));

            // Assert
            Assert.Single(result);
            Assert.IsType<BudgetItem>(result[0]);
        }
        [Fact]
        public void GetSummaryTable_ShouldReturnMonthlySummary_WhenByMonthTrue()
        {
            // Arrange
            _budget = new HomeBudget("test.db", true);
            _budget.categories.Add("Gnomes", _budget.categories.GetCategoryFromId(1).Type);
            var catId = _budget.categories.List().First().Id;
            _budget.expenses.Add(new DateTime(2025, 3, 15), 75, "Electricity", catId);
            _budget.expenses.Add(new DateTime(2025, 3, 20), 25, "Water", catId);

            _presenter.GetDatabase("test.db");

            // Act
            var result = _presenter.GetSummaryTable(true, false, new DateTime(2025, 1, 1), new DateTime(2025, 12, 31));

            // Assert
            Assert.Single(result);
            var row = result[0];
            var month = row.GetType().GetProperty("Month")?.GetValue(row);
            var total = row.GetType().GetProperty("Total")?.GetValue(row);

            Assert.Equal("2025-03", month);
            Assert.Equal(100.0, total);
        }

        [Fact]
        public void GetSummaryTable_ShouldReturnCategorySummary_WhenByCategoryTrue()
        {
            // Arrange
            _budget = new HomeBudget("test.db", true); // reset DB
            _budget.categories.Add("Books", _budget.categories.GetCategoryFromId(1).Type);
            var catId = _budget.categories.List().First(c => c.Description == "Books").Id;
            _budget.expenses.Add(new DateTime(2025, 6, 1), 60, "Textbooks", catId);
            _presenter.GetDatabase("test.db"); // Presenter gets access to updated DB

            // Act
            var result = _presenter.GetSummaryTable(false, true, new DateTime(2025, 1, 1), new DateTime(2025, 12, 31));

            // Assert
            Assert.Single(result);
            var row = result[0];
            var category = row.GetType().GetProperty("Category")?.GetValue(row);
            var total = row.GetType().GetProperty("Total")?.GetValue(row);

            Assert.Equal("Books", category);
            Assert.Equal(60.0, total);
        }

        [Fact]
        public void GetSummaryTable_ShouldReturnTableAndTotal_WhenByMonthAndByCategory()
        {
            // Arrange
            _budget = new HomeBudget("test.db", true); // reset db
            _budget.categories.Add("Books", _budget.categories.GetCategoryFromId(1).Type);
            _budget.categories.Add("Drinks", _budget.categories.GetCategoryFromId(1).Type);

            var foodId = _budget.categories.List().First(c => c.Description == "Books").Id;
            var travelId = _budget.categories.List().First(c => c.Description == "Drinks").Id;

            _budget.expenses.Add(new DateTime(2025, 2, 10), 50, "Lunch", foodId);
            _budget.expenses.Add(new DateTime(2025, 2, 15), 100, "Train", travelId);

            _presenter.GetDatabase("test.db");

            // Act
            var result = _presenter.GetSummaryTable(true, true, new DateTime(2025, 1, 1), new DateTime(2025, 12, 31));

            // Assert
            Assert.Equal(2, result.Count); // one row for February, one for TOTALS

            var totalRow = Assert.IsType<Dictionary<string, object>>(result.Last());
            Assert.Equal("TOTALS", totalRow["Month"]);
            Assert.Equal(50.0, Convert.ToDouble(totalRow["Books"]));
            Assert.Equal(100.0, Convert.ToDouble(totalRow["Drinks"]));
        }

        //Test UpdateExpense
        [Fact]
        public void UpdateExpense_ShouldCallDisplayExpenseUpdate_AndInvokeCallback()
        {
            // Arrange
            MockView mockView = new MockView(); // your mock view that tracks calls
            Presenter presenter = new Presenter(mockView);
            Expense expense = new Expense(1, DateTime.Now, 1, 12, "woahwy");
            bool callbackInvoked = false;

            // Act
            presenter.UpdateExpense(expense, () => callbackInvoked = true);

            // Assert
            Assert.Contains(expense, mockView.ExpenseUpdateCalls);
            Assert.Contains(nameof(mockView.DisplayExpenseUpdate), mockView.CalledMethods);
            Assert.True(callbackInvoked);
        }

        //Test GetNextOrPreviousExpenseId
        [Fact]
        public void GetNextOrPreviousExpenseId_ShouldReturnMinusOne_WhenListIsNull()
        {
            var result = _presenter.GetNextOrPreviousExpenseId(null, 1);
            Assert.Equal(-1, result);
        }

        [Fact]
        public void GetNextOrPreviousExpenseId_ShouldReturnMinusOne_WhenListIsEmpty()
        {
            var result = _presenter.GetNextOrPreviousExpenseId(new List<BudgetItem>(), 1);
            Assert.Equal(-1, result);
        }

        [Fact]
        public void GetNextOrPreviousExpenseId_ShouldReturnMinusOne_WhenDeletedIdNotFound()
        {
            var items = new List<BudgetItem>
            {
                new BudgetItem { ExpenseID = 1 },
                new BudgetItem { ExpenseID = 2 }
            };
            var result = _presenter.GetNextOrPreviousExpenseId(items, 99);
            Assert.Equal(-1, result);
        }

        [Fact]
        public void GetNextOrPreviousExpenseId_ShouldReturnNextId_WhenNextExists()
        {
            var items = new List<BudgetItem>
            {
                new BudgetItem { ExpenseID = 10 },
                new BudgetItem { ExpenseID = 20 },
                new BudgetItem { ExpenseID = 30 }
            };
            var result = _presenter.GetNextOrPreviousExpenseId(items, 20);
            Assert.Equal(30, result);
        }

        [Fact]
        public void GetNextOrPreviousExpenseId_ShouldReturnPreviousId_WhenNoNextExists()
        {
            var items = new List<BudgetItem>
            {
                new BudgetItem { ExpenseID = 10 },
                new BudgetItem { ExpenseID = 20 },
                new BudgetItem { ExpenseID = 30 }
            };
            var result = _presenter.GetNextOrPreviousExpenseId(items, 30);
            Assert.Equal(20, result);
        }

        [Fact]
        public void GetNextOrPreviousExpenseId_ShouldReturnMinusOne_WhenOnlyOneItem()
        {
            var items = new List<BudgetItem>
            {
                new BudgetItem { ExpenseID = 42 }
            };
            var result = _presenter.GetNextOrPreviousExpenseId(items, 42);
            Assert.Equal(-1, result);
        }

        //Test DisplayChartIfEnable
        [Fact]
        public void DisplayChartIfEnabled_ShouldShowChart_WhenBothFiltersEnabled()
        {
            // Arrange
            _mockView.StubDisplayByMonthSummary = true;
            _mockView.StubDisplayByCategorySummary = true;
            _mockView.StubStartDate = new DateTime(2025, 1, 1);
            _mockView.StubEndDate = new DateTime(2025, 12, 31);
            _presenter.GetDatabase("test.db");

            // Act
            _presenter.DisplayChartIfEnabled();

            // Assert
            Assert.Contains(nameof(_mockView.DisplayByMonthSummary), _mockView.CalledMethods);
            Assert.Contains(nameof(_mockView.DisplayByCategorySummary), _mockView.CalledMethods);
            Assert.Contains(nameof(_mockView.ShowChart), _mockView.CalledMethods);
            Assert.Empty(_mockView.CalledMethods.Where(m => m == nameof(_mockView.HideChart)));
            Assert.Single(_mockView.ChartCalls); // Verify ShowChart was called once
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void DisplayChartIfEnabled_ShouldHideChart_WhenAnyFilterDisabled(bool byMonth, bool byCategory)
        {
            // Arrange
            _mockView.StubDisplayByMonthSummary = byMonth;
            _mockView.StubDisplayByCategorySummary = byCategory;
            _presenter.GetDatabase("test.db");

            // Act
            _presenter.DisplayChartIfEnabled();

            // Assert
            Assert.Contains(nameof(_mockView.HideChart), _mockView.CalledMethods);
            Assert.DoesNotContain(nameof(_mockView.ShowChart), _mockView.CalledMethods);
            Assert.Empty(_mockView.ChartCalls);
        }

        [Fact]
        public void DisplayChartIfEnabled_ShouldDoNothing_WhenBudgetIsNull()
        {
            // Act
            _presenter.DisplayChartIfEnabled();

            // Assert
            Assert.DoesNotContain(nameof(_mockView.ShowChart), _mockView.CalledMethods);
            Assert.DoesNotContain(nameof(_mockView.HideChart), _mockView.CalledMethods);
        }

        //Test GetGroupedExpensesByMonthAndCategory
        [Fact]
        public void GetGroupedExpensesByMonthAndCategory_ShouldGroupCorrectlyAndReturnCategoryTotals()
        {
            // Arrange
            _budget = new HomeBudget("test.db", true);
            _presenter.GetDatabase("test.db");

            // Add categories
            _presenter.AddCategory("Food", "Income");
            _presenter.AddCategory("Transport", "Income");
            var foodId = _budget.categories.List().First(c => c.Description == "Food").Id;
            var transportId = _budget.categories.List().First(c => c.Description == "Transport").Id;

            // Add expenses in two different months
            _budget.expenses.Add(new DateTime(2025, 1, 5), 20.50, "Lunch", foodId);
            _budget.expenses.Add(new DateTime(2025, 1, 10), 15.25, "Dinner", foodId);
            _budget.expenses.Add(new DateTime(2025, 1, 15), 40.00, "Bus", transportId);
            _budget.expenses.Add(new DateTime(2025, 2, 5), 30.00, "Groceries", foodId);

            // Act
            var result = _presenter.GetGroupedExpensesByMonthAndCategory(
                new DateTime(2025, 1, 1),
                new DateTime(2025, 12, 31)
            );

            // Assert
            Assert.Equal(2, result.Count); // Two months

            var janRow = result[0];
            Assert.Equal("2025-01", janRow["Month"]);
            Assert.Equal(35.75, Convert.ToDouble(janRow["Food"]));      // 20.50 + 15.25
            Assert.Equal(40.00, Convert.ToDouble(janRow["Transport"])); // 40.00

            var febRow = result[1];
            Assert.Equal("2025-02", febRow["Month"]);
            Assert.Equal(30.00, Convert.ToDouble(febRow["Food"]));      // 30.00
            Assert.Equal(0.0, Convert.ToDouble(febRow["Transport"]));   // No Transport in Feb
        }
        [Fact]
        public void GetGroupedExpensesByMonthAndCategory_ShouldReturnEmptyList_WhenNoItemsInRange()
        {
            // Arrange
            _budget = new HomeBudget("test.db", true);
            _presenter.GetDatabase("test.db");

            // Add category but no expenses
            _presenter.AddCategory("Leisure", "Debit");

            // Act
            var result = _presenter.GetGroupedExpensesByMonthAndCategory(
                new DateTime(2030, 1, 1),
                new DateTime(2030, 12, 31)
            );

            // Assert
            Assert.Empty(result);
        }

    }
}
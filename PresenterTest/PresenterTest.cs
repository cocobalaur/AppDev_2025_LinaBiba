using Budget;
using BudgetModel;
using Views;

namespace PresenterTest
{
    public class PresenterTest
    {
        private readonly MockView _mockView;
        private readonly Presenter _presenter;

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
            Assert.DoesNotContain(nameof(_mockView.DisplaySuccessMessage), _mockView.CalledMethods);
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
            _presenter.AddCategory("Food", "Credit");
            bool result = _presenter.AddCategory("fOoD", "Credit");

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
            _presenter.GetDatabase("newTestingdb.db");
            int expenseId = 1;
            string name = "Updated name";
            string amount = "99.99";
            DateTime date = DateTime.Today;

            _presenter.ProcessNewAddExpense(date, "woah", 12, "Clothes");

            _presenter.AddCategory("newCat", "Income");

            // Act
            bool result = _presenter.UpdateExistingExpense(expenseId, name, amount, date, "newCat", out string message);

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
            Assert.Contains(nameof(_mockView.DisplayErrorMessage), _mockView.CalledMethods);
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

    }
}
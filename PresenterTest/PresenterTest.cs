using Budget;
using BudgetModel;
using static Budget.Category;

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
            _presenter.GetDatabase(databasePath);

            // Assert
            Assert.Equal("", _mockView.LastError);
        }

        [Fact]
        public void GetDatabase_InvalidPath_ShowsError()
        {
            // Arrange
            string invalidPath = "Z:\\nonexistent\\invalidfile.db";

            // Act
            _presenter.GetDatabase(invalidPath);

            // Assert
            Assert.Contains("Error setting up database", _mockView.LastError);
        }


        //Testing AddExpense
        [Fact]
        public void AddExpense_ShouldAddExpense_WhenBudgetIsInitialized()
        {
            // Arrange
            _presenter.GetDatabase("testingdb.db");

            DateTime date = new DateTime(2025, 4, 26);
            string name = "Coffee";
            double amount = 4.50;
            string categoryName = "Food";

            // Act
            _presenter.ProcessNewAddExpense(date, name, amount, categoryName);

            // Assert
            Assert.Equal("", _mockView.LastError);
        }
        [Fact]
        public void AddExpense_ShouldThrowError_WhenInformationIsWrong()
        {
            // Arrange
            _presenter.GetDatabase("testDatabase.db");


            DateTime date = new DateTime(2025, 4, 26);
            string name = "Coffee";
            double amount = 4.50;
            string categoryName = "";

            // Act
            _presenter.ProcessNewAddExpense(date, name, amount, categoryName);

            // Assert
            Assert.Contains("Error adding expense:", _mockView.LastError);
        }
        [Fact]
        public void AddExpense_ShouldShowExpense_WhenBudgetIsNotInitialized()
        {
            // Arrange
            DateTime date = new DateTime(2025, 4, 26);
            string name = "Coffee";
            double amount = 4.50;
            string categoryName = "Food";

            // Act
            _presenter.ProcessNewAddExpense(date, name, amount, categoryName);

            // Assert
            Assert.Equal("Database not initialized.", _mockView.LastError);
        }

        //Testing GetCategories
        [Fact]
        public void GetCategories_ShouldReturnCategories_WhenBudgetIsInitialized()
        {
            // Arrange
            _presenter.GetDatabase("testDatabase.db");

            //Act
            List<Category> categories = _presenter.GetCategories();

            // Assert
            Assert.NotNull(categories);             // Should return a list, even if it's empty
            Assert.Equal("", _mockView.LastError); // No error should be shown
        }

        [Fact]
        public void GetCategories_ShouldShowError_WhenBudgetIsNotInitialized()
        {
            //Act
            List<Category> categories = _presenter.GetCategories();

            // Assert
            Assert.Empty(categories);                                       //Making sure the list return is empty
            Assert.Equal("Database not initialized.", _mockView.LastError); // Error should be shown
        }
        //Testing SetCategoryType
        [Fact]
        public void SetCategoryType_ValidCategoryType_UpdatesSelectedCategoryType()
        {
            // Arrange
            int categoryTypeInt = (int)CategoryType.Income;

            // Act
            _presenter.SetCategoryType(categoryTypeInt);

            //Assert
            CategoryType selectedCategoryType = _presenter.GetSelectedCategoryType();
            Assert.Equal(CategoryType.Income, selectedCategoryType);
        }

        //Testing CreateOrGetCategory
        [Fact]
        public void CreateOrGetCategory_CategoryExists_ReturnCategory()
        {
            //Arrange
            _presenter.GetDatabase("testingdb.db");
            string cateogryDescription = "Income";

            //Act
            Category category = _presenter.CreateOrGetCategory(cateogryDescription);

            //Assert
            Assert.Equal(cateogryDescription, category.Description);
        }

        [Fact]
        public void CreateOrGetCategory_CategoryDoesNotExist_CreatesAndReturnNewCategory()
        {
            //Arrange
            _presenter.GetDatabase("testingdb.db");
            string cateogryDescription = "CategoryToCreate";

            //Act
            Category category = _presenter.CreateOrGetCategory(cateogryDescription);

            //Assert
            Assert.Equal(cateogryDescription, category.Description);
        }

        [Fact]
        public void CreateOrGetCategory_WithoutDatabase_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _presenter.CreateOrGetCategory("Groceries"));
        }
        //Testing AddCategory
        [Fact]
        public void AddCategory_HomeBudgetNotInitialize_ShouldReturnFalse()
        {

            //Arrange
            string categoryName = "WOah";
            string cateogoryType = "Income";

            //Act
            bool unsuccessAddCategory = _presenter.AddCategory(categoryName, cateogoryType);

            //Assert
            Assert.False(unsuccessAddCategory);
        }
        [Fact]
        public void AddCategory_CategoryDescriptionIsEmpty_ShouldReturnFalse()
        {
            //Arrange
            _presenter.GetDatabase("testDatabase.db");
            string categoryName = "";
            string cateogoryType = "Expense";

            //Act
            bool unsuccessAddCategory = _presenter.AddCategory(categoryName, cateogoryType);

            //Assert
            Assert.False(unsuccessAddCategory);
        }

        [Fact]
        public void AddCategory_CategoryTypeDoesntExist_ShouldReturnFalse()
        {

            //Arrange
            string goodDB = "newDB.db";
            string messyDB = "MessDB.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            _presenter.GetDatabase(messyDB);
            string categoryName = "ProperName";
            string cateogoryType = "TypeDoesntExist";

            //Act
            bool failfullAddCateory = _presenter.AddCategory(categoryName, cateogoryType);

            //Assert
            Assert.False(failfullAddCateory);

        }

        [Fact]
        public void AddCategory_CategoryAlreadyExist_ShouldReturnFalse()
        {
            //Arrange
            _presenter.GetDatabase("testDatabase.db");
            string categoryName = "INCOME";
            string cateogoryType = "Credit";

            //Act
            bool unsuccessAddCategory = _presenter.AddCategory(categoryName, cateogoryType);

            //Assert
            Assert.False(unsuccessAddCategory);
        }
        [Fact]
        public void AddCategory_VerificationTheCategoryIsCreated_ShouldReturnTrue()
        {
            //Arrange
            string goodDB = "testingdb.db";
            string messyDB = "MessDB.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            _presenter.GetDatabase(messyDB);
            string categoryName = "Travel";
            string cateogoryType = "Savings";

            //Act
            bool successfullAddCateory = _presenter.AddCategory(categoryName, cateogoryType);
            Category category = _presenter.GetCategories().FirstOrDefault(c => c.Description == categoryName);

            //Assert
            Assert.True(successfullAddCateory);
            Assert.Equal(categoryName, category.Description);
            Assert.Equal(Category.CategoryType.Savings, category.Type);
        }

        [Fact]
        public void AddCategoryThatAlreadyExists_VerificationTheCategoryShouldNotBeCreated_ShouldReturnFalse()
        {
            //Arrange
            _presenter.GetDatabase("testingdb.db");
            string categoryName = "Surgery";
            string cateogoryType = "Savings";

            //Act
            bool failAddCategory = _presenter.AddCategory(categoryName, cateogoryType);
            Category category = _presenter.GetCategories().FirstOrDefault(c => c.Description == categoryName);

            //Assert
            Assert.False(failAddCategory);
        }
    }
}
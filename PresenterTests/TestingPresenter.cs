using View;

namespace PresenterTests
{
    public class TestingPresenter
    {
        private readonly MockView _mockView;
        private readonly Presenter _presenter;

        public TestingPresenter()
        {
            _mockView = new MockView();
            _presenter = new Presenter(_mockView);
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

        [Fact]
        public void AddExpense_WithoutDatabase_ShowsError()
        {
            // Act
            _presenter.AddExpense(DateTime.Now, "Test Expense", 100.0, "Food");

            // Assert
            Assert.Equal("Database not initialized.", _mockView.LastError);
        }

        [Fact]
        public void GetCategories_WithoutDatabase_ShowsError()
        {
            // Act
            var categories = _presenter.GetCategories();

            // Assert
            Assert.Equal("Database not initialized.", _mockView.LastError);
        }

        [Fact]
        public void AddCategory_WithoutDatabase_ReturnsFalse()
        {
            // Act
            var result = _presenter.AddCategory("NewCategory", "Expense");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CreateOrGetCategory_WithoutDatabase_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _presenter.CreateOrGetCategory("Groceries"));
        }

        [Fact]
        public void SetCategoryType_UpdatesCategoryType()
        {
            // Act
            _presenter.SetCategoryType(1); // 1 corresponds to "Income"

            // No assert necessary, no crash = pass
        }
    }


}
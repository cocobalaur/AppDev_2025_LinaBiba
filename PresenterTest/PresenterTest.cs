using System;
using Xunit;
using System.IO;
using System.Collections.Generic;
using System.Data.SQLite;
using Views;
using BudgetModel;
using Budget;
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

        //Testing GetDatabase
        [Fact]
        public void GetDatabase_ShouldInitializeBudget_WhenDatabasePathIsValid()
        {
            // Arrange
            string databasePath = "testDatabase.db";

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
            _presenter.GetDatabase("testDatabase.db");


            DateTime date = new DateTime(2025, 4, 26);
            string name = "Coffee";
            double amount = 4.50;
            string categoryName = "Food";

            // Act
            _presenter.AddExpense(date, name, amount, categoryName);

            // Assert
            Assert.Equal("", _mockView.LastError);
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
            _presenter.AddExpense(date, name, amount, categoryName);

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


        }
        //Testing CreateOrGetCategory

        //Testing AddCategory
        }
}
using System;
using Xunit;
using Budget;

namespace BudgetCodeTests
{
    public class TestCategory
    {
        // ========================================================================

        [Fact]
        public void CategoryObject_New()
        {

            // Arrange
            string descr = "Clothing";
            int id = 42;
            Category.CategoryType type = Category.CategoryType.Credit;

            // Act
            Category category = new Category(id, descr, type);

            // Assert 
            Assert.IsType<Category>(category);
            Assert.Equal(id, category.Id);
            Assert.Equal(descr, category.Description);
            Assert.Equal(type, category.Type);
        }



        // ========================================================================

        [Fact]
        public void CategoryObject_New_WithDefaultType()
        {

            // Arrange
            string descr = "Clothing";
            int id = 42;
            Category.CategoryType defaultType = Category.CategoryType.Expense;

            // Act
            Category category = new Category(id, descr);

            // Assert 
            Assert.Equal(defaultType, category.Type);
        }

        // ========================================================================

        [Fact]
        public void CategoryObject_New_TypeIncome()
        {

            // Arrange
            string descr = "Work";
            int id = 42;
            Category.CategoryType type = Category.CategoryType.Income;

            // Act
            Category category = new Category(id, descr, type);

            // Assert 
            Assert.Equal(type, category.Type);

        }

        // ========================================================================

        [Fact]
        public void CategoryObjectType_New_Expense()
        {

            // Arrange
            string descr = "Eating Out";
            int id = 42;
            Category.CategoryType type = Category.CategoryType.Expense;

            // Act
            Category category = new Category(id, descr, type);

            // Assert 
            Assert.Equal(type, category.Type);

        }

        // ========================================================================

        [Fact]
        public void CategoryObject_New_TypeCredit()
        {

            // Arrange
            string descr = "MasterCard";
            int id = 42;
            Category.CategoryType type = Category.CategoryType.Credit;

            // Act
            Category category = new Category(id, descr, type);

            // Assert 
            Assert.Equal(type, category.Type);

        }

        // ========================================================================

        [Fact]
        public void CategoryObject_New_TypeSavings()
        {

            // Arrange
            string descr = "For House";
            int id = 42;
            Category.CategoryType type = Category.CategoryType.Savings;

            // Act
            Category category = new Category(id, descr, type);

            // Assert 
            Assert.Equal(type, category.Type);

        }

        // ========================================================================

        [Fact]
        public void CategoryObject_GetSetProperties()
        {
            // Question: why am I allowed to change the id?

            // Arrange
            string descr = "Eating Out";
            int id = 42;
            int newID = 15;
            string newDescr = "Restaurants";
            Category category = new Category(id, descr, Category.CategoryType.Expense);

            // Act
            category.Id = newID;
            category.Description = newDescr;
            category.Type = Category.CategoryType.Savings;

            // Assert 
            Assert.Equal(newID, category.Id);
            Assert.Equal(newDescr, category.Description);
            Assert.Equal(Category.CategoryType.Savings, category.Type);

        }



        // ========================================================================

        [Fact]
        public void CategoryObject_ToString()
        {

            // Arrange
            string descr = "Eating Out";
            int id = 42;

            // Act
            Category category = new Category(id, descr);

            // Assert 
            Assert.Equal(descr, category.ToString());
        }

    }
}


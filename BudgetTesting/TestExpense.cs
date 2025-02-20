using System;
using Xunit;
using Budget;

namespace BudgetCodeTests
{
    public class TestExpense
    {
        // ========================================================================

        [Fact]
        public void ExpenseObject_New()
        {

            // Arrange
            DateTime now = DateTime.Now;
            double amount = 24.55;
            string descr = "New Sweater";
            int category = 34;
            int id = 42;

            // Act
            Expense expense = new Expense(id, now, category, amount, descr);

            // Assert 
            Assert.IsType<Expense>(expense);

            Assert.Equal(id, expense.Id);
            Assert.Equal(amount, expense.Amount);
            Assert.Equal(descr, expense.Description);
            Assert.Equal(category, expense.Category);
            Assert.Equal(now, expense.Date);
        }

        // ========================================================================

        [Fact]
        public void ExpenseCopyConstructoryIsDeepCopy()
        {

            // Arrange
            DateTime now = DateTime.Now;
            double amount = 24.55;
            string descr = "New Sweater";
            int category = 34;
            int id = 42;
            Expense expense = new Expense(id, now, category, amount, descr);

            // Act
            Expense copy = new Expense(expense);
            copy.Amount = expense.Amount + 15;

            // Assert 
            Assert.Equal(id, expense.Id);
            Assert.NotEqual(amount, copy.Amount);
            Assert.Equal(expense.Amount + 15, copy.Amount);
            Assert.Equal(descr, expense.Description);
            Assert.Equal(category, expense.Category);
            Assert.Equal(now, expense.Date);
        }


        // ========================================================================

        [Fact]
        public void ExpenseObjectGetSetProperties()
        {
            // question - why cannot I not change the date of an expense.  What if I got the date wrong?

            // Arrange
            DateTime now = DateTime.Now;
            double amount = 24.55;
            string descr = "New Sweater";
            int category = 34;
            int id = 42;
            double newAmount = 54.55;
            string newDescr = "Angora Sweater";
            int newCategory = 38;

            Expense expense = new Expense(id, now, category, amount, descr);

            // Act
            expense.Amount = newAmount;
            expense.Category = newCategory;
            expense.Description = newDescr;

            // Assert 
            Assert.True(typeof(Expense).GetProperty("Date").CanWrite == false);
            Assert.True(typeof(Expense).GetProperty("Id").CanWrite == false);
            Assert.Equal(newAmount, expense.Amount);
            Assert.Equal(newDescr, expense.Description);
            Assert.Equal(newCategory, expense.Category);
        }


    }
}

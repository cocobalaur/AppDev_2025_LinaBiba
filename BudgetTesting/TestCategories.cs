using System;
using Xunit;
using System.IO;
using System.Collections.Generic;
using Budget;

namespace BudgetCodeTests
{
    public class TestCategories
    {
        public int numberOfCategoriesInFile = TestConstants.numberOfCategoriesInFile;
        public String testInputFile = TestConstants.testCategoriesInputFile;
        public int maxIDInCategoryInFile = TestConstants.maxIDInCategoryInFile;
        Category firstCategoryInFile = TestConstants.firstCategoryInFile;
        int IDWithSaveType = TestConstants.CategoryIDWithSaveType;

        // ========================================================================

        [Fact]
        public void CategoriesObject_New()
        {
            // Arrange

            // Act
            Categories categories = new Categories();

            // Assert 
            Assert.IsType<Categories>(categories);
            Assert.True(typeof(Categories).GetProperty("FileName").CanWrite == false);
            Assert.True(typeof(Categories).GetProperty("DirName").CanWrite == false);

        }

        // ========================================================================

        [Fact]
        public void CategoriesObject_New_CreatesDefaultCategories()
        {
            // Arrange

            // Act
            Categories categories = new Categories();

            // Assert 
            Assert.False(categories.List().Count == 0, "Non zero categories");

        }


        // ========================================================================

        [Fact]
        public void CategoriesMethod_ReadFromFile_NotExist_ThrowsException()
        {
            // Arrange
            String badFile = "abc.txt";
            Categories categories = new Categories();

            // Act and Assert
            Assert.Throws<System.IO.FileNotFoundException>(() => categories.ReadFromFile(badFile));

        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_ReadFromFile_ValidateCorrectDataWasRead()
        {
            // Arrange
            String dir = GetSolutionDir();
            Categories categories = new Categories();

            // Act
            categories.ReadFromFile(dir + "\\" + testInputFile);
            List<Category> list = categories.List();
            Category firstCategory = list[0];

            // Assert
            Assert.Equal(numberOfCategoriesInFile, list.Count);
            Assert.Equal(firstCategoryInFile.Id, firstCategory.Id);
            Assert.Equal(firstCategoryInFile.Description, firstCategory.Description);

            String fileDir = Path.GetFullPath(Path.Combine(categories.DirName, ".\\"));
            Assert.Equal(dir, fileDir);
            Assert.Equal(testInputFile, categories.FileName);

        }

        // ========================================================================

        [Fact]
        public void Categories_TypeSavingsReadCorrectlyFromFile()
        {
            // Bug: failed test where data was written to another file, category Savings was changed
            // checking here to see if it is read correctly in an effort to debug

            // Arrange
            String dir = GetSolutionDir();
            Categories categories = new Categories();
            categories.ReadFromFile(dir + "\\" + testInputFile);
            List<Category> list = categories.List();

            // Act
            Category category = categories.GetCategoryFromId(IDWithSaveType);

            // Assert
            Assert.Equal(Category.CategoryType.Savings, category.Type);

        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_List_ReturnsListOfCategories()
        {
            // Arrange
            String dir = GetSolutionDir();
            Categories categories = new Categories();
            categories.ReadFromFile(dir + "\\" + testInputFile);

            // Act
            List<Category> list = categories.List();

            // Assert
            Assert.Equal(numberOfCategoriesInFile, list.Count);

        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_List_ModifyListDoesNotModifyCategoriesInstance()
        {
            // Arrange
            String dir = GetSolutionDir();
            Categories categories = new Categories();
            categories.ReadFromFile(dir + "\\" + testInputFile);
            List<Category> list = categories.List();

            // Act
            list[0].Type = Category.CategoryType.Credit;

            // Assert
            Assert.NotEqual(list[0].Type, categories.List()[0].Type);

        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_Add()
        {
            // Arrange
            String dir = GetSolutionDir();
            Categories categories = new Categories();
            categories.ReadFromFile(dir + "\\" + testInputFile);
            string descr = "New Category";
            Category.CategoryType type = Category.CategoryType.Income;

            // Act
            categories.Add(descr,type);
            List<Category> categoriesList = categories.List();
            int sizeOfList = categories.List().Count;

            // Assert
            Assert.Equal(numberOfCategoriesInFile + 1, sizeOfList);
            Assert.Equal(maxIDInCategoryInFile + 1, categoriesList[sizeOfList - 1].Id);
            Assert.Equal(descr, categoriesList[sizeOfList - 1].Description);

        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_Delete()
        {
            // Arrange
            String dir = GetSolutionDir();
            Categories categories = new Categories();
            categories.ReadFromFile(dir + "\\" + testInputFile);
            int IdToDelete = 3;

            // Act
            categories.Delete(IdToDelete);
            List<Category> categoriesList = categories.List();
            int sizeOfList = categoriesList.Count;

            // Assert
            Assert.Equal(numberOfCategoriesInFile - 1, sizeOfList);
            Assert.False(categoriesList.Exists(e => e.Id == IdToDelete), "correct Category item deleted");

        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_Delete_InvalidIDDoesntCrash()
        {
            // Arrange
            String dir = GetSolutionDir();
            Categories categories = new Categories();
            int IdToDelete = 9999;
            int sizeOfList = categories.List().Count;

            // Act
            try
            {
                categories.Delete(IdToDelete);
                Assert.Equal(sizeOfList, categories.List().Count);
            }

            // Assert
            catch
            {
                Assert.True(false, "Invalid ID causes Delete to break");
            }
        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_WriteToFile()
        {

            // NOTE: Currently failing.  Added new test to try to track down source of 
            //       problem
            // CategoryTypeSavingsReadCorrectlyFromFile()
            //  ... which also fails, so that is why the WriteToFile is not accurate...
            //  ... fix above test, and then this one should pass as well.

            // Arrange
            String dir = GetSolutionDir();
            Categories categories = new Categories();
            categories.ReadFromFile(dir + "\\" + testInputFile);
            string fileName = TestConstants.CategoriesOutputTestFile;
            String outputFile = dir + "\\" + fileName;
            File.Delete(outputFile);

            // Act
            categories.SaveToFile(outputFile);

            // Assert
            Assert.True(File.Exists(outputFile), "output file created");
            Assert.True(FileEquals(dir + "\\" + testInputFile, outputFile), "Input /output files are the same");
            String fileDir = Path.GetFullPath(Path.Combine(categories.DirName, ".\\"));
            Assert.Equal(dir, fileDir);
            Assert.Equal(fileName, categories.FileName);

            // Cleanup
            if (FileEquals(dir + "\\" + testInputFile, outputFile)) {
                File.Delete(outputFile);
            }

        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_WriteToFile_WriteToLastFileWrittenToByDefault()
        {
            // Arrange
            String dir = GetSolutionDir();
            Categories categories = new Categories();
            categories.ReadFromFile(dir + "\\" + testInputFile);
            string fileName = TestConstants.CategoriesOutputTestFile;
            String outputFile = dir + "\\" + fileName;
            File.Delete(outputFile);
            categories.SaveToFile(outputFile); // output file is now last file that was written to.
            File.Delete(outputFile);  // Delete the file

            // Act
            categories.SaveToFile(); // should write to same file as before

            // Assert
            Assert.True(File.Exists(outputFile), "output file created");
            String fileDir = Path.GetFullPath(Path.Combine(categories.DirName, ".\\"));
            Assert.Equal(dir, fileDir);
            Assert.Equal(fileName, categories.FileName);

            // Cleanup
            if (FileEquals(dir + "\\" + testInputFile, outputFile))
            {
                File.Delete(outputFile);
            }

        }


        // ========================================================================

        [Fact]
        public void CategoriesMethod_GetCategoryFromId()
        {
            // Arrange
            String dir = GetSolutionDir();
            Categories categories = new Categories();
            categories.ReadFromFile(dir + "\\" + testInputFile);
            int catID = 15;

            // Act
            Category category = categories.GetCategoryFromId(catID);

            // Assert
            Assert.Equal(catID,category.Id);

        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_SetCategoriesToDefaults()
        {

           // Arrange
            Categories categories = new Categories();
            List<Category> originalList = categories.List();

            // modify list of categories
            categories.Delete(1);
            categories.Delete(2);
            categories.Delete(3);
            categories.Add("Another one ", Category.CategoryType.Credit);

            //"just double check that initial conditions are correct");
            Assert.NotEqual(originalList.Count, categories.List().Count);

            // Act
            categories.SetCategoriesToDefaults();

            // Assert
            Assert.Equal(originalList.Count, categories.List().Count);
            foreach (Category defaultCat in originalList)
            {
                Assert.True(categories.List().Exists(c => c.Description == defaultCat.Description && c.Type == defaultCat.Type));
            }

        }

        // -------------------------------------------------------
        // helpful functions, ... they are not tests
        // -------------------------------------------------------

        private String GetSolutionDir()
        {

            // this is valid for C# .Net Foundation (not for C# .Net Core)
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\"));
        }

        // source taken from: https://www.dotnetperls.com/file-equals

        private bool FileEquals(string path1, string path2)
        {
            byte[] file1 = File.ReadAllBytes(path1);
            byte[] file2 = File.ReadAllBytes(path2);
            if (file1.Length == file2.Length)
            {
                for (int i = 0; i < file1.Length; i++)
                {
                    if (file1[i] != file2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}


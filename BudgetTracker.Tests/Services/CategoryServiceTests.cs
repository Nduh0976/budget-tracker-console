using BudgetTracker.Data.Interfaces;
using BudgetTracker.Models;
using BudgetTracker.Services;
using Moq;

namespace BudgetTracker.Tests.Services
{
    [TestFixture]
    public class CategoryServiceTests
    {
        private Mock<IDataStore> _mockDataStore;
        private CategoryService _categoryService;

        [SetUp]
        public void Setup()
        {
            _mockDataStore = new Mock<IDataStore>();
            _categoryService = new CategoryService(_mockDataStore.Object);
        }

        #region CreateCategory Tests

        [Test]
        public void CreateCategory_WithValidName_ReturnsSuccessResponse()
        {
            // Arrange
            var name = "Test Category";
            var expectedId = 1;

            _mockDataStore.Setup(x => x.GenerateNextCategoryId()).Returns(expectedId);
            _mockDataStore.Setup(x => x.CategoryExists(name)).Returns(false);
            _mockDataStore.Setup(x => x.AddCategory(It.IsAny<Category>()));

            // Act
            var result = _categoryService.CreateCategory(name);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo($"'{name}' Category has been successfully created."));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data.Id, Is.EqualTo(expectedId));
            Assert.That(result.Data.Name, Is.EqualTo(name));

            _mockDataStore.Verify(x => x.AddCategory(It.Is<Category>(c =>
                c.Id == expectedId &&
                c.Name == name)), Times.Once);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void CreateCategory_WithInvalidName_ReturnsFailureResponse(string? invalidName)
        {
            // Act
            var result = _categoryService.CreateCategory(invalidName);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Name cannot be empty or whitespace."));
            Assert.That(result.Data, Is.Null);

            _mockDataStore.Verify(x => x.AddCategory(It.IsAny<Category>()), Times.Never);
        }

        [Test]
        public void CreateCategory_WithExistingName_ReturnsFailureResponse()
        {
            // Arrange
            var name = "Existing Category";
            _mockDataStore.Setup(x => x.CategoryExists(name)).Returns(true);

            // Act
            var result = _categoryService.CreateCategory(name);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo($"A category with the name '{name}' already exists."));
            Assert.That(result.Data, Is.Null);

            _mockDataStore.Verify(x => x.AddCategory(It.IsAny<Category>()), Times.Never);
        }

        [Test]
        public void CreateCategory_WithNameContainingWhitespace_PassesOriginalNameToDataStore()
        {
            // Arrange
            var nameWithSpaces = "  Test Category  ";
            var expectedId = 1;

            _mockDataStore.Setup(x => x.GenerateNextCategoryId()).Returns(expectedId);
            _mockDataStore.Setup(x => x.CategoryExists(nameWithSpaces)).Returns(false);
            _mockDataStore.Setup(x => x.AddCategory(It.IsAny<Category>()));

            // Act
            var result = _categoryService.CreateCategory(nameWithSpaces);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data.Name, Is.EqualTo(nameWithSpaces));
            _mockDataStore.Verify(x => x.CategoryExists(nameWithSpaces), Times.Once);
        }

        #endregion

        #region DeleteCategory Tests

        [Test]
        public void DeleteCategory_WithSelectedCategory_CallsDataStoreRemove()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { Id = categoryId, Name = "Test Category" };
            _mockDataStore.Setup(x => x.GetCategoryById(categoryId)).Returns(category);
            _mockDataStore.Setup(x => x.RemoveCategory(categoryId)).Returns(true);

            // Act
            _categoryService.SetSelectedCategoryById(categoryId);
            var result = _categoryService.DeleteCategory();

            // Assert
            Assert.That(result, Is.True);
            _mockDataStore.Verify(x => x.RemoveCategory(categoryId), Times.Once);
        }

        [Test]
        public void DeleteCategory_WhenRemoveFails_ReturnsFalse()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { Id = categoryId, Name = "Test Category" };
            _mockDataStore.Setup(x => x.GetCategoryById(categoryId)).Returns(category);
            _mockDataStore.Setup(x => x.RemoveCategory(categoryId)).Returns(false);

            // Act
            _categoryService.SetSelectedCategoryById(categoryId);
            var result = _categoryService.DeleteCategory();

            // Assert
            Assert.That(result, Is.False);
            _mockDataStore.Verify(x => x.RemoveCategory(categoryId), Times.Once);
        }

        #endregion

        #region GetCategoriesAsTable Tests

        [Test]
        public void GetCategoriesAsTable_WithCategories_ReturnsFormattedStrings()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Category 1" },
                new Category { Id = 2, Name = "Category 2" }
            };

            _mockDataStore.Setup(x => x.GetCategories()).Returns(categories);

            // Act
            var result = _categoryService.GetCategoriesAsTable();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result, Contains.Item(categories[0].ToString()));
            Assert.That(result, Contains.Item(categories[1].ToString()));
        }

        [Test]
        public void GetCategoriesAsTable_WithNoCategories_ReturnsEmptyList()
        {
            // Arrange
            _mockDataStore.Setup(x => x.GetCategories()).Returns(new List<Category>());

            // Act
            var result = _categoryService.GetCategoriesAsTable();

            // Assert
            Assert.That(result, Is.Empty);
        }

        #endregion

        #region GetCategoryById Tests

        [Test]
        public void GetCategoryById_WithValidId_ReturnsCategory()
        {
            // Arrange
            var categoryId = 1;
            var expectedCategory = new Category { Id = categoryId, Name = "Test Category" };
            _mockDataStore.Setup(x => x.GetCategoryById(categoryId)).Returns(expectedCategory);

            // Act
            var result = _categoryService.GetCategoryById(categoryId);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCategory));
            _mockDataStore.Verify(x => x.GetCategoryById(categoryId), Times.Once);
        }

        [Test]
        public void GetCategoryById_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var categoryId = 999;
            _mockDataStore.Setup(x => x.GetCategoryById(categoryId)).Returns((Category?)null);

            // Act
            var result = _categoryService.GetCategoryById(categoryId);

            // Assert
            Assert.That(result, Is.Null);
            _mockDataStore.Verify(x => x.GetCategoryById(categoryId), Times.Once);
        }

        #endregion

        #region HasSelectedCategory Tests

        [Test]
        public void HasSelectedCategory_WithInitialState_ReturnsTrue()
        {
            // Act
            var result = _categoryService.HasSelectedCategory();

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void HasSelectedCategory_AfterSettingCategory_ReturnsTrue()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { Id = categoryId, Name = "Test Category" };
            _mockDataStore.Setup(x => x.GetCategoryById(categoryId)).Returns(category);

            // Act
            _categoryService.SetSelectedCategoryById(categoryId);
            var result = _categoryService.HasSelectedCategory();

            // Assert
            Assert.That(result, Is.True);
        }

        #endregion

        #region SetSelectedCategoryById Tests

        [Test]
        public void SetSelectedCategoryById_WithValidId_SetsSelectedCategory()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { Id = categoryId, Name = "Test Category" };
            _mockDataStore.Setup(x => x.GetCategoryById(categoryId)).Returns(category);

            // Act
            _categoryService.SetSelectedCategoryById(categoryId);

            // Assert
            _mockDataStore.Verify(x => x.GetCategoryById(categoryId), Times.Once);
            // Verify selection worked by checking HasSelectedCategory
            Assert.That(_categoryService.HasSelectedCategory(), Is.True);
        }

        #endregion

        #region UpdateCategory Tests

        [Test]
        public void UpdateCategory_WithValidName_ReturnsSuccessResponse()
        {
            // Arrange
            var categoryId = 1;
            var originalName = "Original Category";
            var newName = "Updated Category";
            var category = new Category { Id = categoryId, Name = originalName };

            _mockDataStore.Setup(x => x.GetCategoryById(categoryId)).Returns(category);
            _mockDataStore.Setup(x => x.CategoryExists(newName)).Returns(false);
            _mockDataStore.Setup(x => x.UpdateCategoryName(categoryId, newName));

            // Act
            _categoryService.SetSelectedCategoryById(categoryId);
            var result = _categoryService.UpdateCategory(newName);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo($"'{originalName}' Category has been successfully updated."));
            Assert.That(result.Data, Is.EqualTo(category));

            _mockDataStore.Verify(x => x.UpdateCategoryName(categoryId, newName), Times.Once);
        }

        [Test]
        [TestCase("")]
        [TestCase("   ")]
        public void UpdateCategory_WithInvalidName_ReturnsFailureResponse(string? invalidName)
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { Id = categoryId, Name = "Test Category" };
            _mockDataStore.Setup(x => x.GetCategoryById(categoryId)).Returns(category);

            // Act
            _categoryService.SetSelectedCategoryById(categoryId);
            var result = _categoryService.UpdateCategory(invalidName);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Name cannot be empty or whitespace."));
            Assert.That(result.Data, Is.Null);

            _mockDataStore.Verify(x => x.UpdateCategoryName(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void UpdateCategory_WithNullName_ThrowsNullReferenceException()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { Id = categoryId, Name = "Test Category" };
            _mockDataStore.Setup(x => x.GetCategoryById(categoryId)).Returns(category);

            // Act & Assert
            _categoryService.SetSelectedCategoryById(categoryId);
            Assert.Throws<NullReferenceException>(() => _categoryService.UpdateCategory(null));

            _mockDataStore.Verify(x => x.UpdateCategoryName(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void UpdateCategory_WithExistingName_ReturnsFailureResponse()
        {
            // Arrange
            var categoryId = 1;
            var originalName = "Original Category";
            var existingName = "Existing Category";
            var category = new Category { Id = categoryId, Name = originalName };

            _mockDataStore.Setup(x => x.GetCategoryById(categoryId)).Returns(category);
            _mockDataStore.Setup(x => x.CategoryExists(existingName)).Returns(true);

            // Act
            _categoryService.SetSelectedCategoryById(categoryId);
            var result = _categoryService.UpdateCategory(existingName);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo($"A category with the name '{existingName}' already exists."));
            Assert.That(result.Data, Is.Null);

            _mockDataStore.Verify(x => x.UpdateCategoryName(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void UpdateCategory_WithSameName_ReturnsSuccessResponse()
        {
            // Arrange
            var categoryId = 1;
            var categoryName = "Test Category";
            var category = new Category { Id = categoryId, Name = categoryName };

            _mockDataStore.Setup(x => x.GetCategoryById(categoryId)).Returns(category);
            _mockDataStore.Setup(x => x.CategoryExists(categoryName)).Returns(true);
            _mockDataStore.Setup(x => x.UpdateCategoryName(categoryId, categoryName));

            // Act
            _categoryService.SetSelectedCategoryById(categoryId);
            var result = _categoryService.UpdateCategory(categoryName);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo($"'{categoryName}' Category has been successfully updated."));
            Assert.That(result.Data, Is.EqualTo(category));

            _mockDataStore.Verify(x => x.UpdateCategoryName(categoryId, categoryName), Times.Once);
        }

        [Test]
        public void UpdateCategory_WithSameNameDifferentCase_ReturnsSuccessResponse()
        {
            // Arrange
            var categoryId = 1;
            var originalName = "Test Category";
            var sameNameDifferentCase = "test category";
            var category = new Category { Id = categoryId, Name = originalName };

            _mockDataStore.Setup(x => x.GetCategoryById(categoryId)).Returns(category);
            _mockDataStore.Setup(x => x.CategoryExists(sameNameDifferentCase)).Returns(true);
            _mockDataStore.Setup(x => x.UpdateCategoryName(categoryId, sameNameDifferentCase));

            // Act
            _categoryService.SetSelectedCategoryById(categoryId);
            var result = _categoryService.UpdateCategory(sameNameDifferentCase);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo($"'{originalName}' Category has been successfully updated."));
            Assert.That(result.Data, Is.EqualTo(category));

            _mockDataStore.Verify(x => x.UpdateCategoryName(categoryId, sameNameDifferentCase), Times.Once);
        }

        [Test]
        public void UpdateCategory_WithNonExistentSelectedCategory_ThrowsNullReferenceException()
        {
            // Arrange
            var categoryId = 999;
            var newName = "New Name";
            _mockDataStore.Setup(x => x.GetCategoryById(categoryId)).Returns((Category?)null);

            // Act & Assert
            _categoryService.SetSelectedCategoryById(categoryId);
            Assert.Throws<NullReferenceException>(() => _categoryService.UpdateCategory(newName));

            _mockDataStore.Verify(x => x.UpdateCategoryName(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        #endregion
    }
}
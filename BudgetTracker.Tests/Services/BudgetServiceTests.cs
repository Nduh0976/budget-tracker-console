using BudgetTracker.Data.Interfaces;
using BudgetTracker.Models;
using BudgetTracker.Services;
using Moq;

namespace BudgetTracker.Tests.Services
{
    [TestFixture]
    public class BudgetServiceTests
    {
        private Mock<IDataStore> _mockDataStore;
        private BudgetService _budgetService;

        [SetUp]
        public void Setup()
        {
            _mockDataStore = new Mock<IDataStore>();
            _budgetService = new BudgetService(_mockDataStore.Object);
        }

        #region CreateBudget Tests

        [Test]
        public void CreateBudget_WithValidInputs_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = 1;
            var name = "Test Budget";
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 12, 31);
            var amount = 1000m;
            var expectedId = 1;

            _mockDataStore.Setup(x => x.GenerateNextBudgetId()).Returns(expectedId);
            _mockDataStore.Setup(x => x.AddBudget(It.IsAny<Budget>()));

            // Act
            var result = _budgetService.CreateBudget(userId, name, startDate, endDate, amount);

            Assert.Multiple(() =>
            // Assert
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Message, Is.EqualTo($"Budget '{name}' has been successfully created."));
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data.Id, Is.EqualTo(expectedId));
                Assert.That(result.Data.UserId, Is.EqualTo(userId));
                Assert.That(result.Data.Name, Is.EqualTo(name));
                Assert.That(result.Data.StartDate, Is.EqualTo(startDate));
                Assert.That(result.Data.EndDate, Is.EqualTo(endDate));
                Assert.That(result.Data.Amount, Is.EqualTo(amount));
            });

            _mockDataStore.Verify(x => x.AddBudget(It.Is<Budget>(b => b.Id == expectedId
                && b.UserId == userId
                && b.Name == name
                && b.StartDate == startDate
                && b.EndDate == endDate
                && b.Amount == amount)), Times.Once);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void CreateBudget_WithInvalidName_ReturnsFailureResponse(string? invalidName)
        {
            // Arrange
            var userId = 1;
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 12, 31);
            var amount = 1000m;

            // Act
            var result = _budgetService.CreateBudget(userId, invalidName, startDate, endDate, amount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.EqualTo("Name cannot be empty or whitespace."));
                Assert.That(result.Data, Is.Null);
            });

            _mockDataStore.Verify(x => x.AddBudget(It.IsAny<Budget>()), Times.Never);
        }

        [Test]
        public void CreateBudget_WithStartDateAfterEndDate_ReturnsFailureResponse()
        {
            // Arrange
            var userId = 1;
            var name = "Test Budget";
            var startDate = new DateTime(2024, 12, 31);
            var endDate = new DateTime(2024, 1, 1);
            var amount = 1000m;

            // Act
            var result = _budgetService.CreateBudget(userId, name, startDate, endDate, amount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.EqualTo("End date cannot be earlier than start date."));
                Assert.That(result.Data, Is.Null);
            });

            _mockDataStore.Verify(x => x.AddBudget(It.IsAny<Budget>()), Times.Never);
        }

        [Test]
        public void CreateBudget_WithNegativeAmount_ReturnsFailureResponse()
        {
            // Arrange
            var userId = 1;
            var name = "Test Budget";
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 12, 31);
            var amount = -100m;

            // Act
            var result = _budgetService.CreateBudget(userId, name, startDate, endDate, amount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.EqualTo("Amount cannot be negative."));
                Assert.That(result.Data, Is.Null);
            });

            _mockDataStore.Verify(x => x.AddBudget(It.IsAny<Budget>()), Times.Never);
        }

        [Test]
        public void CreateBudget_WithZeroAmount_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = 1;
            var name = "Test Budget";
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 12, 31);
            var amount = 0m;
            var expectedId = 1;

            _mockDataStore.Setup(x => x.GenerateNextBudgetId()).Returns(expectedId);
            _mockDataStore.Setup(x => x.AddBudget(It.IsAny<Budget>()));

            // Act
            var result = _budgetService.CreateBudget(userId, name, startDate, endDate, amount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Data.Amount, Is.EqualTo(0m));
            });
        }

        #endregion

        #region DeleteBudgetByUserId Tests

        [Test]
        public void DeleteBudgetByUserId_WithBudgets_RemovesBudgetsAndExpenses()
        {
            // Arrange
            var userId = 1;
            var budgets = new List<Budget>
            {
                new() { Id = 1, UserId = userId, Name = "Budget 1", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Amount = 1000m },
                new() { Id = 2, UserId = userId, Name = "Budget 2", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Amount = 2000m }
            };

            _mockDataStore.Setup(x => x.GetBudgetsByUserId(userId)).Returns(budgets);
            _mockDataStore.Setup(x => x.RemoveExpensesByBudgetId(It.IsAny<int>()));
            _mockDataStore.Setup(x => x.RemoveBudget(It.IsAny<int>()));

            // Act
            _budgetService.DeleteBudgetByUserId(userId);

            // Assert
            _mockDataStore.Verify(x => x.RemoveExpensesByBudgetId(1), Times.Once);
            _mockDataStore.Verify(x => x.RemoveExpensesByBudgetId(2), Times.Once);
            _mockDataStore.Verify(x => x.RemoveBudget(1), Times.Once);
            _mockDataStore.Verify(x => x.RemoveBudget(2), Times.Once);
        }

        [Test]
        public void DeleteBudgetByUserId_WithNoBudgets_DoesNotCallRemoveMethods()
        {
            // Arrange
            var userId = 1;
            _mockDataStore.Setup(x => x.GetBudgetsByUserId(userId)).Returns(new List<Budget>());

            // Act
            _budgetService.DeleteBudgetByUserId(userId);

            // Assert
            _mockDataStore.Verify(x => x.RemoveExpensesByBudgetId(It.IsAny<int>()), Times.Never);
            _mockDataStore.Verify(x => x.RemoveBudget(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region GetBudgetsAsTableByUserId Tests

        [Test]
        public void GetBudgetsAsTableByUserId_WithBudgets_ReturnsFormattedStrings()
        {
            // Arrange
            var userId = 1;
            var budgets = new List<Budget>
            {
                new() { Id = 1, UserId = userId, Name = "Budget 1", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Amount = 1000m },
                new() { Id = 2, UserId = userId, Name = "Budget 2", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Amount = 2000m }
            };

            _mockDataStore.Setup(x => x.GetBudgetsByUserId(userId)).Returns(budgets);

            // Act
            var result = _budgetService.GetBudgetsAsTableByUserId(userId);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result, Contains.Item(budgets[0].ToString()));
            Assert.That(result, Contains.Item(budgets[1].ToString()));
        }

        [Test]
        public void GetBudgetsAsTableByUserId_WithNoBudgets_ReturnsEmptyList()
        {
            // Arrange
            var userId = 1;
            _mockDataStore.Setup(x => x.GetBudgetsByUserId(userId)).Returns([]);

            // Act
            var result = _budgetService.GetBudgetsAsTableByUserId(userId);

            // Assert
            Assert.That(result, Is.Empty);
        }

        #endregion

        #region SelectedBudget Tests

        [Test]
        public void SelectedBudgetExists_WithNoSelectedBudget_ReturnsFalse()
        {
            // Act
            var result = _budgetService.SelectedBudgetExists();

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void SelectedBudgetExists_WithSelectedBudget_ReturnsTrue()
        {
            // Arrange
            var budget = new Budget { Id = 1, UserId = 1, Name = "Test Budget", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Amount = 1000m };
            _mockDataStore.Setup(x => x.GetBudgetById(1)).Returns(budget);

            // Act
            _budgetService.SetSelectedBudgetById(1);
            var result = _budgetService.SelectedBudgetExists();

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void SetSelectedBudgetById_SetsSelectedBudget()
        {
            // Arrange
            var budgetId = 1;
            var budget = new Budget { Id = budgetId, UserId = 1, Name = "Test Budget", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Amount = 1000m };
            _mockDataStore.Setup(x => x.GetBudgetById(budgetId)).Returns(budget);

            // Act
            _budgetService.SetSelectedBudgetById(budgetId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(_budgetService.GetSelectedBudgetId(), Is.EqualTo(budgetId));
                Assert.That(_budgetService.GetSelectedBudgetName(), Is.EqualTo(budget.Name));
                Assert.That(_budgetService.GetSelectedBudgetAmount(), Is.EqualTo(budget.Amount));
                Assert.That(_budgetService.GetSelectedBudgetStartDate(), Is.EqualTo(budget.StartDate));
                Assert.That(_budgetService.GetSelectedBudgetEndDate(), Is.EqualTo(budget.EndDate));
            });

            _mockDataStore.Verify(x => x.GetBudgetById(budgetId), Times.Once);
        }

        [Test]
        public void GetSelectedBudgetId_ReturnsSelectedBudgetId()
        {
            // Arrange
            var budget = new Budget { Id = 5, UserId = 1, Name = "Test Budget", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Amount = 1000m };
            _mockDataStore.Setup(x => x.GetBudgetById(5)).Returns(budget);

            // Act
            _budgetService.SetSelectedBudgetById(5);
            var result = _budgetService.GetSelectedBudgetId();

            // Assert
            Assert.That(result, Is.EqualTo(5));
        }

        [Test]
        public void GetSelectedBudgetAmount_ReturnsSelectedBudgetAmount()
        {
            // Arrange
            var budget = new Budget { Id = 1, UserId = 1, Name = "Test Budget", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Amount = 1500m };
            _mockDataStore.Setup(x => x.GetBudgetById(1)).Returns(budget);

            // Act
            _budgetService.SetSelectedBudgetById(1);
            var result = _budgetService.GetSelectedBudgetAmount();

            // Assert
            Assert.That(result, Is.EqualTo(1500m));
        }

        [Test]
        public void GetSelectedBudgetName_ReturnsSelectedBudgetName()
        {
            // Arrange
            var budget = new Budget { Id = 1, UserId = 1, Name = "Monthly Budget", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Amount = 1000m };
            _mockDataStore.Setup(x => x.GetBudgetById(1)).Returns(budget);

            // Act
            _budgetService.SetSelectedBudgetById(1);
            var result = _budgetService.GetSelectedBudgetName();

            // Assert
            Assert.That(result, Is.EqualTo("Monthly Budget"));
        }

        [Test]
        public void GetSelectedBudgetStartDate_ReturnsSelectedBudgetStartDate()
        {
            // Arrange
            var startDate = new DateTime(2024, 1, 1);
            var budget = new Budget { Id = 1, UserId = 1, Name = "Test Budget", StartDate = startDate, EndDate = DateTime.Now.AddDays(30), Amount = 1000m };
            _mockDataStore.Setup(x => x.GetBudgetById(1)).Returns(budget);

            // Act
            _budgetService.SetSelectedBudgetById(1);
            var result = _budgetService.GetSelectedBudgetStartDate();

            // Assert
            Assert.That(result, Is.EqualTo(startDate));
        }

        [Test]
        public void GetSelectedBudgetEndDate_ReturnsSelectedBudgetEndDate()
        {
            // Arrange
            var endDate = new DateTime(2024, 12, 31);
            var budget = new Budget { Id = 1, UserId = 1, Name = "Test Budget", StartDate = DateTime.Now, EndDate = endDate, Amount = 1000m };
            _mockDataStore.Setup(x => x.GetBudgetById(1)).Returns(budget);

            // Act
            _budgetService.SetSelectedBudgetById(1);
            var result = _budgetService.GetSelectedBudgetEndDate();

            // Assert
            Assert.That(result, Is.EqualTo(endDate));
        }

        #endregion

        #region Budget Calculation Tests

        [Test]
        public void GetSelectedBudgetTotalSpent_WithExpenses_ReturnsSum()
        {
            // Arrange
            var budget = new Budget { Id = 1, UserId = 1, Name = "Test Budget", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Amount = 1000m };
            var expenses = new List<Expense>
            {
                new() { Id = 1, BudgetId = 1, Amount = 100m, Description = "Expense 1", Date = DateTime.Now },
                new() { Id = 2, BudgetId = 1, Amount = 200m, Description = "Expense 2", Date = DateTime.Now },
                new() { Id = 3, BudgetId = 1, Amount = 50m, Description = "Expense 3", Date = DateTime.Now }
            };

            _mockDataStore.Setup(x => x.GetBudgetById(1)).Returns(budget);
            _mockDataStore.Setup(x => x.GetExpensesByBudgetId(1)).Returns(expenses);

            // Act
            _budgetService.SetSelectedBudgetById(1);
            var result = _budgetService.GetSelectedBudgetTotalSpent();

            // Assert
            Assert.That(result, Is.EqualTo(350m));
        }

        [Test]
        public void GetSelectedBudgetTotalSpent_WithNoExpenses_ReturnsZero()
        {
            // Arrange
            var budget = new Budget { Id = 1, UserId = 1, Name = "Test Budget", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Amount = 1000m };
            _mockDataStore.Setup(x => x.GetBudgetById(1)).Returns(budget);
            _mockDataStore.Setup(x => x.GetExpensesByBudgetId(1)).Returns(new List<Expense>());

            // Act
            _budgetService.SetSelectedBudgetById(1);
            var result = _budgetService.GetSelectedBudgetTotalSpent();

            // Assert
            Assert.That(result, Is.EqualTo(0m));
        }

        [Test]
        public void GetSelectedBudgetRemainingBalance_CalculatesCorrectly()
        {
            // Arrange
            var budget = new Budget { Id = 1, UserId = 1, Name = "Test Budget", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Amount = 1000m };
            var expenses = new List<Expense>
            {
                new() { Id = 1, BudgetId = 1, Amount = 300m, Description = "Expense 1", Date = DateTime.Now }
            };

            _mockDataStore.Setup(x => x.GetBudgetById(1)).Returns(budget);
            _mockDataStore.Setup(x => x.GetExpensesByBudgetId(1)).Returns(expenses);

            // Act
            _budgetService.SetSelectedBudgetById(1);
            var result = _budgetService.GetSelectedBudgetRemainingBalance();

            // Assert
            Assert.That(result, Is.EqualTo(700m));
        }

        [Test]
        public void GetSelectedBudgetRemainingBalance_WithOverspending_ReturnsNegativeValue()
        {
            // Arrange
            var budget = new Budget { Id = 1, UserId = 1, Name = "Test Budget", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Amount = 1000m };
            var expenses = new List<Expense>
            {
                new() { Id = 1, BudgetId = 1, Amount = 1200m, Description = "Expensive Item", Date = DateTime.Now }
            };

            _mockDataStore.Setup(x => x.GetBudgetById(1)).Returns(budget);
            _mockDataStore.Setup(x => x.GetExpensesByBudgetId(1)).Returns(expenses);

            // Act
            _budgetService.SetSelectedBudgetById(1);
            var result = _budgetService.GetSelectedBudgetRemainingBalance();

            // Assert
            Assert.That(result, Is.EqualTo(-200m));
        }

        #endregion
    }
}
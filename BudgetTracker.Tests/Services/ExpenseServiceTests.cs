using BudgetTracker.Data.Interfaces;
using BudgetTracker.Models;
using BudgetTracker.Services;
using Moq;

namespace BudgetTracker.Tests.Services
{
    [TestFixture]
    public class ExpenseServiceTests
    {
        private Mock<IDataStore> _mockDataStore;
        private ExpenseService _expenseService;

        [SetUp]
        public void Setup()
        {
            _mockDataStore = new Mock<IDataStore>();
            _expenseService = new ExpenseService(_mockDataStore.Object);
        }

        #region AddExpense Tests

        [Test]
        public void AddExpense_WithValidInputs_ReturnsSuccessResponse()
        {
            // Arrange
            var budgetId = 1;
            var categoryId = 1;
            var description = "Lunch";
            var date = new DateTime(2024, 6, 15);
            var amount = 25.50m;
            var expectedExpenseId = 1;

            var budget = new Budget
            {
                Id = budgetId,
                UserId = 1,
                Name = "Monthly Budget",
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 30),
                Amount = 1000m
            };

            var category = new Category { Id = categoryId, Name = "Food" };

            _mockDataStore.Setup(x => x.GetBudgetById(budgetId)).Returns(budget);
            _mockDataStore.Setup(x => x.GetCategoryById(categoryId)).Returns(category);
            _mockDataStore.Setup(x => x.GenerateNextExpenseId()).Returns(expectedExpenseId);
            _mockDataStore.Setup(x => x.AddExpense(It.IsAny<Expense>()));

            // Act
            var result = _expenseService.AddExpense(budgetId, categoryId, description, date, amount);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.Success, Is.True);
                Assert.That(result.Message, Is.EqualTo($"Expense '{description}' has been successfully added."));
                Assert.That(result.Data, Is.Not.Null);
            });
            Assert.Multiple(() =>
            {
                Assert.That(result.Data.Id, Is.EqualTo(expectedExpenseId));
                Assert.That(result.Data.BudgetId, Is.EqualTo(budgetId));
                Assert.That(result.Data.CategoryId, Is.EqualTo(categoryId));
                Assert.That(result.Data.Description, Is.EqualTo(description));
                Assert.That(result.Data.Date, Is.EqualTo(date));
                Assert.That(result.Data.Amount, Is.EqualTo(amount));
                Assert.That(result.Data.Category, Is.EqualTo(category));
            });

            _mockDataStore.Verify(x => x.AddExpense(It.Is<Expense>(e =>
                e.Id == expectedExpenseId &&
                e.BudgetId == budgetId &&
                e.CategoryId == categoryId &&
                e.Description == description &&
                e.Date == date &&
                e.Amount == amount &&
                e.Category == category)), Times.Once);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void AddExpense_WithInvalidDescription_ReturnsFailureResponse(string? invalidDescription)
        {
            // Arrange
            var budgetId = 1;
            var categoryId = 1;
            var date = new DateTime(2024, 6, 15);
            var amount = 25.50m;

            var budget = new Budget
            {
                Id = budgetId,
                Name = "Test Budget",
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 30),
                Amount = 1000m
            };

            _mockDataStore.Setup(x => x.GetBudgetById(budgetId)).Returns(budget);

            // Act
            var result = _expenseService.AddExpense(budgetId, categoryId, invalidDescription, date, amount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.EqualTo("Description cannot be empty or whitespace."));
                Assert.That(result.Data, Is.Null);
            });

            _mockDataStore.Verify(x => x.AddExpense(It.IsAny<Expense>()), Times.Never);
        }

        [Test]
        public void AddExpense_WithDateBeforeBudgetStart_ReturnsFailureResponse()
        {
            // Arrange
            var budgetId = 1;
            var categoryId = 1;
            var description = "Lunch";
            var date = new DateTime(2024, 5, 31);
            var amount = 25.50m;

            var budget = new Budget
            {
                Id = budgetId,
                Name = "Test Budget",
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 30),
                Amount = 1000m
            };

            _mockDataStore.Setup(x => x.GetBudgetById(budgetId)).Returns(budget);

            // Act
            var result = _expenseService.AddExpense(budgetId, categoryId, description, date, amount);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.EqualTo("Date must be within budget start and end date."));
                Assert.That(result.Data, Is.Null);
            });

            _mockDataStore.Verify(x => x.AddExpense(It.IsAny<Expense>()), Times.Never);
        }

        [Test]
        public void AddExpense_WithDateAfterBudgetEnd_ReturnsFailureResponse()
        {
            // Arrange
            var budgetId = 1;
            var categoryId = 1;
            var description = "Lunch";
            var date = new DateTime(2024, 7, 1);
            var amount = 25.50m;

            var budget = new Budget
            {
                Id = budgetId,
                Name = "Test Budget",
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 30),
                Amount = 1000m
            };

            _mockDataStore.Setup(x => x.GetBudgetById(budgetId)).Returns(budget);

            // Act
            var result = _expenseService.AddExpense(budgetId, categoryId, description, date, amount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.EqualTo("Date must be within budget start and end date."));
                Assert.That(result.Data, Is.Null);
            });

            _mockDataStore.Verify(x => x.AddExpense(It.IsAny<Expense>()), Times.Never);
        }

        [Test]
        public void AddExpense_WithNegativeAmount_ReturnsFailureResponse()
        {
            // Arrange
            var budgetId = 1;
            var categoryId = 1;
            var description = "Lunch";
            var date = new DateTime(2024, 6, 15);
            var amount = -25.50m;

            var budget = new Budget
            {
                Id = budgetId,
                Name = "Test Budget",
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 30),
                Amount = 1000m
            };

            _mockDataStore.Setup(x => x.GetBudgetById(budgetId)).Returns(budget);

            // Act
            var result = _expenseService.AddExpense(budgetId, categoryId, description, date, amount);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.EqualTo("Amount cannot be negative."));
                Assert.That(result.Data, Is.Null);
            });

            _mockDataStore.Verify(x => x.AddExpense(It.IsAny<Expense>()), Times.Never);
        }

        [Test]
        public void AddExpense_WithZeroAmount_ReturnsSuccessResponse()
        {
            // Arrange
            var budgetId = 1;
            var categoryId = 1;
            var description = "Free Sample";
            var date = new DateTime(2024, 6, 15);
            var amount = 0m;
            var expectedExpenseId = 1;

            var budget = new Budget
            {
                Id = budgetId,
                Name = "Test Budget",
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 30),
                Amount = 1000m
            };

            var category = new Category { Id = categoryId, Name = "Food" };

            _mockDataStore.Setup(x => x.GetBudgetById(budgetId)).Returns(budget);
            _mockDataStore.Setup(x => x.GetCategoryById(categoryId)).Returns(category);
            _mockDataStore.Setup(x => x.GenerateNextExpenseId()).Returns(expectedExpenseId);
            _mockDataStore.Setup(x => x.AddExpense(It.IsAny<Expense>()));

            // Act
            var result = _expenseService.AddExpense(budgetId, categoryId, description, date, amount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Data.Amount, Is.EqualTo(0m));
            });
        }

        #endregion

        #region UpdateExpense Tests

        [Test]
        public void UpdateExpense_WithValidInputs_ReturnsSuccessResponse()
        {
            // Arrange
            var expenseId = 1;
            var categoryId = 2;
            var description = "Updated Lunch";
            var date = "15-06-2024";
            var amount = "30,00";

            var existingExpense = new Expense
            {
                Id = expenseId,
                BudgetId = 1,
                CategoryId = 1,
                Description = "Original Lunch",
                Date = new DateTime(2024, 6, 10),
                Amount = 25.50m
            };

            var budget = new Budget
            {
                Id = 1,
                Name = "Test Budget",
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 30),
                Amount = 1000m
            };

            _mockDataStore.Setup(x => x.GetExpenseById(expenseId)).Returns(existingExpense);
            _mockDataStore.Setup(x => x.GetBudgetById(1)).Returns(budget);
            _mockDataStore.Setup(x => x.UpdateData());

            // Act
            var result = _expenseService.UpdateExpense(expenseId, categoryId, description, date, amount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Message, Is.EqualTo($"Expense '{description}' has been successfully updated."));
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data.CategoryId, Is.EqualTo(categoryId));
                Assert.That(result.Data.Description, Is.EqualTo(description));
                Assert.That(result.Data.Date, Is.EqualTo(new DateTime(2024, 6, 15)));
                Assert.That(result.Data.Amount, Is.EqualTo(30.00m));
            });

            _mockDataStore.Verify(x => x.UpdateData(), Times.Once);
        }

        [Test]
        public void UpdateExpense_WithEmptyValues_KeepsOriginalValues()
        {
            // Arrange
            var expenseId = 1;
            var categoryId = 2;
            var description = "";
            var date = "   ";
            var amount = "";

            var existingExpense = new Expense
            {
                Id = expenseId,
                BudgetId = 1,
                CategoryId = 1,
                Description = "Original Lunch",
                Date = new DateTime(2024, 6, 10),
                Amount = 25.50m
            };

            var budget = new Budget
            {
                Id = 1,
                Name = "Test Budget",
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 30),
                Amount = 1000m
            };

            _mockDataStore.Setup(x => x.GetExpenseById(expenseId)).Returns(existingExpense);
            _mockDataStore.Setup(x => x.GetBudgetById(1)).Returns(budget);
            _mockDataStore.Setup(x => x.UpdateData());

            // Act
            var result = _expenseService.UpdateExpense(expenseId, categoryId, description, date, amount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Data.CategoryId, Is.EqualTo(categoryId));
                Assert.That(result.Data.Description, Is.EqualTo("Original Lunch"));
                Assert.That(result.Data.Date, Is.EqualTo(new DateTime(2024, 6, 10)));
                Assert.That(result.Data.Amount, Is.EqualTo(25.50m));
            });
        }

        [Test]
        public void UpdateExpense_WithDateOutsideBudgetRange_ReturnsFailureResponse()
        {
            // Arrange
            var expenseId = 1;
            var categoryId = 2;
            var description = "Updated Lunch";
            var date = "31-05-2024";
            var amount = "30,00";

            var existingExpense = new Expense
            {
                Id = expenseId,
                BudgetId = 1,
                CategoryId = 1,
                Description = "Original Lunch",
                Date = new DateTime(2024, 6, 10),
                Amount = 25.50m
            };

            var budget = new Budget
            {
                Id = 1,
                Name = "Test Budget",
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 30),
                Amount = 1000m
            };

            _mockDataStore.Setup(x => x.GetExpenseById(expenseId)).Returns(existingExpense);
            _mockDataStore.Setup(x => x.GetBudgetById(1)).Returns(budget);

            // Act
            var result = _expenseService.UpdateExpense(expenseId, categoryId, description, date, amount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.EqualTo("Date must be within budget start and end date."));
                Assert.That(result.Data, Is.Null);
            });

            _mockDataStore.Verify(x => x.UpdateData(), Times.Never);
        }

        [Test]
        public void UpdateExpense_WithNegativeAmount_ReturnsFailureResponse()
        {
            // Arrange
            var expenseId = 1;
            var categoryId = 2;
            var description = "Updated Lunch";
            var date = "15-06-2024";
            var amount = "-30,00";

            var existingExpense = new Expense
            {
                Id = expenseId,
                BudgetId = 1,
                CategoryId = 1,
                Description = "Original Lunch",
                Date = new DateTime(2024, 6, 10),
                Amount = 25.50m
            };

            var budget = new Budget
            {
                Id = 1,
                Name = "Test Budget",
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 30),
                Amount = 1000m
            };

            _mockDataStore.Setup(x => x.GetExpenseById(expenseId)).Returns(existingExpense);
            _mockDataStore.Setup(x => x.GetBudgetById(1)).Returns(budget);

            // Act
            var result = _expenseService.UpdateExpense(expenseId, categoryId, description, date, amount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.EqualTo("Amount cannot be negative."));
                Assert.That(result.Data, Is.Null);
            });

            _mockDataStore.Verify(x => x.UpdateData(), Times.Never);
        }

        [Test]
        public void UpdateExpense_WithDateAfterBudgetEnd_ReturnsFailureResponse()
        {
            // Arrange
            var expenseId = 1;
            var categoryId = 2;
            var description = "Updated Lunch";
            var date = "01-07-2024";
            var amount = "30,00";

            var existingExpense = new Expense
            {
                Id = expenseId,
                BudgetId = 1,
                CategoryId = 1,
                Description = "Original Lunch",
                Date = new DateTime(2024, 6, 10),
                Amount = 25.50m
            };

            var budget = new Budget
            {
                Id = 1,
                Name = "Test Budget",
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 30),
                Amount = 1000m
            };

            _mockDataStore.Setup(x => x.GetExpenseById(expenseId)).Returns(existingExpense);
            _mockDataStore.Setup(x => x.GetBudgetById(1)).Returns(budget);

            // Act
            var result = _expenseService.UpdateExpense(expenseId, categoryId, description, date, amount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.EqualTo("Date must be within budget start and end date."));
                Assert.That(result.Data, Is.Null);
            });

            _mockDataStore.Verify(x => x.UpdateData(), Times.Never);
        }

        #endregion

        #region UpdateBudgetAmount Tests

        [Test]
        public void UpdateBudgetAmount_WithValidAmount_ReturnsSuccessResponse()
        {
            // Arrange
            var budgetId = 1;
            var newAmount = "1500,00";

            var budget = new Budget
            {
                Id = budgetId,
                UserId = 1,
                Name = "Monthly Budget",
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 30),
                Amount = 1000m
            };

            _mockDataStore.Setup(x => x.GetBudgetById(budgetId)).Returns(budget);
            _mockDataStore.Setup(x => x.UpdateData());

            // Act
            var result = _expenseService.UpdateBudgetAmount(budgetId, newAmount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Message, Is.EqualTo($"Budget amount for '{budget.Name}' has been successfully updated."));
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data.Amount, Is.EqualTo(1500.00m));
            });

            _mockDataStore.Verify(x => x.UpdateData(), Times.Once);
        }

        [Test]
        public void UpdateBudgetAmount_WithEmptyAmount_KeepsOriginalAmount()
        {
            // Arrange
            var budgetId = 1;
            var newAmount = "   ";

            var budget = new Budget
            {
                Id = budgetId,
                UserId = 1,
                Name = "Monthly Budget",
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 30),
                Amount = 1000m
            };

            _mockDataStore.Setup(x => x.GetBudgetById(budgetId)).Returns(budget);
            _mockDataStore.Setup(x => x.UpdateData());

            // Act
            var result = _expenseService.UpdateBudgetAmount(budgetId, newAmount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Data.Amount, Is.EqualTo(1000m));
            });

            _mockDataStore.Verify(x => x.UpdateData(), Times.Once);
        }

        [Test]
        public void UpdateBudgetAmount_WithNegativeAmount_ReturnsFailureResponse()
        {
            // Arrange
            var budgetId = 1;
            var newAmount = "-500,00";

            var budget = new Budget
            {
                Id = budgetId,
                UserId = 1,
                Name = "Monthly Budget",
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 30),
                Amount = 1000m
            };

            _mockDataStore.Setup(x => x.GetBudgetById(budgetId)).Returns(budget);

            // Act
            var result = _expenseService.UpdateBudgetAmount(budgetId, newAmount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Message, Is.EqualTo("Amount cannot be negative."));
                Assert.That(result.Data, Is.Null);
            });

            _mockDataStore.Verify(x => x.UpdateData(), Times.Never);
        }

        [Test]
        public void UpdateBudgetAmount_WithZeroAmount_ReturnsSuccessResponse()
        {
            // Arrange
            var budgetId = 1;
            var newAmount = "0,00";

            var budget = new Budget
            {
                Id = budgetId,
                UserId = 1,
                Name = "Monthly Budget",
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 30),
                Amount = 1000m
            };

            _mockDataStore.Setup(x => x.GetBudgetById(budgetId)).Returns(budget);
            _mockDataStore.Setup(x => x.UpdateData());

            // Act
            var result = _expenseService.UpdateBudgetAmount(budgetId, newAmount);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Data.Amount, Is.EqualTo(0m));
            });

            _mockDataStore.Verify(x => x.UpdateData(), Times.Once);
        }

        #endregion

        #region DeleteExpense Tests

        [Test]
        public void DeleteExpense_WithValidId_ReturnsTrue()
        {
            // Arrange
            var expenseId = 1;
            _mockDataStore.Setup(x => x.RemoveExpense(expenseId)).Returns(true);

            // Act
            var result = _expenseService.DeleteExpense(expenseId);

            // Assert
            Assert.That(result, Is.True);
            _mockDataStore.Verify(x => x.RemoveExpense(expenseId), Times.Once);
        }

        [Test]
        public void DeleteExpense_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            var expenseId = 999;
            _mockDataStore.Setup(x => x.RemoveExpense(expenseId)).Returns(false);

            // Act
            var result = _expenseService.DeleteExpense(expenseId);

            // Assert
            Assert.That(result, Is.False);
            _mockDataStore.Verify(x => x.RemoveExpense(expenseId), Times.Once);
        }

        #endregion

        #region GetExpensesByBudgetId Tests

        [Test]
        public void GetExpensesByBudgetId_WithValidBudgetId_ReturnsExpenses()
        {
            // Arrange
            var budgetId = 1;
            var expectedExpenses = new List<Expense>
            {
                new() { Id = 1, BudgetId = budgetId, Description = "Lunch", Amount = 25.50m },
                new() { Id = 2, BudgetId = budgetId, Description = "Dinner", Amount = 35.00m }
            };

            _mockDataStore.Setup(x => x.GetExpensesByBudgetId(budgetId)).Returns(expectedExpenses);

            // Act
            var result = _expenseService.GetExpensesByBudgetId(budgetId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Count(), Is.EqualTo(2));
                Assert.That(result.First().Description, Is.EqualTo("Lunch"));
                Assert.That(result.Last().Description, Is.EqualTo("Dinner"));
            });

            _mockDataStore.Verify(x => x.GetExpensesByBudgetId(budgetId), Times.Once);
        }

        [Test]
        public void GetExpensesByBudgetId_WithEmptyResult_ReturnsEmptyCollection()
        {
            // Arrange
            var budgetId = 1;
            var expectedExpenses = new List<Expense>();

            _mockDataStore.Setup(x => x.GetExpensesByBudgetId(budgetId)).Returns(expectedExpenses);

            // Act
            var result = _expenseService.GetExpensesByBudgetId(budgetId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(0));

            _mockDataStore.Verify(x => x.GetExpensesByBudgetId(budgetId), Times.Once);
        }

        #endregion
    }
}
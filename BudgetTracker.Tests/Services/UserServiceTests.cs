using BudgetTracker.Data.Interfaces;
using BudgetTracker.Models;
using BudgetTracker.Services;
using Moq;

namespace BudgetTracker.Tests.Services
{
    [TestFixture]
    public class UserServiceTests
    {
        private Mock<IDataStore> _mockDataStore;
        private UserService _userService;

        [SetUp]
        public void Setup()
        {
            _mockDataStore = new Mock<IDataStore>();
            _userService = new UserService(_mockDataStore.Object);
        }

        #region ActiveUserExists Tests

        [Test]
        public void ActiveUserExists_WhenNoActiveUser_ReturnsFalse()
        {
            // Act
            var result = _userService.ActiveUserExists();

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ActiveUserExists_WhenActiveUserExists_ReturnsTrue()
        {
            // Arrange
            var user = new User { Id = 1, Username = "testuser", Name = "Test User" };
            _userService.SetActiveUser(user);

            // Act
            var result = _userService.ActiveUserExists();

            // Assert
            Assert.That(result, Is.True);
        }

        #endregion

        #region CreateUser Tests

        [Test]
        public void CreateUser_WithValidInputs_ReturnsSuccessResponse()
        {
            // Arrange
            var username = "testuser";
            var name = "Test User";
            var expectedId = 1;

            _mockDataStore.Setup(x => x.UserExists(username)).Returns(false);
            _mockDataStore.Setup(x => x.GenerateNextUserId()).Returns(expectedId);
            _mockDataStore.Setup(x => x.AddUser(It.IsAny<User>()));

            // Act
            var result = _userService.CreateUser(username, name);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo($"User '{username}' has been successfully created."));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data.Id, Is.EqualTo(expectedId));
            Assert.That(result.Data.Username, Is.EqualTo(username));
            Assert.That(result.Data.Name, Is.EqualTo(name));

            _mockDataStore.Verify(x => x.AddUser(It.Is<User>(u =>
                u.Id == expectedId &&
                u.Username == username &&
                u.Name == name)), Times.Once);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void CreateUser_WithInvalidUsername_ReturnsFailureResponse(string? invalidUsername)
        {
            // Act
            var result = _userService.CreateUser(invalidUsername, "Test User");

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Username cannot be empty or whitespace."));
            Assert.That(result.Data, Is.Null);

            _mockDataStore.Verify(x => x.AddUser(It.IsAny<User>()), Times.Never);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void CreateUser_WithInvalidName_ReturnsFailureResponse(string? invalidName)
        {
            // Act
            var result = _userService.CreateUser("testuser", invalidName);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Name cannot be empty or whitespace."));
            Assert.That(result.Data, Is.Null);

            _mockDataStore.Verify(x => x.AddUser(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void CreateUser_WithExistingUsername_ReturnsFailureResponse()
        {
            // Arrange
            var username = "existinguser";
            _mockDataStore.Setup(x => x.UserExists(username)).Returns(true);

            // Act
            var result = _userService.CreateUser(username, "Test User");

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo($"A user with the username '{username}' already exists."));
            Assert.That(result.Data, Is.Null);

            _mockDataStore.Verify(x => x.AddUser(It.IsAny<User>()), Times.Never);
        }

        #endregion

        #region GetActiveUserId Tests

        [Test]
        public void GetActiveUserId_ReturnsActiveUserId()
        {
            // Arrange
            var user = new User { Id = 5, Username = "testuser", Name = "Test User" };
            _userService.SetActiveUser(user);

            // Act
            var result = _userService.GetActiveUserId();

            // Assert
            Assert.That(result, Is.EqualTo(5));
        }

        #endregion

        #region GetActiveUserName Tests

        [Test]
        public void GetActiveUserName_ReturnsActiveUserName()
        {
            // Arrange
            var user = new User { Id = 1, Username = "testuser", Name = "Test User" };
            _userService.SetActiveUser(user);

            // Act
            var result = _userService.GetActiveUserName();

            // Assert
            Assert.That(result, Is.EqualTo("Test User"));
        }

        #endregion

        #region GetUsers Tests

        [Test]
        public void GetUsers_ReturnsUsersFromDataStore()
        {
            // Arrange
            var expectedUsers = new List<User>
            {
                new User { Id = 1, Username = "user1", Name = "User One" },
                new User { Id = 2, Username = "user2", Name = "User Two" }
            };
            _mockDataStore.Setup(x => x.GetUsers()).Returns(expectedUsers);

            // Act
            var result = _userService.GetUsers();

            // Assert
            Assert.That(result, Is.EqualTo(expectedUsers));
            _mockDataStore.Verify(x => x.GetUsers(), Times.Once);
        }

        #endregion

        #region GetUsersAsTable Tests

        [Test]
        public void GetUsersAsTable_ReturnsFormattedUserStrings()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Username = "user1", Name = "User One" },
                new User { Id = 2, Username = "user2", Name = "User Two" }
            };
            _mockDataStore.Setup(x => x.GetUsers()).Returns(users);

            // Act
            var result = _userService.GetUsersAsTable();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result, Contains.Item(users[0].ToString()));
            Assert.That(result, Contains.Item(users[1].ToString()));
        }

        [Test]
        public void GetUsersAsTable_WithNoUsers_ReturnsEmptyList()
        {
            // Arrange
            _mockDataStore.Setup(x => x.GetUsers()).Returns(new List<User>());

            // Act
            var result = _userService.GetUsersAsTable();

            // Assert
            Assert.That(result, Is.Empty);
        }

        #endregion

        #region RemoveUser Tests

        [Test]
        public void RemoveUser_CallsDataStoreWithActiveUserId()
        {
            // Arrange
            var user = new User { Id = 3, Username = "testuser", Name = "Test User" };
            _userService.SetActiveUser(user);
            _mockDataStore.Setup(x => x.RemoveUser(3)).Returns(true);

            // Act
            var result = _userService.RemoveUser();

            // Assert
            Assert.That(result, Is.True);
            _mockDataStore.Verify(x => x.RemoveUser(3), Times.Once);
        }

        #endregion

        #region SetActiveUserById Tests

        [Test]
        public void SetActiveUserById_SetsActiveUserFromDataStore()
        {
            // Arrange
            var userId = 4;
            var expectedUser = new User { Id = 4, Username = "user4", Name = "User Four" };
            _mockDataStore.Setup(x => x.GetUserById(userId)).Returns(expectedUser);

            // Act
            _userService.SetActiveUserById(userId);

            // Assert
            Assert.That(_userService.GetActiveUserId(), Is.EqualTo(expectedUser.Id));
            Assert.That(_userService.GetActiveUserName(), Is.EqualTo(expectedUser.Name));
            _mockDataStore.Verify(x => x.GetUserById(userId), Times.Once);
        }

        #endregion

        #region SetActiveUser Tests

        [Test]
        public void SetActiveUser_SetsActiveUser()
        {
            // Arrange
            var user = new User { Id = 7, Username = "testuser", Name = "Test User" };

            // Act
            _userService.SetActiveUser(user);

            // Assert
            Assert.That(_userService.GetActiveUserId(), Is.EqualTo(user.Id));
            Assert.That(_userService.GetActiveUserName(), Is.EqualTo(user.Name));
        }

        #endregion

        #region UpdateUser Tests

        [Test]
        public void UpdateUser_WithValidName_ReturnsSuccessResponse()
        {
            // Arrange
            var activeUser = new User { Id = 1, Username = "testuser", Name = "Old Name" };
            var newName = "New Name";
            _userService.SetActiveUser(activeUser);
            _mockDataStore.Setup(x => x.GetUserById(1)).Returns(activeUser);
            _mockDataStore.Setup(x => x.UpdateNameOfUser(1, newName));

            // Act
            var result = _userService.UpdateUser(newName);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo($"User '{activeUser.Username}' has been successfully updated."));
            Assert.That(result.Data, Is.EqualTo(activeUser));
            _mockDataStore.Verify(x => x.UpdateNameOfUser(1, newName), Times.Once);
        }

        [Test]
        [TestCase("")]
        [TestCase("   ")]
        public void UpdateUser_WithInvalidName_ReturnsFailureResponse(string invalidName)
        {
            // Arrange
            var activeUser = new User { Id = 1, Username = "testuser", Name = "Test User" };
            _userService.SetActiveUser(activeUser);

            // Act
            var result = _userService.UpdateUser(invalidName);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Username cannot be empty or whitespace."));
            Assert.That(result.Data, Is.Null);

            _mockDataStore.Verify(x => x.UpdateNameOfUser(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void UpdateUser_WhenUserNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var activeUser = new User { Id = 1, Username = "testuser", Name = "Test User" };
            _userService.SetActiveUser(activeUser);
            _mockDataStore.Setup(x => x.GetUserById(1)).Returns((User)null);

            // Act
            var result = _userService.UpdateUser("New Name");

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Username not found."));
            Assert.That(result.Data, Is.Null);

            _mockDataStore.Verify(x => x.UpdateNameOfUser(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        #endregion
    }
}
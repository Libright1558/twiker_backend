using Microsoft.EntityFrameworkCore;
using twiker_backend.Models.DatabaseContext;
using twiker_backend.Db.Models;
using twiker_backend.Db.Repository;

namespace Db.UserInfoTest
{
    [TestFixture, Category("DbUserData")]
    public class DbUserInfoTests
    {
        private DbContextOptions<TwikerContext> _options;
        private TwikerContext _context;

        [OneTimeSetUp]
        public void Setup()
        {
            DotNetEnv.Env.TraversePath().Load();
            _options = new DbContextOptionsBuilder<TwikerContext>()
            .UseNpgsql(DotNetEnv.Env.GetString("connection_mock"))
            .Options;

            _context = new TwikerContext(_options);

            DbConnectManager.InitDbConnectionString(true);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _context.UserTables.ExecuteDelete();
            _context.Dispose();
        }

        [Test]
        public async Task GetUserData_ExistingUser_ReturnsUserDbData()
        {
            try
            {
                // Arrange
                var userData = new UserTable
                {
                    Firstname = "John",
                    Lastname = "Doe",
                    Username = "johndoe",
                    Email = "john@example.com",
                    Profilepic = "profile.jpg"
                };

                
                var _dbUserInfo = new DbUserInfo(_context);

                // Act
                await _dbUserInfo.WriteUserData(userData);
                var _userData = await _dbUserInfo.FindOneUser("johndoe");
                var result = await _dbUserInfo.GetUserData(_userData!.UserId);

                Assert.Multiple(() =>
                {
                    // Assert
                    Assert.That(result, Is.Not.Null);
                    Assert.That(userData.Firstname, Is.EqualTo(result?.Firstname));
                    Assert.That(userData.Lastname, Is.EqualTo(result?.Lastname));
                    Assert.That(userData.Username, Is.EqualTo(result?.Username));
                    Assert.That(userData.Email, Is.EqualTo(result?.Email));
                    Assert.That(userData.Profilepic, Is.EqualTo(result?.Profilepic));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task GetUserData_NonExistingUser_ReturnsNull()
        {
            try
            {
                // Arrange
                var userId = new Guid("550e8400-e29b-41d4-a716-446655440000");

                var _dbUserInfo = new DbUserInfo(_context);

                // Act
                await _dbUserInfo.DeleteUserData(userId);
                var result = await _dbUserInfo.GetUserData(userId);

                // Assert
                Assert.That(result, Is.Null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task WriteUserData_ValidUser_ReturnsNumberOfAffectedRows()
        {
            try
            {
                // Arrange
                var userInfo = new UserTable
                {
                    Firstname = "Jane",
                    Lastname = "Doe",
                    Username = "janedoe",
                    Email = "jane@example.com",
                    Password = "password123",
                    Profilepic = "jane_profile.jpg"
                };

                var _dbUserInfo = new DbUserInfo(_context);
                
                // Act
                var result = await _dbUserInfo.WriteUserData(userInfo);

                // Assert
                Assert.That(result, Is.EqualTo(1));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task FindOneUser_ExistingUser_ReturnsUserDbData()
        {
            try
            {
                // Arrange
                var userDbData = new UserDbData
                {
                    Username = "testuser",
                    Email = "test@example.com"
                };

                var userData = new UserTable
                {
                    Username = "testuser",
                    Email = "test@example.com",
                    Password = "hashedpassword"
                };

                var _dbUserInfo = new DbUserInfo(_context);

                // Act
                await _dbUserInfo.WriteUserData(userData);
                var result = await _dbUserInfo.FindOneUser("testuser");

                Assert.Multiple(() =>
                {
                    // Assert
                    Assert.That(result, Is.Not.Null);
                    Assert.That(userData.Username, Is.EqualTo(result?.Username));
                    Assert.That(userData.Email, Is.EqualTo(result?.Email));
                    Assert.That(userData.Password, Is.EqualTo(result?.Password));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task FindOneUser_NonExistingUser_ReturnsNull()
        {
            try
            {
                // Arrange
                var userDbData = new UserDbData
                {
                    Username = "nonexistentuser",
                    Email = "nonexistent@example.com"
                };

                var _dbUserInfo = new DbUserInfo(_context);

                // Act
                var result = await _dbUserInfo.FindOneUser("nonexistentuser");

                // Assert
                Assert.That(result, Is.Null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
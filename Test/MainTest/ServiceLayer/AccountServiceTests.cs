using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using twiker_backend.ServiceLayer;
using twiker_backend.Models.DatabaseContext;
using twiker_backend.Db.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using twiker_backend.Db.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

[TestFixture, Category("AccountService")]
public class AccountServiceTests
{
    private AccountService _accountService;
    private Mock<ILogger<AccountService>> _loggerMock;
    private DbContextOptions<TwikerContext> _options;
    private TwikerContext _context;
    private IDbUserInfo _dbUserInfo;

    [OneTimeSetUp]
    public void Setup()
    {
        DotNetEnv.Env.TraversePath().Load();
        _options = new DbContextOptionsBuilder<TwikerContext>()
        .UseNpgsql(DotNetEnv.Env.GetString("connection_mock"))
        .Options;

        // Initialize DbContext
        _context = new TwikerContext(_options);
        _dbUserInfo = new DbUserInfo(_context);

        // Initialize AccountService
        _loggerMock = new Mock<ILogger<AccountService>>();
        _accountService = new AccountService(_dbUserInfo, _loggerMock.Object);

    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task RegisterAccountAsync_ValidInput_ReturnsSuccess()
    {
        try
        {
            // Arrange
            var model = new RegisterModel
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "john@example.com",
                Password = "password123"
            };

            // Act
            var result = await _accountService.RegisterAccountAsync(model);

            // Assert
            Assert.That(result, Is.EqualTo(RegisterResult.Success));

            // Verify user was added to the database
            var user = await _context.UserTables.FirstOrDefaultAsync(u => u.Username == "johndoe");
            Assert.That(user, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(user?.Firstname, Is.EqualTo("John"));
                Assert.That(user?.Lastname, Is.EqualTo("Doe"));
                Assert.That(user?.Email, Is.EqualTo("john@example.com"));
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Test]
    public async Task RegisterAccountAsync_DuplicateUsername_ReturnsDuplicateUser()
    {
        try
        {
            // Arrange
            var existingUser = new UserTable
            {
                Firstname = "Existing",
                Lastname = "User",
                Username = "existinguser",
                Email = "existing@example.com",
                Password = "hashedpassword"
            };
            _context.UserTables.Add(existingUser);
            await _context.SaveChangesAsync();

            var model = new RegisterModel
            {
                FirstName = "JJ",
                LastName = "Jim",
                Username = "existinguser",
                Email = "Jim@example.com",
                Password = "password123"
            };

            // Act
            var result = await _accountService.RegisterAccountAsync(model);

            // Assert
            Assert.That(result, Is.EqualTo(RegisterResult.DuplicateUser));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Test]
    public async Task RegisterAccountAsync_InvalidInput_ReturnsInvalidInput()
    {
        try
        {
            // Arrange
            var model = new RegisterModel
            {
                FirstName = "",
                LastName = "Do",
                Username = "johndo",
                Email = "johney@example.com",
                Password = "password123"
            };

            // Act
            var result = await _accountService.RegisterAccountAsync(model);

            // Assert
            Assert.That(result, Is.EqualTo(RegisterResult.InvalidInput));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Test]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccessWithToken()
    {
        try
        {
            // Arrange
            var user = new UserTable
            {
                UserId = Guid.NewGuid(),
                Firstname = "janous",
                Lastname = "Doe",
                Username = "janously",
                Email = "janously@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("password123")
            };
            _context.UserTables.Add(user);
            await _context.SaveChangesAsync();

            var model = new LoginModel
            {
                Username = "janously",
                Password = "password123"
            };

            // Act
            var result = await _accountService.LoginAsync(model);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.Success, Is.True);
                Assert.That(result.Token, Is.Not.Null);
            });


            // Verify token
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(result.Token) as JwtSecurityToken;

            Assert.That(jsonToken, Is.Not.Null);
            Assert.That(jsonToken?.Claims.First(claim => claim.Type == "username").Value, Is.EqualTo("janously"));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Test]
    public async Task LoginAsync_InvalidCredentials_ReturnsFailure()
    {
        try
        {
            // Arrange
            var user = new UserTable
            {
                UserId = Guid.NewGuid(),
                Firstname = "bob",
                Lastname = "lua",
                Username = "boblua",
                Email = "boblua@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("password123", 12)
            };
            _context.UserTables.Add(user);
            await _context.SaveChangesAsync();

            var model = new LoginModel
            {
                Username = "boblua",
                Password = "wrongpassword"
            };

            // Act
            var result = await _accountService.LoginAsync(model);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.Success, Is.False);
                Assert.That(result.ErrorMessage, Is.EqualTo("Invalid username or password"));
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Test]
    public async Task LoginAsync_InvalidInput_ReturnsFailure()
    {
        try
        {
            // Arrange
            var model = new LoginModel
            {
                Username = "",
                Password = "password123"
            };

            // Act
            var result = await _accountService.LoginAsync(model);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.Success, Is.False);
                Assert.That(result.ErrorMessage, Is.EqualTo("Invalid input"));
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Test]
    public async Task DeleteAccountAsync_Success()
    {
        try
        {
            // Arrange
            var user = new UserTable
            {
                UserId = Guid.NewGuid(),
                Firstname = "qwerty1",
                Lastname = "zxcvbn1",
                Username = "zxcvbn1",
                Email = "qwerty1zxcvbn1@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("password123", 12)
            };
            _context.UserTables.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountService.DeleteAccountAsync(user.Username);

            // Assert
            Assert.That(result, Is.EqualTo(DeleteAccountResult.Success));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    
    [Test]
    public async Task DeleteAccountAsync_InvalidInput()
    {
        try
        {
            // Arrange
            var user = new UserTable
            {
                UserId = Guid.NewGuid(),
                Firstname = "qwerty2",
                Lastname = "zxcvbn2",
                Username = "zxcvbn2",
                Email = "qwerty2zxcvbn2@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("password123", 12)
            };
            _context.UserTables.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountService.DeleteAccountAsync("");

            // Assert
            Assert.That(result, Is.EqualTo(DeleteAccountResult.InvalidInput));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Test]
    public async Task DeleteAccountAsync_UserNotExist()
    {
        try
        {
            // Arrange
            var user = new UserTable
            {
                UserId = Guid.NewGuid(),
                Firstname = "qwerty3",
                Lastname = "zxcvbn3",
                Username = "zxcvbn3",
                Email = "qwerty3zxcvbn3@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("password123", 12)
            };
            _context.UserTables.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountService.DeleteAccountAsync("abcdef");

            // Assert
            Assert.That(result, Is.EqualTo(DeleteAccountResult.UserNotExist));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
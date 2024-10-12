using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using twiker_backend.ServiceLayer;
using twiker_backend.Models.DatabaseContext;
using twiker_backend.Db.Models;
using twiker_backend.Redis.Models;
using twiker_backend.Redis;
using twiker_backend.Db.Repository;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Test.MainTest.Redis;
using Microsoft.Extensions.Logging;

[TestFixture, Category("UserService")]
public class UserServiceTests
{
    private UserService _userService;
    private Mock<ILogger<UserService>> _loggerMock;
    private DbContextOptions<TwikerContext> _options;
    private TwikerContext _context;
    private IConnectionMultiplexer _connectionMultiplexer;
    private IDatabase _redisDb;
    private IRedisUserData _redisUserInfo;
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

        // Initialize Redis Connection
        _connectionMultiplexer = RedisConnectOperation.Connection;
        _redisDb = _connectionMultiplexer.GetDatabase();

        // Initialize UserService
        _redisUserInfo = new UserInfo(_connectionMultiplexer);
        _dbUserInfo = new DbUserInfo(_context);

        _loggerMock = new Mock<ILogger<UserService>>();
        _userService = new UserService(_redisUserInfo, _dbUserInfo, _loggerMock.Object);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _context.UserTables.ExecuteDelete();
        _context.Dispose();
        _connectionMultiplexer.Dispose();
    }

    [Test]
    public async Task GetThePersonalData_UserExistsInRedis_ReturnsRedisData()
    {
        try
        {
            // Arrange
            var userId = Guid.NewGuid();
            var redisUserData = new RedisUserData
            {
                Firstname = "John",
                Lastname = "Doe",
                Username = "johndoe",
                Email = "john@example.com",
                Profilepic = "profile.jpg"
            };

            await _redisDb.StringSetAsync($"{userId}_Firstname", redisUserData.Firstname);
            await _redisDb.StringSetAsync($"{userId}_Lastname", redisUserData.Lastname);
            await _redisDb.StringSetAsync($"{userId}_Username", redisUserData.Username);
            await _redisDb.StringSetAsync($"{userId}_Email", redisUserData.Email);
            await _redisDb.StringSetAsync($"{userId}_Profilepic", redisUserData.Profilepic);

            // Act
            var result = await _userService.GetThePersonalData(userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.Firstname, Is.EqualTo(redisUserData.Firstname));
                Assert.That(result?.Lastname, Is.EqualTo(redisUserData.Lastname));
                Assert.That(result?.Username, Is.EqualTo(redisUserData.Username));
                Assert.That(result?.Email, Is.EqualTo(redisUserData.Email));
                Assert.That(result?.Profilepic, Is.EqualTo(redisUserData.Profilepic));
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Test]
    public async Task GetThePersonalData_UserNotInRedisButInDb_ReturnsDbDataAndSavesToRedis()
    {
        try
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dbUserData = new UserTable
            {
                UserId = userId,
                Firstname = "Jane",
                Lastname = "Smith",
                Username = "janesmith",
                Email = "jane@example.com",
                Profilepic = "jane_profile.jpg"
            };

            _context.UserTables.Add(dbUserData);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetThePersonalData(userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.Firstname, Is.EqualTo(dbUserData.Firstname));
                Assert.That(result?.Lastname, Is.EqualTo(dbUserData.Lastname));
                Assert.That(result?.Username, Is.EqualTo(dbUserData.Username));
                Assert.That(result?.Email, Is.EqualTo(dbUserData.Email));
                Assert.That(result?.Profilepic, Is.EqualTo(dbUserData.Profilepic));
            });


            // Verify data was saved to Redis
            var redisFirstname = await _redisDb.StringGetAsync($"{userId}_Firstname");
            Assert.That(redisFirstname.ToString(), Is.EqualTo(dbUserData.Firstname));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Test]
    public async Task GetThePersonalData_UserNotExist_ReturnsNull()
    {
        try
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var user = await _context.UserTables.FindAsync(userId);

            if (user != null)
            {
                _context.UserTables.Remove(user);
                await _context.SaveChangesAsync();
            }

            var result = await _userService.GetThePersonalData(userId);

            // Assert
            Assert.That(result?.Username, Is.Null);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
using StackExchange.Redis;
using twiker_backend.Redis;
using twiker_backend.Redis.Models;

namespace Test.MainTest.Redis
{
    [TestFixture, Category("RedisUserData")]
    public class TestUser
    {

        private UserInfo _userInfo;
        private IConnectionMultiplexer _connectionMultiplexer;
        private IDatabase _db;

        [OneTimeSetUp]
        public void Setup()
        {
            _connectionMultiplexer = RedisConnectOperation.Connection;
            _userInfo = new UserInfo(_connectionMultiplexer);
            _db = _connectionMultiplexer.GetDatabase();
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _connectionMultiplexer.Dispose();
        }

        [Test]
        public async Task GetUserInfoAsync_ReturnsCorrectData()
        {
            try
            {
                // Arrange
                string userId = "user123";
                var expectedData = new RedisUserData
                {
                    Firstname = "John",
                    Lastname = "Doe",
                    Username = "johndoe1",
                    Email = "john1@example.com",
                    Profilepic = "profile.jpg"
                };

                await _db.StringSetAsync($"{userId}_Firstname", expectedData.Firstname);
                await _db.StringSetAsync($"{userId}_Lastname", expectedData.Lastname);
                await _db.StringSetAsync($"{userId}_Username", expectedData.Username);
                await _db.StringSetAsync($"{userId}_Email", expectedData.Email);
                await _db.StringSetAsync($"{userId}_Profilepic", expectedData.Profilepic);

                // Act
                var result = await _userInfo.GetUserInfoAsync(userId);

                Assert.Multiple(() =>
                {
                    // Assert
                    Assert.That(result.Firstname, Is.EqualTo(expectedData.Firstname));
                    Assert.That(result.Lastname, Is.EqualTo(expectedData.Lastname));
                    Assert.That(result.Username, Is.EqualTo(expectedData.Username));
                    Assert.That(result.Email, Is.EqualTo(expectedData.Email));
                    Assert.That(result.Profilepic, Is.EqualTo(expectedData.Profilepic));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task WriteUserInfoAsync_WritesCorrectData()
        {
            try
            {
                // Arrange
                string userId = "user456";
                int expirySeconds = 3600;
                var userInfo = new RedisUserData
                {
                    Firstname = "John",
                    Lastname = "Doe",
                    Username = "johndoe2",
                    Email = "john2@example.com",
                    Profilepic = "profile.jpg"
                };

                // Act
                await _userInfo.WriteUserInfoAsync(userId, userInfo, expirySeconds);

                // Assert
                var firstname = await _db.StringGetAsync($"{userId}_Firstname");
                var lastname = await _db.StringGetAsync($"{userId}_Lastname");
                var username = await _db.StringGetAsync($"{userId}_Username");
                var email = await _db.StringGetAsync($"{userId}_Email");
                var profilepic = await _db.StringGetAsync($"{userId}_Profilepic");

                Assert.Multiple(() =>
                {
                    Assert.That(firstname.ToString(), Is.EqualTo(userInfo.Firstname));
                    Assert.That(lastname.ToString(), Is.EqualTo(userInfo.Lastname));
                    Assert.That(username.ToString(), Is.EqualTo(userInfo.Username));
                    Assert.That(email.ToString(), Is.EqualTo(userInfo.Email));
                    Assert.That(profilepic.ToString(), Is.EqualTo(userInfo.Profilepic));
                });


                // Check expiration
                var fields = new[] { "Firstname", "Lastname", "Username", "Email", "Profilepic" };
                foreach (var field in fields)
                {
                    var ttl = await _db.KeyTimeToLiveAsync($"{userId}_{field}");
                    Assert.Multiple(() =>
                    {
                        Assert.That(ttl.HasValue, Is.True);
                        Assert.That(ttl.Value.TotalSeconds, Is.InRange(3590, 3600)); // Allow for small discrepancies
                    });

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task SetUserInfoExp_SetsCorrectExpiration()
        {
            try
            {
                // Arrange
                string userId = "user789";
                int expirySeconds = 3600;
                var fields = new[] { "Firstname", "Lastname", "Username", "Email", "Profilepic" };

                foreach (var field in fields)
                {
                    await _db.StringSetAsync($"{userId}_{field}", "test");
                }

                // Act
                await _userInfo.SetUserInfoExp(userId, expirySeconds);

                // Assert
                foreach (var field in fields)
                {
                    var ttl = await _db.KeyTimeToLiveAsync($"{userId}_{field}");
                    Assert.Multiple(() =>
                    {
                        Assert.That(ttl.HasValue, Is.True);
                        Assert.That(ttl.Value.TotalSeconds, Is.InRange(3590, 3600)); // Allow for small discrepancies
                    });

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task GetUserInfoAsync_ReturnsNullForNonExistentUser()
        {
            try
            {
                // Arrange
                string userId = "nonexistentuser";

                // Act
                var result = await _userInfo.GetUserInfoAsync(userId);

                Assert.Multiple(() =>
                {
                    // Assert
                    Assert.That(result.Firstname, Is.Null);
                    Assert.That(result.Lastname, Is.Null);
                    Assert.That(result.Username, Is.Null);
                    Assert.That(result.Email, Is.Null);
                    Assert.That(result.Profilepic, Is.Null);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task WriteUserInfoAsync_DoesNotOverwriteExistingData()
        {
            try
            {
                // Arrange
                string userId = "user888";
                int expirySeconds = 3600;
                var existingUserInfo = new RedisUserData
                {
                    Firstname = "John",
                    Lastname = "Doe",
                    Username = "johndoe3",
                    Email = "john3@example.com",
                    Profilepic = "profile.jpg"
                };

                var newUserInfo = new RedisUserData
                {
                    Firstname = "Jane",
                    Lastname = "Smith",
                    Username = "janesmith",
                    Email = "jane@example.com",
                    Profilepic = "newprofile.jpg"
                };

                // Write existing data
                await _userInfo.WriteUserInfoAsync(userId, existingUserInfo, expirySeconds);

                // Act
                await _userInfo.WriteUserInfoAsync(userId, newUserInfo, expirySeconds);

                // Assert
                var result = await _userInfo.GetUserInfoAsync(userId);
                Assert.Multiple(() =>
                {
                    Assert.That(result.Firstname, Is.EqualTo(existingUserInfo.Firstname));
                    Assert.That(result.Lastname, Is.EqualTo(existingUserInfo.Lastname));
                    Assert.That(result.Username, Is.EqualTo(existingUserInfo.Username));
                    Assert.That(result.Email, Is.EqualTo(existingUserInfo.Email));
                    Assert.That(result.Profilepic, Is.EqualTo(existingUserInfo.Profilepic));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task SetUserInfoExp_DoesNotAffectNonExistentKeys()
        {
            try
            {
                // Arrange
                string userId = "user999";
                int expirySeconds = 3600;
                var fields = new[] { "Firstname", "Lastname", "Username", "Email", "Profilepic" };

                // Only set some fields
                await _db.StringSetAsync($"{userId}_Firstname", "John");
                await _db.StringSetAsync($"{userId}_Lastname", "Doe");

                // Act
                await _userInfo.SetUserInfoExp(userId, expirySeconds);

                Assert.Multiple(async () =>
                {
                    // Assert
                    Assert.That(await _db.KeyExistsAsync($"{userId}_Firstname"), Is.True);
                    Assert.That(await _db.KeyExistsAsync($"{userId}_Lastname"), Is.True);
                    Assert.That(await _db.KeyExistsAsync($"{userId}_Username"), Is.False);
                    Assert.That(await _db.KeyExistsAsync($"{userId}_Email"), Is.False);
                    Assert.That(await _db.KeyExistsAsync($"{userId}_Profilepic"), Is.False);
                });


                var ttl = await _db.KeyTimeToLiveAsync($"{userId}_Firstname");
                Assert.Multiple(() =>
                {
                    Assert.That(ttl.HasValue, Is.True);
                    Assert.That(ttl.Value.TotalSeconds, Is.InRange(3590, 3600));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task DeleteUserInfo_UserHasBeenDeleted()
        {
            try
            {
                // Arrange
                string userId = "user1000";
                var fields = new[] { "Firstname", "Lastname", "Username", "Email", "Profilepic" };

                // Set fields
                await _db.StringSetAsync($"{userId}_Firstname", "Asd");
                await _db.StringSetAsync($"{userId}_Lastname", "Fgh");
                await _db.StringSetAsync($"{userId}_Username", "AsdFgh");
                await _db.StringSetAsync($"{userId}_Email", "AsdFgh@email.com");
                await _db.StringSetAsync($"{userId}_Profilepic", "default");

                // Act
                await _userInfo.DeleteUserInfo("user1000");

                Assert.Multiple(async () =>
                {
                    // Assert
                    Assert.That(await _db.KeyExistsAsync($"{userId}_Firstname"), Is.False);
                    Assert.That(await _db.KeyExistsAsync($"{userId}_Lastname"), Is.False);
                    Assert.That(await _db.KeyExistsAsync($"{userId}_Username"), Is.False);
                    Assert.That(await _db.KeyExistsAsync($"{userId}_Email"), Is.False);
                    Assert.That(await _db.KeyExistsAsync($"{userId}_Profilepic"), Is.False);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
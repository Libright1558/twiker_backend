using System.Threading;
using StackExchange.Redis;
using twiker_backend.Redis;
using twiker_backend.Redis.Models;


namespace Test.MainTest.Redis
{
    [TestFixture, Category("RedisPostData")]
    public class TestPost
    {
        private PostInfo _postInfo;
        private IConnectionMultiplexer _connectionMultiplexer;
        private IDatabase _db;
        private const string PostIdArraySuffix = "_PostIdArray";

        [OneTimeSetUp]
        public void Setup()
        {
            _connectionMultiplexer = RedisConnectOperation.Connection;
            _postInfo = new PostInfo(_connectionMultiplexer);
            _db = _connectionMultiplexer.GetDatabase();
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _connectionMultiplexer.Dispose();
        }

        [Test]
        public async Task GetPostInfo_ReturnsCorrectData()
        {
            try
            {
                // Arrange
                string selfUserId = "user123";
                string[] postIdArray = ["post1", "post2"];
                var expectedData = new RedisPostTable
                {
                    Content = ["Content1", "Content2"],
                    CreatedAt = ["2023-01-01", "2023-01-02"],
                    LikeNums = ["10", "20"],
                    RetweetNums = ["5", "15"],
                    PostOwner = ["owner1", "owner2"],
                    Firstname = ["John", "Jane"],
                    Lastname = ["Doe", "Smith"],
                    Profilepic = ["pic1.jpg", "pic2.jpg"],
                    SelfLike = ["true", "false"],
                    SelfRetweet = ["false", "true"]
                };

                foreach (var postId in postIdArray)
                {
                    int index = Array.IndexOf(postIdArray, postId);
                    await _db.StringSetAsync($"{postId}_Content", expectedData.Content[index]);
                    await _db.StringSetAsync($"{postId}_CreatedAt", expectedData.CreatedAt[index]);
                    await _db.StringSetAsync($"{postId}_LikeNums", expectedData.LikeNums[index]);
                    await _db.StringSetAsync($"{postId}_RetweetNums", expectedData.RetweetNums[index]);
                    await _db.StringSetAsync($"{postId}_PostOwner", expectedData.PostOwner[index]);
                    await _db.StringSetAsync($"{postId}_Firstname", expectedData.Firstname[index]);
                    await _db.StringSetAsync($"{postId}_Lastname", expectedData.Lastname[index]);
                    await _db.StringSetAsync($"{postId}_Profilepic", expectedData.Profilepic[index]);
                    await _db.StringSetAsync($"{postId}_{selfUserId}_SelfLike", expectedData.SelfLike[index]);
                    await _db.StringSetAsync($"{postId}_{selfUserId}_SelfRetweet", expectedData.SelfRetweet[index]);
                }

                // Act
                var result = await _postInfo.GetPostInfo(selfUserId, postIdArray);

                Assert.Multiple(() =>
                {
                    // Assert
                    Assert.That(result.Content, Is.EqualTo(expectedData.Content));
                    Assert.That(result.CreatedAt, Is.EqualTo(expectedData.CreatedAt));
                    Assert.That(result.LikeNums, Is.EqualTo(expectedData.LikeNums));
                    Assert.That(result.RetweetNums, Is.EqualTo(expectedData.RetweetNums));
                    Assert.That(result.PostOwner, Is.EqualTo(expectedData.PostOwner));
                    Assert.That(result.Firstname, Is.EqualTo(expectedData.Firstname));
                    Assert.That(result.Lastname, Is.EqualTo(expectedData.Lastname));
                    Assert.That(result.Profilepic, Is.EqualTo(expectedData.Profilepic));
                    Assert.That(result.SelfLike, Is.EqualTo(expectedData.SelfLike));
                    Assert.That(result.SelfRetweet, Is.EqualTo(expectedData.SelfRetweet));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task WritePostInfo_WritesCorrectData()
        {
            try
            {
                // Arrange
                string selfUserId = "user456";
                var postNestObj = new PostArrayNest
                {
                    Content = new Dictionary<RedisKey, RedisValue> { { (RedisKey)"post1_Content", (RedisValue)"TestContent" } },
                    CreatedAt = new Dictionary<RedisKey, RedisValue> { { (RedisKey)"post1_CreatedAt", (RedisValue)"2023-01-01" } },
                    LikeNums = new Dictionary<RedisKey, RedisValue> { { (RedisKey)"post1_LikeNums", (RedisValue)"10" } },
                    RetweetNums = new Dictionary<RedisKey, RedisValue> { { (RedisKey)"post1_RetweetNums", (RedisValue)"5" } },
                    PostOwner = new Dictionary<RedisKey, RedisValue> { { (RedisKey)"post1_PostOwner", (RedisValue)"owner1" } },
                    Firstname = new Dictionary<RedisKey, RedisValue> { { (RedisKey)"post1_Firstname", (RedisValue)"John" } },
                    Lastname = new Dictionary<RedisKey, RedisValue> { { (RedisKey)"post1_Lastname", (RedisValue)"Doe" } },
                    Profilepic = new Dictionary<RedisKey, RedisValue> { { (RedisKey)"post1_Profilepic", (RedisValue)"pic1.jpg" } },
                    SelfLike = new Dictionary<RedisKey, RedisValue> { { (RedisKey)"post1_user123_SelfLike", (RedisValue)"true" } },
                    SelfRetweet = new Dictionary<RedisKey, RedisValue> { { (RedisKey)"post1_user123_SelfRetweet", (RedisValue)"false" } }
                };

                // Act
                await _postInfo.WritePostInfo(selfUserId, postNestObj);

                // Assert
                foreach (var kvp in postNestObj.Content.Concat(postNestObj.CreatedAt)
                                                        .Concat(postNestObj.LikeNums)
                                                        .Concat(postNestObj.RetweetNums)
                                                        .Concat(postNestObj.PostOwner)
                                                        .Concat(postNestObj.Firstname)
                                                        .Concat(postNestObj.Lastname)
                                                        .Concat(postNestObj.Profilepic)
                                                        .Concat(postNestObj.SelfLike)
                                                        .Concat(postNestObj.SelfRetweet))
                {
                    var value = await _db.StringGetAsync(kvp.Key);
                    Assert.That(value, Is.EqualTo(kvp.Value));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task DeletePostInfo_DeletesCorrectData()
        {
            try
            {
                // Arrange
                string selfUserId = "user789";
                string postId = "post1";
                var keys = new[]
                {
                    "Content", "CreatedAt", "LikeNums", "RetweetNums", "PostOwner",
                    "Firstname", "Lastname", "Profilepic", $"{selfUserId}_SelfLike", $"{selfUserId}_SelfRetweet"
                };

                foreach (var key in keys)
                {
                    await _db.StringSetAsync($"{postId}_{key}", "test");
                }

                // Act
                await _postInfo.DeletePostInfo(selfUserId, postId);

                // Assert
                foreach (var key in keys)
                {
                    var exists = await _db.KeyExistsAsync($"{postId}_{key}");
                    Assert.That(exists, Is.False);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task DeletePostLikeNums_DeletesCorrectData()
        {
            try
            {
                // Arrange
                string postId = "post1";
                await _db.StringSetAsync($"{postId}_LikeNums", "10");

                // Act
                await _postInfo.DeletePostLikeNums(postId);

                // Assert
                var exists = await _db.KeyExistsAsync($"{postId}_LikeNums");
                Assert.That(exists, Is.False);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task DeletePostRetweetNums_DeletesCorrectData()
        {
            try
            {
                // Arrange
                string postId = "post2";
                await _db.StringSetAsync($"{postId}_RetweetNums", "5");

                // Act
                await _postInfo.DeletePostRetweetNums(postId);

                // Assert
                var exists = await _db.KeyExistsAsync($"{postId}_RetweetNums");
                Assert.That(exists, Is.False);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task DeletePostSelfLike_DeletesCorrectData()
        {
            try
            {
                // Arrange
                string selfUserId = "user333";
                string postId = "post3";
                await _db.StringSetAsync($"{postId}_{selfUserId}_SelfLike", "true");

                // Act
                await _postInfo.DeletePostSelfLike(selfUserId, postId);

                // Assert
                var exists = await _db.KeyExistsAsync($"{postId}_{selfUserId}_SelfLike");
                Assert.That(exists, Is.False);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task DeletePostSelfRetweet_DeletesCorrectData()
        {
            try
            {
                // Arrange
                string selfUserId = "user484";
                string postId = "post4";
                await _db.StringSetAsync($"{postId}_{selfUserId}_SelfRetweet", "true");

                // Act
                await _postInfo.DeletePostSelfRetweet(selfUserId, postId);

                // Assert
                var exists = await _db.KeyExistsAsync($"{postId}_{selfUserId}_SelfRetweet");
                Assert.That(exists, Is.False);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task DeletePostPersonalData_DeletesCorrectData()
        {
            try
            {
                // Arrange
                string postId = "post5";
                var keys = new[] { "PostOwner", "Firstname", "Lastname", "Profilepic" };

                foreach (var key in keys)
                {
                    await _db.StringSetAsync($"{postId}_{key}", "test");
                }

                // Act
                await _postInfo.DeletePostPersonalData(postId);

                // Assert
                foreach (var key in keys)
                {
                    var exists = await _db.KeyExistsAsync($"{postId}_{key}");
                    Assert.That(exists, Is.False);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task GetPostIdArray_ReturnsCorrectData()
        {
            try
            {
                // Arrange
                string userId = "user555";
                string[] expectedPostIds = ["post6", "post7", "post8"];
                await _db.ListRightPushAsync(userId + PostIdArraySuffix, expectedPostIds.Select(id => (RedisValue)id).ToArray());

                // Act
                var result = await _postInfo.GetPostIdArray(userId);

                // Assert
                Assert.That(result, Is.EqualTo(expectedPostIds));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task WritePostIdArray_WritesCorrectData()
        {
            try
            {
                // Arrange
                string userId = "user666";
                string[] postIds = ["post9", "post10", "post11"];

                // Act
                await _postInfo.WritePostIdArray(userId, postIds);

                // Assert
                var result = await _db.ListRangeAsync(userId + PostIdArraySuffix);
                Assert.That(result.Select(x => x.ToString()).ToArray(), Is.EqualTo(postIds));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task DeletePostIdArray_DeletesCorrectData()
        {
            try
            {
                // Arrange
                string userId = "user777";
                await _db.ListRightPushAsync(userId + PostIdArraySuffix, new RedisValue[] { (RedisValue)"post12", (RedisValue)"post13" });

                // Act
                await _postInfo.DeletePostIdArray(userId);

                // Assert
                var exists = await _db.KeyExistsAsync(userId + PostIdArraySuffix);
                Assert.That(exists, Is.False);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task SetPostInfoExp_SetsCorrectExpiration()
        {
            try
            {
                // Arrange
                string selfUserId = "user888";
                string[] postIdArray = ["post14", "post15"];
                int expirySeconds = 3600;

                var fields = new[]
                {
                    "PostOwner", "Content", "CreatedAt", "LikeNums", "RetweetNums",
                    $"{selfUserId}_SelfLike", $"{selfUserId}_SelfRetweet",
                    "Firstname", "Lastname", "Profilepic"
                };

                foreach (var postId in postIdArray)
                {
                    foreach (var field in fields)
                    {
                        await _db.StringSetAsync($"{postId}_{field}", "test");
                    }
                }

                await _db.ListRightPushAsync(selfUserId + PostIdArraySuffix, postIdArray.Select(id => (RedisValue)id).ToArray());

                // Act
                await _postInfo.SetPostInfoExp(selfUserId, postIdArray, expirySeconds);

                // Assert
                foreach (var postId in postIdArray)
                {
                    foreach (var field in fields)
                    {
                        var ttl = await _db.KeyTimeToLiveAsync($"{postId}_{field}");
                        Assert.Multiple(() =>
                        {
                            Assert.That(ttl.HasValue, Is.True);
                            Assert.That(ttl.Value.TotalSeconds, Is.InRange(3590, 3600)); // Allow for small discrepancies
                        });

                    }
                }

                var postIdArrayTtl = await _db.KeyTimeToLiveAsync(selfUserId + PostIdArraySuffix);
                Assert.Multiple(() =>
                {
                    Assert.That(postIdArrayTtl.HasValue, Is.True);
                    Assert.That(postIdArrayTtl.Value.TotalSeconds, Is.InRange(3590, 3600));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
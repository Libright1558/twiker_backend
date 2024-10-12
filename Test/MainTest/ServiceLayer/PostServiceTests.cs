using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using twiker_backend.ServiceLayer;
using twiker_backend.Models.DatabaseContext;
using twiker_backend.Db.Models;
using twiker_backend.Redis;
using twiker_backend.Db.Repository;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Test.MainTest.Redis;
using Microsoft.Extensions.Logging;

[TestFixture, Category("PostService")]
public class PostServiceTests
{
    private PostService _postService;
    private Mock<ILogger<PostService>> _loggerMock;
    private DbContextOptions<TwikerContext> _options;
    private TwikerContext _context;
    private IConnectionMultiplexer _connectionMultiplexer;
    private IDatabase _redisDb;
    private IRedisPostData _redisPostInfo;
    private IDbPostInfo _dbPostInfo;

    [OneTimeSetUp]
    public void Setup()
    {
        DotNetEnv.Env.TraversePath().Load();
        _options = new DbContextOptionsBuilder<TwikerContext>()
        .UseNpgsql(DotNetEnv.Env.GetString("connection_mock"))
        .Options;

        // Initialize DbContext
        _context = new TwikerContext(_options);

        // Setup Redis connection
        _connectionMultiplexer = RedisConnectOperation.Connection;
        _redisDb = _connectionMultiplexer.GetDatabase();

        // Initialize PostService
        _redisPostInfo = new PostInfo(_connectionMultiplexer);
        _dbPostInfo = new DbPostInfo(_context);

        _loggerMock = new Mock<ILogger<PostService>>();
        _postService = new PostService(_redisPostInfo, _dbPostInfo, _loggerMock.Object);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _context.PostTables.ExecuteDelete();
        _context.UserTables.ExecuteDelete();
        _context.Dispose();
        _connectionMultiplexer.Dispose();
    }

    [Test]
    public async Task GetPost_NoExistingData_ReturnsPostsFromDbAndCachesInRedis()
    {
        try
        {
            // Arrange
            var userId = Guid.NewGuid();
            var username = "testuser1";
            var user = new UserTable { 
                UserId = userId,
                Firstname = "Jane",
                Lastname = "Smith",
                Username = username,
                Email = "jane1@example.com",
                Profilepic = "jane_profile.jpg"
            };

            _context.UserTables.Add(user);
            await _context.SaveChangesAsync();

            var post = new PostTable { PostId = Guid.NewGuid(), Postby = username, Content = "Test post" };
            _context.PostTables.Add(post);
            await _context.SaveChangesAsync();

            // Act
            var result = await _postService.GetPost(userId, username);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result?[0]?.Content, Is.EqualTo("Test post"));

            // Verify data was cached in Redis
            var cachedPostIds = await _redisDb.ListRangeAsync($"{userId}_PostIdArray");
            Assert.That(cachedPostIds.Length, Is.EqualTo(1));
            Assert.That(cachedPostIds[0].ToString(), Is.EqualTo(post.PostId.ToString()));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Test]
    public async Task GetPost_ExistingDataInRedis_ReturnsCachedData()
    {
        try
        {
            // Arrange
            var userId = Guid.NewGuid();
            var username = "testuser2";
            var user = new UserTable { 
                UserId = userId,
                Firstname = "Jane",
                Lastname = "Smith",
                Username = username,
                Email = "jane2@example.com",
                Profilepic = "jane_profile.jpg" 
            };
            _context.UserTables.Add(user);
            await _context.SaveChangesAsync();

            var postId = Guid.NewGuid();
            await _redisDb.ListRightPushAsync($"{userId}_PostIdArray", postId.ToString());
            await _redisDb.StringSetAsync($"{postId}_Content", "Cached post");

            // Act
            var result = await _postService.GetPost(userId, username);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result?[0]?.Content, Is.EqualTo("Cached post"));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Test]
    public async Task WritePost_ValidInput_AddsPostToDbAndInvalidatesRedisCache()
    {
        try
        {
            // Arrange
            var userId = Guid.NewGuid();
            var username = "testuser3";
            var user = new UserTable { 
                UserId = userId,
                Firstname = "Jane",
                Lastname = "Smith",
                Username = username,
                Email = "jane3@example.com",
                Profilepic = "jane_profile.jpg" 
            };
            _context.UserTables.Add(user);
            await _context.SaveChangesAsync();

            var content = "New test post";

            // Act
            var result = await _postService.WritePost(userId, username, content);

            // Assert
            Assert.That(result, Is.Not.Null);

            // Verify post was added to DB
            var postInDb = await _context.PostTables.FirstOrDefaultAsync(p => p.Content == content);
            Assert.That(postInDb, Is.Not.Null);

            // Verify Redis cache was invalidated
            var cachedPostIds = await _redisDb.ListRangeAsync($"{userId}_PostIdArray");
            Assert.That(cachedPostIds.Length, Is.EqualTo(0));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Test]
    public async Task UpdateLike_NewLike_AddsLikeAndUpdatesRedisCache()
    {
        try
        {
            // Arrange
            var userId = Guid.NewGuid();
            var username = "testuser4";
            var user = new UserTable { 
                UserId = userId,
                Firstname = "Jane",
                Lastname = "Smith",
                Username = username,
                Email = "jane4@example.com",
                Profilepic = "jane_profile.jpg" 
            };
            _context.UserTables.Add(user);
            await _context.SaveChangesAsync();

            var postId = Guid.NewGuid();
            var post = new PostTable { PostId = postId, Postby = "testuser4", Content = "Test post" };
            _context.PostTables.Add(post);
            await _context.SaveChangesAsync();

            // Act
            await _postService.UpdateLike(postId, userId, username);

            // Assert
            var likeInDb = await _context.LikeTables.FirstOrDefaultAsync(l => l.PostId == postId && l.Username == username);
            Assert.That(likeInDb, Is.Not.Null);

            // Verify Redis cache was updated
            var cachedLikeNum = await _redisDb.StringGetAsync($"{postId}_LikeNums");
            Assert.That(cachedLikeNum.HasValue, Is.False);
            var cachedSelfLike = await _redisDb.StringGetAsync($"{postId}_{userId}_SelfLike");
            Assert.That(cachedSelfLike.HasValue, Is.False);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Test]
    public async Task UpdateRetweet_NewRetweet_AddsRetweetAndUpdatesRedisCache()
    {
        try
        {
            // Arrange
            var userId = Guid.NewGuid();
            var username = "testuser5";
            var user = new UserTable { 
                UserId = userId,
                Firstname = "Jane",
                Lastname = "Smith",
                Username = username,
                Email = "jane5@example.com",
                Profilepic = "jane_profile.jpg" 
            };
            _context.UserTables.Add(user);
            await _context.SaveChangesAsync();

            var postId = Guid.NewGuid();
            var post = new PostTable { PostId = postId, Postby = "testuser5", Content = "Test post" };
            _context.PostTables.Add(post);
            await _context.SaveChangesAsync();

            // Act
            await _postService.UpdateRetweet(postId, userId, username);

            // Assert
            var retweetInDb = await _context.RetweetTables.FirstOrDefaultAsync(r => r.PostId == postId && r.Username == username);
            Assert.That(retweetInDb, Is.Not.Null);

            // Verify Redis cache was updated
            var cachedRetweetNum = await _redisDb.StringGetAsync($"{postId}_RetweetNums");
            Assert.That(cachedRetweetNum.HasValue, Is.False);
            var cachedSelfRetweet = await _redisDb.StringGetAsync($"{postId}_{userId}_SelfRetweet");
            Assert.That(cachedSelfRetweet.HasValue, Is.False);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Test]
    public async Task DeletePost_ExistingPost_DeletesPostAndUpdatesRedisCache()
    {
        try
        {
            // Arrange
            var userId = Guid.NewGuid();
            var username = "testuser6";
            var user = new UserTable { 
                UserId = userId,
                Firstname = "Jane",
                Lastname = "Smith",
                Username = username,
                Email = "jane6@example.com",
                Profilepic = "jane_profile.jpg" 
            };
            _context.UserTables.Add(user);
            await _context.SaveChangesAsync();

            var postId = Guid.NewGuid();
            var post = new PostTable { PostId = postId, Postby = "testuser6", Content = "Test post" };
            _context.PostTables.Add(post);
            await _context.SaveChangesAsync();
            await _redisDb.StringSetAsync($"{postId}_Content", "Test post");

            // Act
            await _postService.DeletePost(userId, postId);

            // Assert
            var postInDb = await _context.PostTables.FindAsync(postId);
            Assert.That(postInDb, Is.Null);

            // Verify Redis cache was updated
            var cachedContent = await _redisDb.StringGetAsync($"{postId}_Content");
            Assert.That(cachedContent.HasValue, Is.False);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
using Microsoft.EntityFrameworkCore;
using twiker_backend.Models.DatabaseContext;
using twiker_backend.Db.Models;
using twiker_backend.Db.Repository;

namespace Db.PostInfoTest
{
    [TestFixture, Category("DbPostData")]
    public class DbPostInfoTests
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
            _context.PostTables.ExecuteDelete();
            _context.UserTables.ExecuteDelete();
            _context.Dispose();
        }

        [Test]
        public async Task AppendPost_Test()
        {
            try
            {
                // Arrange
                var _dbPostInfo = new DbPostInfo(_context);
                var _DbUserInfo = new DbUserInfo(_context);
                var user = new UserTable {
                    Firstname = "eee",
                    Lastname = "rrr",
                    Username = "pusheen1",
                    Email = "cat1@gmail.com",
                    Password = "?????",
                    Profilepic = "path"
                };
                await _DbUserInfo.WriteUserData(user);

                // Act
                var result = await _dbPostInfo.AppendPost("pusheen1", "meow");
                var postResult = await _dbPostInfo.GetPostsByUser("pusheen1");
                var post = postResult.ToList();

                Assert.Multiple(() =>
                {
                    // Assert
                    Assert.That(post[0]!.PostId, Is.EqualTo(result.PostId));
                    Assert.That(post[0]!.CreatedAt, Is.EqualTo(result.CreatedAt));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task AppendLike_Test()
        {
            try
            {
                // Arrange
                var _dbPostInfo = new DbPostInfo(_context);
                var _DbUserInfo = new DbUserInfo(_context);
                var user = new UserTable {
                    Firstname = "eee",
                    Lastname = "rrr",
                    Username = "pusheen2",
                    Email = "cat2@gmail.com",
                    Password = "?????",
                    Profilepic = "path"
                };
                await _DbUserInfo.WriteUserData(user);
                var post = await _dbPostInfo.AppendPost("pusheen2", "meow");
                var postId = post.PostId;

                // Act
                var result = await _dbPostInfo.AddLike(postId, "pusheen2");

                // Assert
                Assert.That(result, Is.EqualTo(1));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task AppendRetweet_Test()
        {
            try
            {
                // Arrange
                var _dbPostInfo = new DbPostInfo(_context);
                var _DbUserInfo = new DbUserInfo(_context);
                var user = new UserTable {
                    Firstname = "eee",
                    Lastname = "rrr",
                    Username = "pusheen3",
                    Email = "cat3@gmail.com",
                    Password = "?????",
                    Profilepic = "path"
                };
                await _DbUserInfo.WriteUserData(user);
                var post = await _dbPostInfo.AppendPost("pusheen3", "meow");
                var postId = post.PostId;

                // Act
                var result = await _dbPostInfo.AddRetweet(postId, "pusheen3");

                // Assert
                Assert.That(result, Is.EqualTo(1));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task DeletePost_Test()
        {
            try
            {
                // Arrange
                var _dbPostInfo = new DbPostInfo(_context);
                var _DbUserInfo = new DbUserInfo(_context);
                var user = new UserTable {
                    Firstname = "eee",
                    Lastname = "rrr",
                    Username = "pusheen4",
                    Email = "cat4@gmail.com",
                    Password = "?????",
                    Profilepic = "path"
                };
                await _DbUserInfo.WriteUserData(user);
                var post = await _dbPostInfo.AppendPost("pusheen4", "meow");
                var postId = post.PostId;

                // Act
                var result = await _dbPostInfo.DeletePost(postId);

                // Assert
                Assert.That(result, Is.EqualTo(1));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task DeleteLike_Test()
        {
            try
            {
                // Arrange
                var _dbPostInfo = new DbPostInfo(_context);
                var _DbUserInfo = new DbUserInfo(_context);
                var user = new UserTable {
                    Firstname = "eee",
                    Lastname = "rrr",
                    Username = "pusheen5",
                    Email = "cat5@gmail.com",
                    Password = "?????",
                    Profilepic = "path"
                };
                await _DbUserInfo.WriteUserData(user);
                var post = await _dbPostInfo.AppendPost("pusheen5", "meow");
                var postId = post.PostId;
                await _dbPostInfo.AddLike(postId, "pusheen5");

                // Act
                var LikeResult = await _dbPostInfo.DeleteLike(postId, "pusheen5");

                // Assert
                Assert.That(LikeResult, Is.EqualTo(1));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task DeleteRetweet_Test()
        {
            try
            {
                // Arrange
                var _dbPostInfo = new DbPostInfo(_context);
                var _DbUserInfo = new DbUserInfo(_context);
                var user = new UserTable {
                    Firstname = "eee",
                    Lastname = "rrr",
                    Username = "pusheen6",
                    Email = "cat6@gmail.com",
                    Password = "?????",
                    Profilepic = "path"
                };
                await _DbUserInfo.WriteUserData(user);
                var post = await _dbPostInfo.AppendPost("pusheen6", "meow");
                var postId = post.PostId;
                await _dbPostInfo.AddRetweet(postId, "pusheen6");

                // Act
                var RetweetResult = await _dbPostInfo.DeleteRetweet(postId, "pusheen6");

                // Assert
                Assert.That(RetweetResult, Is.EqualTo(1));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task GetPostsByUser_Test()
        {
            try
            {
                // Arrange
                var _dbPostInfo = new DbPostInfo(_context);
                var _DbUserInfo = new DbUserInfo(_context);
                var user = new UserTable {
                    Firstname = "eee",
                    Lastname = "rrr",
                    Username = "pusheen7",
                    Email = "cat7@gmail.com",
                    Password = "?????",
                    Profilepic = "path"
                };
                await _DbUserInfo.WriteUserData(user);
                var post = await _dbPostInfo.AppendPost("pusheen7", "meow");
                var postId = post.PostId;

                // Act
                var result = await _dbPostInfo.GetPostsByUser("pusheen7");
                var posts = result.ToList();

                Assert.Multiple(() =>
                {
                    // Assert
                    Assert.That(posts[0]!.Postby, Is.EqualTo("pusheen7"));
                    Assert.That(posts[0]!.Content, Is.EqualTo("meow"));
                    Assert.That(posts[0]!.CreatedAt, Is.Not.Null);
                    Assert.That(posts[0]!.PostId, Is.EqualTo(postId));
                    Assert.That(posts[0]!.LikeNum, Is.EqualTo(0));
                    Assert.That(posts[0]!.RetweetNum, Is.EqualTo(0));
                    Assert.That(posts[0]!.SelfLike, Is.EqualTo(false));
                    Assert.That(posts[0]!.SelfRetweet, Is.EqualTo(false));
                    Assert.That(posts[0]!.Firstname, Is.EqualTo("eee"));
                    Assert.That(posts[0]!.Lastname, Is.EqualTo("rrr"));
                    Assert.That(posts[0]!.Profilepic, Is.EqualTo("path"));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task FetchLikeNum_Test()
        {
            try
            {
                // Arrange
                var _dbPostInfo = new DbPostInfo(_context);
                var _DbUserInfo = new DbUserInfo(_context);
                var user = new UserTable {
                    Firstname = "eee",
                    Lastname = "rrr",
                    Username = "pusheen8",
                    Email = "cat8@gmail.com",
                    Password = "?????",
                    Profilepic = "path"
                };
                await _DbUserInfo.WriteUserData(user);
                var post = await _dbPostInfo.AppendPost("pusheen8", "meow");
                var postId = post.PostId;

                // Act
                var result = await _dbPostInfo.FetchLikeNumAsync([postId]);

                // Assert
                Assert.That(result[0]!.LikeNum, Is.EqualTo(0));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task FetchRetweetNum_Test()
        {
            try
            {
                // Arrange
                var _dbPostInfo = new DbPostInfo(_context);
                var _DbUserInfo = new DbUserInfo(_context);
                var user = new UserTable {
                    Firstname = "eee",
                    Lastname = "rrr",
                    Username = "pusheen9",
                    Email = "cat9@gmail.com",
                    Password = "?????",
                    Profilepic = "path"
                };
                await _DbUserInfo.WriteUserData(user);
                var post = await _dbPostInfo.AppendPost("pusheen9", "meow");
                var postId = post.PostId;

                // Act
                var result = await _dbPostInfo.FetchRetweetNumAsync([postId]);

                // Assert
                Assert.That(result[0]!.RetweetNum, Is.EqualTo(0));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task FetchSelfLike_Test()
        {
            try
            {
                // Arrange
                var _dbPostInfo = new DbPostInfo(_context);
                var _DbUserInfo = new DbUserInfo(_context);
                var user = new UserTable {
                    Firstname = "eee",
                    Lastname = "rrr",
                    Username = "pusheen10",
                    Email = "cat10@gmail.com",
                    Password = "?????",
                    Profilepic = "path"
                };
                await _DbUserInfo.WriteUserData(user);
                var post = await _dbPostInfo.AppendPost("pusheen10", "meow");
                var postId = post.PostId;

                // Act
                var result = await _dbPostInfo.FetchSelfLikeAsync([postId], "pusheen10");

                // Assert
                Assert.That(result[0]!.SelfLike, Is.EqualTo(false));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task FetchSelfRetweet_Test()
        {
            try
            {
                // Arrange
                var _dbPostInfo = new DbPostInfo(_context);
                var _DbUserInfo = new DbUserInfo(_context);
                var user = new UserTable {
                    Firstname = "eee",
                    Lastname = "rrr",
                    Username = "pusheen11",
                    Email = "cat11@gmail.com",
                    Password = "?????",
                    Profilepic = "path"
                };
                await _DbUserInfo.WriteUserData(user);
                var post = await _dbPostInfo.AppendPost("pusheen11", "meow");
                var postId = post.PostId;

                // Act
                var result = await _dbPostInfo.FetchSelfRetweetAsync([postId], "pusheen11");

                // Assert
                Assert.That(result[0]!.SelfRetweet, Is.EqualTo(false));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task FetchPostOwner_Test()
        {
            try
            {
                // Arrange
                var _dbPostInfo = new DbPostInfo(_context);
                var _DbUserInfo = new DbUserInfo(_context);
                var user = new UserTable {
                    Firstname = "eee",
                    Lastname = "rrr",
                    Username = "pusheen12",
                    Email = "cat12@gmail.com",
                    Password = "?????",
                    Profilepic = "path"
                };
                await _DbUserInfo.WriteUserData(user);
                var post = await _dbPostInfo.AppendPost("pusheen12", "meow");
                var postId = post.PostId;

                // Act
                var result = await _dbPostInfo.FetchPostOwnerAsync([postId]);

                // Assert
                Assert.That(result[0]!.Postby, Is.EqualTo("pusheen12"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task FetchPostContent_Test()
        {
            try
            {
                // Arrange
                var _dbPostInfo = new DbPostInfo(_context);
                var _DbUserInfo = new DbUserInfo(_context);
                var user = new UserTable {
                    Firstname = "eee",
                    Lastname = "rrr",
                    Username = "pusheen13",
                    Email = "cat13@gmail.com",
                    Password = "?????",
                    Profilepic = "path"
                };
                await _DbUserInfo.WriteUserData(user);
                var post = await _dbPostInfo.AppendPost("pusheen13", "meow");
                var postId = post.PostId;

                // Act
                var result = await _dbPostInfo.FetchPostContentAsync([postId]);

                // Assert
                Assert.That(result[0]!.Content, Is.EqualTo("meow"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task FetchPostPostTime_Test()
        {
            try
            {
                // Arrange
                var _dbPostInfo = new DbPostInfo(_context);
                var _DbUserInfo = new DbUserInfo(_context);
                var user = new UserTable {
                    Firstname = "eee",
                    Lastname = "rrr",
                    Username = "pusheen14",
                    Email = "cat14@gmail.com",
                    Password = "?????",
                    Profilepic = "path"
                };
                await _DbUserInfo.WriteUserData(user);
                var post = await _dbPostInfo.AppendPost("pusheen14", "meow");
                var postId = post.PostId;

                // Act
                var result = await _dbPostInfo.FetchPostPostTimeAsync([postId]);

                // Assert
                Assert.That(result[0]!.CreatedAt, Is.Not.Null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }   
        }

        [Test]
        public async Task FetchPostFirstname_Test()
        {
            try
            {
                // Arrange
                var _dbPostInfo = new DbPostInfo(_context);
                var _DbUserInfo = new DbUserInfo(_context);
                var user = new UserTable {
                    Firstname = "eee",
                    Lastname = "rrr",
                    Username = "pusheen15",
                    Email = "cat15@gmail.com",
                    Password = "?????",
                    Profilepic = "path"
                };
                await _DbUserInfo.WriteUserData(user);
                var post = await _dbPostInfo.AppendPost("pusheen15", "meow");
                var postId = post.PostId;

                // Act
                var result = await _dbPostInfo.FetchPostFirstnameAsync([postId]);

                // Assert
                Assert.That(result[0]!.Firstname, Is.EqualTo("eee"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task FetchPostLastname_Test()
        {
            try
            {
                // Arrange
                var _dbPostInfo = new DbPostInfo(_context);
                var _DbUserInfo = new DbUserInfo(_context);
                var user = new UserTable {
                    Firstname = "eee",
                    Lastname = "rrr",
                    Username = "pusheen16",
                    Email = "cat16@gmail.com",
                    Password = "?????",
                    Profilepic = "path"
                };
                await _DbUserInfo.WriteUserData(user);
                var post = await _dbPostInfo.AppendPost("pusheen16", "meow");
                var postId = post.PostId;

                // Act
                var result = await _dbPostInfo.FetchPostLastnameAsync([postId]);

                // Assert
                Assert.That(result[0]!.Lastname, Is.EqualTo("rrr"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [Test]
        public async Task FetchPostProfilepic_Test()
        {
            try
            {
                // Arrange
                var _dbPostInfo = new DbPostInfo(_context);
                var _DbUserInfo = new DbUserInfo(_context);
                var user = new UserTable {
                    Firstname = "eee",
                    Lastname = "rrr",
                    Username = "pusheen17",
                    Email = "cat17@gmail.com",
                    Password = "?????",
                    Profilepic = "path"
                };
                await _DbUserInfo.WriteUserData(user);
                var post = await _dbPostInfo.AppendPost("pusheen17", "meow");
                var postId = post.PostId;

                // Act
                var result = await _dbPostInfo.FetchPostProfilepicAsync([postId]);

                // Assert
                Assert.That(result[0]!.Profilepic, Is.EqualTo("path"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
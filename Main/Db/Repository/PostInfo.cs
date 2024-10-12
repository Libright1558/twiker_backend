using Microsoft.EntityFrameworkCore;
using Dapper;
using System.Threading.Tasks;
using System.Linq;
using Npgsql;
using Db.QueryPostSql;
using twiker_backend.Models.DatabaseContext;
using twiker_backend.Db.Models;

namespace twiker_backend.Db.Repository
{

    public class DbPostInfo(TwikerContext context) : IDbPostInfo
    {
        private readonly TwikerContext _context = context;
        public async Task<PostFetch> AppendPost(string postBy, string content)
        {
            try
            {
                var post = new PostTable { Postby = postBy, Content = content };
                _context.PostTables.Add(post);
                await _context.SaveChangesAsync();
                return new PostFetch {
                    PostId = post.PostId, 
                    CreatedAt = post.CreatedAt
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<int> AddLike(Guid postId, string username)
        {
            try
            {
                var like = new LikeTable { PostId = postId, Username = username };
                _context.LikeTables.Add(like);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<int> AddRetweet(Guid postId, string username)
        {
            try
            {
                var retweet = new RetweetTable { PostId = postId, Username = username };
                _context.RetweetTables.Add(retweet);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<int> DeletePost(Guid postId)
        {
            try
            {
                var post = await _context.PostTables.FindAsync(postId);
                if (post != null)
                {
                    _context.PostTables.Remove(post);
                }
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            
        }

        public async Task<int> DeleteLike(Guid postId, string username)
        {
            try
            {
                var like = await _context.LikeTables.FirstOrDefaultAsync(l => l.PostId == postId && l.Username == username);
                if (like != null)
                {
                    _context.LikeTables.Remove(like);
                }
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<int> DeleteRetweet(Guid postId, string username)
        {
            try
            {
                var retweet = await _context.RetweetTables.FirstOrDefaultAsync(r => r.PostId == postId && r.Username == username);
                if (retweet != null)
                {
                    _context.RetweetTables.Remove(retweet);
                }
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<IEnumerable<PostFetch?>> GetPostsByUser(string postBy)
        {
            using var connection = new NpgsqlConnection(DbConnectManager.DbConnectionString);
            
            try
            {
                await connection.OpenAsync();
                var posts = await connection.QueryAsync<PostFetch>(
                QueryPostSqlString.FetchPost, 
                new { PostBy = postBy });

                return posts;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            finally
            {
                await connection.CloseAsync();
            }        
        }

        public async Task<List<PostFetch>> FetchLikeNumAsync(Guid[] postIdArray)
        {
            try
            {
                return await _context.PostTables
                .Where(p => postIdArray.Contains(p.PostId))
                .GroupJoin(_context.LikeTables,
                    post => post.PostId,
                    like => like.PostId,
                    (posts, likes) => new { posts.PostId, LikeNum = likes.Count() })
                .Select(result => new PostFetch {
                    PostId = result.PostId, 
                    LikeNum = result.LikeNum
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<PostFetch>> FetchSingleLikeNumAsync(Guid postId)
        {
            try
            {
                return await _context.LikeTables
                .Where(p => p.PostId == postId)
                .GroupBy(
                    p => p.PostId,
                    p => p.Username,
                    (PostId, User) => new { PostId, LikeNum = User.Count() })
                .Select(result => new PostFetch {
                    PostId = result.PostId, 
                    LikeNum = result.LikeNum
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }


        public async Task<List<PostFetch>> FetchRetweetNumAsync(Guid[] postIdArray)
        {
            try
            {
                return await _context.PostTables
                .Where(p => postIdArray.Contains(p.PostId))
                .GroupJoin(_context.RetweetTables,
                    post => post.PostId,
                    retweet => retweet.PostId,
                    (posts, retweets) => new { posts.PostId, RetweetNum = retweets.Count() })
                .Select(result => new PostFetch {
                    PostId = result.PostId, 
                    RetweetNum = result.RetweetNum
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<PostFetch>> FetchSingleRetweetNumAsync(Guid postId)
        {
            try
            {
                return await _context.RetweetTables
                .Where(p => p.PostId == postId)
                .GroupBy(
                    p => p.PostId,
                    p => p.Username,
                    (PostId, User) => new { PostId, RetweetNum = User.Count() })
                .Select(result => new PostFetch {
                    PostId = result.PostId, 
                    RetweetNum = result.RetweetNum
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<PostFetch>> FetchSelfLikeAsync(Guid[] postIdArray, string username)
        {
            try
            {
                return await _context.PostTables
                .Where(p => postIdArray.Contains(p.PostId))
                .GroupJoin(_context.LikeTables,
                    post => post.PostId,
                    like => like.PostId,
                    (posts, likes) => new { posts.PostId, SelfLike = likes.Where(l => l.Username == username).Any() })
                .Select(result => new PostFetch {
                    PostId = result.PostId, 
                    SelfLike = result.SelfLike
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<bool> FetchSingleSelfLikeAsync(Guid postId, string username)
        {
            try
            {
                var result = await _context.LikeTables
                .FirstOrDefaultAsync(p => p.Username == username && p.PostId == postId);

                return result != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<PostFetch>> FetchSelfRetweetAsync(Guid[] postIdArray, string username)
        {
            try
            {
                return await _context.PostTables
                .Where(p => postIdArray.Contains(p.PostId))
                .GroupJoin(_context.RetweetTables,
                    post => post.PostId,
                    retweet => retweet.PostId,
                    (posts, retweets) => new { posts.PostId, SelfRetweet = retweets.Where(l => l.Username == username).Any() })
                .Select(result => new PostFetch {
                    PostId = result.PostId, 
                    SelfRetweet = result.SelfRetweet
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<bool> FetchSingleSelfRetweetAsync(Guid postId, string username)
        {
            try
            {
                var result = await _context.RetweetTables
                .FirstOrDefaultAsync(p => p.Username == username && p.PostId == postId);

                return result != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<PostFetch>> FetchPostOwnerAsync(Guid[] postIdArray)
        {
            try
            {
                return await _context.PostTables
                .Join(postIdArray,
                PostTables => PostTables.PostId,
                IdArray => IdArray,
                (Post, IdArray) => new {
                    PostId = IdArray,
                    Post.Postby
                })
                .Select(p => new PostFetch {
                    PostId = p.PostId, 
                    Postby = p.Postby
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<PostFetch>> FetchSinglePostOwnerAsync(Guid postId)
        {
            try
            {
                return await _context.PostTables
                .Where(p => p.PostId == postId)
                .Select(p => new PostFetch {
                    PostId = p.PostId, 
                    Postby = p.Postby
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<PostFetch>> FetchPostContentAsync(Guid[] postIdArray)
        {
            try
            {
                return await _context.PostTables
                .Join(postIdArray,
                PostTables => PostTables.PostId,
                IdArray => IdArray,
                (Post, IdArray) => new {
                    PostId = IdArray,
                    Post.Content
                })
                .Select(p => new PostFetch {
                    PostId = p.PostId, 
                    Content = p.Content
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<PostFetch>> FetchSinglePostContentAsync(Guid postId)
        {
            try
            {
                return await _context.PostTables
                .Where(p => p.PostId == postId)
                .Select(p => new PostFetch {
                    PostId = p.PostId, 
                    Content = p.Content
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<PostFetch>> FetchPostPostTimeAsync(Guid[] postIdArray)
        {
            try
            {
                return await _context.PostTables
                .Join(postIdArray,
                PostTables => PostTables.PostId,
                IdArray => IdArray,
                (Post, IdArray) => new {
                    PostId = IdArray,
                    Post.CreatedAt
                })
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostFetch {
                    PostId = p.PostId, 
                    CreatedAt = p.CreatedAt
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<PostFetch>> FetchSinglePostPostTimeAsync(Guid postId)
        {
            try
            {
                return await _context.PostTables
                .Where(p => p.PostId == postId)
                .Select(p => new PostFetch {
                    PostId = p.PostId, 
                    CreatedAt = p.CreatedAt
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<PostFetch>> FetchPostFirstnameAsync(Guid[] postIdArray)
        {
            try
            {
                return await _context.PostTables
                .Join(_context.UserTables,
                    post => post.Postby,
                    user => user.Username,
                    (post, user) => new { post.PostId, user.Firstname })
                .Join(postIdArray,
                    IdAndFirstname => IdAndFirstname.PostId,
                    IdArray => IdArray,
                    (Post, IdArray) => new { PostId = IdArray, Post.Firstname })
                .Select(result => new PostFetch {
                    PostId = result.PostId, 
                    Firstname = result.Firstname
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<PostFetch>> FetchSinglePostFirstnameAsync(Guid postId)
        {
            try
            {
                return await _context.PostTables
                .Where(p => p.PostId == postId)
                .Join(_context.UserTables,
                    post => post.Postby,
                    user => user.Username,
                    (post, user) => new { post.PostId, user.Firstname })
                .Select(result => new PostFetch {
                    PostId = result.PostId, 
                    Firstname = result.Firstname
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<PostFetch>> FetchPostLastnameAsync(Guid[] postIdArray)
        {
            try
            {
                return await _context.PostTables
                .Join(_context.UserTables,
                    post => post.Postby,
                    user => user.Username,
                    (post, user) => new { post.PostId, user.Lastname })
                .Join(postIdArray,
                    IdAndLastname => IdAndLastname.PostId,
                    IdArray => IdArray,
                    (Post, IdArray) => new { PostId = IdArray, Post.Lastname })
                .Select(result => new PostFetch {
                    PostId = result.PostId, 
                    Lastname = result.Lastname
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<PostFetch>> FetchSinglePostLastnameAsync(Guid postId)
        {
            try
            {
                return await _context.PostTables
                .Where(p => p.PostId == postId)
                .Join(_context.UserTables,
                    post => post.Postby,
                    user => user.Username,
                    (post, user) => new { post.PostId, user.Lastname })
                .Select(result => new PostFetch {
                    PostId = result.PostId, 
                    Lastname = result.Lastname
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<PostFetch>> FetchPostProfilepicAsync(Guid[] postIdArray)
        {
            try
            {
                return await _context.PostTables
                .Join(_context.UserTables,
                    post => post.Postby,
                    user => user.Username,
                    (post, user) => new { post.PostId, user.Profilepic })
                .Join(postIdArray,
                    IdAndProfilepic => IdAndProfilepic.PostId,
                    IdArray => IdArray,
                    (Post, IdArray) => new { PostId = IdArray, Post.Profilepic })
                .Select(result => new PostFetch {
                    PostId = result.PostId, 
                    Profilepic = result.Profilepic
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<PostFetch>> FetchSinglePostProfilepicAsync(Guid postId)
        {
            try
            {
                return await _context.PostTables
                .Where(p => p.PostId == postId)
                .Join(_context.UserTables,
                    post => post.Postby,
                    user => user.Username,
                    (post, user) => new { post.PostId, user.Profilepic })
                .Select(result => new PostFetch {
                    PostId = result.PostId, 
                    Profilepic = result.Profilepic
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}
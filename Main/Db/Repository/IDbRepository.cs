using twiker_backend.Db.Models;

namespace twiker_backend.Db.Repository
{
    public interface IDbUserInfo
    {
        Task<UserDbData?> GetUserData(Guid UserId);

        Task<int> WriteUserData(UserTable UserInfo);

        Task DeleteUserData(Guid UserId);

        Task<UserDbData?> FindOneUser(string UsernameOrEmail);
    }

    public interface IDbPostInfo
    {
        Task<PostFetch> AppendPost(string postBy, string content);

        Task<int> AddLike(Guid postId, string username);

        Task<int> AddRetweet(Guid postId, string username);

        Task<int> DeletePost(Guid postId);

        Task<int> DeleteLike(Guid postId, string username);

        Task<int> DeleteRetweet(Guid postId, string username);

        Task<IEnumerable<PostFetch?>> GetPostsByUser(string postBy);

        Task<List<PostFetch>> FetchLikeNumAsync(Guid[] postIdArray);

        Task<List<PostFetch>> FetchSingleLikeNumAsync(Guid postId);

        Task<List<PostFetch>> FetchRetweetNumAsync(Guid[] postIdArray);

        Task<List<PostFetch>> FetchSingleRetweetNumAsync(Guid postId);

        Task<List<PostFetch>> FetchSelfLikeAsync(Guid[] postIdArray, string username);

        Task<bool> FetchSingleSelfLikeAsync(Guid postId, string username);

        Task<List<PostFetch>> FetchSelfRetweetAsync(Guid[] postIdArray, string username);

        Task<bool> FetchSingleSelfRetweetAsync(Guid postId, string username);

        Task<List<PostFetch>> FetchPostOwnerAsync(Guid[] postIdArray);

        Task<List<PostFetch>> FetchSinglePostOwnerAsync(Guid postId);

        Task<List<PostFetch>> FetchPostContentAsync(Guid[] postIdArray);

        Task<List<PostFetch>> FetchSinglePostContentAsync(Guid postId);

        Task<List<PostFetch>> FetchPostPostTimeAsync(Guid[] postIdArray);

        Task<List<PostFetch>> FetchSinglePostPostTimeAsync(Guid postId);

        Task<List<PostFetch>> FetchPostFirstnameAsync(Guid[] postIdArray);

        Task<List<PostFetch>> FetchSinglePostFirstnameAsync(Guid postId);

        Task<List<PostFetch>> FetchPostLastnameAsync(Guid[] postIdArray);

        Task<List<PostFetch>> FetchSinglePostLastnameAsync(Guid postId);

        Task<List<PostFetch>> FetchPostProfilepicAsync(Guid[] postIdArray);

        Task<List<PostFetch>> FetchSinglePostProfilepicAsync(Guid postId);
    }
}
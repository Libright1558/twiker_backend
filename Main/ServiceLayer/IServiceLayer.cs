using twiker_backend.Db.Models;
using twiker_backend.Redis.Models;

namespace twiker_backend.ServiceLayer
{
    public interface IAccessTokenService
    {
        Task<TokenRefreshResult> RefreshTokenAsync(string token);
    }

    public interface IAccountService
    {
        Task<RegisterResult> RegisterAccountAsync(RegisterModel model);

        Task<LoginResult> LoginAsync(LoginModel model);

        Task<DeleteAccountResult> DeleteAccountAsync(string userIdentifier);
    }

    public interface IPostService
    {
        Task<PostFetch?[]> GetPost (Guid userId, string username);

        Task<PostFetch> WritePost (Guid userId, string postBy, string content);

        Task UpdateLike (Guid postId, Guid userId, string username);

        Task UpdateRetweet (Guid postId, Guid userId, string username);

        Task DeletePost (Guid userId, Guid postId);
    }

    public interface IUserService
    {
        Task<RedisUserData?> GetThePersonalData (Guid userId);
    }
}

using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace twiker_backend.ServiceLayer
{
    public class AccessTokenService(ILogger<AccessTokenService> logger) : IAccessTokenService
    {
        private readonly ILogger<AccessTokenService> _logger = logger;

        public async Task<TokenRefreshResult> RefreshTokenAsync(string token, Guid userId, string username)
        {
            try
            {
                DotNetEnv.Env.TraversePath().Load();
                var privateKeyPath = DotNetEnv.Env.GetString("private_key");
                var privateKey = await File.ReadAllTextAsync(privateKeyPath!);

                var tokenHandler = new JsonWebTokenHandler();

                RSA _rsa = RSA.Create();
                _rsa.ImportFromPem(privateKey.ToCharArray());
                var key = new RsaSecurityKey(_rsa);
                
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(
                    [
                        new Claim("userId", userId.ToString()),
                        new Claim("username", username),
                        new Claim("Access_token", "true")
                    ]),
                    Expires = DateTime.UtcNow.AddMinutes(60),
                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256Signature)
                };

                var accessToken = tokenHandler.CreateToken(tokenDescriptor);
                return new TokenRefreshResult { Success = true, AccessToken = accessToken };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshTokenAsync Error");
                return new TokenRefreshResult { Success = false, ErrorMessage = $"An error occurred: {ex.Message}" };
            }
        }
    }

    public class TokenRefreshResult
    {
        public bool Success { get; set; }
        public string? AccessToken { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
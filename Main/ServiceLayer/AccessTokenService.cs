using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
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

        public async Task<TokenRefreshResult> RefreshTokenAsync(string token)
        {
            try
            {
                DotNetEnv.Env.TraversePath().Load();
                var publicKeyPath = DotNetEnv.Env.GetString("public_key");
                var privateKeyPath = DotNetEnv.Env.GetString("private_key");

                var publicKey = await File.ReadAllTextAsync(publicKeyPath!);
                var privateKey = await File.ReadAllTextAsync(privateKeyPath!);

                var tokenHandler = new JwtSecurityTokenHandler();
                var _pubRsa = RSA.Create();
                _pubRsa.ImportFromPem(publicKey.ToCharArray());
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(_pubRsa),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                ClaimsPrincipal principal;
                try
                {
                    principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                }
                catch
                {
                    return new TokenRefreshResult { Success = false, ErrorMessage = "Invalid token" };
                }

                var userId = principal.FindFirst("userId")?.Value;
                var username = principal.FindFirst("username")?.Value;
                var authentication_token = principal.FindFirst("Authentication_token")?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(authentication_token))
                {
                    return new TokenRefreshResult { Success = false, ErrorMessage = "Invalid token claims" };
                }

                RSA _rsa = RSA.Create();
                _rsa.ImportFromPem(privateKey.ToCharArray());
                var key = new RsaSecurityKey(_rsa);
                
                var newToken = new JwtSecurityToken(
                    claims:
                    [
                        new Claim("userId", userId),
                        new Claim("username", username),
                        new Claim("Access_token", "true")
                    ],
                    expires: DateTime.UtcNow.AddMinutes(10),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
                );

                var accessToken = tokenHandler.WriteToken(newToken);

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
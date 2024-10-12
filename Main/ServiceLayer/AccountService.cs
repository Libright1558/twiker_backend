using System;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Claims;
using BC = BCrypt.Net.BCrypt;
using twiker_backend.Db.Repository;
using twiker_backend.Db.Models;
using Microsoft.Extensions.Logging;

namespace twiker_backend.ServiceLayer
{
    public class AccountService : IAccountService
    {
        private readonly ILogger<AccountService> _logger;

        private readonly IDbUserInfo _dbUserInfo;

        public AccountService(IDbUserInfo dbUserInfo, ILogger<AccountService> logger)
        {
            _dbUserInfo = dbUserInfo;
            _logger = logger;
            Environment.SetEnvironmentVariable("ProfilePath", "/images/UserProfiles/ProfilePic.jpeg");
        }

        private static async Task<string> GenerateJwtToken(UserDbData user)
        {
            DotNetEnv.Env.TraversePath().Load();
            var privateKeyPath = DotNetEnv.Env.GetString("private_key");
            var privateKeyText = await File.ReadAllTextAsync(privateKeyPath!);

            RSA _rsa = RSA.Create();
            _rsa.ImportFromPem(privateKeyText.ToCharArray());
            var RSAkey = new RsaSecurityKey(_rsa);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim("userId", user.UserId.ToString()),
                    new Claim("username", user.Username!),
                    new Claim("Authentication_token", "true")
                ]),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(RSAkey, SecurityAlgorithms.RsaSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<RegisterResult> RegisterAccountAsync(RegisterModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.LastName) ||
                string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Password))
                {
                    return RegisterResult.InvalidInput;
                }

                UserDbData? FoundUser = await _dbUserInfo.FindOneUser(model.Username);

                if (FoundUser == null)
                {
                    string passwordHash =  BC.HashPassword(model.Password, 12);
                    string defaultProfile = Environment.GetEnvironmentVariable("ProfilePath")!;

                    UserTable newUser = new()
                    {
                        Firstname = model.FirstName.Trim(),
                        Lastname = model.LastName.Trim(),
                        Username = model.Username.Trim(),
                        Email = model.Email.Trim(),
                        Password = passwordHash,
                        Profilepic = defaultProfile
                    };

                    await _dbUserInfo.WriteUserData(newUser);
                    return RegisterResult.Success;
                }

                return RegisterResult.DuplicateUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RegisterAccountAsync Error");
                Console.WriteLine(ex.ToString());
                return RegisterResult.Error;
            }
            
        }

        public async Task<LoginResult> LoginAsync(LoginModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
                {
                    return new LoginResult { Success = false, ErrorMessage = "Invalid input" };
                }

                UserDbData? FoundUser = await _dbUserInfo.FindOneUser(model.Username);

                if (FoundUser != null)
                {
                    bool isPasswordValid = BC.Verify(model.Password, FoundUser.Password);

                    if (isPasswordValid)
                    {
                        var token = await GenerateJwtToken(FoundUser);
                        return new LoginResult { Success = true, Token = token };
                    }
                }

                return new LoginResult { Success = false, ErrorMessage = "Invalid username or password" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginAsync");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }

    public enum RegisterResult
    {
        Success,
        InvalidInput,
        DuplicateUser,
        Error
    }

    public class LoginResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
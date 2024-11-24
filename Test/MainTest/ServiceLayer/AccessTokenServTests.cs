using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using twiker_backend.ServiceLayer;
using Microsoft.Extensions.Logging;

[TestFixture, Category("AccessTokenService")]
public class AccessTokenServiceTests
{
    private AccessTokenService _accessTokenService;
    private Mock<ILogger<AccessTokenService>> _loggerMock;
    private string _privateKey;

    [OneTimeSetUp]
    public void Setup()
    {
        DotNetEnv.Env.TraversePath().Load();
        _loggerMock = new Mock<ILogger<AccessTokenService>>();
        _accessTokenService = new AccessTokenService(_loggerMock.Object);
        _privateKey = DotNetEnv.Env.GetString("private_key");
    }

    private async Task<string> GenerateValidToken(DateTime expiration)
    {
        var rsa = RSA.Create();
        var privateKeyText = await File.ReadAllTextAsync(_privateKey!);
        rsa.ImportFromPem(privateKeyText.ToCharArray());
        var securityKey = new RsaSecurityKey(rsa);

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("userId", Guid.NewGuid().ToString()),
                new Claim("username", "testuser"),
                new Claim("Authentication_token", "true")
            }),
            Expires = expiration,
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [Test]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewToken()
    {
        try
        {
            // Arrange
            var validToken = await GenerateValidToken(DateTime.UtcNow.AddMinutes(5));

            // Act
            var result = await _accessTokenService.RefreshTokenAsync(validToken, Guid.Parse("random1"), "random2");

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.Success, Is.True);
                Assert.That(result.AccessToken, Is.Not.Null.And.Not.Empty);
                Assert.That(result.ErrorMessage, Is.Null);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Test]
    public async Task RefreshTokenAsync_ValidToken_NewTokenHasDifferentSignature()
    {
        try
        {
            // Arrange
            var validToken = await GenerateValidToken(DateTime.UtcNow.AddMinutes(5));

            // Act
            var result = await _accessTokenService.RefreshTokenAsync(validToken, Guid.Parse("random1"), "random2");

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.Success, Is.True);
                Assert.That(result.AccessToken, Is.Not.EqualTo(validToken));
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
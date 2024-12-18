using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using twiker_backend.ServiceLayer;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using twiker_backend.CustomAttributes.Authentication;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]"), JwtAuthorize]
public class TokenController : ControllerBase
{
    private readonly ILogger<TokenController> _logger;

    private readonly IAccessTokenService _tokenService;

    public TokenController(IAccessTokenService tokenService, ILogger<TokenController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var authorizationHeader = Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return Unauthorized();
            }

            var token = authorizationHeader.Substring("Bearer ".Length);

            var result = await _tokenService.RefreshTokenAsync(token);

            if (result.Success)
            {
                IHeaderDictionary Headers = Response.Headers;
                Headers.Append("Authorization", $"Bearer {result.AccessToken}");
                return Ok();
            }

            return Unauthorized(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GainAccessTokenController RefreshToken Error");
            Console.WriteLine(ex.ToString());
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
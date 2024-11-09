using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using twiker_backend.Db.Models;
using twiker_backend.ServiceLayer;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using twiker_backend.CustomAttributes.Authentication;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;

    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService, ILogger<AccountController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        try
        {
            var result = await _accountService.RegisterAccountAsync(model);

            return result switch
            {
                RegisterResult.Success => Ok(),
                RegisterResult.InvalidInput => BadRequest(),
                RegisterResult.DuplicateUser => Conflict(),
                RegisterResult.Error => StatusCode(500),
                _ => StatusCode(500)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AccountController Register Error");
            Console.WriteLine(ex.ToString());
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        try
        {
            var result = await _accountService.LoginAsync(model);

            if (result.Success)
            {
                IHeaderDictionary Headers = Response.Headers;
                Headers.Append("Authorization", $"Bearer {result.Token}");
                return Ok();
            }

            return Unauthorized(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AccountController Login Error");
            Console.WriteLine(ex.ToString());
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("deleteAccount"), AccessAuthorize]
    public async Task<IActionResult> DeleteAccount([FromBody] string userIdentifier)
    {
        try
        {
            var result = await _accountService.DeleteAccountAsync(userIdentifier);
            return result switch
            {
                DeleteAccountResult.Success => Ok(),
                DeleteAccountResult.InvalidInput => BadRequest(),
                DeleteAccountResult.UserNotExist => NotFound(),
                DeleteAccountResult.Error => StatusCode(500),
                _ => StatusCode(500)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AccountController DeleteAccount Error");
            Console.WriteLine(ex.ToString());
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
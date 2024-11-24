using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using twiker_backend.ServiceLayer;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]"), Authorize(AuthenticationSchemes = "AccessScheme")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;

    private readonly IUserService _userService;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("getUser")]
    public async Task<IActionResult> GetPersonalData()
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst("userId")?.Value!);

            var userData = await _userService.GetThePersonalData(userId);
            if (userData == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }
            return Ok(userData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UserController GetPersonalData Error");
            Console.WriteLine(ex.ToString());
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
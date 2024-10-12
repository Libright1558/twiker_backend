using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using twiker_backend.ServiceLayer;
using Microsoft.Extensions.Logging;
using twiker_backend.CustomAttributes.Authentication;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]"), AccessAuthorize]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;

    private readonly IUserService _userService;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetPersonalData(Guid userId)
    {
        try
        {
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
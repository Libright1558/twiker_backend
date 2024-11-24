using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using twiker_backend.ServiceLayer;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration.UserSecrets;

[ApiController]
[Route("api/[controller]"), Authorize(AuthenticationSchemes = "AccessScheme")]
public class PostController : ControllerBase
{
    private readonly ILogger<PostController> _logger;

    private readonly IPostService _postService;

    public PostController(IPostService postService, ILogger<PostController> logger)
    {
        _postService = postService;
        _logger = logger;
    }

    [HttpGet("getPost")]
    public async Task<IActionResult> GetPosts()
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst("userId")?.Value!);
            string username = User.FindFirst("username")?.Value!;

            var posts = await _postService.GetPost(userId, username);
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostController GetPosts Error");
            Console.WriteLine(ex.ToString());
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("createPost")]
    public async Task<IActionResult> CreatePost ([FromBody] string content)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst("userId")?.Value!);
            string username = User.FindFirst("username")?.Value!;

            if (string.IsNullOrEmpty(content))
            {
                return StatusCode(400, "content missing");
            }

            var post = await _postService.WritePost(userId, username, content);
            return CreatedAtAction(nameof(GetPosts), new { userId }, post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostController CreatePost Error");
            Console.WriteLine(ex.ToString());
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("like")]
    public async Task<IActionResult> UpdateLike ([FromBody] string postId)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst("userId")?.Value!);
            string username = User.FindFirst("username")?.Value!;

            if (string.IsNullOrEmpty(postId))
            {
                return StatusCode(400, "postId missing");
            }

            await _postService.UpdateLike(Guid.Parse(postId), userId, username);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostController UpdateLike Error");
            Console.WriteLine(ex.ToString());
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("retweet")]
    public async Task<IActionResult> UpdateRetweet ([FromBody] string postId)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst("userId")?.Value!);
            string username = User.FindFirst("username")?.Value!;

            if (string.IsNullOrEmpty(postId))
            {
                return StatusCode(400, "postId missing");
            }

            await _postService.UpdateRetweet(Guid.Parse(postId), userId, username);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostController UpdateRetweet Error");
            Console.WriteLine(ex.ToString());
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("deletePost")]
    public async Task<IActionResult> DeletePost([FromBody] string postId)
    {
        try
        {
            Guid userId = Guid.Parse(User.FindFirst("userId")?.Value!);

            if (string.IsNullOrEmpty(postId))
            {
                return StatusCode(400, "postId missing");
            }

            await _postService.DeletePost(userId, Guid.Parse(postId));
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostController DeletePost Error");
            Console.WriteLine(ex.ToString());
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
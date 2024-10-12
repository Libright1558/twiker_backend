using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using twiker_backend.ServiceLayer;
using Microsoft.Extensions.Logging;
using twiker_backend.CustomAttributes.Authentication;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]"), AccessAuthorize]
public class PostController : ControllerBase
{
    private readonly ILogger<PostController> _logger;

    private readonly IPostService _postService;

    public PostController(IPostService postService, ILogger<PostController> logger)
    {
        _postService = postService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetPosts([FromQuery] Guid userId, [FromQuery] string username)
    {
        try
        {
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

    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        try
        {
            var post = await _postService.WritePost(request.UserId, request.PostBy, request.Content);
            return CreatedAtAction(nameof(GetPosts), new { userId = request.UserId }, post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostController CreatePost Error");
            Console.WriteLine(ex.ToString());
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("like")]
    public async Task<IActionResult> UpdateLike([FromBody] UpdateInteractionRequest request)
    {
        try
        {
            await _postService.UpdateLike(request.PostId, request.UserId, request.Username);
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
    public async Task<IActionResult> UpdateRetweet([FromBody] UpdateInteractionRequest request)
    {
        try
        {
            await _postService.UpdateRetweet(request.PostId, request.UserId, request.Username);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostController UpdateRetweet Error");
            Console.WriteLine(ex.ToString());
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{postId}")]
    public async Task<IActionResult> DeletePost(Guid postId, [FromQuery] Guid userId)
    {
        try
        {
            await _postService.DeletePost(userId, postId);
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

public class CreatePostRequest
{
    public Guid UserId { get; set; }
    public string PostBy { get; set; } = null!;
    public string Content { get; set; } = null!;
}

public class UpdateInteractionRequest
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
}
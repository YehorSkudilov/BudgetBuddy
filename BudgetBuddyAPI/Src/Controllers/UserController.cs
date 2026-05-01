using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetBuddyAPI;

[ApiController]
[Route("api/user")]
public class UserController : BaseController
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public UserController(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    // =========================
    // REGISTER
    // =========================
    [HttpPost("[action]")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var exists = await _db.Users.AnyAsync(u => u.email == req.email);
        if (exists)
            return Conflict(new { message = "A user with that email already exists." });

        var user = new User
        {
            id = Guid.NewGuid(),
            email = req.email,
            username = req.username,
            password = BCrypt.Net.BCrypt.HashPassword(req.password),
            created_at = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _tokenService.Generate(user);

        return Ok(new
        {
            token,
            user = new
            {
                user.id,
                user.email,
                user.username,
                user.created_at
            }
        });
    }

    // =========================
    // LOGIN
    // =========================
    [HttpPost("[action]")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.email == req.email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(req.password, user.password))
            return Unauthorized(new { message = "Invalid email or password." });

        var token = _tokenService.Generate(user);

        return Ok(new
        {
            token,
            user = new
            {
                user.id,
                user.email,
                user.username,
                user.created_at
            }
        });
    }

    // =========================
    // GET CURRENT USER
    // =========================
    [Authorize]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetUser()
    {
        var userId = UserId;

        var user = await _db.Users
            .Where(u => u.id == userId)
            .Select(u => new
            {
                u.id,
                u.email,
                u.username,
                u.created_at
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new { message = "User not found." });

        return Ok(user);
    }

    // =========================
    // DELETE USER (SECURED)
    // =========================
    [Authorize]
    [HttpDelete("[action]")]
    public async Task<IActionResult> DeleteUser()
    {
        var userId = UserId;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.id == userId);
        if (user == null)
            return NotFound(new { message = "User not found." });

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return Ok(new { message = "User deleted." });
    }
}

public record RegisterRequest(string email, string username, string password);
public record LoginRequest(string email, string password);
public record UpdateUserRequest(string? email, string? username, string? password);
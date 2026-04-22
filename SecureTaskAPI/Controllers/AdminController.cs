using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("admin")]
[Authorize(Roles = "admin")] // Kun admin må tilgå disse endpoints!
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    // GET /admin/users — Alle brugere med statistik
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _db.Users
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Role,
                TotalTasks = u.Tasks.Count,
                CompletedTasks = u.Tasks.Count(t => t.IsCompleted),
                PendingTasks = u.Tasks.Count(t => !t.IsCompleted)
            })
            .ToListAsync();

        return Ok(users);
    }

    // GET /admin/users/{id} — Én bruger med alle opgaver
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _db.Users
            .Include(u => u.Tasks)
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Role,
                Tasks = u.Tasks.Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.IsCompleted
                })
            })
            .FirstOrDefaultAsync();

        if (user is null) return NotFound();
        return Ok(user);
    }

    // DELETE /admin/users/{id} — Slet en bruger
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        // Beskyt admin kontoen mod at blive slettet!
        if (user.Role == "admin")
            return BadRequest("Admin kontoen kan ikke slettes!");

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
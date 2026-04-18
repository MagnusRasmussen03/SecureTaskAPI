using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

// [ApiController] fortæller .NET at dette er en API controller
// [Route("tasks")] betyder alle endpoints starter med /tasks
// [Authorize] betyder alle endpoints kræver JWT token
[ApiController]
[Route("tasks")]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;

    // Constructor injection - ITaskService gives til os automatisk
    public TaskController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    // Hjælpemetode der henter brugerens ID fra JWT tokenet
    // Vi bruger den i alle endpoints
    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    // GET /tasks
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tasks = await _taskService.GetUserTasksAsync(GetUserId());
        return Ok(tasks);
    }

    // GET /tasks/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _taskService.GetTaskAsync(id);
        return task is null ? NotFound() : Ok(task);
    }

    // POST /tasks
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        var task = await _taskService.CreateTaskAsync(request.Title, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    // PUT /tasks/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TaskItem updatedTask)
    {
        var task = await _taskService.UpdateTaskAsync(id, updatedTask, GetUserId());
        return task is null ? NotFound() : Ok(task);
    }

    // DELETE /tasks/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _taskService.DeleteTaskAsync(id, GetUserId());
        return deleted ? NoContent() : NotFound();
    }

    // GET /tasks/statistics
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var stats = await _taskService.GetStatisticsAsync(GetUserId());
        return Ok(stats);
    }

    // GET /tasks/pending
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var tasks = await _taskService.GetPendingTasksAsync(GetUserId());
        return Ok(tasks);
    }
}

// Request model til oprettelse af opgave
public record CreateTaskRequest(string Title);
using Microsoft.EntityFrameworkCore;

// TaskRepository implementerer ITaskRepository interfacet
// Det er HER den faktiske databaselogik lever
// Controllers og Services ved ikke hvordan data hentes
// de kalder bare metoderne på interfacet!
public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _db;

    // Constructor injection - AppDbContext gives til os automatisk
    public TaskRepository(AppDbContext db)
    {
        _db = db;
    }

    // Hent alle opgaver tilhørende en specifik bruger
    public async Task<List<TaskItem>> GetAllAsync(int userId)
    {
        return await _db.Tasks
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    // Hent én opgave via id
    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _db.Tasks.FindAsync(id);
    }

    // Opret en ny opgave
    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return task;
    }

    // Opdater en opgave - med IDOR beskyttelse!
    public async Task<TaskItem?> UpdateAsync(int id, TaskItem updatedTask, int userId)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return null;
        if (task.UserId != userId) return null;

        task.Title = updatedTask.Title;
        task.IsCompleted = updatedTask.IsCompleted;
        await _db.SaveChangesAsync();
        return task;
    }

    // Slet en opgave - med IDOR beskyttelse!
    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return false;
        if (task.UserId != userId) return false;

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return true;
    }

    // Hent statistik for en brugers opgaver (aggregering)
    public async Task<TaskStatistics> GetStatisticsAsync(int userId)
    {
        var tasks = await _db.Tasks
            .Where(t => t.UserId == userId)
            .ToListAsync();

        return new TaskStatistics
        {
            TotalTasks = tasks.Count,
            CompletedTasks = tasks.Count(t => t.IsCompleted),
            PendingTasks = tasks.Count(t => !t.IsCompleted)
        };
    }

    // Hent ufærdige opgaver sorteret efter titel (filtering + sorting)
    public async Task<List<TaskItem>> GetPendingTasksAsync(int userId)
    {
        return await _db.Tasks
            .Where(t => t.UserId == userId && !t.IsCompleted)
            .OrderBy(t => t.Title)
            .ToListAsync();
    }

    // Hent alle brugere med antal opgaver (JOIN + COUNT)
    public async Task<List<UserTaskCount>> GetUserTaskCountsAsync()
    {
        return await _db.Users
            .Select(u => new UserTaskCount
            {
                Username = u.Username,
                TaskCount = u.Tasks.Count,
                CompletedCount = u.Tasks.Count(t => t.IsCompleted)
            })
            .ToListAsync();
    }
}
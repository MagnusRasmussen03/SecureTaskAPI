// TaskService implementerer ITaskService
// Den bruger ITaskRepository til at hente data
// Service laget ved IKKE hvordan data gemmes
// det er repository'ets ansvar!
public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    // Constructor injection - ITaskRepository gives til os automatisk
    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<List<TaskItem>> GetUserTasksAsync(int userId)
    {
        return await _taskRepository.GetAllAsync(userId);
    }

    public async Task<TaskItem?> GetTaskAsync(int id)
    {
        return await _taskRepository.GetByIdAsync(id);
    }

    // Bemærk: Service laget validerer input!
    // Repository laget ville bare gemme uden at tjekke
    public async Task<TaskItem> CreateTaskAsync(string title, int userId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Titel må ikke være tom!");

        var task = new TaskItem
        {
            Title = title,
            IsCompleted = false,
            UserId = userId
        };

        return await _taskRepository.CreateAsync(task);
    }

    public async Task<TaskItem?> UpdateTaskAsync(int id, TaskItem updatedTask, int userId)
    {
        return await _taskRepository.UpdateAsync(id, updatedTask, userId);
    }

    public async Task<bool> DeleteTaskAsync(int id, int userId)
    {
        return await _taskRepository.DeleteAsync(id, userId);
    }
}
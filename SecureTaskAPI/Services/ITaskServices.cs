// ITaskService er business-logik laget
// Det er her regler og validering lever
// Repository ved kun om database
// Service ved om forretningsregler
public interface ITaskService
{
    Task<List<TaskItem>> GetUserTasksAsync(int userId);
    Task<TaskItem?> GetTaskAsync(int id);
    Task<TaskItem> CreateTaskAsync(string title, int userId);
    Task<TaskItem?> UpdateTaskAsync(int id, TaskItem updatedTask, int userId);
    Task<bool> DeleteTaskAsync(int id, int userId);
}
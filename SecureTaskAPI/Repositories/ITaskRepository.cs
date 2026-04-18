// ITaskRepository er et interface - en kontrakt der definerer
// HVAD repository'et kan, uden at sige HVORDAN det gør det.
// Det er Repository Pattern i praksis!
public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync(int userId);
    Task<TaskItem?> GetByIdAsync(int id);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task<TaskItem?> UpdateAsync(int id, TaskItem updatedTask, int userId);
    Task<bool> DeleteAsync(int id, int userId);
    // Hent statistik for en brugers opgaver
    Task<TaskStatistics> GetStatisticsAsync(int userId);
    
    // Hent ufærdige opgaver sorteret efter titel
    Task<List<TaskItem>> GetPendingTasksAsync(int userId);
    
    // Hent alle brugere med antal opgaver (JOIN + COUNT)
    Task<List<UserTaskCount>> GetUserTaskCountsAsync();
}

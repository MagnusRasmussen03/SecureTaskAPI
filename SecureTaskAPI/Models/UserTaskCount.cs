// UserTaskCount model - returneres af GetUserTaskCountsAsync
public class UserTaskCount
{
    public string Username { get; set; } = string.Empty;
    public int TaskCount { get; set; }
    public int CompletedCount { get; set; }
}
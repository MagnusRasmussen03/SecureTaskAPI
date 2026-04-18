// Statistik model - returneres af GetStatisticsAsync
public class TaskStatistics
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }

    // Beregnet felt - ingen setter!
    // Udregnes automatisk fra de andre felter
    public double CompletionPercentage => 
        TotalTasks == 0 ? 0 : (double)CompletedTasks / TotalTasks * 100;
}
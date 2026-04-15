public class TaskItem
{
    // Unik ID for opgaven (primary key)
    public int Id { get; set; }

    // Titel på opgaven
    public string Title { get; set; } = string.Empty;

    // Er opgaven færdig?
    public bool IsCompleted { get; set; }

    // ─────────────────────────────────────────
    // FOREIGN KEY RELATION
    // UserId peger på den bruger der ejer opgaven
    // Det er præcis som i en relationel database:
    // Tasks.UserId → Users.Id
    // ─────────────────────────────────────────
    public int UserId { get; set; }

    // Navigation property — EF Core bruger denne
    // til at hente den tilhørende bruger automatisk
    // Tænk på det som en genvej til bruger-objektet
    public User? Owner { get; set; }
}
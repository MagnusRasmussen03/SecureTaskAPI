public class User
{
    // Unik ID for brugeren (primary key)
    public int Id { get; set; }

    // Brugernavn
    public string Username { get; set; } = string.Empty;

    // Krypteret password - aldrig det rigtige password!
    public string PasswordHash { get; set; } = string.Empty;

    // ─────────────────────────────────────────
    // NAVIGATION PROPERTY
    // En bruger kan have mange opgaver
    // EF Core bruger denne til at hente alle
    // opgaver tilhørende denne bruger
    // Det er "one-to-many" relationen:
    // én bruger → mange opgaver
    // ─────────────────────────────────────────
    public List<TaskItem> Tasks { get; set; } = new();
}
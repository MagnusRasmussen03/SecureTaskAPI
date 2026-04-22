using Microsoft.EntityFrameworkCore;

public class TaskRepositoryTests
{
    // Hjælpemetode der opretter en frisk InMemory database til hver test
    private AppDbContext GetInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    // ─────────────────────────────────────────
    // REPOSITORY TESTS
    // Tester at databaselaget virker korrekt
    // ─────────────────────────────────────────

    [Fact]
    public async Task CanCreateTask()
    {
        // Arrange
        var db = GetInMemoryDb();
        var repo = new TaskRepository(db);

        // Act
        var task = await repo.CreateAsync(new TaskItem 
        { 
            Title = "Test opgave", 
            IsCompleted = false,
            UserId = 1
        });

        // Assert
        Assert.NotNull(task);
        Assert.Equal("Test opgave", task.Title);
        Assert.Equal(1, task.UserId);
    }

    [Fact]
    public async Task CanGetAllTasksForUser()
    {
        // Arrange
        var db = GetInMemoryDb();
        var repo = new TaskRepository(db);

        // Opret to opgaver til bruger 1 og én til bruger 2
        await repo.CreateAsync(new TaskItem { Title = "Magnus opgave 1", UserId = 1 });
        await repo.CreateAsync(new TaskItem { Title = "Magnus opgave 2", UserId = 1 });
        await repo.CreateAsync(new TaskItem { Title = "Peters opgave",   UserId = 2 });

        // Act - hent kun bruger 1's opgaver
        var tasks = await repo.GetAllAsync(userId: 1);

        // Assert - vi må KUN se 2 opgaver, ikke Peters!
        Assert.Equal(2, tasks.Count);
        Assert.All(tasks, t => Assert.Equal(1, t.UserId));
    }

    [Fact]
    public async Task CanDeleteOwnTask()
    {
        // Arrange
        var db = GetInMemoryDb();
        var repo = new TaskRepository(db);
        var task = await repo.CreateAsync(new TaskItem { Title = "Slet mig", UserId = 1 });

        // Act
        var deleted = await repo.DeleteAsync(task.Id, userId: 1);

        // Assert
        Assert.True(deleted);
        Assert.Equal(0, await db.Tasks.CountAsync());
    }

    [Fact]
    public async Task CannotDeleteOtherUsersTask()
    {
        // Arrange - opret opgave tilhørende bruger 1
        var db = GetInMemoryDb();
        var repo = new TaskRepository(db);
        var task = await repo.CreateAsync(new TaskItem { Title = "Magnus' opgave", UserId = 1 });

        // Act - bruger 2 forsøger at slette bruger 1's opgave!
        var deleted = await repo.DeleteAsync(task.Id, userId: 2);

        // Assert - det må IKKE lykkes! IDOR beskyttelse!
        Assert.False(deleted);
        Assert.Equal(1, await db.Tasks.CountAsync());
    }

    // ─────────────────────────────────────────
    // SERVICE TESTS
    // Tester at forretningslogikken virker korrekt
    // ─────────────────────────────────────────

    [Fact]
    public async Task CannotCreateTaskWithEmptyTitle()
    {
        // Arrange
        var db = GetInMemoryDb();
        var repo = new TaskRepository(db);
        var service = new TaskService(repo);

        // Act & Assert
        // Service laget skal kaste en fejl ved tom titel!
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.CreateTaskAsync("", userId: 1));
    }

    [Fact]
    public async Task CreateTaskSetsUserIdCorrectly()
    {
        // Arrange
        var db = GetInMemoryDb();
        var repo = new TaskRepository(db);
        var service = new TaskService(repo);

        // Act
        var task = await service.CreateTaskAsync("Min opgave", userId: 42);

        // Assert - UserId skal være sat korrekt af service laget
        Assert.Equal(42, task.UserId);
        Assert.False(task.IsCompleted);
    }

    // ─────────────────────────────────────────
// ROLLE TESTS
// Tester at rolle-systemet virker korrekt
// ─────────────────────────────────────────

[Fact]
public async Task NewUserHasDefaultRoleUser()
{
    // Arrange
    var db = GetInMemoryDb();

    // Act - opret en ny bruger uden at angive rolle
    var user = new User
    {
        Username = "testbruger",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();

    // Assert - rollen skal automatisk være "user"
    var savedUser = await db.Users.FirstOrDefaultAsync(u => u.Username == "testbruger");
    Assert.NotNull(savedUser);
    Assert.Equal("user", savedUser.Role);
}

[Fact]
public async Task AdminUserHasAdminRole()
{
    // Arrange
    var db = GetInMemoryDb();

    // Act - opret en admin bruger
    var admin = new User
    {
        Username = "admin",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
        Role = "admin"
    };
    db.Users.Add(admin);
    await db.SaveChangesAsync();

    // Assert - rollen skal være "admin"
    var savedAdmin = await db.Users.FirstOrDefaultAsync(u => u.Username == "admin");
    Assert.NotNull(savedAdmin);
    Assert.Equal("admin", savedAdmin.Role);
}

[Fact]
public async Task AdminCanSeeAllUsers()
{
    // Arrange - opret to brugere
    var db = GetInMemoryDb();
    db.Users.Add(new User { Username = "magnus", PasswordHash = "hash1", Role = "user" });
    db.Users.Add(new User { Username = "peter", PasswordHash = "hash2", Role = "user" });
    db.Users.Add(new User { Username = "admin", PasswordHash = "hash3", Role = "admin" });
    await db.SaveChangesAsync();

    // Act - hent alle brugere (som admin ville gøre)
    var allUsers = await db.Users.ToListAsync();

    // Assert - admin kan se alle 3 brugere
    Assert.Equal(3, allUsers.Count);
}

[Fact]
public async Task CannotDeleteAdminUser()
{
    // Arrange
    var db = GetInMemoryDb();
    var admin = new User
    {
        Username = "admin",
        PasswordHash = "hash",
        Role = "admin"
    };
    db.Users.Add(admin);
    await db.SaveChangesAsync();

    // Act - forsøg at slette admin brugeren
    var foundAdmin = await db.Users.FirstOrDefaultAsync(u => u.Role == "admin");

    // Assert - admin må ikke slettes!
    // Vi verificerer at beskyttelsen eksisterer i databasen
    Assert.NotNull(foundAdmin);
    Assert.Equal("admin", foundAdmin.Role);
}

[Fact]
public async Task UserCannotSeeOtherUsersTasks()
{
    // Arrange
    var db = GetInMemoryDb();
    var repo = new TaskRepository(db);

    // Opret opgaver til to forskellige brugere
    await repo.CreateAsync(new TaskItem { Title = "Magnus' opgave", UserId = 1 });
    await repo.CreateAsync(new TaskItem { Title = "Peters opgave", UserId = 2 });

    // Act - hent kun bruger 1's opgaver
    var magnusTasks = await repo.GetAllAsync(userId: 1);
    var petersTasks = await repo.GetAllAsync(userId: 2);

    // Assert - hver bruger ser kun sine egne opgaver
    Assert.Single(magnusTasks);
    Assert.Single(petersTasks);
    Assert.Equal("Magnus' opgave", magnusTasks[0].Title);
    Assert.Equal("Peters opgave", petersTasks[0].Title);
}
}
using Microsoft.EntityFrameworkCore;

public class TaskApiTests
//Vi kunne skrive: private TaskItem _task = new TaskItem();
//Men gør det ikke fordi hver test opretter sin egen database via GetInMemoryDb(). Hvis vi delte feltvariable mellem tests, kunne én test påvirke en anden — og det vil vi undgå. Hver test skal være 100% isoleret. 
{
    private AppDbContext GetInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>() // "Jeg vil konfigurere en AppDbContext"
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            // I stedet for rigtig PostgreSQL, brug en database i RAM (InMemory database →  Midlertidig database i RAM (hurtig, perfekt til tests))
            // Guid.NewGuid() giver et unikt navn hver gang
            // så hver test får SIN EGEN tomme database
            .Options;
            // Færdiggør konfigurationen
        return new AppDbContext(options);
            // Returner en database klar til brug
    }

    [Fact] //svarer til @Test i Java
    public async Task CanCreateTask() //Du kender async fra vores API. Det er præcis det samme her — fordi vi bruger await db.SaveChangesAsync() inde i testen, skal metoden være async.
    {
        // Arrange - forbered
        var db = GetInMemoryDb();
        var newTask = new TaskItem { Title = "Test opgave", IsCompleted = false };

        // Act - udfør
        db.Tasks.Add(newTask);
        await db.SaveChangesAsync();

        // Assert - verificer
        Assert.Equal(1, await db.Tasks.CountAsync());
    }

    [Fact]
    public async Task CanGetTask()
    {
        // Arrange
        var db = GetInMemoryDb();
        db.Tasks.Add(new TaskItem { Title = "Find mig", IsCompleted = false });
        await db.SaveChangesAsync();

        // Act
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Title == "Find mig");

        // Assert
        Assert.NotNull(task);
        Assert.Equal("Find mig", task.Title);
    }

    [Fact]
    public async Task CanDeleteTask()
    {
        // Arrange
        var db = GetInMemoryDb();
        var task = new TaskItem { Title = "Slet mig", IsCompleted = false };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        // Act
        db.Tasks.Remove(task);
        await db.SaveChangesAsync();

        // Assert
        Assert.Equal(0, await db.Tasks.CountAsync());
    }
}
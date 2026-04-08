using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args); // ← Forbereder alt

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql("Host=localhost;Port=5433;Database=securetaskdb;Username=admin;Password=password123"));

var app = builder.Build();  // ← Bygger selve API'en

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Hent alle opgaver fra databasen
app.MapGet("/tasks", async (AppDbContext db) =>
    await db.Tasks.ToListAsync());

// Hent én opgave
app.MapGet("/tasks/{id}", async (int id, AppDbContext db) =>
{
    var task = await db.Tasks.FindAsync(id);
    return task is null ? Results.NotFound() : Results.Ok(task);
});

// Opret en ny opgave
app.MapPost("/tasks", async (TaskItem newTask, AppDbContext db) =>
{
    //SaveChangesAsync()  →  "Jeg KAN køre i baggrunden"
    //await               →  "Kør i baggrunden og vent på svaret"


    //db.SaveChanges()        // Blokerer - den gamle måde
    //db.SaveChangesAsync()   // Blokerer ikke - den moderne måde
    //Dette er to forskellige metoder.
    db.Tasks.Add(newTask);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{newTask.Id}", newTask);
});

// Opdater en opgave
app.MapPut("/tasks/{id}", async (int id, TaskItem updatedTask, AppDbContext db) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();

    task.Title = updatedTask.Title;
    task.IsCompleted = updatedTask.IsCompleted;
    await db.SaveChangesAsync();
    return Results.Ok(task);
});

// Slet en opgave
app.MapDelete("/tasks/{id}", async (int id, AppDbContext db) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
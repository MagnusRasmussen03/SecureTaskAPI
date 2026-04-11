using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args); // ← Forbereder alt

var host = builder.Configuration["POSTGRES_HOST"] 
    ?? throw new InvalidOperationException("POSTGRES_HOST er ikke sat!");
var port = builder.Configuration["POSTGRES_PORT"] 
    ?? throw new InvalidOperationException("POSTGRES_PORT er ikke sat!");
var db = builder.Configuration["POSTGRES_DB"] 
    ?? throw new InvalidOperationException("POSTGRES_DB er ikke sat!");
var user = builder.Configuration["POSTGRES_USER"] 
    ?? throw new InvalidOperationException("POSTGRES_USER er ikke sat!");
var password = builder.Configuration["POSTGRES_PASSWORD"] 
    ?? throw new InvalidOperationException("POSTGRES_PASSWORD er ikke sat!");


var connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={password}";

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();  // ← Bygger selve API'en

// Kør migrations automatisk ved opstart
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

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
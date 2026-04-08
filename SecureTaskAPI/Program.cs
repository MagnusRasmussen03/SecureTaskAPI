var builder = WebApplication.CreateBuilder(args); // ← Forbereder alt

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();  // ← Bygger selve API'en

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var tasks = new List <TaskItem>
{
    new TaskItem { Id =1, Title = "Lær C#", IsCompleted = false },
    new TaskItem { Id =2, Title = "Byg en REST API", IsCompleted = false },
    new TaskItem { Id =3, Title = "Lær Docker", IsCompleted = false }
};

// Hent alle opgaver
app.MapGet("/tasks", () => tasks);

// Hent én opgave med et bestemt id
app.MapGet("/tasks/{id}", (int id) =>
{
    var task = tasks.FirstOrDefault(t => t.Id == id);
    return task is null ? Results.NotFound() : Results.Ok(task);
});

// Opret en ny opgave
app.MapPost("/tasks", (TaskItem newTask) =>
{
    newTask.Id = tasks.Count + 1;
    tasks.Add(newTask);
    return Results.Created($"/tasks/{newTask.Id}", newTask);
});

// Opdater en opgave
app.MapPut("/tasks/{id}", (int id, TaskItem updatedTask) =>
{
    var task = tasks.FirstOrDefault(t => t.Id == id);
    if (task is null) return Results.NotFound();
    
    task.Title = updatedTask.Title;
    task.IsCompleted = updatedTask.IsCompleted;
    return Results.Ok(task);
});

// Slet en opgave
app.MapDelete("/tasks/{id}", (int id) =>
{
    var task = tasks.FirstOrDefault(t => t.Id == id);
    if (task is null) return Results.NotFound();
    
    tasks.Remove(task);
    return Results.NoContent();
});


app.Run();
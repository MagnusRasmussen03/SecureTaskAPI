var builder = WebApplication.CreateBuilder(args); // ← Forbereder alt

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();  // ← Bygger selve API'en

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Database konfiguration
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

// JWT konfiguration
var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? throw new InvalidOperationException("JWT_SECRET er ikke sat!");
var jwtIssuer = builder.Configuration["JWT_ISSUER"]
    ?? throw new InvalidOperationException("JWT_ISSUER er ikke sat!");
var jwtAudience = builder.Configuration["JWT_AUDIENCE"]
    ?? throw new InvalidOperationException("JWT_AUDIENCE er ikke sat!");

var connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={password}";

// Registrer database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Registrer Repository og Service
// AddScoped betyder: opret én instans per HTTP request
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

// JWT autentificering
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// Tilføj controllers
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Kør migrations automatisk ved opstart
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

// Map controllers automatisk
app.MapControllers();

// Auth endpoints forbliver i Program.cs
app.MapPost("/register", async (RegisterRequest request, AppDbContext dbContext) =>
{
    if (await dbContext.Users.AnyAsync(u => u.Username == request.Username))
        return Results.BadRequest("Brugernavnet er allerede taget!");

    var newUser = new User
    {
        Username = request.Username,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
    };

    dbContext.Users.Add(newUser);
    await dbContext.SaveChangesAsync();
    return Results.Ok("Bruger oprettet!");
});

app.MapPost("/login", async (RegisterRequest request, AppDbContext dbContext) =>
{
    var foundUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

    if (foundUser is null || !BCrypt.Net.BCrypt.Verify(request.Password, foundUser.PasswordHash))
        return Results.Unauthorized();

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, foundUser.Username),
        new Claim(ClaimTypes.NameIdentifier, foundUser.Id.ToString())
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: creds
    );

    return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
});

app.Run();

record RegisterRequest(string Username, string Password);
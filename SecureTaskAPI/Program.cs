// Vi importerer de biblioteker vi skal bruge
// Microsoft.EntityFrameworkCore    → Til at tale med databasen (EF Core)
// JwtBearer                        → Til at håndtere JWT tokens
// IdentityModel.Tokens             → Til at validere og oprette tokens
// System.Text                      → Til at konvertere tekst til bytes
// System.IdentityModel.Tokens.Jwt  → Til at bygge selve JWT tokenet
// System.Security.Claims           → Til at gemme brugerinfo i tokenet
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

// ─────────────────────────────────────────────
// BYG FASEN - Her konfigurerer vi hele API'en
// inden den starter op
// ─────────────────────────────────────────────
var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────
// DATABASE KONFIGURATION
// Vi henter alle database-oplysninger fra vores
// miljøvariabler (appsettings.Development.json lokalt,
// .env i Docker) - aldrig hardcodet i koden!
// ?? throw betyder: "hvis variablen ikke findes, kast en fejl"
// ─────────────────────────────────────────────
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

// ─────────────────────────────────────────────
// JWT KONFIGURATION
// Vi henter JWT-oplysninger fra miljøvariabler
// JWT_SECRET   → Den hemmelige nøgle til at signere tokens
// JWT_ISSUER   → Hvem udstedte tokenet (vores API)
// JWT_AUDIENCE → Hvem er tokenet til (vores brugere)
// ─────────────────────────────────────────────
var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? throw new InvalidOperationException("JWT_SECRET er ikke sat!");
var jwtIssuer = builder.Configuration["JWT_ISSUER"]
    ?? throw new InvalidOperationException("JWT_ISSUER er ikke sat!");
var jwtAudience = builder.Configuration["JWT_AUDIENCE"]
    ?? throw new InvalidOperationException("JWT_AUDIENCE er ikke sat!");

// ─────────────────────────────────────────────
// BYG CONNECTION STRING
// Vi samler alle database-oplysninger til én streng
// som EF Core bruger til at forbinde til PostgreSQL
// F.eks: "Host=localhost;Port=5433;Database=securetaskdb;..."
// ─────────────────────────────────────────────
var connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={password}";

// ─────────────────────────────────────────────
// REGISTRER DATABASE SERVICE
// Vi fortæller API'en: "Brug AppDbContext til at tale
// med databasen, og brug denne connection string"
// AddDbContext gør at vi kan få AppDbContext
// injiceret i vores endpoints automatisk
// ─────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ─────────────────────────────────────────────
// JWT AUTENTIFICERING SETUP
// Her konfigurerer vi hvordan API'en skal validere
// JWT tokens den modtager fra brugere
// ─────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Tjek at tokenet er udstedt af vores API
            ValidateIssuer = true,
            // Tjek at tokenet er til vores brugere
            ValidateAudience = true,
            // Tjek at tokenet ikke er udløbet
            ValidateLifetime = true,
            // Tjek at tokenet er signeret med vores hemmelige nøgle
            ValidateIssuerSigningKey = true,
            // Den gyldige issuer (skal matche det vi satte i JWT_ISSUER)
            ValidIssuer = jwtIssuer,
            // Den gyldige audience (skal matche det vi satte i JWT_AUDIENCE)
            ValidAudience = jwtAudience,
            // Den hemmelige nøgle - konverteret fra tekst til bytes
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

// Tilføj autorisation-systemet så vi kan bruge .RequireAuthorization()
builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

// ─────────────────────────────────────────────
// KØR FASEN - Her bygger vi selve applikationen
// og tilføjer vores endpoints
// ─────────────────────────────────────────────
var app = builder.Build();

// ─────────────────────────────────────────────
// AUTOMATISKE MIGRATIONS
// Hver gang API'en starter, tjekker den om der
// er nye migrations der ikke er kørt endnu
// og kører dem automatisk - både lokalt og i Docker
// ─────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ─────────────────────────────────────────────
// VIGTIGT: Rækkefølgen her er ikke ligegyldig!
// UseAuthentication skal komme FØR UseAuthorization
// Først identificerer vi hvem brugeren er (Authentication)
// Derefter tjekker vi om de må det de prøver (Authorization)
// ─────────────────────────────────────────────
app.UseAuthentication();
app.UseAuthorization();

// ─────────────────────────────────────────────
// ENDPOINT: POST /register
// Opretter en ny bruger i databasen
// Modtager: { "username": "magnus", "password": "abc123" }
// Returnerer: "Bruger oprettet!" eller fejl
// ─────────────────────────────────────────────
app.MapPost("/register", async (RegisterRequest request, AppDbContext dbContext) =>
{
    // Tjek om brugernavnet allerede er taget
    // AnyAsync returnerer true hvis der findes én eller flere brugere med dette navn
    if (await dbContext.Users.AnyAsync(u => u.Username == request.Username))
        return Results.BadRequest("Brugernavnet er allerede taget!");

    // Opret en ny bruger
    // BCrypt.HashPassword krypterer passwordet - vi gemmer ALDRIG det rigtige password!
    // F.eks: "password123" → "$2a$11$xyz..." (et langt krypteret hash)
    var newUser = new User
    {
        Username = request.Username,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
    };

    // Gem brugeren i databasen
    dbContext.Users.Add(newUser);
    await dbContext.SaveChangesAsync();
    return Results.Ok("Bruger oprettet!");
});

// ─────────────────────────────────────────────
// ENDPOINT: POST /login
// Logger en bruger ind og returnerer et JWT token
// Modtager: { "username": "magnus", "password": "abc123" }
// Returnerer: { "token": "eyJhbGci..." } eller 401 Unauthorized
// ─────────────────────────────────────────────
app.MapPost("/login", async (RegisterRequest request, AppDbContext dbContext) =>
{
    // Find brugeren i databasen via brugernavn
    var foundUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

    // Tjek om brugeren eksisterer OG om passwordet er korrekt
    // BCrypt.Verify sammenligner det indtastede password med det gemte hash
    // Vi returnerer Unauthorized hvis ENTEN brugeren ikke findes ELLER passwordet er forkert
    // På den måde afslører vi ikke om det er brugernavnet eller passwordet der er forkert
    if (foundUser is null || !BCrypt.Net.BCrypt.Verify(request.Password, foundUser.PasswordHash))
        return Results.Unauthorized();

    // ─────────────────────────────────────────
    // BYGG JWT TOKEN
    // Claims er information vi gemmer inde i tokenet
    // Tænk på det som felter på et adgangskort
    // ─────────────────────────────────────────
    var claims = new[]
    {
        // Gem brugernavnet i tokenet
        new Claim(ClaimTypes.Name, foundUser.Username),
        // Gem brugerens ID i tokenet
        new Claim(ClaimTypes.NameIdentifier, foundUser.Id.ToString())
    };

    // Den hemmelige nøgle konverteret til bytes
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

    // SigningCredentials definerer HVORDAN vi signerer tokenet
    // HmacSha256 er en stærk krypteringsalgoritme
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    // Byg selve tokenet med alle oplysninger
    var token = new JwtSecurityToken(
        issuer: jwtIssuer,              // Hvem udstedte tokenet
        audience: jwtAudience,          // Hvem er tokenet til
        claims: claims,                 // Brugerinfo gemt i tokenet
        expires: DateTime.UtcNow.AddHours(1),  // Tokenet udløber om 1 time
        signingCredentials: creds       // Sådan signeres tokenet
    );

    // Konverter tokenet til en streng og returner det
    // F.eks: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
});

// ─────────────────────────────────────────────
// TASKS ENDPOINTS - Alle kræver at man er logget ind!
// .RequireAuthorization() betyder:
// "Du skal have et gyldigt JWT token for at bruge dette endpoint"
// Uden token får du 401 Unauthorized
// ─────────────────────────────────────────────

// Hent alle opgaver fra databasen
app.MapGet("/tasks", async (AppDbContext dbContext) =>
    await dbContext.Tasks.ToListAsync())
    .RequireAuthorization(); // ← Kræver JWT token!

// Hent én specifik opgave via id
// Returnerer 404 hvis opgaven ikke findes
app.MapGet("/tasks/{id}", async (int id, AppDbContext dbContext) =>
{
    var task = await dbContext.Tasks.FindAsync(id);
    return task is null ? Results.NotFound() : Results.Ok(task);
}).RequireAuthorization();

// Opret en ny opgave
// Returnerer 201 Created med den nye opgave
app.MapPost("/tasks", async (TaskItem newTask, AppDbContext dbContext) =>
{
    dbContext.Tasks.Add(newTask);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/tasks/{newTask.Id}", newTask);
}).RequireAuthorization();

// Opdater en eksisterende opgave
// Returnerer 404 hvis opgaven ikke findes
app.MapPut("/tasks/{id}", async (int id, TaskItem updatedTask, AppDbContext dbContext) =>
{
    var task = await dbContext.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();

    task.Title = updatedTask.Title;
    task.IsCompleted = updatedTask.IsCompleted;
    await dbContext.SaveChangesAsync();
    return Results.Ok(task);
}).RequireAuthorization();

// Slet en opgave
// Returnerer 204 No Content hvis det lykkedes
// Returnerer 404 hvis opgaven ikke findes
app.MapDelete("/tasks/{id}", async (int id, AppDbContext dbContext) =>
{
    var task = await dbContext.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();

    dbContext.Tasks.Remove(task);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.Run();

// ─────────────────────────────────────────────
// REQUEST MODEL
// RegisterRequest er en simpel dataklasse der
// beskriver hvad /register og /login forventer
// record er en C# klasse der kun holder data
// Svarende til: { "username": "...", "password": "..." }
// ─────────────────────────────────────────────
record RegisterRequest(string Username, string Password);
using ProjectLexagonBackend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting; // UUSI: Tarvitaan Rate Limiterille
using System.Threading.RateLimiting;     // UUSI: Tarvitaan asetuksille

var builder = WebApplication.CreateBuilder(args);

// Hae yhteysosoite ensisijaisesti ympäristömuuttujasta
var connectionString = Environment.GetEnvironmentVariable("LEXAGON_CONNECTIONSTRING");
if (string.IsNullOrWhiteSpace(connectionString))
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ProjectLexagonContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- UUSI: RATE LIMITER PALVELUN REKISTERÖINTI ---
builder.Services.AddRateLimiter(options =>
{
    // Jos raja paukkuu, palautetaan 429 (Too Many Requests)
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Luodaan policy nimeltä "fixed"
    options.AddFixedWindowLimiter(policyName: "fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;                     // Max 10 pyyntöä...
        limiterOptions.Window = TimeSpan.FromMinutes(1);     // ...minuutin ikkunassa
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;                       // Sallitaan 2 pyyntöä jonoon
    });
});
// -------------------------------------------------

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// --- UUSI: RATE LIMITER MIDDLEWARE ---
// Tämän pitää olla ennen MapControllers-komentoa!
app.UseRateLimiter();
// -------------------------------------

app.MapControllers();
app.Run();
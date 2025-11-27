using ProjectLexagonBackend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides; // Tarvitaan IP-osoitteiden selvittämiseen
using System.Net;

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("LEXAGON_CONNECTIONSTRING");
if (string.IsNullOrWhiteSpace(connectionString))
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ProjectLexagonContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. Määritellään, että luotetaan Apachen välittämään IP-osoitteeseen (X-Forwarded-For)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Tärkeä: Tyhjennetään oletusverkot ja luotetaan "loopback" proxypalvelimeen (Apache on samassa koneessa)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// 2. Muutetaan Rate Limiter "Partitioned" -malliksi (Per IP)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // "fixed" policy, joka jakaa (partition) käyttäjät IP-osoitteen mukaan
    options.AddPolicy("fixed", httpContext =>
    {
        // Haetaan käyttäjän oikea IP. Jos ei löydy, käytetään "unknown".
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey: remoteIp, factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 2,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });
});

var app = builder.Build();

// Ota Forwarded Headers käyttöön HETI alussa, jotta IP on oikein myöhemmissä vaiheissa
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseRateLimiter(); // Rajoitin käyttöön

app.MapControllers();
app.Run();
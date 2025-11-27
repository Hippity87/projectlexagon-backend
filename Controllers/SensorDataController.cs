using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;  // Add this for async/query extensions
using ProjectLexagonBackend.Data;
using ProjectLexagonBackend.Models;

namespace ProjectLexagonBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
public class SensorDataController : ControllerBase
{
    private readonly ProjectLexagonContext _context;

    public SensorDataController(ProjectLexagonContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> PostSensorData([FromBody] SensorDataDto data)
    {
        Console.WriteLine($"POST! data: {data?.Temperature}, {data?.Humidity}");
        if (data == null)
        {
            Console.WriteLine("DATA ON NULL");
            return BadRequest();
        }

        var entity = new SensorData
        {
            Temperature = data.Temperature,
            Humidity = data.Humidity,
            Timestamp = DateTime.UtcNow
        };
        _context.SensorData.Add(entity);
        await _context.SaveChangesAsync();

        return Ok(new { status = "ok", received = entity });
    }

    [HttpGet]
    public async Task<IActionResult> GetSensorData([FromQuery] int? lastN = null)
    {
        IQueryable<SensorData> query = _context.SensorData;

        // Jos käyttäjä pyytää määrän, käytetään sitä. 
        // MUTTA: laitetaan silti maksimikatto (esim. max 1000), ettei joku pyydä lastN=9999999
        int takeAmount = lastN.HasValue && lastN.Value > 0 ? lastN.Value : 50;

        // Turvallisuus: Pakotetaan katto. Jos pyydetään yli 200, annetaan vain 200.
        if (takeAmount > 200) takeAmount = 200;

        var data = await query
            .OrderByDescending(x => x.Timestamp)
            .Take(takeAmount)
            .ToListAsync();

        data.Reverse(); // Käännetään aina, jotta graafi piirtyy oikein
        return Ok(data);
    }
}

public class SensorDataDto
{
    public float Temperature { get; set; }
    public float Humidity { get; set; }
}
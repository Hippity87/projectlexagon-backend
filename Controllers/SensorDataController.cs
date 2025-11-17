using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;  // Add this for async/query extensions
using ProjectLexagonBackend.Data;
using ProjectLexagonBackend.Models;

namespace ProjectLexagonBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
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

        if (lastN.HasValue && lastN.Value > 0)
        {
            // Efficient: Sort descending, take top N, then reverse to oldest-first
            var recentData = await query
                .OrderByDescending(x => x.Timestamp)
                .Take(lastN.Value)
                .ToListAsync();

            recentData.Reverse();  // Now oldest to newest
            return Ok(recentData);
        }
        else
        {
            // Fallback: Return all, sorted descending (as before)
            return Ok(await query.OrderByDescending(x => x.Timestamp).ToListAsync());
        }
    }
}

public class SensorDataDto
{
    public float Temperature { get; set; }
    public float Humidity { get; set; }
}
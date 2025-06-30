using Microsoft.AspNetCore.Mvc;
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
    public IActionResult GetAll()
    {
        return Ok(_context.SensorData.OrderByDescending(x => x.Timestamp).ToList());
    }
}

public class SensorDataDto
{
    public float Temperature { get; set; }
    public float Humidity { get; set; }
}

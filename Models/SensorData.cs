using Microsoft.EntityFrameworkCore;  // Required for IndexAttribute
using System.ComponentModel.DataAnnotations.Schema;  // If you have other annotations; optional here

namespace ProjectLexagonBackend.Models;

[Index(nameof(Timestamp))]  // Apply here for a single-column index on Timestamp
public class SensorData
{
    public int Id { get; set; }
    public double Temperature { get; set; }  // Or float, matching your DTO if needed
    public double Humidity { get; set; }
    public DateTime Timestamp { get; set; }
}
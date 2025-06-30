using Microsoft.EntityFrameworkCore;
using ProjectLexagonBackend.Models;

namespace ProjectLexagonBackend.Data;

public class ProjectLexagonContext : DbContext
{
    public ProjectLexagonContext(DbContextOptions<ProjectLexagonContext> options) : base(options) { }

    public DbSet<SensorData> SensorData { get; set; }
}

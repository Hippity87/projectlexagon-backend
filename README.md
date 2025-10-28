# ProjectLexagon Backend — ASP.NET Core Web API (.NET 8)

Minimal backend for the ProjectLexagon ecosystem.  
Provides REST endpoints used by personal frontends (e.g., QuantumConnect_Rework’s Weather UI).

## What’s Inside
- **Tech**: .NET 8, ASP.NET Core Web API, EF Core + Npgsql, PostgreSQL 16, Kestrel.
- **Data**:
  - `lexagon` DB — `SensorData` table (temperature, humidity, timestamp).
  - `pelitietokanta` DB — game-related tables (`Games`, `Moves`) when used.
- **Service**: systemd unit `projectlexagon-backend.service` (prod); optional Apache reverse proxy.

## Very Short Dev Flow
```bash
dotnet restore
dotnet build
# (Set ConnectionStrings__Default to your Postgres)
dotnet run
```
## Notes

Intended for my own environment; APIs are stable enough for my UIs but undocumented beyond this README.

Migrations use EF Core with the Npgsql provider.

© ProjectLexagon — Jesse Laine
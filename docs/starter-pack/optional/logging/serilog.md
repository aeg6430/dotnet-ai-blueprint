# Optional: Serilog (Console, rolling file, Seq)

This pack’s default API entry template uses **NLog** (`templates/Program.cs` + `templates/nlog.config`). If you prefer **Serilog**, treat this page as an optional swap-in.

## What you copy

- Replace your API `Program.cs` with [`templates/Program.Serilog.cs`](../../../../templates/Program.Serilog.cs) (same `Startup` pattern as the NLog sample).
- Merge the `Serilog` section from [`templates/appsettings.serilog.json`](../../../../templates/appsettings.serilog.json) into `appsettings.json` (and environment-specific files as needed).

**Seq `serverUrl` and `apiKey` are read only from configuration** (see `WriteTo` → `Seq` → `Args` in `appsettings.serilog.json`). Do not embed URLs or keys in `Program.Serilog.cs`.

## NuGet packages (API project)

Add references to your API `.csproj` (pin versions to your org’s policy; examples show a recent `net8.0`-friendly set):

```xml
<PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
<PackageReference Include="Serilog.Settings.Configuration" Version="8.0.4" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.Seq" Version="8.0.0" />
<PackageReference Include="Serilog.Enrichers.Environment" Version="3.0.1" />
<PackageReference Include="Serilog.Enrichers.Thread" Version="4.0.0" />
```

## Configuration and secrets

- **Development**: keep `serverUrl` in `appsettings.Development.json` or the shared `appsettings.json` fragment; use an empty `apiKey` if your local Seq does not require ingestion keys.
- **Production**: set `apiKey` via `appsettings.Production.json`, user secrets, a secret store, or environment variables. ASP.NET Core maps nested keys such as `Serilog__WriteTo__2__Args__apiKey` (index may differ if you reorder `WriteTo`).

If you do not use Seq in an environment, remove the `Seq` `WriteTo` entry for that environment’s settings file, or point `serverUrl` at a valid instance.

## Request logging (recommended)

`Program.Serilog.cs` does not modify `Startup` for you. In `Startup.Configure`, add **one** call to `UseSerilogRequestLogging` after `UseRouting()` and before authentication/endpoints, for example:

```csharp
app.UseRouting();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints => { /* ... */ });
```

Tune the message template in code if you need correlation IDs or fewer fields.

## Operational notes

- File sink path in the template is under the app base directory (`logs/app-.log` with daily rolling). Ensure the process identity can create that folder in each environment.
- Bootstrap logging uses Console only so startup failures are visible even if Serilog configuration fails to load; the full pipeline (including Seq) comes from configuration after the host starts building `LoggerConfiguration`.

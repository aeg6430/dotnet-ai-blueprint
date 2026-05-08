var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Minimal endpoint to keep the skeleton quiet; cross-cutting boundaries should live at the HTTP edge.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

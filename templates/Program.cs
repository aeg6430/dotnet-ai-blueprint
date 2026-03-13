using NLog;
using NLog.Web;

var logger = LogManager.Setup()
                       .LoadConfigurationFromFile("nlog.config")
                       .GetCurrentClassLogger();

try
{
    logger.Info("Starting up");

    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    if (!builder.Environment.IsDevelopment())
    {
        var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    }

    var startup = new Startup(builder.Configuration);
    startup.ConfigureServices(builder.Services);

    var app = builder.Build();
    startup.Configure(app, app.Environment);

    app.Run();
}
catch (Exception e)
{
    logger.Fatal(e, "Application start-up failed");
    throw;
}
finally
{
    LogManager.Shutdown();
}

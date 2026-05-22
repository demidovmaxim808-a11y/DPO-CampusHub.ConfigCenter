using Microsoft.Extensions.Options;
using CampusHub.ConfigCenter.Models;
using CampusHub.ConfigCenter.Middleware;
using CampusHub.ConfigCenter.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddXmlFile("portal.xml", optional: true, reloadOnChange: true)
    .AddIniFile("notifications.ini", optional: true, reloadOnChange: true)
    .AddTextFile("customsettings.txt")
    .AddInMemoryCollection(new Dictionary<string, string?> { ["Notifications:Sender"] = "memory-sender@campus-hub.edu" })
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// ПРАВИЛЬНАЯ РЕГИСТРАЦИЯ IOptions (через builder.Services, а не app.Services)
builder.Services.Configure<PortalOptions>(builder.Configuration.GetSection("Portal"));
builder.Services.Configure<NotificationOptions>(builder.Configuration.GetSection("Notifications"));

var app = builder.Build();

app.UseMiddleware<PortalHeaderMiddleware>();

app.MapGet("/", async (context) => await context.Response.WriteAsync(@"
<h1>CampusHub.ConfigCenter</h1>
<p>Учебный сервис диагностики и управления конфигурацией</p>
<ul>
    <li><a href=""/config/raw"">/config/raw</a></li>
    <li><a href=""/config/section/portal"">/config/section/portal</a></li>
    <li><a href=""/config/tree"">/config/tree</a></li>
    <li><a href=""/config/connection"">/config/connection</a></li>
    <li><a href=""/config/providers"">/config/providers</a></li>
    <li><a href=""/config/custom"">/config/custom</a></li>
    <li><a href=""/config/bind"">/config/bind</a></li>
    <li><a href=""/config/options"">/config/options</a></li>
    <li><a href=""/config/effective"">/config/effective</a></li>
</ul>
"));

app.MapGet("/config/raw", (IConfiguration config) => Results.Json(new
{
    Portal_Title = config["Portal:Title"],
    Portal_SupportEmail = config["Portal:SupportEmail"],
    Notifications_Sender = config["Notifications:Sender"]
}));

app.MapGet("/config/section/portal", (IConfiguration config) =>
    Results.Json(config.GetSection("Portal").GetChildren().ToDictionary(c => c.Key, c => c.Value)));

app.MapGet("/config/tree", (IConfiguration config) =>
{
    var result = new Dictionary<string, object>();
    void BuildTree(IConfigurationSection section, Dictionary<string, object> target)
    {
        foreach (var child in section.GetChildren())
        {
            if (child.GetChildren().Any())
            {
                var sub = new Dictionary<string, object>();
                BuildTree(child, sub);
                target[child.Key] = sub;
            }
            else target[child.Key] = child.Value ?? "null";
        }
    }
    BuildTree(config.GetSection("Portal"), result);
    return Results.Json(result);
});

app.MapGet("/config/connection", (IConfiguration config) =>
    Results.Json(new { DefaultConnection = config.GetConnectionString("DefaultConnection") }));

app.MapGet("/config/providers", (IConfiguration config) =>
    Results.Json(((IConfigurationRoot)config).Providers.Select((p, idx) => new
    {
        Order = idx,
        ProviderType = p.GetType().Name
    })));

app.MapGet("/config/custom", (IConfiguration config) => Results.Json(new
{
    CustomGreeting = config["CustomGreeting"],
    CustomMaintenanceMode = config["CustomMaintenanceMode"],
    CustomLastUpdate = config["CustomLastUpdate"]
}));

app.MapGet("/config/bind", (IConfiguration config) =>
{
    var portalOptions = new PortalOptions();
    config.GetSection("Portal").Bind(portalOptions);
    return Results.Json(portalOptions);
});

app.MapGet("/config/options", (IOptions<PortalOptions> portalOpt, IOptions<NotificationOptions> notifOpt) =>
    Results.Json(new { Portal = portalOpt.Value, Notifications = notifOpt.Value }));

app.MapGet("/config/effective", (IConfiguration config) => Results.Json(new
{
    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
    Portal_Title = config["Portal:Title"],
    Portal_SupportEmail = config["Portal:SupportEmail"],
    Notifications_Sender = config["Notifications:Sender"],
    ConflictNotes = new
    {
        Portal_Title = "Победило значение из commandLineArgs (наивысший приоритет)",
        Portal_SupportEmail = "Победило значение из Environment Variable",
        Notifications_Sender = "Победило значение из InMemory (добавлен после INI)"
    }
}));

app.Run();
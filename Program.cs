using System.Diagnostics;
using RadarOverlay.Components;
using RadarOverlay.Services;
using RadarOverlay.Models.Gsi;

var builder = WebApplication.CreateBuilder(args);

// Festen Port erzwingen, damit die portable .exe ohne launchSettings.json korrekt startet.
builder.WebHost.UseUrls($"http://localhost:{Cs2SetupService.Port}");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<SettingsService>();
builder.Services.AddSingleton<Cs2SetupService>();
builder.Services.AddSingleton<GameStateService>();
builder.Services.AddSingleton<FaceitApiClient>();
builder.Services.AddSingleton<FaceitService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapPost("/", async (GsiPayload payload, FaceitService faceitService, GameStateService gameStateService) =>
{
    var state = await faceitService.ProcessPayload(payload);
    gameStateService.UpdateState(state);
    return Results.Ok();
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// GSI-Konfiguration automatisch in den CS2-Ordner schreiben (best effort).
var cs2Setup = app.Services.GetRequiredService<Cs2SetupService>();
var setupResult = cs2Setup.WriteCfg();
Console.WriteLine(setupResult.Error != null
    ? $"[Setup] GSI-cfg nicht geschrieben: {setupResult.Error}"
    : $"[Setup] GSI-cfg geschrieben nach: {setupResult.CfgFolder}");

// Sobald der Server lauscht: Setup-Seite im Standardbrowser öffnen.
var url = $"http://localhost:{Cs2SetupService.Port}";
app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine($"[RadarOverlay] läuft auf {url}  –  Setup öffnet sich im Browser …");
    try
    {
        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
    }
    catch
    {
        Console.WriteLine($"[RadarOverlay] Browser konnte nicht geöffnet werden. Bitte manuell aufrufen: {url}");
    }
});

app.Run();

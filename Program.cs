using RadarOverlay.Components;
using RadarOverlay.Services;
using RadarOverlay.Models.Gsi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<GameStateService>();
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

app.Run();

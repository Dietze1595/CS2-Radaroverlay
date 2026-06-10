using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RadarOverlay.Models.Faceit;

namespace RadarOverlay.Services;

/// <summary>
/// Typisierter Zugriff auf die Faceit-APIs. Kapselt sowohl die offizielle Data-API v4
/// (Bearer-Token nötig) als auch die internen Endpunkte (Live-Match, Room-Stats, History).
/// Holt sich pro Aufruf einen HttpClient aus der Factory (kein langlebiger Client im Singleton).
/// </summary>
public class FaceitApiClient
{
    private const string OpenApiBase = "https://open.faceit.com/data/v4";
    private const string InternalApiBase = "https://api.faceit.com";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SettingsService _settings;

    public FaceitApiClient(IHttpClientFactory httpClientFactory, SettingsService settings)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings;
    }

    private HttpClient CreateClient() => _httpClientFactory.CreateClient();

    // ---------- Offizielle Data-API v4 (Bearer-Token) ----------

    public Task<FaceitPlayer?> GetPlayerBySteamIdAsync(string steamId) =>
        GetOfficialAsync<FaceitPlayer>($"{OpenApiBase}/players?game_player_id={steamId}&game=cs2");

    public Task<FaceitStats?> GetPlayerStatsAsync(string playerId) =>
        GetOfficialAsync<FaceitStats>($"{OpenApiBase}/players/{playerId}/stats/cs2");

    private async Task<T?> GetOfficialAsync<T>(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.FaceitToken ?? "");

        using var response = await CreateClient().SendAsync(request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<T>()
            : default;
    }

    // ---------- Interne API (ohne Auth) ----------

    /// <summary>Liefert die Room-/Match-ID eines laufenden Matches (VOTING/READY/ONGOING) oder null.</summary>
    public async Task<string?> GetLiveMatchIdAsync(string userId)
    {
        using var response = await CreateClient()
            .GetAsync($"{InternalApiBase}/match/v1/matches/groupByState?userId={userId}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var payload = doc.RootElement.GetProperty("payload");
        foreach (var property in payload.EnumerateObject())
        {
            if (property.Name is "VOTING" or "READY" or "ONGOING")
            {
                return property.Value[0].GetProperty("id").GetString();
            }
        }
        return null;
    }

    /// <summary>Match-Detaildaten (Teams, Roster, Win-Probability) für die Room-Stats.</summary>
    public async Task<FaceitMatchV2?> GetMatchAsync(string roomId)
    {
        using var response = await CreateClient().GetAsync($"{InternalApiBase}/match/v2/match/{roomId}");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<FaceitMatchV2>()
            : null;
    }

    /// <summary>Letzte Matches eines Spielers (für die Last-20-Aggregation).</summary>
    public async Task<List<FaceitHistoryMatch>?> GetRecentMatchesAsync(string userId, int size = 50)
    {
        using var response = await CreateClient()
            .GetAsync($"{InternalApiBase}/stats/v1/stats/time/users/{userId}/games/cs2?size={size}");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<List<FaceitHistoryMatch>>()
            : null;
    }
}

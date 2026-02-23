using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RadarOverlay.Models;
using RadarOverlay.Models.Faceit;
using RadarOverlay.Models.Gsi;

namespace RadarOverlay.Services;

public class FaceitService
{
    private readonly HttpClient _httpClient;
    private readonly string _token;
    private readonly Dictionary<string, FaceitPlayerInfo> _playerRoster = new();
    private readonly Dictionary<string, TeamInfo> _teamDatabase = new();

    public FaceitService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _token = configuration["FaceitToken"] ?? "";
    }

    public async Task<OverlayState> ProcessPayload(GsiPayload payload)
    {
        if (payload.Auth == null || payload.Auth.Token != "test" || payload.Player?.State == null)
        {
            return ReturnEmpty();
        }

        var mySteamId = payload.Provider?.SteamId ?? "";
        var playerSteamId = payload.Player.SteamId ?? "";

        if (_teamDatabase.TryGetValue(mySteamId, out var teamInfo))
        {
            if (_playerRoster.TryGetValue(playerSteamId, out var playerInfo))
            {
                return CreateRequest(payload, playerInfo, teamInfo);
            }
            else
            {
                // Trigger background fetch
                _ = FetchPlayerAndTeamData(mySteamId, playerSteamId);
                return CreateRequest(payload, null, teamInfo);
            }
        }
        else
        {
            // Trigger background fetch
            _ = FetchPlayerAndTeamData(mySteamId, playerSteamId);
            return ReturnEmpty();
        }
    }

    private async Task FetchPlayerAndTeamData(string mySteamId, string playerSteamId)
    {
        try
        {
            var faceitPlayer = await GetPlayerBySteamId(mySteamId);
            if (faceitPlayer != null)
            {
                var roomId = await GetFaceitMatchId(faceitPlayer.PlayerId!);
                if (!string.IsNullOrEmpty(roomId))
                {
                    await GetLiveStats(mySteamId, roomId);
                }
                else
                {
                    _teamDatabase[mySteamId] = new TeamInfo { GameName = "MM" };
                }
            }

            await GetPlayerIdFromPlayer(playerSteamId, mySteamId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching Faceit data: {ex.Message}");
        }
    }

    private async Task<FaceitPlayer?> GetPlayerBySteamId(string steamId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://open.faceit.com/data/v4/players?game_player_id={steamId}&game=cs2");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<FaceitPlayer>();
        }
        return null;
    }

    private async Task<string?> GetFaceitMatchId(string userId)
    {
        var response = await _httpClient.GetAsync($"https://api.faceit.com/match/v1/matches/groupByState?userId={userId}");
        if (response.IsSuccessStatusCode)
        {
            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var payload = doc.RootElement.GetProperty("payload");
            foreach (var property in payload.EnumerateObject())
            {
                if (property.Name == "VOTING" || property.Name == "READY" || property.Name == "ONGOING")
                {
                    return property.Value[0].GetProperty("id").GetString();
                }
            }
        }
        return null;
    }

    private async Task GetLiveStats(string mySteamId, string roomId)
    {
        var response = await _httpClient.GetAsync($"https://api.faceit.com/match/v2/match/{roomId}");
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<FaceitMatchV2>();
            var payload = data?.Payload;
            if (payload?.Teams != null)
            {
                var ownFaction = CheckForValue(payload.Teams.Faction1, mySteamId) ? payload.Teams.Faction1 : payload.Teams.Faction2;
                var enemyFaction = ownFaction == payload.Teams.Faction1 ? payload.Teams.Faction2 : payload.Teams.Faction1;

                if (ownFaction != null && enemyFaction != null)
                {
                    var ownAvgElo = (int)ownFaction.Roster!.Average(r => r.Elo);
                    var enemyAvgElo = (int)enemyFaction.Roster!.Average(r => r.Elo);
                    var winElo = ownFaction.Stats != null ? CalculateRatingChange(ownFaction.Stats.WinProbability, 50) : CalculateRatingChangeOld(ownAvgElo, enemyAvgElo);

                    _teamDatabase[mySteamId] = new TeamInfo
                    {
                        GameName = "Faceit",
                        OwnTeamName = ownFaction.Name,
                        EnemyTeamName = enemyFaction.Name,
                        OwnTeamAvgElo = ownAvgElo,
                        EnemyTeamAvgElo = enemyAvgElo,
                        WinElo = winElo,
                        LossElo = 50 - winElo
                    };
                }
            }
        }
    }

    private async Task GetPlayerIdFromPlayer(string playerSteamId, string mySteamId)
    {
        var faceitPlayer = await GetPlayerBySteamId(playerSteamId);
        if (faceitPlayer != null)
        {
            var stats = await GetStatsFromPlayer(faceitPlayer.PlayerId!);
            var history = await GetLast20Matches(faceitPlayer.PlayerId!);

            _playerRoster[playerSteamId] = new FaceitPlayerInfo
            {
                Nickname = faceitPlayer.Nickname,
                Elo = faceitPlayer.Games?.Cs2?.FaceitElo ?? 0,
                Avatar = faceitPlayer.Avatar,
                Flag = $"https://cdn-frontend.faceit.com/web/112-1536332382/src/app/assets/images-compress/flags/{faceitPlayer.Country?.ToUpper()}.png",
                FaceitLevel = faceitPlayer.Games?.Cs2?.SkillLevel ?? 0,
                Wins = stats?.Lifetime?.Wins ?? "0",
                PlayedGames = stats?.Lifetime?.Matches ?? "0",
                CurrentWinstreak = stats?.Lifetime?.CurrentWinStreak ?? "0",
                LongestWinstreak = stats?.Lifetime?.LongestWinStreak ?? "0",
                Player20Kills = history.Kills,
                Player20Hs = history.Hs,
                Player20Kd = history.Kd,
                Player20Kr = history.Kr
            };
        }
        else
        {
            _playerRoster[playerSteamId] = new FaceitPlayerInfo
            {
                Nickname = "No Faceitaccount",
                Elo = 0,
                Avatar = "",
                Flag = "",
                FaceitLevel = 0,
                Wins = "0",
                PlayedGames = "0",
                CurrentWinstreak = "0",
                LongestWinstreak = "0"
            };
        }
    }

    private async Task<FaceitStats?> GetStatsFromPlayer(string userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://open.faceit.com/data/v4/players/{userId}/stats/cs2");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<FaceitStats>();
        }
        return null;
    }

    private async Task<(int Kills, int Hs, string Kd, string Kr)> GetLast20Matches(string userId)
    {
        var response = await _httpClient.GetAsync($"https://api.faceit.com/stats/v1/stats/time/users/{userId}/games/cs2?size=50");
        if (response.IsSuccessStatusCode)
        {
            var matches = await response.Content.ReadFromJsonAsync<List<FaceitHistoryMatch>>();
            if (matches != null)
            {
                int totalKills = 0, totalHs = 0;
                double totalKd = 0, totalKr = 0;
                int count = 0;
                foreach (var m in matches)
                {
                    if (m.GameMode == "5v5")
                    {
                        totalKills += int.Parse(m.Kills ?? "0");
                        totalHs += (int)(double.Parse(m.HsPercentage ?? "0") * 100);
                        totalKd += double.Parse(m.KdRatio ?? "0") * 100;
                        totalKr += double.Parse(m.KrRatio ?? "0") * 100;
                        count++;
                        if (count == 20) break;
                    }
                }
                if (count > 0)
                {
                    return (totalKills / count, totalHs / (count * 100), (totalKd / (count * 100)).ToString("F2"), (totalKr / (count * 100)).ToString("F2"));
                }
            }
        }
        return (0, 0, "0.00", "0.00");
    }

    private bool CheckForValue(FaceitFaction? faction, string steamId)
    {
        return faction?.Roster?.Any(r => r.GameId == steamId) ?? false;
    }

    private int CalculateRatingChange(double winProb, int factor)
    {
        return (int)Math.Round(factor - winProb * factor);
    }

    private int CalculateRatingChangeOld(int ownElo, int enemyElo)
    {
        double diff = enemyElo - ownElo;
        double prob = 1.0 / (1.0 + Math.Pow(10, diff / 400.0));
        return (int)Math.Round(50 * (1.0 - prob));
    }

    private OverlayState CreateRequest(GsiPayload gsi, FaceitPlayerInfo? player, TeamInfo? team)
    {
        return new OverlayState
        {
            Phase = gsi.Map?.Phase,
            Round = gsi.Player?.State?.Health ?? 0,
            Activity = gsi.Player?.Activity,
            Bomb = gsi.Round?.Bomb,
            MySteamId = gsi.Provider?.SteamId,
            PlayerSteamId = gsi.Player?.SteamId,
            GameState = 1,
            GameName = team?.GameName,
            Kills = gsi.Player?.MatchStats?.Kills ?? 0,
            Assists = gsi.Player?.MatchStats?.Assists ?? 0,
            Deaths = gsi.Player?.MatchStats?.Deaths ?? 0,
            Mvps = gsi.Player?.MatchStats?.Mvps ?? 0,
            Score = gsi.Player?.MatchStats?.Score ?? 0,
            RoundKills = gsi.Player?.State?.RoundKills ?? 0,
            Name = player?.Nickname,
            Avatar = player?.Avatar,
            Flag = player?.Flag,
            Elo = player?.Elo ?? 0,
            FaceitLevel = player?.FaceitLevel ?? 0,
            Wins = player?.Wins,
            PlayedGames = player?.PlayedGames,
            CurrentWinstreak = player?.CurrentWinstreak,
            LongestWinstreak = player?.LongestWinstreak,
            OwnTeamName = team?.OwnTeamName,
            EnemyTeamName = team?.EnemyTeamName,
            OwnTeamAvgElo = team?.OwnTeamAvgElo ?? 0,
            EnemyTeamAvgElo = team?.EnemyTeamAvgElo ?? 0,
            WinElo = team?.WinElo ?? 0,
            LossElo = team?.LossElo ?? 0,
            Player20Kills = player?.Player20Kills ?? 0,
            Player20Hs = player?.Player20Hs ?? 0,
            Player20Kd = player?.Player20Kd,
            Player20Kr = player?.Player20Kr
        };
    }

    private OverlayState ReturnEmpty() => new OverlayState { GameState = 0 };

    private class FaceitPlayerInfo
    {
        public string? Nickname { get; set; }
        public int Elo { get; set; }
        public string? Avatar { get; set; }
        public string? Flag { get; set; }
        public int FaceitLevel { get; set; }
        public string? Wins { get; set; }
        public string? PlayedGames { get; set; }
        public string? CurrentWinstreak { get; set; }
        public string? LongestWinstreak { get; set; }
        public int Player20Kills { get; set; }
        public int Player20Hs { get; set; }
        public string? Player20Kd { get; set; }
        public string? Player20Kr { get; set; }
    }

    private class TeamInfo
    {
        public string? GameName { get; set; }
        public string? OwnTeamName { get; set; }
        public string? EnemyTeamName { get; set; }
        public int OwnTeamAvgElo { get; set; }
        public int EnemyTeamAvgElo { get; set; }
        public int WinElo { get; set; }
        public int LossElo { get; set; }
    }
}

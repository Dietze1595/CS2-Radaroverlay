using RadarOverlay.Models;
using RadarOverlay.Models.Faceit;
using RadarOverlay.Models.Gsi;

namespace RadarOverlay.Services;

public class FaceitService
{
    private readonly FaceitApiClient _api;
    private readonly Dictionary<string, FaceitPlayerInfo> _playerRoster = new();
    private readonly Dictionary<string, TeamInfo> _teamDatabase = new();

    public FaceitService(FaceitApiClient api)
    {
        _api = api;
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
            var faceitPlayer = await _api.GetPlayerBySteamIdAsync(mySteamId);
            if (faceitPlayer != null)
            {
                var roomId = await _api.GetLiveMatchIdAsync(faceitPlayer.PlayerId!);
                if (!string.IsNullOrEmpty(roomId))
                {
                    await LoadRoomStats(mySteamId, roomId);
                }
                else
                {
                    _teamDatabase[mySteamId] = new TeamInfo { GameName = "MM" };
                }
            }

            await LoadPlayerInfo(playerSteamId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching Faceit data: {ex.Message}");
        }
    }

    private async Task LoadRoomStats(string mySteamId, string roomId)
    {
        var data = await _api.GetMatchAsync(roomId);
        var payload = data?.Payload;
        if (payload?.Teams == null)
        {
            return;
        }

        var ownFaction = CheckForValue(payload.Teams.Faction1, mySteamId) ? payload.Teams.Faction1 : payload.Teams.Faction2;
        var enemyFaction = ownFaction == payload.Teams.Faction1 ? payload.Teams.Faction2 : payload.Teams.Faction1;

        if (ownFaction != null && enemyFaction != null)
        {
            var ownAvgElo = (int)ownFaction.Roster!.Average(r => r.Elo);
            var enemyAvgElo = (int)enemyFaction.Roster!.Average(r => r.Elo);
            var winElo = ownFaction.Stats != null
                ? CalculateRatingChange(ownFaction.Stats.WinProbability, 50)
                : CalculateRatingChangeOld(ownAvgElo, enemyAvgElo);

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

    private async Task LoadPlayerInfo(string playerSteamId)
    {
        var faceitPlayer = await _api.GetPlayerBySteamIdAsync(playerSteamId);
        if (faceitPlayer != null)
        {
            var stats = await _api.GetPlayerStatsAsync(faceitPlayer.PlayerId!);
            var matches = await _api.GetRecentMatchesAsync(faceitPlayer.PlayerId!);
            var history = AggregateLast20(matches);

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
                Player20Kr = history.Kr,
                Last5Results = GetLast5Results(matches)
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

    private static (int Kills, int Hs, string Kd, string Kr) AggregateLast20(List<FaceitHistoryMatch>? matches)
    {
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
        return (0, 0, "0.00", "0.00");
    }

    // Letzte bis zu 5 5v5-Spiele als Sieg(true)/Niederlage(false), neuestes zuerst.
    private static List<bool> GetLast5Results(List<FaceitHistoryMatch>? matches)
    {
        var results = new List<bool>();
        if (matches == null) return results;
        foreach (var m in matches)
        {
            if (m.GameMode == "5v5")
            {
                results.Add(m.Result == "1");
                if (results.Count == 5) break;
            }
        }
        return results;
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
            // Bevorzugt die dedizierte CS2 "bomb"-Komponente, faellt auf round.bomb zurueck
            Bomb = gsi.Bomb?.State ?? gsi.Round?.Bomb,
            BombCountdown = ParseDouble(gsi.Bomb?.Countdown),
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
            Player20Kr = player?.Player20Kr,
            Last5Results = player?.Last5Results
        };
    }

    private OverlayState ReturnEmpty() => new OverlayState { GameState = 0 };

    private static double? ParseDouble(string? value)
    {
        return double.TryParse(value, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

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
        public List<bool>? Last5Results { get; set; }
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

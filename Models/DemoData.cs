namespace RadarOverlay.Models;

/// <summary>
/// Statischer, vollständig befüllter OverlayState für den Demo-Modus (?demo=1).
/// Erlaubt das Ansehen des Layouts ohne CS2, GSI oder Faceit-API.
/// </summary>
public static class DemoData
{
    /// <summary>
    /// Wie <see cref="Sample"/>, aber mit gelegter Bombe und laufendem Countdown –
    /// zum Ansehen des Bomben-Timers via ?demo=1&amp;bomb=1.
    /// </summary>
    public static OverlayState SamplePlanted
    {
        get
        {
            var s = Sample;
            s.Bomb = "planted";
            s.BombCountdown = 25.0;   // ~15s bereits abgelaufen -> Ring startet synchron mittendrin
            return s;
        }
    }

    public static OverlayState Sample => new()
    {
        // Sichtbarkeits-Flags: Seite anzeigen, voller (eckiger) Radar, kein Loader
        Phase = "live",
        Activity = "playing",
        GameState = 1,
        RequestState = 0,
        Round = 100,                       // Health
        Bomb = null,
        MySteamId = "76561190000000001",   // != PlayerSteamId  -> .main.square (voller Radar)
        PlayerSteamId = "76561190000000002",

        // Faceit-Spielerprofil
        Name = "s1mple",
        Avatar = "https://i.pravatar.cc/150?img=12",
        Flag = "https://flagcdn.com/de.svg",
        Elo = 3200,
        FaceitLevel = 10,
        Wins = "1543",
        PlayedGames = "2876",
        CurrentWinstreak = "4",
        LongestWinstreak = "17",

        // Faceit-Room-Stats
        GameName = "Faceit",
        OwnTeamName = "team_s1mple",
        EnemyTeamName = "team_donk",
        OwnTeamAvgElo = 3150,
        EnemyTeamAvgElo = 2980,
        WinElo = 22,
        LossElo = 28,

        // Aktuelle Match-Stats
        Kills = 24,
        Assists = 6,
        Deaths = 14,
        Mvps = 4,
        Score = 58,
        RoundKills = 3,                    // -> 3 von 5 Totenköpfen sichtbar

        // Last-20-Stats
        Player20Kills = 21,
        Player20Hs = 52,
        Player20Kd = "1.34",
        Player20Kr = "0.82",

        // Letzte 5 Spiele (neuestes zuerst): W W L W L
        Last5Results = new List<bool> { true, true, false, true, false }
    };
}

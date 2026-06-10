namespace RadarOverlay.Models;

public class OverlayState
{
    public string? Phase { get; set; }
    public int Round { get; set; }
    public string? Activity { get; set; }
    public string? Bomb { get; set; }
    public double? BombCountdown { get; set; }
    public string? MySteamId { get; set; }
    public string? PlayerSteamId { get; set; }
    public int GameState { get; set; }
    public int RequestState { get; set; }
    public string? GameName { get; set; }
    public int Kills { get; set; }
    public int Assists { get; set; }
    public int Deaths { get; set; }
    public int Mvps { get; set; }
    public int Score { get; set; }
    public int RoundKills { get; set; }
    public string? Name { get; set; }
    public string? Avatar { get; set; }
    public string? Flag { get; set; }
    public int Elo { get; set; }
    public int FaceitLevel { get; set; }
    public string? Wins { get; set; }
    public string? PlayedGames { get; set; }
    public string? CurrentWinstreak { get; set; }
    public string? LongestWinstreak { get; set; }
    public string? OwnTeamName { get; set; }
    public string? EnemyTeamName { get; set; }
    public int OwnTeamAvgElo { get; set; }
    public int EnemyTeamAvgElo { get; set; }
    public int WinElo { get; set; }
    public int LossElo { get; set; }
    public int Player20Kills { get; set; }
    public int Player20Hs { get; set; }
    public string? Player20Kd { get; set; }
    public string? Player20Kr { get; set; }

    // Letzte 5 Spiele: true = gewonnen, false = verloren (neuestes zuerst)
    public List<bool>? Last5Results { get; set; }
}

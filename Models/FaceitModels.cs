using System.Text.Json.Serialization;

namespace RadarOverlay.Models.Faceit;

public class FaceitPlayer
{
    [JsonPropertyName("player_id")]
    public string? PlayerId { get; set; }
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }
    public string? Country { get; set; }
    public Games? Games { get; set; }
}

public class Games
{
    public GameDetail? Cs2 { get; set; }
}

public class GameDetail
{
    [JsonPropertyName("game_player_id")]
    public string? GamePlayerId { get; set; }

    [JsonPropertyName("faceit_elo")]
    public int FaceitElo { get; set; }

    [JsonPropertyName("skill_level")]
    public int SkillLevel { get; set; }
}

public class FaceitStats
{
    public LifetimeStats? Lifetime { get; set; }
}

public class LifetimeStats
{
    public string? Wins { get; set; }
    public string? Matches { get; set; }

    [JsonPropertyName("Current Win Streak")]
    public string? CurrentWinStreak { get; set; }

    [JsonPropertyName("Longest Win Streak")]
    public string? LongestWinStreak { get; set; }
}

public class FaceitMatchResponse
{
    public List<FaceitMatch>? Payload { get; set; }
}

public class FaceitMatch
{
    public string? Id { get; set; }
}

public class FaceitMatchV2
{
    public FaceitMatchV2Payload? Payload { get; set; }
}

public class FaceitMatchV2Payload
{
    public FaceitMatchV2Teams? Teams { get; set; }
}

public class FaceitMatchV2Teams
{
    public FaceitFaction? Faction1 { get; set; }
    public FaceitFaction? Faction2 { get; set; }
}

public class FaceitFaction
{
    public string? Name { get; set; }
    public List<FaceitRosterPlayer>? Roster { get; set; }
    public FaceitFactionStats? Stats { get; set; }
}

public class FaceitRosterPlayer
{
    public string? GameId { get; set; }
    public int Elo { get; set; }
}

public class FaceitFactionStats
{
    [JsonPropertyName("winProbability")]
    public double WinProbability { get; set; }
}

public class FaceitHistoryMatch
{
    public string? GameMode { get; set; }

    [JsonPropertyName("i6")]
    public string? Kills { get; set; }

    [JsonPropertyName("c4")]
    public string? HsPercentage { get; set; }

    [JsonPropertyName("c2")]
    public string? KdRatio { get; set; }

    [JsonPropertyName("c3")]
    public string? KrRatio { get; set; }
}

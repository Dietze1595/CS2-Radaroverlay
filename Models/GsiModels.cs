using System.Text.Json.Serialization;

namespace RadarOverlay.Models.Gsi;

public class GsiPayload
{
    public Provider? Provider { get; set; }
    public Map? Map { get; set; }
    public Player? Player { get; set; }
    public Round? Round { get; set; }
    public Auth? Auth { get; set; }
}

public class Provider
{
    public string? SteamId { get; set; }
}

public class Map
{
    public string? Phase { get; set; }
}

public class Player
{
    public string? SteamId { get; set; }
    public string? Activity { get; set; }
    public PlayerState? State { get; set; }

    [JsonPropertyName("match_stats")]
    public MatchStats? MatchStats { get; set; }
}

public class PlayerState
{
    public int Health { get; set; }

    [JsonPropertyName("round_kills")]
    public int RoundKills { get; set; }
}

public class MatchStats
{
    public int Kills { get; set; }
    public int Assists { get; set; }
    public int Deaths { get; set; }
    public int Mvps { get; set; }
    public int Score { get; set; }
}

public class Round
{
    public string? Bomb { get; set; }
}

public class Auth
{
    public string? Token { get; set; }
}

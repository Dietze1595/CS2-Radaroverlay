using RadarOverlay.Models;

namespace RadarOverlay.Services;

public class GameStateService
{
    private OverlayState _currentState = new() { GameState = 0 };
    public OverlayState CurrentState => _currentState;

    /// <summary>Zeitpunkt des letzten GSI-Posts von CS2 (UTC).</summary>
    public DateTime? LastUpdateUtc { get; private set; }

    /// <summary>True, wenn CS2 kürzlich Daten geschickt hat (innerhalb von 35s; heartbeat = 10s).</summary>
    public bool IsConnected =>
        LastUpdateUtc.HasValue && DateTime.UtcNow - LastUpdateUtc.Value < TimeSpan.FromSeconds(35);

    public event Action? OnChange;

    public void UpdateState(OverlayState newState)
    {
        _currentState = newState;
        LastUpdateUtc = DateTime.UtcNow;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

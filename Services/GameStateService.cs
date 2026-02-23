using RadarOverlay.Models;

namespace RadarOverlay.Services;

public class GameStateService
{
    private OverlayState _currentState = new() { GameState = 0 };
    public OverlayState CurrentState => _currentState;

    public event Action? OnChange;

    public void UpdateState(OverlayState newState)
    {
        _currentState = newState;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

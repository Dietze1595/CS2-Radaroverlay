using System.Text.Json;

namespace RadarOverlay.Services;

public class AppSettings
{
    public string? FaceitToken { get; set; }

    /// <summary>Optionaler manueller CS2-cfg-Pfad, falls die Auto-Erkennung scheitert.</summary>
    public string? Cs2CfgPath { get; set; }
}

/// <summary>
/// Lädt/speichert die Benutzereinstellungen in %APPDATA%\RadarOverlay\settings.json.
/// Ersetzt das manuelle Editieren von appsettings.json.
/// </summary>
public class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _path;
    private readonly object _lock = new();
    private AppSettings _settings;

    public SettingsService()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RadarOverlay");
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "settings.json");
        _settings = Load();
    }

    public string SettingsPath => _path;

    public string? FaceitToken
    {
        get { lock (_lock) return _settings.FaceitToken; }
    }

    public string? Cs2CfgPath
    {
        get { lock (_lock) return _settings.Cs2CfgPath; }
    }

    public void SetToken(string? token)
    {
        lock (_lock)
        {
            _settings.FaceitToken = string.IsNullOrWhiteSpace(token) ? null : token.Trim();
            Persist();
        }
    }

    public void SetCfgPath(string? path)
    {
        lock (_lock)
        {
            _settings.Cs2CfgPath = string.IsNullOrWhiteSpace(path) ? null : path.Trim();
            Persist();
        }
    }

    private AppSettings Load()
    {
        try
        {
            if (File.Exists(_path))
            {
                return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_path)) ?? new AppSettings();
            }
        }
        catch
        {
            // Beschädigte Datei -> mit Defaults neu beginnen
        }
        return new AppSettings();
    }

    private void Persist()
    {
        try
        {
            File.WriteAllText(_path, JsonSerializer.Serialize(_settings, JsonOptions));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Konnte settings.json nicht schreiben: {ex.Message}");
        }
    }
}

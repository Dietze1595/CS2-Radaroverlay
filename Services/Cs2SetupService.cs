using System.Text.RegularExpressions;

namespace RadarOverlay.Services;

public class Cs2SetupStatus
{
    /// <summary>CS2-cfg-Ordner gefunden (Auto oder manuell).</summary>
    public string? CfgFolder { get; set; }

    /// <summary>Unsere GSI-cfg liegt bereits im Ordner.</summary>
    public bool CfgPresent { get; set; }

    public string? Error { get; set; }

    public bool Found => CfgFolder != null;
}

/// <summary>
/// Findet die CS2-Installation und schreibt die Game-State-Integration-cfg automatisch
/// in den richtigen Ordner (...\game\csgo\cfg).
/// </summary>
public class Cs2SetupService
{
    public const int Port = 3001;
    private const string CfgFileName = "gamestate_integration_radaroverlay.cfg";
    private const string GsiToken = "test";

    private readonly SettingsService _settings;

    public Cs2SetupService(SettingsService settings) => _settings = settings;

    /// <summary>Inhalt der GSI-Konfigurationsdatei für CS2.</summary>
    public static string CfgContent =>
        $$"""
        "RadarOverlay CS2"
        {
         "uri" "http://localhost:{{Port}}"
         "timeout" "5.0"
         "buffer"  "0.1"
         "throttle" "0.1"
         "heartbeat" "10.0"
         "auth"
         {
           "token" "{{GsiToken}}"
         }
         "data"
         {
           "provider"            "1"
           "map"                 "1"
           "round"               "1"
           "player_id"           "1"
           "player_state"        "1"
           "player_weapons"      "1"
           "player_match_stats"  "1"
           "bomb"                "1"
         }
        }
        """;

    public Cs2SetupStatus GetStatus()
    {
        var folder = FindCfgFolder();
        return new Cs2SetupStatus
        {
            CfgFolder = folder,
            CfgPresent = folder != null && File.Exists(Path.Combine(folder, CfgFileName))
        };
    }

    /// <summary>Schreibt (oder aktualisiert) die GSI-cfg im CS2-Ordner.</summary>
    public Cs2SetupStatus WriteCfg()
    {
        var folder = FindCfgFolder();
        if (folder == null)
        {
            return new Cs2SetupStatus { Error = "CS2-cfg-Ordner nicht gefunden. Bitte den Pfad manuell angeben." };
        }

        try
        {
            File.WriteAllText(Path.Combine(folder, CfgFileName), CfgContent);
            return new Cs2SetupStatus { CfgFolder = folder, CfgPresent = true };
        }
        catch (Exception ex)
        {
            return new Cs2SetupStatus { CfgFolder = folder, Error = $"Schreiben fehlgeschlagen: {ex.Message}" };
        }
    }

    /// <summary>Ermittelt den CS2-cfg-Ordner: manueller Override, dann Auto-Erkennung über Steam-Bibliotheken.</summary>
    public string? FindCfgFolder()
    {
        var manual = _settings.Cs2CfgPath;
        if (!string.IsNullOrWhiteSpace(manual) && Directory.Exists(manual))
        {
            return manual;
        }

        foreach (var library in GetSteamLibraries())
        {
            var cfg = Path.Combine(library, "steamapps", "common",
                "Counter-Strike Global Offensive", "game", "csgo", "cfg");
            if (Directory.Exists(cfg))
            {
                return cfg;
            }
        }

        return null;
    }

    private static IEnumerable<string> GetSteamLibraries()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var root in GetSteamRoots())
        {
            if (seen.Add(root))
            {
                yield return root;
            }

            // Weitere Bibliotheken aus libraryfolders.vdf (andere Laufwerke/Ordner)
            var vdf = Path.Combine(root, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(vdf))
            {
                continue;
            }

            string content;
            try { content = File.ReadAllText(vdf); }
            catch { continue; }

            foreach (Match match in Regex.Matches(content, "\"path\"\\s*\"([^\"]+)\""))
            {
                var path = match.Groups[1].Value.Replace(@"\\", @"\");
                if (Directory.Exists(path) && seen.Add(path))
                {
                    yield return path;
                }
            }
        }
    }

    private static IEnumerable<string> GetSteamRoots()
    {
        var candidates = new List<string>();

        var pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        if (!string.IsNullOrEmpty(pf86)) candidates.Add(Path.Combine(pf86, "Steam"));

        var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        if (!string.IsNullOrEmpty(pf)) candidates.Add(Path.Combine(pf, "Steam"));

        try
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady) continue;
                var rootDir = drive.RootDirectory.FullName;
                candidates.Add(Path.Combine(rootDir, "Steam"));
                candidates.Add(Path.Combine(rootDir, "SteamLibrary"));
            }
        }
        catch
        {
            // Laufwerks-Enumeration fehlgeschlagen -> nur die Standardpfade nutzen
        }

        foreach (var candidate in candidates)
        {
            if (Directory.Exists(candidate))
            {
                yield return candidate;
            }
        }
    }
}

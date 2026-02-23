# **RadarOverlay CS2 (Blazor Edition)**

## Introduction

Dies ist eine umgewandelte Version des RadarOverlay-Projekts, nun basierend auf **C# Blazor (.NET 8)**.
Es zeigt Live-Match-Statistiken von Faceit und CS2 direkt in einem Webbrowser oder als OBS-Overlay an.
Das Design ist so optimiert, dass es über dem Radar platziert werden kann, um Stream-Sniping zu verhindern.

## Installation

1.  **GSI Konfiguration:** Kopiere die Datei `gamestate_integration_radaroverlay.cfg` in deinen CS2 cfg-Ordner:
    `\Steam\steamapps\common\Counter-Strike Global Offensive\game\csgo\cfg`
2.  **Faceit Token:** Öffne `appsettings.json` und trage deinen Faceit Bearer Token unter `"FaceitToken"` ein.
    - Erstelle einen App/Token auf [developers.faceit.com](https://developers.faceit.com/apps).
3.  **Starten:**
    - Stelle sicher, dass [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installiert ist.
    - Führe `dotnet run` im Hauptverzeichnis aus.
    - Das Overlay ist unter `http://localhost:3001` (16:9) oder `http://localhost:3001?res=43` (4:3) erreichbar.

## CS2 Anpassungen

- Das Projekt wurde vollständig auf **CS2** (Counter-Strike 2) abgestimmt.
- Alle Faceit API-Abfragen nutzen das `cs2` Spiel-Präfix.
- Die GSI-Konfiguration wurde für CS2 optimiert.

## OBS Integration

- Füge eine neue Browser-Quelle in OBS hinzu.
- URL: `http://localhost:3001`
- Breite: 1920, Höhe: 1080

namespace ETD.Web.Services;

/// <summary>
/// Maps a service slug to whether Step 2 (Preisrahmen / parameters) should be shown.
/// Used by both the wizard navigation and the markup.
/// </summary>
public static class ServiceParams
{
    private static readonly HashSet<string> WithParams = new(StringComparer.OrdinalIgnoreCase)
    {
        "elektroinstallation",  // Neubau/Sanierung + Anzahl Räume
        "smart-home-knx",       // Räume + Funktionsumfang
        "photovoltaik",         // Dachfläche + Speicher
        "wallbox",              // kW + Distanz
        "klimatechnik",         // Räume + Multi-Split
        "sicherheit",           // Komponenten-Auswahl
        // sat, beleuchtung, netzwerk, sprechanlagen → just go to step 3 (Zeit/Ort)
    };

    public static bool HasParamsFor(string slug) => WithParams.Contains(slug);

    public static bool HasParamsForAny(IEnumerable<string> slugs) => slugs.Any(HasParamsFor);
}

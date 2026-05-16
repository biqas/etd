using System.ComponentModel.DataAnnotations;

namespace ETD.Web.Models;

public sealed class QuoteRequest
{
    [Required(ErrorMessage = "Bitte mindestens eine Leistung auswählen.")]
    [MinLength(1, ErrorMessage = "Bitte mindestens eine Leistung auswählen.")]
    public List<string> Services { get; set; } = new();

    /// <summary>
    /// Comma-separated service slugs submitted via an InputText hidden field on step 1.
    /// Parsed into <see cref="Services"/> in <c>Was.razor</c> before validation runs.
    /// </summary>
    public string? ServicesRaw { get; set; }

    /// <summary>
    /// Timeframe value submitted via an InputText hidden field on step 3.
    /// Synced from the selected radio chip by JavaScript; copied into <see cref="Timeframe"/>.
    /// </summary>
    public string? TimeframeRaw { get; set; }

    [Required, RegularExpression("^(privat|gewerbe)$", ErrorMessage = "Bitte Privat oder Gewerbe wählen.")]
    public string Audience { get; set; } = "privat";

    [StringLength(500)]
    public string? Notes { get; set; }

    // ----- Wallbox -----
    public int? WallboxKw { get; set; }
    public int? WallboxDistanceMeters { get; set; }

    // ----- Photovoltaik -----
    public int? PvAreaSqm { get; set; }
    /// <summary>PV battery storage capacity in kWh. 0 = no storage.</summary>
    public int? PvStorageKwh { get; set; }

    // ----- Elektroinstallation -----
    /// <summary>"neubau" or "sanierung".</summary>
    public string? ElektroProjektart { get; set; }
    public int? ElektroRoomCount { get; set; }
    public bool? ElektroZaehlerschrankNeu { get; set; }

    // ----- Smart Home / KNX -----
    public int? KnxRoomCount { get; set; }
    /// <summary>Pipe-separated function slugs (light|heating|blinds|security|scenes|voice).</summary>
    public string? KnxFunctions { get; set; }

    // ----- Klimatechnik -----
    public int? KlimaRoomCount { get; set; }
    /// <summary>"single", "multi" or "vrf".</summary>
    public string? KlimaSystem { get; set; }

    // ----- Sicherheitstechnik -----
    /// <summary>Pipe-separated component slugs (alarm|video|brandmelder|zutritt).</summary>
    public string? SecurityComponents { get; set; }
    public int? SecuritySensorCount { get; set; }

    [Required, RegularExpression("^(asap|2w|4w|flex)$")]
    public string Timeframe { get; set; } = "flex";

    [Required, RegularExpression(@"^\d{5}$", ErrorMessage = "Bitte 5-stellige PLZ eingeben.")]
    public string Plz { get; set; } = "";

    [Required, StringLength(80)]
    public string City { get; set; } = "";

    [Required, StringLength(80, MinimumLength = 2, ErrorMessage = "Bitte Vor- und Nachname eingeben.")]
    public string Name { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, Phone]
    public string Phone { get; set; } = "";

    [Range(typeof(bool), "true", "true", ErrorMessage = "Bitte Datenschutzhinweis bestätigen.")]
    public bool ConsentPrivacy { get; set; }

    public string? CompanyName2 { get; set; }  // Honeypot — must stay empty
}

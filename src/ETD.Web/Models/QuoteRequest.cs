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

    public int? WallboxKw { get; set; }
    public int? WallboxDistanceMeters { get; set; }
    public int? PvAreaSqm { get; set; }

    /// <summary>
    /// PV battery storage capacity in kWh. 0 = no storage.
    /// </summary>
    public int? PvStorageKwh { get; set; }

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

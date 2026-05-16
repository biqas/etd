using System.ComponentModel.DataAnnotations;

namespace ETD.Web.Models;

public sealed class QuoteRequest
{
    [Required(ErrorMessage = "Bitte mindestens eine Leistung auswählen.")]
    [MinLength(1, ErrorMessage = "Bitte mindestens eine Leistung auswählen.")]
    public List<string> Services { get; set; } = new();

    [Required, RegularExpression("^(privat|gewerbe)$", ErrorMessage = "Bitte Privat oder Gewerbe wählen.")]
    public string Audience { get; set; } = "privat";

    [StringLength(500)]
    public string? Notes { get; set; }

    public int? WallboxKw { get; set; }
    public int? WallboxDistanceMeters { get; set; }
    public int? PvAreaSqm { get; set; }
    public bool? PvWithStorage { get; set; }

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

namespace ETD.Web.Models;

public sealed record PriceEstimate(int LowEuro, int HighEuro, string Disclaimer = "Verbindliches Angebot erst nach Vor-Ort-Termin");

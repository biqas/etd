namespace ETD.Web.Models;

public sealed record ServiceItem(
    string Slug,
    string Title,
    string Icon,
    string ShortDescription,
    string LongDescription,
    IReadOnlyList<string> Bullets,
    bool HasPriceEstimator = false,
    bool IsTopTier = true);

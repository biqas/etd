using ETD.Web.Models;

namespace ETD.Web.Services;

public static class PriceEstimator
{
    public static PriceEstimate EstimateWallbox(int kw, int distanceMeters)
    {
        kw = Math.Clamp(kw, 4, 22);
        distanceMeters = Math.Clamp(distanceMeters, 1, 50);
        // Base 1500 for a standard 11kw wallbox at short distance; scale up for power and cable run
        var low = 1500 + (kw - 11) * 30 + Math.Max(0, distanceMeters - 5) * 20;
        var high = low + 400 + (kw > 11 ? 200 : 0) + (distanceMeters > 15 ? 300 : 0);
        return new PriceEstimate(Math.Max(low, 1500), Math.Max(high, low + 300));
    }

    public static PriceEstimate EstimatePv(int areaSqm, bool withStorage)
    {
        areaSqm = Math.Clamp(areaSqm, 10, 200);
        var kwp = Math.Round(areaSqm / 6.0, MidpointRounding.AwayFromZero);
        var low = (int)(kwp * 1100);
        var high = (int)(kwp * 1500);
        if (withStorage)
        {
            low += 6000;
            high += 9000;
        }
        return new PriceEstimate(low, high);
    }
}

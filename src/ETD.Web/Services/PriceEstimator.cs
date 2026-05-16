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

    public static PriceEstimate EstimatePv(int areaSqm, int storageKwh)
    {
        areaSqm = Math.Clamp(areaSqm, 10, 200);
        storageKwh = Math.Clamp(storageKwh, 0, 30);
        var kwp = Math.Round(areaSqm / 6.0, MidpointRounding.AwayFromZero);
        var low = (int)(kwp * 1100);
        var high = (int)(kwp * 1500);
        // Battery storage: ~600 €/kWh low, ~900 €/kWh high (Lithium 2024 installed)
        low += storageKwh * 600;
        high += storageKwh * 900;
        return new PriceEstimate(low, high);
    }

    /// <summary>
    /// Elektroinstallation: very rough — based on rooms + project type.
    /// Sanierung is per-room cheaper than Neubau because the wiring backbone is partly there.
    /// </summary>
    public static PriceEstimate EstimateElektroinstallation(int roomCount, bool isNeubau, bool zaehlerschrankNeu)
    {
        roomCount = Math.Clamp(roomCount, 1, 12);
        var perRoom = isNeubau ? (low: 1800, high: 2800) : (low: 900, high: 1700);
        var low = roomCount * perRoom.low;
        var high = roomCount * perRoom.high;
        if (zaehlerschrankNeu) { low += 1200; high += 2200; }
        return new PriceEstimate(low, high);
    }

    /// <summary>
    /// KNX project. Cost scales primarily with rooms (each needs sensors, actuators, wiring)
    /// and which function categories are active.
    /// </summary>
    public static PriceEstimate EstimateKnx(int roomCount, int functionCount)
    {
        roomCount = Math.Clamp(roomCount, 1, 12);
        functionCount = Math.Clamp(functionCount, 1, 6);
        // ~ 800-1400 € per room for basic light + blinds. Each extra function category
        // adds about 15-25 % because of bus devices + programming time.
        var basePerRoom = (low: 800, high: 1400);
        var funcMultiplier = 1.0 + (functionCount - 1) * 0.20;
        var low = (int)(roomCount * basePerRoom.low * funcMultiplier);
        var high = (int)(roomCount * basePerRoom.high * funcMultiplier);
        // Project setup (planning, programming, commissioning) is a flat overhead
        low += 1500;
        high += 2800;
        return new PriceEstimate(low, high);
    }

    /// <summary>Klimatechnik: Multi-Split / VRF cost more than single split.</summary>
    public static PriceEstimate EstimateKlima(int roomCount, string system)
    {
        roomCount = Math.Clamp(roomCount, 1, 8);
        var (low, high) = system?.ToLowerInvariant() switch
        {
            "vrf"   => (roomCount * 3200, roomCount * 4800),
            "multi" => (roomCount * 1800, roomCount * 2900),
            _       => (roomCount * 1500, roomCount * 2400) // single-split (one indoor unit per outdoor)
        };
        return new PriceEstimate(low, high);
    }

    /// <summary>
    /// Sicherheitstechnik: total cost = base setup per active component category
    /// + per-sensor/camera unit cost.
    /// </summary>
    public static PriceEstimate EstimateSecurity(int componentCount, int unitCount)
    {
        componentCount = Math.Clamp(componentCount, 1, 4);
        unitCount = Math.Clamp(unitCount, 1, 30);
        // Each component category brings a 600-1000 € baseline (panel, software, install).
        var baseline = (low: componentCount * 600, high: componentCount * 1000);
        // Unit pricing averages across alarm sensor / camera / smoke detector / reader.
        var perUnit = (low: 220, high: 380);
        return new PriceEstimate(
            baseline.low + unitCount * perUnit.low,
            baseline.high + unitCount * perUnit.high);
    }
}

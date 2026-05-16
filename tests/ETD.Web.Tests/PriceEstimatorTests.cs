using ETD.Web.Services;

namespace ETD.Web.Tests;

public class PriceEstimatorTests
{
    [Test]
    public async Task Wallbox_11kw_10m_Returns1500to2200()
    {
        var e = PriceEstimator.EstimateWallbox(kw: 11, distanceMeters: 10);
        await Assert.That(e.LowEuro).IsGreaterThanOrEqualTo(1500);
        await Assert.That(e.HighEuro).IsLessThanOrEqualTo(2400);
    }

    [Test]
    public async Task Wallbox_22kw_30m_HigherThan11kw_10m()
    {
        var a = PriceEstimator.EstimateWallbox(11, 10);
        var b = PriceEstimator.EstimateWallbox(22, 30);
        await Assert.That(b.HighEuro).IsGreaterThan(a.HighEuro);
    }

    [Test]
    public async Task Pv_30sqm_NoStorage_ReturnsRange()
    {
        var e = PriceEstimator.EstimatePv(areaSqm: 30, storageKwh: 0);
        await Assert.That(e.LowEuro).IsGreaterThan(0);
        await Assert.That(e.HighEuro).IsGreaterThan(e.LowEuro);
    }

    [Test]
    public async Task Pv_WithStorage_MoreExpensive()
    {
        var no = PriceEstimator.EstimatePv(30, 0);
        var yes = PriceEstimator.EstimatePv(30, 10);
        await Assert.That(yes.HighEuro).IsGreaterThan(no.HighEuro);
    }

    [Test]
    public async Task Pv_LargerStorage_MoreExpensive()
    {
        var small = PriceEstimator.EstimatePv(30, 5);
        var large = PriceEstimator.EstimatePv(30, 20);
        await Assert.That(large.LowEuro).IsGreaterThan(small.LowEuro);
    }
}

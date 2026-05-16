using ETD.Web.Services;

namespace ETD.Web.Tests;

public class ServiceCatalogTests
{
    [Test]
    public async Task TopServices_AreSevenAndAllHaveSlug()
    {
        var top = ServiceCatalog.TopServices;
        await Assert.That(top.Count).IsEqualTo(7);
        await Assert.That(top.All(s => !string.IsNullOrWhiteSpace(s.Slug))).IsTrue();
    }

    [Test]
    public async Task FindBySlug_ReturnsExpectedService()
    {
        var s = ServiceCatalog.FindBySlug("wallbox");
        await Assert.That(s).IsNotNull();
        await Assert.That(s!.Title).Contains("Wallbox");
    }

    [Test]
    public async Task FindBySlug_ReturnsNullForUnknownSlug()
    {
        await Assert.That(ServiceCatalog.FindBySlug("xxxxx")).IsNull();
    }
}

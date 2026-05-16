using ETD.Web.Models;
using ETD.Web.Services;
using Microsoft.AspNetCore.DataProtection;

namespace ETD.Web.Tests;

public class WizardStateProtectorTests
{
    private static WizardStateProtector NewProtector()
    {
        var dp = DataProtectionProvider.Create("test");
        return new WizardStateProtector(dp);
    }

    [Test]
    public async Task Protect_Then_Unprotect_Roundtrips()
    {
        var p = NewProtector();
        var q = new QuoteRequest { Audience = "gewerbe", Plz = "63599", City = "Biebergemünd", Name = "Max", Email = "x@y.de", Phone = "06050", ConsentPrivacy = true, Services = { "wallbox" } };
        var token = p.Protect(q);
        var q2 = p.Unprotect(token);
        await Assert.That(q2!.Audience).IsEqualTo("gewerbe");
        await Assert.That(q2.Services).Contains("wallbox");
    }

    [Test]
    public async Task Unprotect_TamperedToken_ReturnsNull()
    {
        var p = NewProtector();
        var q = new QuoteRequest { Name = "Max", Email = "x@y.de", Phone = "060", ConsentPrivacy = true, Services = { "wallbox" }, Plz = "63599", City = "B" };
        var token = p.Protect(q);
        var tampered = token[..^4] + "AAAA";
        await Assert.That(p.Unprotect(tampered)).IsNull();
    }
}

using System.ComponentModel.DataAnnotations;
using ETD.Web.Models;

namespace ETD.Web.Tests;

public class QuoteRequestValidationTests
{
    private static IList<ValidationResult> Validate(QuoteRequest q)
    {
        var ctx = new ValidationContext(q);
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(q, ctx, errors, validateAllProperties: true);
        return errors;
    }

    [Test]
    public async Task Empty_NameFails()
    {
        var q = new QuoteRequest { Email = "x@y.de", Phone = "060509", Audience = "privat", Services = new() { "wallbox" }, ConsentPrivacy = true };
        var errors = Validate(q);
        await Assert.That(errors.Any(e => e.MemberNames.Contains(nameof(QuoteRequest.Name)))).IsTrue();
    }

    [Test]
    public async Task Invalid_EmailFails()
    {
        var q = new QuoteRequest { Name = "Max", Email = "broken", Phone = "060509", Audience = "privat", Services = new() { "wallbox" }, ConsentPrivacy = true };
        var errors = Validate(q);
        await Assert.That(errors.Any(e => e.MemberNames.Contains(nameof(QuoteRequest.Email)))).IsTrue();
    }

    [Test]
    public async Task Missing_ConsentFails()
    {
        var q = new QuoteRequest { Name = "Max", Email = "x@y.de", Phone = "060509", Audience = "privat", Services = new() { "wallbox" }, ConsentPrivacy = false };
        var errors = Validate(q);
        await Assert.That(errors.Any(e => e.MemberNames.Contains(nameof(QuoteRequest.ConsentPrivacy)))).IsTrue();
    }

    [Test]
    public async Task Valid_Request_HasNoErrors()
    {
        var q = new QuoteRequest { Name = "Max", Email = "x@y.de", Phone = "060509", Audience = "privat", Services = new() { "wallbox" }, ConsentPrivacy = true, Plz = "63599", City = "B" };
        await Assert.That(Validate(q).Count).IsEqualTo(0);
    }
}

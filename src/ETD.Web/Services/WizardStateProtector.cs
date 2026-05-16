using System.Text.Json;
using ETD.Web.Models;
using Microsoft.AspNetCore.DataProtection;

namespace ETD.Web.Services;

public sealed class WizardStateProtector
{
    private readonly IDataProtector protector;

    public WizardStateProtector(IDataProtectionProvider provider)
    {
        protector = provider.CreateProtector("ETD.Wizard.v1");
    }

    public string Protect(QuoteRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        return protector.Protect(json);
    }

    public QuoteRequest? Unprotect(string? token)
    {
        if (string.IsNullOrEmpty(token)) return null;
        try
        {
            var json = protector.Unprotect(token);
            return JsonSerializer.Deserialize<QuoteRequest>(json);
        }
        catch
        {
            return null;
        }
    }
}

using ETD.Web.Components;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Persist DataProtection keys to a writable folder (mounted PersistentVolumeClaim in K8s),
// so wizard-state tokens (?s=…) and antiforgery cookies survive pod restarts and roll across replicas.
// Falls der Pfad nicht existiert / nicht writable → fallback auf in-memory keys (Dev).
var keysPath = builder.Configuration["DataProtection:KeysPath"]
    ?? (OperatingSystem.IsWindows() ? null : "/var/keys");
var dp = builder.Services.AddDataProtection().SetApplicationName("etd-web");
if (!string.IsNullOrWhiteSpace(keysPath))
{
    try
    {
        Directory.CreateDirectory(keysPath);
        dp.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DataProtection] could not persist keys to '{keysPath}': {ex.Message}. Keys are ephemeral.");
    }
}
builder.Services.AddSingleton<ETD.Web.Services.WizardStateProtector>();
builder.Services.AddSingleton(new ETD.Web.Services.SubmissionRateLimiter(maxPerHour: 5));
builder.Services.Configure<ETD.Web.Services.SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddSingleton<ETD.Web.Services.IQuoteMailer, ETD.Web.Services.SmtpQuoteMailer>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddRazorComponents();   // no AddInteractiveServerComponents — static SSR only

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapDefaultEndpoints();
app.MapRazorComponents<App>().WithStaticAssets();

app.Run();

using ETD.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDataProtection();
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
app.UseStaticFiles();
app.UseAntiforgery();

app.MapDefaultEndpoints();
app.MapRazorComponents<App>();

app.Run();

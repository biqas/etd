using ETD.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDataProtection();
builder.Services.AddSingleton<ETD.Web.Services.WizardStateProtector>();

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

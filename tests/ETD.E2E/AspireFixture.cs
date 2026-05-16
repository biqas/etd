using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using TUnit.Core.Interfaces;

namespace ETD.E2E;

/// <summary>
/// Session-scoped fixture that boots the Aspire AppHost (which starts Mailpit
/// and the Blazor web app) and provides a Playwright browser instance.
/// </summary>
public sealed class AspireFixture : IAsyncInitializer, IAsyncDisposable
{
    public DistributedApplication? App { get; private set; }
    public Uri WebBaseUri { get; private set; } = new("about:blank");
    public Uri MailpitApiUri { get; private set; } = new("about:blank");
    public IBrowser? Browser { get; private set; }

    public async Task InitializeAsync()
    {
        // Install Playwright browser binaries (no-op if already installed).
        Microsoft.Playwright.Program.Main(["install", "chromium"]);

        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.ETD_AppHost>();

        App = await builder.BuildAsync();
        await App.StartAsync();

        // Wait for resources to be healthy before proceeding.
        // ResourceNotificationService exposes WaitForResourceHealthyAsync.
        var notifications = App.Services
            .GetRequiredService<Aspire.Hosting.ApplicationModel.ResourceNotificationService>();

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        await notifications.WaitForResourceHealthyAsync("etd-web", cts.Token);
        await notifications.WaitForResourceHealthyAsync("mailpit", cts.Token);

        // Use CreateHttpClient to get the web app's base URL. Aspire sets the
        // BaseAddress to the resource's HTTP endpoint, which we also use for
        // Playwright to ensure consistent URL routing.
        using var webClient = App.CreateHttpClient("etd-web", "http");
        WebBaseUri = webClient.BaseAddress!;
        MailpitApiUri = App.GetEndpoint("mailpit", "ui");

        Console.WriteLine($"[AspireFixture] WebBaseUri = {WebBaseUri}");
        Console.WriteLine($"[AspireFixture] MailpitApiUri = {MailpitApiUri}");

        // Warm up: wait for the web app to serve an HTTP 200 on /angebot.
        // CreateHttpClient already configures service discovery; we use it here.
        var warmUpAttempts = 0;
        while (warmUpAttempts < 30)
        {
            try
            {
                var resp = await webClient.GetAsync("/angebot");
                if ((int)resp.StatusCode < 500)
                {
                    Console.WriteLine($"[AspireFixture] Web app warm-up OK (attempt {warmUpAttempts + 1}): {resp.StatusCode}");
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AspireFixture] Warm-up attempt {warmUpAttempts + 1} failed: {ex.Message}");
            }

            await Task.Delay(500);
            warmUpAttempts++;
        }

        var pw = await Playwright.CreateAsync();
        Browser = await pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.CloseAsync();
        }

        if (App is not null)
        {
            await App.DisposeAsync();
        }
    }
}

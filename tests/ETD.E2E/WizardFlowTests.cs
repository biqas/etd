using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace ETD.E2E;

[ClassDataSource<AspireFixture>(Shared = SharedType.PerTestSession)]
public class WizardFlowTests(AspireFixture fx)
{
    [Test]
    [Timeout(120_000)]
    public async Task FullWizardSubmission_DeliversTwoMailsToMailpit(CancellationToken cancellationToken)
    {
        var page = await fx.Browser!.NewPageAsync(new BrowserNewPageOptions
        {
            BaseURL = fx.WebBaseUri.ToString(),
        });

        // Use a generous navigation timeout. The Blazor SSR Enhance JS
        // turns form POSTs into fetch calls + history.pushState, which
        // Playwright tracks as URL changes.
        page.SetDefaultNavigationTimeout(60_000);
        page.SetDefaultTimeout(30_000);

        // ── Step 1: Was brauchen Sie? ──────────────────────────────────────────
        await page.GotoAsync("/angebot");

        // The tile checkboxes have display:none — click the parent label, not
        // the hidden <input>.  Elektroinstallation has no price estimator so
        // the wizard skips step 2 and goes straight to step 3.
        await page.Locator("label.tile")
            .Filter(new LocatorFilterOptions { HasText = "Elektroinstallation" })
            .ClickAsync();

        // Click "Weiter" and wait for the URL to change to step 3.
        // With Enhanced SSR, navigation happens via pushState so WaitForURL works.
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Weiter" }).ClickAsync();
        await page.WaitForURLAsync(new Regex(@"/angebot/zeit-ort"), new PageWaitForURLOptions { Timeout = 60_000 });

        // ── Step 3: Wann & wo? ────────────────────────────────────────────────
        await page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Wann & wo?" }).WaitForAsync();

        // The chip radio inputs also have display:none — click the parent label.
        await page.Locator("label.chip")
            .Filter(new LocatorFilterOptions { HasText = "So bald wie möglich" })
            .ClickAsync();

        // PLZ and Ort labels are siblings (not wrapping) — target by placeholder.
        await page.Locator("input[placeholder='63599']").FillAsync("63599");
        await page.Locator("input[placeholder='Biebergemünd']").FillAsync("Biebergemünd");

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Weiter" }).ClickAsync();
        await page.WaitForURLAsync(new Regex(@"/angebot/kontakt"), new PageWaitForURLOptions { Timeout = 60_000 });

        // ── Step 4: Kontakt ───────────────────────────────────────────────────
        await page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Ihre Kontaktdaten" }).WaitForAsync();

        // Name/E-Mail/Telefon labels are also siblings — target by placeholder.
        await page.Locator("input[placeholder='Max Muster']").FillAsync("Max Tester");
        await page.Locator("input[type='email']").FillAsync("max@tester.example");
        await page.Locator("input[type='tel']").FillAsync("01601234567");

        // The Datenschutz consent checkbox is an InputCheckbox rendered as a
        // visible <input type="checkbox"> (not inside a .tile), so CheckAsync works.
        await page.Locator("label")
            .Filter(new LocatorFilterOptions { HasText = "Datenschutzerklärung" })
            .Locator("input[type=checkbox]")
            .CheckAsync();

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Anfrage absenden" }).ClickAsync();

        // ── Success page ───────────────────────────────────────────────────────
        await page.WaitForURLAsync(new Regex(@"/angebot/erfolg"), new PageWaitForURLOptions { Timeout = 60_000 });
        await page.GetByText("Vielen Dank!").WaitForAsync();

        await Assert.That(await page.GetByText("Vielen Dank!").IsVisibleAsync()).IsTrue();

        // ── Mailpit API assertion ──────────────────────────────────────────────
        // Give the SMTP delivery a moment to propagate into Mailpit's store.
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

        using var http = new HttpClient
        {
            BaseAddress = fx.MailpitApiUri,
        };

        var messages = await http.GetStringAsync("/api/v1/messages", cancellationToken);

        await Assert.That(messages).Contains("Max Tester");
        await Assert.That(messages).Contains("Ihre Anfrage");
    }
}

using Microsoft.Playwright;

namespace ETD.E2E;

[ClassDataSource<AspireFixture>(Shared = SharedType.PerTestSession)]
public class PrivacyTests(AspireFixture fx)
{
    [Test]
    public async Task Home_NeverRequestsExternalOrigin()
    {
        var page = await fx.Browser!.NewPageAsync(new BrowserNewPageOptions
        {
            BaseURL = fx.WebBaseUri.ToString(),
        });
        var ownOrigin = fx.WebBaseUri.Host;

        var external = new List<string>();
        page.Request += (_, r) =>
        {
            try
            {
                var host = new Uri(r.Url).Host;
                if (!host.Equals(ownOrigin, StringComparison.OrdinalIgnoreCase) &&
                    !host.Equals("localhost", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrEmpty(host))
                {
                    external.Add(r.Url);
                }
            }
            catch { }
        };

        await page.GotoAsync("/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Assert.That(external).IsEmpty();
    }
}

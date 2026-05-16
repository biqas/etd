using ETD.Web.Services;

namespace ETD.Web.Tests;

public class SubmissionRateLimiterTests
{
    [Test]
    public async Task FirstFiveSubmissions_AreAllowed()
    {
        var l = new SubmissionRateLimiter(maxPerHour: 5);
        for (int i = 0; i < 5; i++)
            await Assert.That(l.TryConsume("1.2.3.4")).IsTrue();
    }

    [Test]
    public async Task SixthSubmission_IsBlocked()
    {
        var l = new SubmissionRateLimiter(maxPerHour: 5);
        for (int i = 0; i < 5; i++) l.TryConsume("1.2.3.4");
        await Assert.That(l.TryConsume("1.2.3.4")).IsFalse();
    }

    [Test]
    public async Task DifferentIp_IsIndependent()
    {
        var l = new SubmissionRateLimiter(maxPerHour: 5);
        for (int i = 0; i < 5; i++) l.TryConsume("1.2.3.4");
        await Assert.That(l.TryConsume("9.9.9.9")).IsTrue();
    }
}

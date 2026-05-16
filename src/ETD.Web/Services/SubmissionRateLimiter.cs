using System.Collections.Concurrent;

namespace ETD.Web.Services;

public sealed class SubmissionRateLimiter
{
    private readonly int maxPerHour;
    private readonly ConcurrentDictionary<string, List<DateTime>> hits = new();
    private readonly object gate = new();

    public SubmissionRateLimiter(int maxPerHour = 5) => this.maxPerHour = maxPerHour;

    public bool TryConsume(string clientIp)
    {
        if (string.IsNullOrWhiteSpace(clientIp)) clientIp = "unknown";
        var now = DateTime.UtcNow;
        var cutoff = now.AddHours(-1);

        lock (gate)
        {
            var list = hits.GetOrAdd(clientIp, _ => new List<DateTime>());
            list.RemoveAll(t => t < cutoff);
            if (list.Count >= maxPerHour) return false;
            list.Add(now);
            return true;
        }
    }
}

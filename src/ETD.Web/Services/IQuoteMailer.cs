using ETD.Web.Models;

namespace ETD.Web.Services;

public interface IQuoteMailer
{
    Task SendAsync(QuoteRequest request, CancellationToken ct = default);
}

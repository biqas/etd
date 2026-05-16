using ETD.Web.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ETD.Web.Services;

public sealed class SmtpOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public string? User { get; set; }
    public string? Pass { get; set; }
    public string From { get; set; } = "anfrage@elektrotechnikdesch.de";
    public string To { get; set; } = "mail@ElektroTechnikDesch.de";
    public bool UseTls { get; set; } = false;
}

public sealed class SmtpQuoteMailer : IQuoteMailer
{
    private readonly SmtpOptions opts;
    private readonly ILogger<SmtpQuoteMailer> log;

    public SmtpQuoteMailer(IOptions<SmtpOptions> options, ILogger<SmtpQuoteMailer> log)
    {
        opts = options.Value;
        this.log = log;
    }

    public async Task SendAsync(QuoteRequest q, CancellationToken ct = default)
    {
        var owner = BuildOwnerMessage(q);
        var customer = BuildCustomerMessage(q);

        using var client = new SmtpClient();
        var socketOption = opts.UseTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
        await client.ConnectAsync(opts.Host, opts.Port, socketOption, ct);
        if (!string.IsNullOrEmpty(opts.User))
            await client.AuthenticateAsync(opts.User, opts.Pass, ct);
        await client.SendAsync(owner, ct);
        await client.SendAsync(customer, ct);
        await client.DisconnectAsync(true, ct);

        log.LogInformation("Quote mail sent for {Email} (services: {Services})", q.Email, string.Join(",", q.Services));
    }

    private MimeMessage BuildOwnerMessage(QuoteRequest q)
    {
        var m = new MimeMessage();
        m.From.Add(MailboxAddress.Parse(opts.From));
        m.To.Add(MailboxAddress.Parse(opts.To));
        m.Subject = $"Neue Anfrage von {q.Name} ({string.Join(", ", q.Services)})";
        m.Body = new TextPart("plain")
        {
            Text =
$@"Neue Angebotsanfrage über die Webseite.

Name:        {q.Name}
E-Mail:      {q.Email}
Telefon:     {q.Phone}
Audience:    {q.Audience}
PLZ / Ort:   {q.Plz} {q.City}
Zeitrahmen:  {q.Timeframe}

Leistungen:
{string.Join("\n", q.Services.Select(s => "  - " + s))}

Wallbox kW:       {q.WallboxKw}
Wallbox Distanz:  {q.WallboxDistanceMeters} m
PV Fläche:        {q.PvAreaSqm} m²
PV Speicher:      {q.PvWithStorage}

Notiz:
{q.Notes}"
        };
        return m;
    }

    private MimeMessage BuildCustomerMessage(QuoteRequest q)
    {
        var m = new MimeMessage();
        m.From.Add(MailboxAddress.Parse(opts.From));
        m.To.Add(MailboxAddress.Parse(q.Email));
        m.Subject = "Ihre Anfrage bei Elektrotechnik Desch";
        m.Body = new TextPart("plain")
        {
            Text =
$@"Hallo {q.Name},

vielen Dank für Ihre Anfrage über unsere Webseite. Wir haben sie soeben empfangen und melden uns innerhalb von 24 Stunden bei Ihnen zurück.

Falls es eilt, erreichen Sie uns direkt unter 06050 9062874.

Mit freundlichen Grüßen
Christoph Desch
Elektrotechnik Desch
Von-Cancrin-Str. 22, 63599 Biebergemünd
06050 9062874 · mail@ElektroTechnikDesch.de"
        };
        return m;
    }
}

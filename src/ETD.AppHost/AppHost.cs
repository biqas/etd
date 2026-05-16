var builder = DistributedApplication.CreateBuilder(args);

// SMTP — in dev: Mailpit container; in prod: external server (configured via env var)
var smtpHost = builder.Configuration["Smtp:Host"];

var smtp = string.IsNullOrEmpty(smtpHost)
    ? builder.AddContainer("mailpit", "axllent/mailpit", "latest")
        .WithHttpEndpoint(port: 8025, targetPort: 8025, name: "ui")
        .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp")
    : null;

var web = builder.AddProject<Projects.ETD_Web>("etd-web")
    .WithEnvironment("Smtp__From", "anfrage@elektrotechnikdesch.de")
    .WithEnvironment("Smtp__To", "mail@ElektroTechnikDesch.de")
    .WithExternalHttpEndpoints();

if (smtp is not null)
{
    web.WithReference(smtp.GetEndpoint("smtp"))
       .WithEnvironment("Smtp__Host", smtp.GetEndpoint("smtp").Property(EndpointProperty.Host))
       .WithEnvironment("Smtp__Port", smtp.GetEndpoint("smtp").Property(EndpointProperty.Port));
}

builder.Build().Run();

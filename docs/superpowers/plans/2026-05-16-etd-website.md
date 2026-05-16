# ETD Website Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a professional, GDPR-compliant website for Elektrotechnik Desch (Meisterbetrieb in Biebergemünd) running on Blazor Static SSR, orchestrated by .NET Aspire 13.3, deployed to a CIVO Kubernetes cluster in Frankfurt — replacing the current WordPress site.

**Architecture:** Blazor with static server-side rendering only (no SignalR, no WebSocket — every request is stateless, server restart never breaks a user's flow). Forms use Blazor `EditForm` + `[SupplyParameterFromForm]` + Enhanced Form Navigation. Quote wizard state lives in localStorage + signed hidden form fields; the server holds nothing per user. Aspire 13.3 models the application and publishes Kubernetes manifests directly (no Helm).

**Tech Stack:** .NET 10 · C# 13 · Blazor Static SSR · .NET Aspire 13.3.0 · CIVO Kubernetes (Frankfurt) · nginx-ingress + cert-manager · GitHub Container Registry · GitHub Actions · TUnit · Playwright · Mailpit (dev) · SMTP via `mail.ElektroTechnikDesch.de` (prod)

**Reference spec:** [`docs/superpowers/specs/2026-05-16-etd-website-design.md`](../specs/2026-05-16-etd-website-design.md)

---

## File structure

```
etd/
├── ETD.sln
├── Directory.Build.props                       # SDK/lang/nullable settings shared across projects
├── Directory.Packages.props                    # Central package management
├── global.json                                  # Pin .NET SDK 10
├── Dockerfile                                   # multi-stage build for ETD.Web
├── .dockerignore
├── .github/workflows/
│   ├── ci.yml                                   # build + test on PR
│   └── deploy.yml                               # build + push image + aspire publish + kubectl apply
├── src/
│   ├── ETD.ServiceDefaults/                     # Shared Aspire defaults
│   │   ├── ETD.ServiceDefaults.csproj
│   │   └── Extensions.cs                        # AddServiceDefaults / MapDefaultEndpoints
│   ├── ETD.AppHost/                             # Aspire orchestration + K8s publish
│   │   ├── ETD.AppHost.csproj
│   │   └── AppHost.cs                           # Project model + Mailpit (dev) + K8s publish target
│   └── ETD.Web/
│       ├── ETD.Web.csproj
│       ├── Program.cs                           # static SSR setup, services, endpoints
│       ├── appsettings.json
│       ├── Components/
│       │   ├── _Imports.razor
│       │   ├── App.razor                        # Root document
│       │   ├── Routes.razor                     # Router
│       │   ├── Layout/
│       │   │   ├── MainLayout.razor
│       │   │   ├── Header.razor
│       │   │   ├── Footer.razor
│       │   │   ├── CtaBanner.razor              # Red end-of-page CTA
│       │   │   └── BreadcrumbBar.razor          # Subpage breadcrumbs
│       │   ├── Shared/
│       │   │   ├── HeroFullBleed.razor          # Dark Hero with overlay
│       │   │   ├── ServiceCard.razor            # Card on homepage / overview
│       │   │   ├── TrustBar.razor               # 4-stat trust strip
│       │   │   ├── ReferenceStrip.razor         # Horizontal projects scroller
│       │   │   ├── ServiceDetailHero.razor      # Hero for /leistungen/{slug}
│       │   │   └── FaqAccordion.razor           # Accordion on detail pages
│       │   └── Pages/
│       │       ├── Home.razor                   # /
│       │       ├── Leistungen.razor             # /leistungen
│       │       ├── ServiceDetail.razor          # /leistungen/{slug} — one component, data-driven
│       │       ├── Gewerbe.razor                # /gewerbe
│       │       ├── Notdienst.razor              # /notdienst
│       │       ├── Referenzen.razor             # /referenzen
│       │       ├── UeberUns.razor               # /ueber-uns
│       │       ├── Kontakt.razor                # /kontakt
│       │       ├── Impressum.razor              # /impressum
│       │       ├── Datenschutz.razor            # /datenschutz
│       │       ├── Cookies.razor                # /cookies
│       │       ├── NotFound.razor               # 404
│       │       └── Angebot/
│       │           ├── Was.razor                # /angebot — step 1
│       │           ├── Preisrahmen.razor        # /angebot/preisrahmen — step 2 (conditional)
│       │           ├── ZeitOrt.razor            # /angebot/zeit-ort — step 3
│       │           ├── Kontakt.razor            # /angebot/kontakt — step 4
│       │           └── Erfolg.razor             # /angebot/erfolg
│       ├── Models/
│       │   ├── ServiceItem.cs                   # static catalog entry
│       │   ├── QuoteRequest.cs                  # full wizard payload + validation
│       │   └── PriceEstimate.cs                 # range record for Step 2
│       ├── Services/
│       │   ├── ServiceCatalog.cs                # in-process static catalog (no DB)
│       │   ├── IQuoteMailer.cs
│       │   ├── SmtpQuoteMailer.cs               # MimeKit-based SMTP send
│       │   ├── PriceEstimator.cs                # Slider-based estimation logic
│       │   ├── WizardStateProtector.cs          # ASP.NET Core data-protection wrapped serialization
│       │   └── SubmissionRateLimiter.cs         # in-memory per-IP limiter
│       └── wwwroot/
│           ├── css/
│           │   ├── tokens.css                   # design tokens (colors, type, spacing)
│           │   ├── base.css                     # reset + base typography
│           │   ├── components.css               # buttons, cards, hero, etc.
│           │   └── site.css                     # imports the above
│           ├── js/
│           │   └── animations.js                # Intersection-Observer fade-up
│           ├── fonts/
│           │   ├── inter-400.woff2
│           │   ├── inter-600.woff2
│           │   ├── inter-700.woff2
│           │   ├── inter-800.woff2
│           │   └── inter-900.woff2
│           ├── img/
│           │   ├── hero/
│           │   ├── services/
│           │   ├── references/
│           │   └── illustrations/
│           ├── favicon.svg
│           ├── robots.txt
│           └── sitemap.xml
└── tests/
    ├── ETD.Web.Tests/
    │   ├── ETD.Web.Tests.csproj
    │   ├── ServiceCatalogTests.cs
    │   ├── PriceEstimatorTests.cs
    │   ├── WizardStateProtectorTests.cs
    │   ├── QuoteRequestValidationTests.cs
    │   └── SubmissionRateLimiterTests.cs
    └── ETD.E2E/
        ├── ETD.E2E.csproj
        ├── PlaywrightFixture.cs
        ├── HomePageTests.cs
        ├── PrivacyTests.cs               # asserts no external network calls
        └── WizardFlowTests.cs            # full end-to-end submission with Mailpit assert
```

---

## Phase 0 — Solution scaffold

### Task 0.1: Pin SDK and central package management

**Files:**
- Create: `global.json`
- Create: `Directory.Build.props`
- Create: `Directory.Packages.props`
- Create: `.editorconfig`

- [ ] **Step 1: Create `global.json`**

```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestFeature"
  }
}
```

- [ ] **Step 2: Create `Directory.Build.props`**

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>13</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
</Project>
```

- [ ] **Step 3: Create `Directory.Packages.props`**

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Aspire.Hosting.AppHost" Version="13.3.0" />
    <PackageVersion Include="Aspire.Hosting.Kubernetes" Version="13.3.0" />
    <PackageVersion Include="Microsoft.Extensions.ServiceDiscovery" Version="13.3.0" />
    <PackageVersion Include="Microsoft.Extensions.Http.Resilience" Version="9.0.0" />
    <PackageVersion Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.10.0" />
    <PackageVersion Include="OpenTelemetry.Extensions.Hosting" Version="1.10.0" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.10.0" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.Http" Version="1.10.0" />
    <PackageVersion Include="MailKit" Version="4.8.0" />
    <PackageVersion Include="TUnit" Version="0.10.0" />
    <PackageVersion Include="TUnit.Assertions" Version="0.10.0" />
    <PackageVersion Include="Microsoft.Playwright" Version="1.49.0" />
    <PackageVersion Include="Microsoft.Playwright.TUnit" Version="1.49.0" />
    <PackageVersion Include="Aspire.Hosting.Testing" Version="13.3.0" />
  </ItemGroup>
</Project>
```

- [ ] **Step 4: Create `.editorconfig`**

```
root = true

[*]
indent_style = space
indent_size = 4
end_of_line = lf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

[*.{json,yml,yaml,md,razor,html,css,js}]
indent_size = 2
```

- [ ] **Step 5: Commit**

```bash
git add global.json Directory.Build.props Directory.Packages.props .editorconfig
git commit -m "chore: pin SDK + central package management"
```

---

### Task 0.2: Create ETD.ServiceDefaults

**Files:**
- Create: `src/ETD.ServiceDefaults/ETD.ServiceDefaults.csproj`
- Create: `src/ETD.ServiceDefaults/Extensions.cs`

- [ ] **Step 1: Install Aspire templates (once per dev machine)**

Run: `dotnet new install Aspire.ProjectTemplates::13.3.0`
Expected: "The following template packages will be installed ... Success"

- [ ] **Step 2: Create the project**

Run: `dotnet new aspire-servicedefaults -n ETD.ServiceDefaults -o src/ETD.ServiceDefaults`

- [ ] **Step 3: Verify it builds**

Run: `dotnet build src/ETD.ServiceDefaults/ETD.ServiceDefaults.csproj`
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add src/ETD.ServiceDefaults/
git commit -m "feat: add Aspire service defaults project"
```

---

### Task 0.3: Create ETD.Web (Blazor Static SSR)

**Files:**
- Create: `src/ETD.Web/ETD.Web.csproj`
- Create: `src/ETD.Web/Program.cs`
- Create: `src/ETD.Web/Components/App.razor`
- Create: `src/ETD.Web/Components/Routes.razor`
- Create: `src/ETD.Web/Components/_Imports.razor`
- Create: `src/ETD.Web/Components/Pages/Home.razor`
- Create: `src/ETD.Web/appsettings.json`
- Create: `src/ETD.Web/Properties/launchSettings.json`

- [ ] **Step 1: Generate base Blazor app, interactivity = None, auth = None**

Run: `dotnet new blazor -n ETD.Web -o src/ETD.Web --interactivity None --auth None --empty`

- [ ] **Step 2: Open `src/ETD.Web/ETD.Web.csproj` and confirm `<TargetFramework>net10.0</TargetFramework>`**

If a different version appears, change it to `net10.0`. The project should inherit central package management from `Directory.Packages.props`.

- [ ] **Step 3: Add ServiceDefaults reference**

Edit `src/ETD.Web/ETD.Web.csproj` and add:

```xml
<ItemGroup>
  <ProjectReference Include="..\ETD.ServiceDefaults\ETD.ServiceDefaults.csproj" />
</ItemGroup>
```

- [ ] **Step 4: Update `src/ETD.Web/Program.cs` with static SSR setup**

```csharp
using ETD.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

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
```

- [ ] **Step 5: Ensure `App.razor` references no interactive render mode**

The file should not import `InteractiveServer` or similar. The body should have:

```razor
<!DOCTYPE html>
<html lang="de">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />
    <link rel="stylesheet" href="@Assets["css/site.css"]" />
    <link rel="icon" href="favicon.svg" type="image/svg+xml" />
    <HeadOutlet />
</head>
<body>
    <Routes />
    <script src="_framework/blazor.web.js"></script>
</body>
</html>
```

- [ ] **Step 6: Run it**

Run: `dotnet run --project src/ETD.Web -- --urls http://localhost:5080`
Expected: server starts, `curl http://localhost:5080` returns 200 with HTML body.

Kill the server with Ctrl+C.

- [ ] **Step 7: Commit**

```bash
git add src/ETD.Web/
git commit -m "feat: scaffold Blazor static SSR web project"
```

---

### Task 0.4: Create ETD.AppHost (Aspire 13.3 orchestration)

**Files:**
- Create: `src/ETD.AppHost/ETD.AppHost.csproj`
- Create: `src/ETD.AppHost/AppHost.cs`

- [ ] **Step 1: Generate the AppHost**

Run: `dotnet new aspire-apphost -n ETD.AppHost -o src/ETD.AppHost`

- [ ] **Step 2: Add the Web project + Kubernetes hosting package references**

Edit `src/ETD.AppHost/ETD.AppHost.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\ETD.Web\ETD.Web.csproj" />
</ItemGroup>
<ItemGroup>
  <PackageReference Include="Aspire.Hosting.Kubernetes" />
</ItemGroup>
```

- [ ] **Step 3: Replace `AppHost.cs` with the orchestration model**

```csharp
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

// Kubernetes publishing target — used by `aspire publish --publisher kubernetes`
builder.AddKubernetesEnvironment("k8s")
    .WithProperties(env =>
    {
        env.DefaultImagePullPolicy = "IfNotPresent";
        env.DefaultImageRegistry = "ghcr.io";
    });

builder.Build().Run();
```

> **Note for the executor:** Aspire 13.3 ships the Kubernetes publishing API in `Aspire.Hosting.Kubernetes`. If `AddKubernetesEnvironment` is not found at this exact version, check the Aspire 13.3 release notes for the equivalent extension method (the publisher API has been iterated through 9.x → 13.x). The intent is: declare a Kubernetes publishing environment so `aspire publish` writes K8s YAML to disk.

- [ ] **Step 4: Build and run**

Run: `dotnet build src/ETD.AppHost`
Expected: `Build succeeded.`

Run: `dotnet run --project src/ETD.AppHost`
Expected: Aspire dashboard URL is printed, the `etd-web` and `mailpit` resources start. Verify in the dashboard. Kill with Ctrl+C.

- [ ] **Step 5: Commit**

```bash
git add src/ETD.AppHost/
git commit -m "feat: add Aspire AppHost with Mailpit dev SMTP + K8s publish target"
```

---

### Task 0.5: Solution file + test projects

**Files:**
- Create: `ETD.sln`
- Create: `tests/ETD.Web.Tests/ETD.Web.Tests.csproj`
- Create: `tests/ETD.Web.Tests/SmokeTests.cs`
- Create: `tests/ETD.E2E/ETD.E2E.csproj`
- Create: `tests/ETD.E2E/PlaywrightFixture.cs`

- [ ] **Step 1: Create solution and add projects**

```bash
dotnet new sln -n ETD
dotnet sln add src/ETD.ServiceDefaults/ETD.ServiceDefaults.csproj
dotnet sln add src/ETD.Web/ETD.Web.csproj
dotnet sln add src/ETD.AppHost/ETD.AppHost.csproj
```

- [ ] **Step 2: Create TUnit unit test project**

```bash
dotnet new classlib -n ETD.Web.Tests -o tests/ETD.Web.Tests
rm tests/ETD.Web.Tests/Class1.cs
```

Edit `tests/ETD.Web.Tests/ETD.Web.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="TUnit" />
    <PackageReference Include="TUnit.Assertions" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\ETD.Web\ETD.Web.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 3: Add a smoke test so the project actually compiles + runs**

`tests/ETD.Web.Tests/SmokeTests.cs`:

```csharp
namespace ETD.Web.Tests;

public class SmokeTests
{
    [Test]
    public async Task Math_Works()
    {
        await Assert.That(1 + 1).IsEqualTo(2);
    }
}
```

- [ ] **Step 4: Create E2E project**

```bash
dotnet new classlib -n ETD.E2E -o tests/ETD.E2E
rm tests/ETD.E2E/Class1.cs
```

Edit `tests/ETD.E2E/ETD.E2E.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="TUnit" />
    <PackageReference Include="TUnit.Assertions" />
    <PackageReference Include="Microsoft.Playwright" />
    <PackageReference Include="Microsoft.Playwright.TUnit" />
    <PackageReference Include="Aspire.Hosting.Testing" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\ETD.AppHost\ETD.AppHost.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 5: Add projects to solution + run tests**

```bash
dotnet sln add tests/ETD.Web.Tests/ETD.Web.Tests.csproj
dotnet sln add tests/ETD.E2E/ETD.E2E.csproj
dotnet test tests/ETD.Web.Tests
```

Expected: `Passed! 1/1` or equivalent TUnit output.

- [ ] **Step 6: Commit**

```bash
git add ETD.sln tests/
git commit -m "test: add TUnit unit and Playwright E2E projects"
```

---

## Phase 1 — Design tokens & layout

### Task 1.1: Self-host Inter typeface

**Files:**
- Create: `src/ETD.Web/wwwroot/fonts/inter-400.woff2`
- Create: `src/ETD.Web/wwwroot/fonts/inter-600.woff2`
- Create: `src/ETD.Web/wwwroot/fonts/inter-700.woff2`
- Create: `src/ETD.Web/wwwroot/fonts/inter-800.woff2`
- Create: `src/ETD.Web/wwwroot/fonts/inter-900.woff2`
- Create: `src/ETD.Web/wwwroot/fonts/LICENSE.md`

- [ ] **Step 1: Download Inter (SIL Open Font License) variable woff2 subset**

```bash
mkdir -p src/ETD.Web/wwwroot/fonts
cd src/ETD.Web/wwwroot/fonts
curl -L -o inter.zip https://github.com/rsms/inter/releases/download/v4.0/Inter-4.0.zip
unzip -j inter.zip "Inter Web/Inter-Regular.woff2" -d .
mv Inter-Regular.woff2 inter-400.woff2
unzip -j inter.zip "Inter Web/Inter-SemiBold.woff2" -d .
mv Inter-SemiBold.woff2 inter-600.woff2
unzip -j inter.zip "Inter Web/Inter-Bold.woff2" -d .
mv Inter-Bold.woff2 inter-700.woff2
unzip -j inter.zip "Inter Web/Inter-ExtraBold.woff2" -d .
mv Inter-ExtraBold.woff2 inter-800.woff2
unzip -j inter.zip "Inter Web/Inter-Black.woff2" -d .
mv Inter-Black.woff2 inter-900.woff2
rm inter.zip
cd ../../../../..
```

- [ ] **Step 2: Add the SIL OFL license text**

`src/ETD.Web/wwwroot/fonts/LICENSE.md`:

```markdown
# Inter typeface license

Inter is licensed under the SIL Open Font License 1.1.

- Author: Rasmus Andersson (https://rsms.me/inter/)
- License: SIL Open Font License Version 1.1
- License text: https://openfontlicense.org/

Files in this directory: inter-400 / 600 / 700 / 800 / 900.woff2 (Inter 4.0).
```

- [ ] **Step 3: Commit**

```bash
git add src/ETD.Web/wwwroot/fonts/
git commit -m "feat: self-host Inter typeface (SIL OFL)"
```

---

### Task 1.2: Design tokens stylesheet

**Files:**
- Create: `src/ETD.Web/wwwroot/css/tokens.css`

- [ ] **Step 1: Write the design tokens**

```css
@font-face { font-family: 'Inter'; font-style: normal; font-weight: 400; font-display: swap; src: url('/fonts/inter-400.woff2') format('woff2'); }
@font-face { font-family: 'Inter'; font-style: normal; font-weight: 600; font-display: swap; src: url('/fonts/inter-600.woff2') format('woff2'); }
@font-face { font-family: 'Inter'; font-style: normal; font-weight: 700; font-display: swap; src: url('/fonts/inter-700.woff2') format('woff2'); }
@font-face { font-family: 'Inter'; font-style: normal; font-weight: 800; font-display: swap; src: url('/fonts/inter-800.woff2') format('woff2'); }
@font-face { font-family: 'Inter'; font-style: normal; font-weight: 900; font-display: swap; src: url('/fonts/inter-900.woff2') format('woff2'); }

:root {
    --red: #DC1F26;
    --red-dark: #A8161C;
    --red-soft: #FEF2F2;

    --ink: #0E1116;
    --ink-2: #2C333D;
    --muted: #6B7280;

    --line: #E5E7EB;
    --bg: #FAFAFA;
    --white: #FFFFFF;

    --hero-dark: #0E1116;
    --hero-dark-2: #1A1F2A;

    --radius-btn: 6px;
    --radius-card: 12px;
    --radius-hero: 16px;

    --shadow-card: 0 4px 14px rgba(15, 23, 42, 0.06);
    --shadow-card-hover: 0 12px 30px rgba(15, 23, 42, 0.08);
    --shadow-red: 0 8px 24px rgba(220, 31, 38, 0.35);

    --ease-out: cubic-bezier(.2, .8, .2, 1);
    --motion-fast: 200ms var(--ease-out);
    --motion: 250ms var(--ease-out);
    --motion-slow: 500ms var(--ease-out);

    --container: 1280px;
    --gutter: 32px;
}
```

- [ ] **Step 2: Commit**

```bash
git add src/ETD.Web/wwwroot/css/tokens.css
git commit -m "feat: add design tokens stylesheet"
```

---

### Task 1.3: Base + components CSS

**Files:**
- Create: `src/ETD.Web/wwwroot/css/base.css`
- Create: `src/ETD.Web/wwwroot/css/components.css`
- Create: `src/ETD.Web/wwwroot/css/site.css`

- [ ] **Step 1: Write base reset & typography**

`src/ETD.Web/wwwroot/css/base.css`:

```css
*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
html { -webkit-text-size-adjust: 100%; scroll-behavior: smooth; }
body {
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, system-ui, sans-serif;
    color: var(--ink);
    background: var(--bg);
    line-height: 1.55;
    -webkit-font-smoothing: antialiased;
}
img, svg { display: block; max-width: 100%; height: auto; }
a { color: var(--red); text-decoration: none; }
a:hover { color: var(--red-dark); }
::selection { background: var(--red); color: #fff; }
button { font: inherit; cursor: pointer; }
.container { max-width: var(--container); margin: 0 auto; padding: 0 var(--gutter); }

h1, h2, h3, h4 { font-weight: 800; letter-spacing: -0.02em; line-height: 1.1; }
h1 { font-size: clamp(2rem, 4vw, 3.25rem); }
h2 { font-size: clamp(1.5rem, 3vw, 2.25rem); }
h3 { font-size: 1.25rem; }
.eyebrow { font-size: 12px; font-weight: 700; color: var(--red); text-transform: uppercase; letter-spacing: 0.12em; margin-bottom: 14px; }

.fade-in { opacity: 0; transform: translateY(14px); transition: opacity var(--motion-slow), transform var(--motion-slow); }
.fade-in.in { opacity: 1; transform: translateY(0); }
@media (prefers-reduced-motion: reduce) {
    .fade-in { opacity: 1; transform: none; transition: none; }
}
```

- [ ] **Step 2: Write component CSS (buttons, cards, hero, ...)**

`src/ETD.Web/wwwroot/css/components.css`:

```css
/* Buttons */
.btn { display: inline-flex; align-items: center; gap: 8px; padding: 12px 22px; border-radius: var(--radius-btn); font-weight: 700; font-size: 14px; border: none; transition: transform var(--motion-fast), background var(--motion-fast); text-decoration: none; }
.btn-primary { background: var(--red); color: #fff; box-shadow: var(--shadow-red); }
.btn-primary:hover { background: var(--red-dark); color: #fff; transform: translateY(-1px); }
.btn-ghost { background: transparent; color: var(--ink); border: 1px solid var(--line); }
.btn-ghost:hover { border-color: var(--red); color: var(--red); }
.btn-ghost-dark { background: transparent; color: #fff; border: 1px solid rgba(255,255,255,0.2); }
.btn-ghost-dark:hover { border-color: #fff; color: #fff; }

/* Cards */
.card { background: var(--white); border: 1px solid var(--line); border-radius: var(--radius-card); padding: 24px; transition: transform var(--motion-fast), box-shadow var(--motion-fast), border-color var(--motion-fast); }
.card:hover { transform: translateY(-3px); box-shadow: var(--shadow-card-hover); border-color: var(--red); }

/* Hero full-bleed */
.hero { position: relative; min-height: 560px; background: var(--hero-dark); color: #fff; overflow: hidden; }
.hero::before { content: ''; position: absolute; inset: 0; background: linear-gradient(180deg, rgba(14,17,22,0.55), rgba(14,17,22,0.85)); z-index: 1; }
.hero .hero-img { position: absolute; inset: 0; width: 100%; height: 100%; object-fit: cover; z-index: 0; }
.hero .hero-content { position: relative; z-index: 2; max-width: 720px; padding: 96px var(--gutter) 64px; }
.hero h1 em { color: var(--red); font-style: normal; }

/* Trust bar */
.trust-bar { position: relative; z-index: 2; display: flex; gap: 48px; padding: 24px var(--gutter); background: linear-gradient(180deg, transparent, rgba(0,0,0,0.4)); color: #fff; }
.trust-bar strong { display: block; color: var(--red); font-size: 28px; font-weight: 900; }
.trust-bar span { font-size: 11px; color: #9CA3AF; text-transform: uppercase; letter-spacing: 0.08em; }

/* End-of-page CTA */
.cta-banner { background: linear-gradient(135deg, var(--red-dark), var(--red)); color: #fff; padding: 56px var(--gutter); text-align: center; }
.cta-banner h2 { font-size: 1.75rem; margin-bottom: 16px; }
.cta-banner .btn { background: #fff; color: var(--red); }
.cta-banner .btn:hover { background: var(--ink); color: #fff; }
```

- [ ] **Step 3: Aggregator stylesheet**

`src/ETD.Web/wwwroot/css/site.css`:

```css
@import url('tokens.css');
@import url('base.css');
@import url('components.css');
```

- [ ] **Step 4: Commit**

```bash
git add src/ETD.Web/wwwroot/css/
git commit -m "feat: add base + component CSS"
```

---

### Task 1.4: Intersection-Observer fade-up

**Files:**
- Create: `src/ETD.Web/wwwroot/js/animations.js`

- [ ] **Step 1: Write the script**

```javascript
(() => {
    if (!('IntersectionObserver' in window)) return;
    const io = new IntersectionObserver((entries) => {
        for (const e of entries) {
            if (e.isIntersecting) {
                e.target.classList.add('in');
                io.unobserve(e.target);
            }
        }
    }, { rootMargin: '0px 0px -80px 0px', threshold: 0.05 });

    const start = () => document.querySelectorAll('.fade-in').forEach(el => io.observe(el));
    if (document.readyState !== 'loading') start();
    else document.addEventListener('DOMContentLoaded', start);
    // Re-arm after enhanced navigation
    Blazor.addEventListener?.('enhancedload', start);
})();
```

- [ ] **Step 2: Reference it from `App.razor`**

Add inside `<body>` before `<script src="_framework/blazor.web.js">`:

```razor
<script src="js/animations.js"></script>
```

- [ ] **Step 3: Commit**

```bash
git add src/ETD.Web/wwwroot/js/animations.js src/ETD.Web/Components/App.razor
git commit -m "feat: intersection-observer fade-up animation"
```

---

### Task 1.5: Layout components — Header, Footer, CtaBanner

**Files:**
- Create: `src/ETD.Web/Components/Layout/Header.razor`
- Create: `src/ETD.Web/Components/Layout/Footer.razor`
- Create: `src/ETD.Web/Components/Layout/CtaBanner.razor`
- Modify: `src/ETD.Web/Components/Layout/MainLayout.razor`
- Modify: `src/ETD.Web/wwwroot/css/components.css` (add header/footer styles)

- [ ] **Step 1: Add header styles to `components.css`**

Append to `src/ETD.Web/wwwroot/css/components.css`:

```css
.site-header { position: sticky; top: 0; z-index: 50; background: rgba(255,255,255,0.85); backdrop-filter: blur(20px); border-bottom: 1px solid var(--line); }
.site-header-inner { display: flex; align-items: center; justify-content: space-between; height: 64px; }
.brand { display: flex; align-items: center; gap: 10px; font-weight: 800; color: var(--ink); }
.brand-mark { width: 36px; height: 36px; background: var(--red); color: #fff; border-radius: 8px; display: flex; align-items: center; justify-content: center; font-weight: 900; letter-spacing: 0.02em; box-shadow: var(--shadow-red); }
.nav-links { display: flex; gap: 28px; font-size: 14px; font-weight: 600; }
.nav-links a { color: var(--ink-2); }
.nav-links a:hover, .nav-links a[aria-current="page"] { color: var(--red); }
.site-header .phone-cta { background: var(--red); color: #fff; padding: 8px 16px; border-radius: var(--radius-btn); font-size: 13px; font-weight: 700; }
@media (max-width: 768px) { .nav-links { display: none; } }

.site-footer { background: var(--ink); color: #D1D5DB; padding: 56px 0 24px; }
.site-footer .container { display: grid; gap: 40px; grid-template-columns: 2fr 1fr 1fr 1fr; }
.site-footer h4 { color: #fff; margin-bottom: 16px; font-size: 14px; letter-spacing: 0.06em; text-transform: uppercase; }
.site-footer a { color: #D1D5DB; font-size: 13px; line-height: 2; display: block; }
.site-footer a:hover { color: #fff; }
.site-footer .legal { grid-column: 1 / -1; border-top: 1px solid #1F2937; padding-top: 16px; font-size: 12px; color: #6B7280; display: flex; gap: 24px; flex-wrap: wrap; }
@media (max-width: 768px) { .site-footer .container { grid-template-columns: 1fr; } }
```

- [ ] **Step 2: Write `Header.razor`**

```razor
<header class="site-header">
    <div class="container site-header-inner">
        <a href="/" class="brand">
            <span class="brand-mark">ETD</span>
            <span>Elektrotechnik Desch</span>
        </a>
        <nav class="nav-links" aria-label="Hauptnavigation">
            <a href="/leistungen">Leistungen</a>
            <a href="/gewerbe">Gewerbe</a>
            <a href="/referenzen">Referenzen</a>
            <a href="/ueber-uns">Über uns</a>
            <a href="/kontakt">Kontakt</a>
        </nav>
        <a href="tel:+4960509062874" class="phone-cta">06050 9062874</a>
    </div>
</header>
```

- [ ] **Step 3: Write `Footer.razor`**

```razor
<footer class="site-footer">
    <div class="container">
        <div>
            <a href="/" class="brand"><span class="brand-mark">ETD</span><span>Elektrotechnik Desch</span></a>
            <p style="margin-top:12px;font-size:13px">Meisterbetrieb für Elektro- und Kältetechnik.<br/>Innungsmitglied · Sachkundezertifikat A1.</p>
        </div>
        <div>
            <h4>Leistungen</h4>
            <a href="/leistungen/elektroinstallation">Elektroinstallation</a>
            <a href="/leistungen/smart-home-knx">Smart Home / KNX</a>
            <a href="/leistungen/photovoltaik">Photovoltaik</a>
            <a href="/leistungen/wallbox">Wallbox</a>
            <a href="/leistungen/klimatechnik">Klimatechnik</a>
            <a href="/leistungen/e-check">E-Check</a>
            <a href="/leistungen/sicherheit">Sicherheit</a>
        </div>
        <div>
            <h4>Kontakt</h4>
            <a href="tel:+4960509062874">06050 9062874</a>
            <a href="mailto:mail@ElektroTechnikDesch.de">mail@ElektroTechnikDesch.de</a>
            <p style="font-size:13px;line-height:1.6">Von-Cancrin-Str. 22<br/>63599 Biebergemünd</p>
        </div>
        <div>
            <h4>Angebot</h4>
            <a href="/angebot">Angebot anfragen</a>
            <a href="/notdienst">Notdienst</a>
        </div>
        <div class="legal">
            <a href="/impressum">Impressum</a>
            <a href="/datenschutz">Datenschutz</a>
            <a href="/cookies">Cookies</a>
            <span style="margin-left:auto">© @DateTime.UtcNow.Year Elektrotechnik Desch</span>
        </div>
    </div>
</footer>
```

- [ ] **Step 4: Write `CtaBanner.razor`**

```razor
<section class="cta-banner fade-in">
    <div class="container">
        <h2>Bereit für Ihr Projekt? Wir auch.</h2>
        <p style="opacity:0.85;margin-bottom:24px">Beschreiben Sie uns Ihr Anliegen — Sie hören innerhalb 24 h von uns.</p>
        <a href="/angebot" class="btn">Angebot anfragen →</a>
    </div>
</section>
```

- [ ] **Step 5: Rewrite `MainLayout.razor`**

```razor
@inherits LayoutComponentBase

<Header />
<main>
    @Body
</main>
<CtaBanner />
<Footer />
```

- [ ] **Step 6: Build & visually verify**

Run: `dotnet run --project src/ETD.Web -- --urls http://localhost:5080`
Open `http://localhost:5080`. Expected: header, the existing Home page placeholder, red CTA banner, dark footer. Kill server.

- [ ] **Step 7: Commit**

```bash
git add src/ETD.Web/Components/Layout/ src/ETD.Web/wwwroot/css/components.css
git commit -m "feat: site header, footer, and final CTA banner"
```

---

### Task 1.6: Shared components — HeroFullBleed, ServiceCard, TrustBar

**Files:**
- Create: `src/ETD.Web/Components/Shared/HeroFullBleed.razor`
- Create: `src/ETD.Web/Components/Shared/ServiceCard.razor`
- Create: `src/ETD.Web/Components/Shared/TrustBar.razor`

- [ ] **Step 1: HeroFullBleed.razor**

```razor
@code {
    [Parameter, EditorRequired] public string Eyebrow { get; set; } = "";
    [Parameter, EditorRequired] public string Title { get; set; } = "";
    [Parameter] public string? TitleAccent { get; set; }
    [Parameter, EditorRequired] public string Sub { get; set; } = "";
    [Parameter] public string PrimaryCtaText { get; set; } = "Angebot anfragen";
    [Parameter] public string PrimaryCtaHref { get; set; } = "/angebot";
    [Parameter] public string? SecondaryCtaText { get; set; }
    [Parameter] public string? SecondaryCtaHref { get; set; }
    [Parameter] public string? ImageSrc { get; set; }
    [Parameter] public string ImageAlt { get; set; } = "";
    [Parameter] public RenderFragment? TrustStrip { get; set; }
}

<section class="hero">
    @if (!string.IsNullOrEmpty(ImageSrc))
    {
        <img class="hero-img" src="@ImageSrc" alt="@ImageAlt" fetchpriority="high" />
    }
    <div class="container">
        <div class="hero-content fade-in">
            <div class="eyebrow" style="color:var(--red)">@Eyebrow</div>
            <h1>@Title @if (TitleAccent is not null) { <em>@TitleAccent</em> }</h1>
            <p style="font-size:1.125rem;color:#D1D5DB;margin:16px 0 28px;max-width:540px">@Sub</p>
            <div style="display:flex;gap:12px;flex-wrap:wrap">
                <a href="@PrimaryCtaHref" class="btn btn-primary">@PrimaryCtaText →</a>
                @if (SecondaryCtaText is not null && SecondaryCtaHref is not null)
                {
                    <a href="@SecondaryCtaHref" class="btn btn-ghost-dark">@SecondaryCtaText</a>
                }
            </div>
        </div>
    </div>
    @if (TrustStrip is not null)
    {
        <div class="container"><div class="trust-bar">@TrustStrip</div></div>
    }
</section>
```

- [ ] **Step 2: ServiceCard.razor**

```razor
@code {
    [Parameter, EditorRequired] public string Icon { get; set; } = "";
    [Parameter, EditorRequired] public string Title { get; set; } = "";
    [Parameter, EditorRequired] public string Description { get; set; } = "";
    [Parameter, EditorRequired] public string Href { get; set; } = "";
}

<a href="@Href" class="card service-card fade-in">
    <div class="service-icon">@Icon</div>
    <h3>@Title</h3>
    <p style="font-size:14px;color:var(--muted);margin-top:6px">@Description</p>
    <div style="font-size:13px;color:var(--red);margin-top:16px;font-weight:700">Mehr erfahren →</div>
</a>

<style>
.service-card { display: block; }
.service-card .service-icon { width: 52px; height: 52px; background: var(--red-soft); color: var(--red); border-radius: var(--radius-card); display: flex; align-items: center; justify-content: center; font-size: 24px; margin-bottom: 16px; transition: background var(--motion-fast), color var(--motion-fast); }
.service-card:hover .service-icon { background: var(--red); color: #fff; }
</style>
```

- [ ] **Step 3: TrustBar.razor (a thin wrapper for `<TrustStrip>`)**

```razor
<div><strong>15+</strong><span>Jahre Erfahrung</span></div>
<div><strong>500+</strong><span>Projekte</span></div>
<div><strong>24/7</strong><span>Notdienst</span></div>
<div><strong>★ Innung</strong><span>Mitglied</span></div>
```

- [ ] **Step 4: Commit**

```bash
git add src/ETD.Web/Components/Shared/
git commit -m "feat: shared Hero, ServiceCard, TrustBar components"
```

---

### Task 1.7: Service catalog (static data)

**Files:**
- Create: `src/ETD.Web/Models/ServiceItem.cs`
- Create: `src/ETD.Web/Services/ServiceCatalog.cs`
- Create: `tests/ETD.Web.Tests/ServiceCatalogTests.cs`

- [ ] **Step 1: Write the failing test first**

`tests/ETD.Web.Tests/ServiceCatalogTests.cs`:

```csharp
using ETD.Web.Services;

namespace ETD.Web.Tests;

public class ServiceCatalogTests
{
    [Test]
    public async Task TopServices_AreSevenAndAllHaveSlug()
    {
        var top = ServiceCatalog.TopServices;
        await Assert.That(top.Count).IsEqualTo(7);
        await Assert.That(top.All(s => !string.IsNullOrWhiteSpace(s.Slug))).IsTrue();
    }

    [Test]
    public async Task FindBySlug_ReturnsExpectedService()
    {
        var s = ServiceCatalog.FindBySlug("wallbox");
        await Assert.That(s).IsNotNull();
        await Assert.That(s!.Title).Contains("Wallbox");
    }

    [Test]
    public async Task FindBySlug_ReturnsNullForUnknownSlug()
    {
        await Assert.That(ServiceCatalog.FindBySlug("xxxxx")).IsNull();
    }
}
```

- [ ] **Step 2: Run it — should fail with "ServiceCatalog not found"**

Run: `dotnet test tests/ETD.Web.Tests --filter ServiceCatalogTests`
Expected: FAIL — compile error or "type not found".

- [ ] **Step 3: Implement `ServiceItem.cs`**

```csharp
namespace ETD.Web.Models;

public sealed record ServiceItem(
    string Slug,
    string Title,
    string Icon,
    string ShortDescription,
    string LongDescription,
    IReadOnlyList<string> Bullets,
    bool HasPriceEstimator = false,
    bool IsTopTier = true);
```

- [ ] **Step 4: Implement `ServiceCatalog.cs`**

```csharp
using ETD.Web.Models;

namespace ETD.Web.Services;

public static class ServiceCatalog
{
    public static IReadOnlyList<ServiceItem> All { get; } = new ServiceItem[]
    {
        new("elektroinstallation", "Elektroinstallation", "⚡",
            "Alt- und Neubau, Sanierung, Verteilung, Stromkreise.",
            "Wir installieren saubere Elektroanlagen für Privat- und Gewerbekunden — vom Zählerschrank bis zur letzten Steckdose. Auch Sanierungen im Bestand führen wir staubarm und termingerecht durch.",
            new[] { "Zählerschrank & Unterverteilung", "Schalter, Steckdosen, Lampen", "Herd- und Wallbox-Anschluss", "Sanierung bewohnter Räume" }),

        new("smart-home-knx", "Smart Home & KNX", "🏠",
            "Lichtsteuerung, Heizung, Jalousien, Szenen — herstellerunabhängig.",
            "KNX ist der offene Standard für Gebäudeautomation. Wir planen, programmieren und montieren herstellerunabhängig — Sie sind nicht an eine App oder Cloud gebunden.",
            new[] { "Lichtsteuerung & Szenen", "Heizungs- und Jalousie-Steuerung", "Anwesenheits- & Energie-Reports", "Erweiterbar für 30+ Jahre" }),

        new("photovoltaik", "Photovoltaik & Speicher", "☀",
            "PV-Anlagen, Batteriespeicher, Energiemanagement.",
            "Wir planen und installieren PV-Anlagen mit und ohne Speicher. Anmeldung beim Netzbetreiber und Marktstammdatenregister übernehmen wir komplett.",
            new[] { "Dachflächen-Auslegung", "Batteriespeicher-Integration", "Anmeldung Netzbetreiber & MaStR", "Energiemanagement / EVU-Box" },
            HasPriceEstimator: true),

        new("wallbox", "E-Mobilität & Wallbox", "🔌",
            "Wallbox-Installation, Lastmanagement, Förderberatung.",
            "Vom 11 kW-Standard bis zur 22 kW-Lösung mit Lastmanagement. Wir kümmern uns um die elektrische Anbindung, Anmeldung und Förderfähigkeit.",
            new[] { "11 kW oder 22 kW", "Lastmanagement bei mehreren Boxen", "Anmeldung beim Netzbetreiber", "KfW-Förderberatung" },
            HasPriceEstimator: true),

        new("klimatechnik", "Klima- und Kältetechnik", "❄",
            "Klimaanlagen Privat & Gewerbe — zertifiziert nach §6 ChemKlimaschutzV.",
            "Wir installieren und warten Klimaanlagen für Wohn- und Geschäftsräume. Mit Sachkundezertifikat A1 und Betriebszertifizierung gemäß §6 ChemKlimaschutzV.",
            new[] { "Multi-Split & VRF-Systeme", "Wohnraum- und Gewerbekühlung", "Wartung & Dichtheitsprüfung", "Inbetriebnahme & Anmeldung" }),

        new("e-check", "E-Check / DGUV V3", "✓",
            "Gesetzlich vorgeschriebene Prüfung ortsfester und ortsveränderlicher Anlagen.",
            "Als Arbeitgeber sind Sie verpflichtet, elektrische Anlagen regelmäßig prüfen zu lassen. Wir führen E-Checks normgerecht durch und stellen das Prüfprotokoll aus.",
            new[] { "Ortsfeste Anlagen (DIN VDE 0100-600 / 0105-100)", "Ortsveränderliche Geräte (DGUV V3 § 5)", "Wiederholungsprüfungen", "Versicherungsrelevantes Protokoll" }),

        new("sicherheit", "Sicherheitstechnik", "🔒",
            "Alarmanlagen, Videoüberwachung, Brandmeldetechnik, Zutritt.",
            "Schutz für Haus, Hof und Betrieb. Wir planen vor-Ort, installieren und schulen Sie auf das System ein.",
            new[] { "Funk- und Hybrid-Alarmanlagen", "IP-Videoüberwachung (DSGVO-konform)", "Brandmeldetechnik nach DIN 14675", "Zutrittskontrolle" }),

        new("sat", "Sat- und Antennenanlagen", "📡",
            "Sat ZF, Unicable, SAT>IP — auch im Bestand.",
            "Klassische ZF mit Multischalter, Einkabel-Lösung im Bestand, oder SAT>IP fürs ganze Haus über das Netzwerk.",
            new[] { "Sat ZF mit Multischalter", "Unicable (Einkabel-Lösung)", "SAT>IP (TV über LAN/WLAN)", "Antennenmontage" },
            IsTopTier: false),

        new("beleuchtung", "Beleuchtung & LED", "💡",
            "Lichtplanung Wohn- und Gewerbe, LED-Sanierung, Außenbeleuchtung.",
            "Gute Beleuchtung ist mehr als hell — wir planen Lichtszenen, sanieren auf LED und sparen damit nachhaltig Stromkosten.",
            new[] { "Lichtplanung mit DIALux", "LED-Sanierung im Bestand", "Außen- und Fassadenbeleuchtung", "Notbeleuchtung / Fluchtweg" },
            IsTopTier: false),

        new("netzwerk", "Netzwerk & EDV", "🌐",
            "Strukturierte Verkabelung, WLAN, Patchschränke.",
            "CAT 6/7-Verkabelung für Neubau und Bestand. Patchfelder, WLAN-Ausleuchtung und Glasfaser-Vorbereitung aus einer Hand.",
            new[] { "CAT 6/7 strukturierte Verkabelung", "Patchfelder & Schränke", "WLAN-Ausleuchtung", "Glasfaser-Vorbereitung" },
            IsTopTier: false),

        new("sprechanlagen", "Türsprechanlagen", "📞",
            "Video-Türsprechanlagen, Ritto Twinbus, Briefkastenanlagen.",
            "Wir verbauen klassische Audio- und moderne Video-Sprechanlagen — auch nachrüstbar im Bestand.",
            new[] { "Ritto Twinbus", "Video-Türsprechanlagen", "Briefkastenanlagen", "Smartphone-Integration" },
            IsTopTier: false),
    };

    public static IReadOnlyList<ServiceItem> TopServices { get; } = All.Where(s => s.IsTopTier).ToList();

    public static IReadOnlyList<ServiceItem> SecondaryServices { get; } = All.Where(s => !s.IsTopTier).ToList();

    public static ServiceItem? FindBySlug(string slug) => All.FirstOrDefault(s => s.Slug == slug);
}
```

- [ ] **Step 5: Run tests — should pass**

Run: `dotnet test tests/ETD.Web.Tests --filter ServiceCatalogTests`
Expected: 3/3 passing.

- [ ] **Step 6: Commit**

```bash
git add src/ETD.Web/Models/ServiceItem.cs src/ETD.Web/Services/ServiceCatalog.cs tests/ETD.Web.Tests/ServiceCatalogTests.cs
git commit -m "feat: in-memory service catalog with 11 services"
```

---

## Phase 2 — Static content pages

### Task 2.1: Home page

**Files:**
- Modify: `src/ETD.Web/Components/Pages/Home.razor`
- Create: `src/ETD.Web/wwwroot/img/hero/home-hero.webp` (manually sourced from current site or CC0)

- [ ] **Step 1: Add hero image asset**

Place a 2400-px-wide WebP hero image at `src/ETD.Web/wwwroot/img/hero/home-hero.webp`. If no real image exists yet, use a CC0 placeholder from Unsplash (search "electrician working / circuit board / smart home / wallbox") and record its license in a sibling `home-hero.license.txt`.

- [ ] **Step 2: Replace `Home.razor`**

```razor
@page "/"
@using ETD.Web.Services
@using ETD.Web.Components.Shared

<PageTitle>ETD — Elektrotechnik Desch · Meisterbetrieb Biebergemünd</PageTitle>
<HeadContent>
    <meta name="description" content="Elektroinstallation, Smart Home, Photovoltaik, Wallbox und Klimatechnik aus einem Meisterbetrieb in Biebergemünd. Für Privat und Gewerbe." />
</HeadContent>

<HeroFullBleed
    Eyebrow="Meisterbetrieb · Biebergemünd"
    Title="Strom, der einfach"
    TitleAccent="läuft."
    Sub="Elektroinstallation, Smart Home, Klimatechnik und mehr — für Privat und Gewerbe. Sauber gemacht, beim ersten Mal."
    PrimaryCtaText="Angebot anfragen"
    PrimaryCtaHref="/angebot"
    SecondaryCtaText="Leistungen ansehen"
    SecondaryCtaHref="/leistungen"
    ImageSrc="img/hero/home-hero.webp"
    ImageAlt="Elektriker bei der Arbeit an einer Unterverteilung">
    <TrustStrip><TrustBar /></TrustStrip>
</HeroFullBleed>

<section class="container fade-in" style="padding:72px 0">
    <div class="eyebrow">Was wir machen</div>
    <h2 style="max-width:680px">Vom Lichtschalter bis zum Smart-Home — <em style="color:var(--red);font-style:normal">alles aus einer Hand.</em></h2>
    <p style="color:var(--muted);max-width:680px;margin-top:12px">Sieben Top-Leistungen für Privat- und Gewerbekunden. Plus Sat, Beleuchtung, Netzwerk und Türsprechanlagen.</p>

    <div style="display:grid;grid-template-columns:repeat(auto-fit, minmax(260px, 1fr));gap:18px;margin-top:36px">
        @foreach (var s in ServiceCatalog.TopServices)
        {
            <ServiceCard Icon="@s.Icon" Title="@s.Title" Description="@s.ShortDescription" Href="@($"/leistungen/{s.Slug}")" />
        }
    </div>
</section>

<section style="background:var(--white);padding:72px 0;border-top:1px solid var(--line);border-bottom:1px solid var(--line)">
    <div class="container fade-in">
        <div class="eyebrow">Warum ETD</div>
        <h2>Meisterbetrieb. <em style="color:var(--red);font-style:normal">Innung.</em> Zertifiziert.</h2>
        <div style="display:grid;grid-template-columns:repeat(auto-fit, minmax(220px, 1fr));gap:24px;margin-top:36px">
            <div class="card"><strong style="color:var(--red);font-size:24px;display:block">15+ Jahre</strong><span style="color:var(--muted);font-size:14px">Erfahrung in Elektro- und Kältetechnik</span></div>
            <div class="card"><strong style="color:var(--red);font-size:24px;display:block">Innung</strong><span style="color:var(--muted);font-size:14px">Mitglied der regionalen Innung</span></div>
            <div class="card"><strong style="color:var(--red);font-size:24px;display:block">Sachkunde A1</strong><span style="color:var(--muted);font-size:14px">+ Betriebszertifizierung §6 ChemKlimaschutzV</span></div>
            <div class="card"><strong style="color:var(--red);font-size:24px;display:block">Region MKK</strong><span style="color:var(--muted);font-size:14px">Biebergemünd & Umgebung</span></div>
        </div>
    </div>
</section>
```

- [ ] **Step 3: Run & visually verify**

Run: `dotnet run --project src/ETD.Web -- --urls http://localhost:5080`
Open `http://localhost:5080`. Expected: hero with dark background + image overlay + trust bar, service grid with 7 cards, "Why ETD" trust band, CTA banner, footer. Kill server.

- [ ] **Step 4: Commit**

```bash
git add src/ETD.Web/Components/Pages/Home.razor src/ETD.Web/wwwroot/img/
git commit -m "feat: home page with hero, service grid, trust band"
```

---

### Task 2.2: Leistungen overview page

**Files:**
- Create: `src/ETD.Web/Components/Pages/Leistungen.razor`

- [ ] **Step 1: Write the page**

```razor
@page "/leistungen"
@using ETD.Web.Services
@using ETD.Web.Components.Shared

<PageTitle>Leistungen — ETD Elektrotechnik Desch</PageTitle>
<HeadContent>
    <meta name="description" content="Alle Leistungen von Elektrotechnik Desch im Überblick: Elektroinstallation, Smart Home, Photovoltaik, Wallbox, Klimatechnik, E-Check, Sicherheitstechnik und mehr." />
</HeadContent>

<section class="container fade-in" style="padding:72px 0 24px">
    <div class="eyebrow">Leistungen</div>
    <h1 style="max-width:780px">Alle Leistungen von <em style="color:var(--red);font-style:normal">Elektrotechnik Desch</em></h1>
    <p style="color:var(--ink-2);max-width:680px;margin-top:14px;font-size:1.0625rem">Sieben Kernleistungen mit eigener Detailseite, plus vier weitere Leistungen unten als Direkt-Sprungmarken.</p>
</section>

<section class="container fade-in" style="padding-bottom:56px">
    <div style="display:grid;grid-template-columns:repeat(auto-fit, minmax(280px, 1fr));gap:18px">
        @foreach (var s in ServiceCatalog.TopServices)
        {
            <ServiceCard Icon="@s.Icon" Title="@s.Title" Description="@s.ShortDescription" Href="@($"/leistungen/{s.Slug}")" />
        }
    </div>
</section>

<section style="background:var(--white);padding:56px 0;border-top:1px solid var(--line)">
    <div class="container">
        <div class="eyebrow">Weitere Leistungen</div>
        <h2 style="margin-bottom:24px">Weitere Bereiche, in denen wir aktiv sind.</h2>
        @foreach (var s in ServiceCatalog.SecondaryServices)
        {
            <article id="@s.Slug" class="fade-in" style="padding:24px 0;border-top:1px solid var(--line)">
                <div style="display:flex;gap:24px;align-items:flex-start;flex-wrap:wrap">
                    <div style="font-size:36px;width:72px;flex-shrink:0">@s.Icon</div>
                    <div style="flex:1;min-width:240px">
                        <h3 style="font-size:1.375rem;margin-bottom:8px">@s.Title</h3>
                        <p style="color:var(--ink-2);margin-bottom:12px">@s.LongDescription</p>
                        <ul style="font-size:14px;color:var(--ink-2);list-style:none;padding:0">
                            @foreach (var b in s.Bullets)
                            {
                                <li style="padding:3px 0 3px 18px;position:relative">
                                    <span style="position:absolute;left:0;color:var(--red);font-weight:800">▸</span>@b
                                </li>
                            }
                        </ul>
                    </div>
                    <div>
                        <a href="/angebot" class="btn btn-primary">Anfragen →</a>
                    </div>
                </div>
            </article>
        }
    </div>
</section>
```

- [ ] **Step 2: Verify** — start server, open `/leistungen`, check top grid + each anchor (`#sat`, `#beleuchtung`, `#netzwerk`, `#sprechanlagen`) navigates correctly.

- [ ] **Step 3: Commit**

```bash
git add src/ETD.Web/Components/Pages/Leistungen.razor
git commit -m "feat: leistungen overview page with anchor sections"
```

---

### Task 2.3: Service detail page (data-driven, one component for all 7)

**Files:**
- Create: `src/ETD.Web/Components/Shared/ServiceDetailHero.razor`
- Create: `src/ETD.Web/Components/Pages/ServiceDetail.razor`

- [ ] **Step 1: Detail hero component**

```razor
@code {
    [Parameter, EditorRequired] public string Eyebrow { get; set; } = "";
    [Parameter, EditorRequired] public string Title { get; set; } = "";
    [Parameter, EditorRequired] public string Sub { get; set; } = "";
}

<section class="hero" style="min-height:380px">
    <div class="container">
        <div class="hero-content fade-in" style="padding:80px var(--gutter) 56px">
            <div class="eyebrow" style="color:var(--red)">@Eyebrow</div>
            <h1>@Title</h1>
            <p style="font-size:1.0625rem;color:#D1D5DB;max-width:560px;margin:14px 0 0">@Sub</p>
        </div>
    </div>
</section>
```

- [ ] **Step 2: ServiceDetail.razor**

```razor
@page "/leistungen/{Slug}"
@using ETD.Web.Models
@using ETD.Web.Services
@using ETD.Web.Components.Shared

@code {
    [Parameter] public string Slug { get; set; } = "";
    private ServiceItem? service;

    protected override void OnParametersSet()
    {
        service = ServiceCatalog.FindBySlug(Slug);
    }
}

@if (service is null)
{
    <section class="container" style="padding:96px 0">
        <h1>Seite nicht gefunden</h1>
        <p style="margin-top:12px"><a href="/leistungen">Zurück zur Übersicht</a></p>
    </section>
}
else
{
    <PageTitle>@service.Title — ETD Elektrotechnik Desch</PageTitle>
    <HeadContent>
        <meta name="description" content="@service.ShortDescription" />
    </HeadContent>

    <ServiceDetailHero Eyebrow="Leistung" Title="@service.Title" Sub="@service.LongDescription" />

    <section class="container fade-in" style="padding:64px 0">
        <div style="display:grid;grid-template-columns:1.5fr 1fr;gap:48px;align-items:start">
            <div>
                <div class="eyebrow">Was Sie bekommen</div>
                <h2 style="margin-bottom:18px">Inkl. <em style="color:var(--red);font-style:normal">aller Schritte</em>, ohne Wenn und Aber.</h2>
                <ul style="font-size:1.0625rem;list-style:none;padding:0">
                    @foreach (var b in service.Bullets)
                    {
                        <li style="padding:10px 0 10px 28px;position:relative;border-top:1px solid var(--line)">
                            <span style="position:absolute;left:0;top:11px;color:var(--red);font-weight:800;font-size:1.25rem">✓</span>@b
                        </li>
                    }
                </ul>
            </div>
            <aside class="card" style="position:sticky;top:88px">
                <div class="eyebrow">So geht's weiter</div>
                <h3 style="margin-bottom:12px">Kostenfreies Angebot anfragen</h3>
                <p style="color:var(--muted);font-size:14px;margin-bottom:18px">Beschreiben Sie uns Ihr Projekt — Sie hören innerhalb 24 h von uns.</p>
                <a href="/angebot" class="btn btn-primary" style="width:100%;justify-content:center">Angebot anfragen →</a>
                <p style="margin-top:14px;font-size:13px;color:var(--muted);text-align:center">oder anrufen: <a href="tel:+4960509062874" style="font-weight:700">06050 9062874</a></p>
            </aside>
        </div>
    </section>
}
```

- [ ] **Step 3: Verify all 7 slugs work**

Run server. Open in turn: `/leistungen/elektroinstallation`, `/leistungen/smart-home-knx`, `/leistungen/photovoltaik`, `/leistungen/wallbox`, `/leistungen/klimatechnik`, `/leistungen/e-check`, `/leistungen/sicherheit`. Each should render with its own title and bullets.

- [ ] **Step 4: Commit**

```bash
git add src/ETD.Web/Components/Pages/ServiceDetail.razor src/ETD.Web/Components/Shared/ServiceDetailHero.razor
git commit -m "feat: data-driven service detail page for all 7 top services"
```

---

### Task 2.4: Gewerbe page

**Files:**
- Create: `src/ETD.Web/Components/Pages/Gewerbe.razor`

- [ ] **Step 1: Write the page**

```razor
@page "/gewerbe"
@using ETD.Web.Components.Shared

<PageTitle>Gewerbe & Industrie — ETD</PageTitle>
<HeadContent>
    <meta name="description" content="Wartung, E-Check, Schaltanlagen und Maschinenanschluss für Gewerbe und Industrie aus dem Meisterbetrieb Elektrotechnik Desch." />
</HeadContent>

<ServiceDetailHero
    Eyebrow="Für Ihren Betrieb"
    Title="Gewerbe & Industrie."
    Sub="Wartungsverträge, E-Check, Schaltanlagen, Maschinenanschluss — wir halten Ihre Produktion am Laufen." />

<section class="container fade-in" style="padding:64px 0">
    <div style="display:grid;grid-template-columns:repeat(auto-fit, minmax(280px, 1fr));gap:24px">
        <div class="card">
            <div style="font-size:32px;margin-bottom:12px">🔧</div>
            <h3>Wartungsverträge</h3>
            <p style="color:var(--muted);font-size:14px;margin-top:8px">Regelmäßige Wartung Ihrer Elektro- und Klimaanlagen. Festpreis, planbar, mit Reaktionszeit-Garantie.</p>
        </div>
        <div class="card">
            <div style="font-size:32px;margin-bottom:12px">⚙</div>
            <h3>Schalt- und Maschinenanschluss</h3>
            <p style="color:var(--muted);font-size:14px;margin-top:8px">Aufbau und Anschluss von Schaltanlagen, Maschinen und Produktionslinien. CE-konforme Dokumentation.</p>
        </div>
        <div class="card">
            <div style="font-size:32px;margin-bottom:12px">✓</div>
            <h3>E-Check / DGUV V3</h3>
            <p style="color:var(--muted);font-size:14px;margin-top:8px">Pflichtprüfungen ortsfester und ortsveränderlicher Anlagen — fristgerecht und mit Versicherungs-relevantem Protokoll.</p>
        </div>
        <div class="card">
            <div style="font-size:32px;margin-bottom:12px">🚨</div>
            <h3>Notdienst für Betriebe</h3>
            <p style="color:var(--muted);font-size:14px;margin-top:8px">Wir kommen schnell, wenn Ihre Anlage stillsteht. Für Wartungskunden Reaktionszeit-Garantie.</p>
        </div>
    </div>
</section>
```

- [ ] **Step 2: Commit**

```bash
git add src/ETD.Web/Components/Pages/Gewerbe.razor
git commit -m "feat: gewerbe page"
```

---

### Task 2.5: Notdienst, Referenzen, ÜberUns, Kontakt pages

**Files:**
- Create: `src/ETD.Web/Components/Pages/Notdienst.razor`
- Create: `src/ETD.Web/Components/Pages/Referenzen.razor`
- Create: `src/ETD.Web/Components/Pages/UeberUns.razor`
- Create: `src/ETD.Web/Components/Pages/Kontakt.razor`

- [ ] **Step 1: Notdienst.razor**

```razor
@page "/notdienst"

<PageTitle>Notdienst — ETD</PageTitle>

<section class="hero" style="min-height:480px">
    <div class="container">
        <div class="hero-content fade-in" style="padding:96px var(--gutter) 56px">
            <div class="eyebrow" style="color:var(--red)">Notdienst · 24/7 *</div>
            <h1>Strom weg.<br><em style="color:var(--red);font-style:normal">Wir kommen.</em></h1>
            <p style="font-size:1.125rem;color:#D1D5DB;max-width:540px;margin:16px 0 28px">Bei akuten Störungen erreichen Sie uns direkt am Telefon. Wartungskunden haben Reaktionszeit-Garantie.</p>
            <a href="tel:+4960509062874" class="btn btn-primary" style="font-size:1.125rem;padding:16px 28px">📞 06050 9062874 anrufen</a>
            <p style="font-size:13px;color:#9CA3AF;margin-top:32px">* außerhalb der Geschäftszeiten Rückruf garantiert — keine 0900-Nummer, keine versteckten Gebühren.</p>
        </div>
    </div>
</section>
```

- [ ] **Step 2: Referenzen.razor (Holzhaus reference + placeholder grid)**

```razor
@page "/referenzen"
@using ETD.Web.Components.Shared

<PageTitle>Referenzen — ETD</PageTitle>

<ServiceDetailHero Eyebrow="Projekte" Title="Referenzen." Sub="Ein Auszug aus unseren Kundenanlagen — vom Einfamilienhaus bis zum Holzhaus in Blockbohlenbauweise." />

<section class="container fade-in" style="padding:64px 0">
    <div style="display:grid;grid-template-columns:repeat(auto-fit, minmax(280px, 1fr));gap:24px">
        <article class="card" style="overflow:hidden;padding:0">
            <div style="aspect-ratio:4/3;background:linear-gradient(135deg, #1a1f2a, #0e1116);display:flex;align-items:center;justify-content:center;color:#fca5a5;font-size:64px">🏡</div>
            <div style="padding:20px">
                <div class="eyebrow">Featured Case</div>
                <h3>Holzhaus in Blockbohlenbauweise</h3>
                <p style="color:var(--muted);font-size:14px;margin-top:8px">Komplette Elektroinstallation in einem Holzhaus mit besonderen Anforderungen an Leitungsführung und Brandschutz.</p>
            </div>
        </article>
        @for (int i = 0; i < 5; i++)
        {
            <article class="card" style="overflow:hidden;padding:0">
                <div style="aspect-ratio:4/3;background:linear-gradient(135deg, #f3f4f6, #e5e7eb);display:flex;align-items:center;justify-content:center;color:var(--muted);font-size:14px">Weitere Projekte folgen</div>
                <div style="padding:20px">
                    <h3 style="font-size:1rem;color:var(--muted)">Projekt-Slot @(i + 2)</h3>
                </div>
            </article>
        }
    </div>
</section>
```

- [ ] **Step 3: UeberUns.razor**

```razor
@page "/ueber-uns"
@using ETD.Web.Components.Shared

<PageTitle>Über uns — ETD</PageTitle>

<ServiceDetailHero Eyebrow="Über uns" Title="Christoph Desch · Meister­betrieb." Sub="Seit Jahren als zuverlässiger Partner für Privat- und Gewerbekunden in der Region Main-Kinzig-Kreis." />

<section class="container fade-in" style="padding:64px 0">
    <div style="display:grid;grid-template-columns:1fr 2fr;gap:48px;align-items:start">
        <div class="card" style="text-align:center">
            <div style="width:140px;height:140px;background:var(--red);color:#fff;border-radius:50%;display:flex;align-items:center;justify-content:center;margin:0 auto 16px;font-size:48px;font-weight:900;box-shadow:var(--shadow-red)">CD</div>
            <h3>me. Christoph Desch</h3>
            <p style="color:var(--muted);font-size:14px;margin-top:6px">Inhaber, Elektrotechniker­meister</p>
        </div>
        <div>
            <div class="eyebrow">Qualifikationen</div>
            <h2 style="margin-bottom:18px">Zertifikate und <em style="color:var(--red);font-style:normal">Mitgliedschaften.</em></h2>
            <ul style="list-style:none;padding:0;font-size:1.0625rem">
                <li style="padding:12px 0 12px 28px;position:relative;border-top:1px solid var(--line)"><span style="position:absolute;left:0;top:14px;color:var(--red);font-weight:800">✓</span>Elektrotechniker­meister (HWK)</li>
                <li style="padding:12px 0 12px 28px;position:relative;border-top:1px solid var(--line)"><span style="position:absolute;left:0;top:14px;color:var(--red);font-weight:800">✓</span>Innungsmitglied</li>
                <li style="padding:12px 0 12px 28px;position:relative;border-top:1px solid var(--line)"><span style="position:absolute;left:0;top:14px;color:var(--red);font-weight:800">✓</span>Sachkundezertifikat A1 (Kältetechnik)</li>
                <li style="padding:12px 0 12px 28px;position:relative;border-top:1px solid var(--line)"><span style="position:absolute;left:0;top:14px;color:var(--red);font-weight:800">✓</span>Betriebszertifizierung gemäß §6 ChemKlimaschutzV</li>
            </ul>
        </div>
    </div>
</section>
```

- [ ] **Step 4: Kontakt.razor**

```razor
@page "/kontakt"
@using ETD.Web.Components.Shared

<PageTitle>Kontakt — ETD</PageTitle>

<ServiceDetailHero Eyebrow="Kontakt" Title="So erreichen Sie uns." Sub="Direkter Draht zum Meisterbetrieb — Anruf, E-Mail oder Angebotsanfrage." />

<section class="container fade-in" style="padding:64px 0">
    <div style="display:grid;grid-template-columns:1.5fr 1fr;gap:48px">
        <div>
            <div class="eyebrow">Adresse</div>
            <h2 style="margin-bottom:18px">Elektrotechnik Desch</h2>
            <p style="font-size:1.0625rem;line-height:1.8">
                Von-Cancrin-Straße 22<br/>
                63599 Biebergemünd<br/>
                Hessen, Deutschland
            </p>
            <div style="margin-top:32px;display:grid;gap:16px">
                <a href="tel:+4960509062874" class="btn btn-primary" style="justify-content:flex-start">📞 06050 9062874</a>
                <a href="mailto:mail@ElektroTechnikDesch.de" class="btn btn-ghost" style="justify-content:flex-start">✉ mail@ElektroTechnikDesch.de</a>
                <a href="/angebot" class="btn btn-ghost" style="justify-content:flex-start">📝 Angebot anfragen</a>
            </div>
        </div>
        <div>
            <div class="card" style="padding:0;overflow:hidden">
                <a href="https://www.openstreetmap.org/?mlat=50.1583&amp;mlon=9.3556#map=15/50.1583/9.3556" target="_blank" rel="noopener" style="display:block">
                    <img src="img/map-static.png" alt="Karte Biebergemünd" style="width:100%;display:block" loading="lazy" />
                </a>
                <div style="padding:16px;font-size:13px;color:var(--muted);text-align:center">Klick öffnet OpenStreetMap in neuem Tab</div>
            </div>
        </div>
    </div>
</section>
```

- [ ] **Step 5: Map image asset**

Generate a static map of Biebergemünd via OpenStreetMap export (https://www.openstreetmap.org/export) or the staticmap PHP renderer. Save as `src/ETD.Web/wwwroot/img/map-static.png` (1200 × 800). Record license in `map-static.license.txt`.

- [ ] **Step 6: Commit**

```bash
git add src/ETD.Web/Components/Pages/Notdienst.razor src/ETD.Web/Components/Pages/Referenzen.razor src/ETD.Web/Components/Pages/UeberUns.razor src/ETD.Web/Components/Pages/Kontakt.razor src/ETD.Web/wwwroot/img/
git commit -m "feat: notdienst, referenzen, ueber-uns, kontakt pages"
```

---

### Task 2.6: Legal pages — Impressum, Datenschutz, Cookies

**Files:**
- Create: `src/ETD.Web/Components/Pages/Impressum.razor`
- Create: `src/ETD.Web/Components/Pages/Datenschutz.razor`
- Create: `src/ETD.Web/Components/Pages/Cookies.razor`

- [ ] **Step 1: Impressum.razor**

```razor
@page "/impressum"

<PageTitle>Impressum — ETD</PageTitle>

<section class="container fade-in" style="padding:64px 0;max-width:760px">
    <div class="eyebrow">Pflichtangaben</div>
    <h1>Impressum</h1>

    <h2 style="margin-top:32px">Angaben gemäß § 5 TMG</h2>
    <p style="margin-top:12px">
        Elektrotechnik Desch<br/>
        Inhaber: me. Christoph Desch<br/>
        Von-Cancrin-Straße 22<br/>
        63599 Biebergemünd
    </p>

    <h2 style="margin-top:24px">Kontakt</h2>
    <p style="margin-top:12px">
        Telefon: <a href="tel:+4960509062874">+49 6050 9062874</a><br/>
        E-Mail: <a href="mailto:mail@ElektroTechnikDesch.de">mail@ElektroTechnikDesch.de</a>
    </p>

    <h2 style="margin-top:24px">Umsatzsteuer-ID</h2>
    <p style="margin-top:12px">
        Umsatzsteuer-Identifikationsnummer gemäß § 27 a Umsatzsteuergesetz:<br/>
        DE330546558
    </p>

    <h2 style="margin-top:24px">Steuernummer</h2>
    <p style="margin-top:12px">019 811 62300</p>

    <h2 style="margin-top:24px">Berufsbezeichnung und berufsrechtliche Regelungen</h2>
    <p style="margin-top:12px">
        Berufsbezeichnung: Elektrotechnikermeister (verliehen in der Bundesrepublik Deutschland)<br/>
        Zuständige Kammer: Handwerkskammer Wiesbaden<br/>
        Es gelten die Vorschriften der Handwerksordnung (HwO) und der zugehörigen Handwerksverordnung.
    </p>

    <h2 style="margin-top:24px">Verantwortlich für den Inhalt nach § 18 Abs. 2 MStV</h2>
    <p style="margin-top:12px">me. Christoph Desch (Anschrift wie oben)</p>

    <h2 style="margin-top:24px">EU-Streitschlichtung</h2>
    <p style="margin-top:12px">
        Die Europäische Kommission stellt eine Plattform zur Online-Streitbeilegung (OS) bereit:
        <a href="https://ec.europa.eu/consumers/odr" target="_blank" rel="noopener">https://ec.europa.eu/consumers/odr</a>.
        Wir sind nicht bereit oder verpflichtet, an Streitbeilegungsverfahren vor einer Verbraucher­schlichtungsstelle teilzunehmen.
    </p>

    <h2 style="margin-top:24px">Haftung für Inhalte</h2>
    <p style="margin-top:12px">
        Als Diensteanbieter sind wir gemäß § 7 Abs. 1 TMG für eigene Inhalte auf diesen Seiten nach den allgemeinen Gesetzen verantwortlich.
        Nach den §§ 8 bis 10 TMG sind wir als Diensteanbieter jedoch nicht verpflichtet, übermittelte oder gespeicherte fremde Informationen zu überwachen.
    </p>
</section>
```

- [ ] **Step 2: Datenschutz.razor**

```razor
@page "/datenschutz"

<PageTitle>Datenschutzerklärung — ETD</PageTitle>

<section class="container fade-in" style="padding:64px 0;max-width:760px">
    <div class="eyebrow">Datenschutz</div>
    <h1>Datenschutzerklärung</h1>
    <p style="color:var(--muted);margin-top:8px">Stand: @DateTime.UtcNow.ToString("MMMM yyyy", new System.Globalization.CultureInfo("de-DE"))</p>

    <h2 style="margin-top:32px">1. Verantwortlicher</h2>
    <p style="margin-top:12px">
        Elektrotechnik Desch<br/>
        me. Christoph Desch<br/>
        Von-Cancrin-Straße 22, 63599 Biebergemünd<br/>
        E-Mail: <a href="mailto:mail@ElektroTechnikDesch.de">mail@ElektroTechnikDesch.de</a>
    </p>

    <h2 style="margin-top:24px">2. Hosting</h2>
    <p style="margin-top:12px">Diese Website wird auf eigenen Kubernetes-Servern im Rechenzentrum von CIVO Ltd. in Frankfurt am Main, Deutschland gehostet. Die Daten verlassen die Europäische Union nicht.</p>

    <h2 style="margin-top:24px">3. Server-Logfiles</h2>
    <p style="margin-top:12px">Der Server erfasst beim Aufruf automatisch HTTP-Zugriffsdaten (Datum, Pfad, Statuscode, User-Agent, gekürzte IP-Adresse). Die IP-Adresse wird vor Speicherung auf <code>/24</code> gekürzt; ein Personenbezug ist daher nicht herstellbar. Rechtsgrundlage: Art. 6 Abs. 1 lit. f DSGVO (berechtigtes Interesse an IT-Sicherheit).</p>

    <h2 style="margin-top:24px">4. Angebotsanfrage / Kontaktformular</h2>
    <p style="margin-top:12px">Wenn Sie eine Angebotsanfrage oder eine Nachricht an uns senden, werden die Inhalte zur Bearbeitung Ihres Anliegens per E-Mail an <code>mail@ElektroTechnikDesch.de</code> versandt. Eine Speicherung in einer Datenbank findet nicht statt. Die Übertragung erfolgt verschlüsselt (TLS). Rechtsgrundlage: Art. 6 Abs. 1 lit. b DSGVO (Vertragsanbahnung) bzw. lit. f (berechtigtes Interesse an Kommunikation).</p>

    <h2 style="margin-top:24px">5. Cookies und lokale Speicherung</h2>
    <p style="margin-top:12px">Wir setzen keine Tracking- oder Werbecookies ein. Für den Angebots-Wizard speichern wir Zwischenstände ausschließlich im <strong>localStorage</strong> Ihres Browsers (Schlüssel <code>etd.wizard.v1</code>). Diese Daten werden nicht an unseren Server übertragen und können jederzeit durch das Löschen Ihrer Browserdaten entfernt werden. Da dies funktional unverzichtbar ist, ist eine Einwilligung nach § 25 Abs. 2 Nr. 2 TTDSG nicht erforderlich.</p>

    <h2 style="margin-top:24px">6. Schriften</h2>
    <p style="margin-top:12px">Wir verwenden die Schriftart „Inter" (SIL OFL), die direkt vom Server dieser Website ausgeliefert wird. Es findet keine Verbindung zu Google Fonts oder anderen Drittanbieter-CDNs statt.</p>

    <h2 style="margin-top:24px">7. Karte</h2>
    <p style="margin-top:12px">Auf der Kontaktseite zeigen wir ein statisches Kartenbild. Beim Klick auf das Bild öffnen Sie OpenStreetMap in einem neuen Tab — erst dann findet eine Datenübertragung an Dritte statt.</p>

    <h2 style="margin-top:24px">8. Ihre Rechte</h2>
    <p style="margin-top:12px">Sie haben jederzeit das Recht auf Auskunft (Art. 15), Berichtigung (Art. 16), Löschung (Art. 17), Einschränkung (Art. 18), Datenübertragbarkeit (Art. 20) und Widerspruch (Art. 21) gemäß DSGVO sowie das Recht auf Beschwerde bei einer Aufsichtsbehörde (Art. 77). Wenden Sie sich an die oben genannten Kontaktdaten.</p>
</section>
```

- [ ] **Step 3: Cookies.razor**

```razor
@page "/cookies"

<PageTitle>Cookies — ETD</PageTitle>

<section class="container fade-in" style="padding:64px 0;max-width:760px">
    <div class="eyebrow">Datenschutz</div>
    <h1>Cookies und lokale Speicherung</h1>

    <p style="margin-top:16px;font-size:1.0625rem">Diese Website setzt <strong>keine Cookies</strong>. Wir nutzen ausschließlich die lokale Speicherung Ihres Browsers (<code>localStorage</code>), um Zwischenstände des Angebots-Wizards bei Ihnen zu speichern — diese Daten werden nicht an uns übertragen.</p>

    <h2 style="margin-top:32px">Was wird gespeichert?</h2>
    <ul style="margin-top:12px;padding-left:20px">
        <li><strong>Schlüssel:</strong> <code>etd.wizard.v1</code></li>
        <li><strong>Inhalt:</strong> Ihre Eingaben aus dem Angebots-Wizard, bis Sie abschicken oder abbrechen</li>
        <li><strong>Lebensdauer:</strong> bis zum Absenden oder bis Sie die Browser-Daten löschen</li>
    </ul>

    <h2 style="margin-top:32px">Daten löschen</h2>
    <p style="margin-top:12px">In den meisten Browsern können Sie unter <em>Einstellungen → Datenschutz → Browserdaten löschen</em> die Website-Daten von <code>elektrotechnikdesch.de</code> manuell entfernen.</p>

    <p style="margin-top:32px;padding:16px 18px;background:var(--white);border-left:4px solid var(--red);border-radius:8px;font-size:14px;color:var(--muted)">Da keine Tracking-Technologien zum Einsatz kommen, ist eine Einwilligung nach § 25 Abs. 2 TTDSG nicht erforderlich. Sollten wir künftig Funktionen einbauen, die eine Einwilligung benötigen, ergänzen wir hier einen Consent-Banner.</p>
</section>
```

- [ ] **Step 4: Commit**

```bash
git add src/ETD.Web/Components/Pages/Impressum.razor src/ETD.Web/Components/Pages/Datenschutz.razor src/ETD.Web/Components/Pages/Cookies.razor
git commit -m "feat: legal pages (Impressum, Datenschutz, Cookies)"
```

---

### Task 2.7: 404 page + sitemap.xml + robots.txt

**Files:**
- Create: `src/ETD.Web/Components/Pages/NotFound.razor`
- Modify: `src/ETD.Web/Components/Routes.razor`
- Create: `src/ETD.Web/wwwroot/robots.txt`
- Create: `src/ETD.Web/wwwroot/sitemap.xml`

- [ ] **Step 1: NotFound.razor**

```razor
@page "/error"

<PageTitle>Nicht gefunden — ETD</PageTitle>

<section class="container fade-in" style="padding:96px 0;text-align:center">
    <h1 style="font-size:4rem">404</h1>
    <p style="font-size:1.125rem;color:var(--muted);margin-top:8px">Die Seite, die Sie suchen, existiert nicht.</p>
    <div style="margin-top:24px">
        <a href="/" class="btn btn-primary">Zur Startseite</a>
    </div>
</section>
```

- [ ] **Step 2: Wire 404 into the router**

Edit `src/ETD.Web/Components/Routes.razor`:

```razor
<Router AppAssembly="typeof(Program).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
    <NotFound>
        <LayoutView Layout="typeof(Layout.MainLayout)">
            <PageTitle>Nicht gefunden — ETD</PageTitle>
            <section class="container" style="padding:96px 0;text-align:center">
                <h1 style="font-size:4rem">404</h1>
                <p style="font-size:1.125rem;color:var(--muted);margin-top:8px">Die Seite existiert nicht.</p>
                <a href="/" class="btn btn-primary" style="margin-top:24px">Zur Startseite</a>
            </section>
        </LayoutView>
    </NotFound>
</Router>
```

- [ ] **Step 3: robots.txt**

```
User-agent: *
Allow: /
Sitemap: https://www.elektrotechnikdesch.de/sitemap.xml
```

- [ ] **Step 4: sitemap.xml**

```xml
<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
  <url><loc>https://www.elektrotechnikdesch.de/</loc><changefreq>monthly</changefreq><priority>1.0</priority></url>
  <url><loc>https://www.elektrotechnikdesch.de/leistungen</loc><changefreq>monthly</changefreq><priority>0.9</priority></url>
  <url><loc>https://www.elektrotechnikdesch.de/leistungen/elektroinstallation</loc><changefreq>monthly</changefreq><priority>0.8</priority></url>
  <url><loc>https://www.elektrotechnikdesch.de/leistungen/smart-home-knx</loc><changefreq>monthly</changefreq><priority>0.8</priority></url>
  <url><loc>https://www.elektrotechnikdesch.de/leistungen/photovoltaik</loc><changefreq>monthly</changefreq><priority>0.8</priority></url>
  <url><loc>https://www.elektrotechnikdesch.de/leistungen/wallbox</loc><changefreq>monthly</changefreq><priority>0.8</priority></url>
  <url><loc>https://www.elektrotechnikdesch.de/leistungen/klimatechnik</loc><changefreq>monthly</changefreq><priority>0.8</priority></url>
  <url><loc>https://www.elektrotechnikdesch.de/leistungen/e-check</loc><changefreq>monthly</changefreq><priority>0.8</priority></url>
  <url><loc>https://www.elektrotechnikdesch.de/leistungen/sicherheit</loc><changefreq>monthly</changefreq><priority>0.8</priority></url>
  <url><loc>https://www.elektrotechnikdesch.de/gewerbe</loc><changefreq>monthly</changefreq><priority>0.8</priority></url>
  <url><loc>https://www.elektrotechnikdesch.de/notdienst</loc><changefreq>monthly</changefreq><priority>0.9</priority></url>
  <url><loc>https://www.elektrotechnikdesch.de/referenzen</loc><changefreq>monthly</changefreq><priority>0.6</priority></url>
  <url><loc>https://www.elektrotechnikdesch.de/ueber-uns</loc><changefreq>yearly</changefreq><priority>0.6</priority></url>
  <url><loc>https://www.elektrotechnikdesch.de/angebot</loc><changefreq>monthly</changefreq><priority>1.0</priority></url>
  <url><loc>https://www.elektrotechnikdesch.de/kontakt</loc><changefreq>yearly</changefreq><priority>0.7</priority></url>
</urlset>
```

- [ ] **Step 5: Commit**

```bash
git add src/ETD.Web/Components/Routes.razor src/ETD.Web/wwwroot/robots.txt src/ETD.Web/wwwroot/sitemap.xml
git commit -m "feat: 404 page, sitemap.xml, robots.txt"
```

---

## Phase 3 — Quote wizard

### Task 3.1: QuoteRequest model + validation tests

**Files:**
- Create: `src/ETD.Web/Models/QuoteRequest.cs`
- Create: `tests/ETD.Web.Tests/QuoteRequestValidationTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
using System.ComponentModel.DataAnnotations;
using ETD.Web.Models;

namespace ETD.Web.Tests;

public class QuoteRequestValidationTests
{
    private static IList<ValidationResult> Validate(QuoteRequest q)
    {
        var ctx = new ValidationContext(q);
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(q, ctx, errors, validateAllProperties: true);
        return errors;
    }

    [Test]
    public async Task Empty_NameFails()
    {
        var q = new QuoteRequest { Email = "x@y.de", Phone = "060509", Audience = "privat", Services = new() { "wallbox" }, ConsentPrivacy = true };
        var errors = Validate(q);
        await Assert.That(errors.Any(e => e.MemberNames.Contains(nameof(QuoteRequest.Name)))).IsTrue();
    }

    [Test]
    public async Task Invalid_EmailFails()
    {
        var q = new QuoteRequest { Name = "Max", Email = "broken", Phone = "060509", Audience = "privat", Services = new() { "wallbox" }, ConsentPrivacy = true };
        var errors = Validate(q);
        await Assert.That(errors.Any(e => e.MemberNames.Contains(nameof(QuoteRequest.Email)))).IsTrue();
    }

    [Test]
    public async Task Missing_ConsentFails()
    {
        var q = new QuoteRequest { Name = "Max", Email = "x@y.de", Phone = "060509", Audience = "privat", Services = new() { "wallbox" }, ConsentPrivacy = false };
        var errors = Validate(q);
        await Assert.That(errors.Any(e => e.MemberNames.Contains(nameof(QuoteRequest.ConsentPrivacy)))).IsTrue();
    }

    [Test]
    public async Task Valid_Request_HasNoErrors()
    {
        var q = new QuoteRequest { Name = "Max", Email = "x@y.de", Phone = "060509", Audience = "privat", Services = new() { "wallbox" }, ConsentPrivacy = true };
        await Assert.That(Validate(q).Count).IsEqualTo(0);
    }
}
```

- [ ] **Step 2: Run — fails because QuoteRequest doesn't exist**

Run: `dotnet test tests/ETD.Web.Tests --filter QuoteRequestValidationTests`
Expected: compile error.

- [ ] **Step 3: Implement `QuoteRequest.cs`**

```csharp
using System.ComponentModel.DataAnnotations;

namespace ETD.Web.Models;

public sealed class QuoteRequest
{
    [Required(ErrorMessage = "Bitte mindestens eine Leistung auswählen.")]
    [MinLength(1, ErrorMessage = "Bitte mindestens eine Leistung auswählen.")]
    public List<string> Services { get; set; } = new();

    [Required, RegularExpression("^(privat|gewerbe)$", ErrorMessage = "Bitte Privat oder Gewerbe wählen.")]
    public string Audience { get; set; } = "privat";

    [StringLength(500)]
    public string? Notes { get; set; }

    public int? WallboxKw { get; set; }
    public int? WallboxDistanceMeters { get; set; }
    public int? PvAreaSqm { get; set; }
    public bool? PvWithStorage { get; set; }

    [Required, RegularExpression("^(asap|2w|4w|flex)$")]
    public string Timeframe { get; set; } = "flex";

    [Required, RegularExpression(@"^\d{5}$", ErrorMessage = "Bitte 5-stellige PLZ eingeben.")]
    public string Plz { get; set; } = "";

    [Required, StringLength(80)]
    public string City { get; set; } = "";

    [Required, StringLength(80, MinimumLength = 2, ErrorMessage = "Bitte Vor- und Nachname eingeben.")]
    public string Name { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, Phone]
    public string Phone { get; set; } = "";

    [Range(typeof(bool), "true", "true", ErrorMessage = "Bitte Datenschutzhinweis bestätigen.")]
    public bool ConsentPrivacy { get; set; }

    // Honeypot — must be empty
    public string? CompanyName2 { get; set; }
}
```

- [ ] **Step 4: Run tests — pass**

Run: `dotnet test tests/ETD.Web.Tests --filter QuoteRequestValidationTests`
Expected: 4/4 passing.

- [ ] **Step 5: Commit**

```bash
git add src/ETD.Web/Models/QuoteRequest.cs tests/ETD.Web.Tests/QuoteRequestValidationTests.cs
git commit -m "feat: QuoteRequest model with DataAnnotation validation"
```

---

### Task 3.2: WizardStateProtector (data-protection wrapped serialization)

**Files:**
- Create: `src/ETD.Web/Services/WizardStateProtector.cs`
- Create: `tests/ETD.Web.Tests/WizardStateProtectorTests.cs`

- [ ] **Step 1: Failing tests**

```csharp
using ETD.Web.Models;
using ETD.Web.Services;
using Microsoft.AspNetCore.DataProtection;

namespace ETD.Web.Tests;

public class WizardStateProtectorTests
{
    private static WizardStateProtector NewProtector()
    {
        var dp = DataProtectionProvider.Create("test");
        return new WizardStateProtector(dp);
    }

    [Test]
    public async Task Protect_Then_Unprotect_Roundtrips()
    {
        var p = NewProtector();
        var q = new QuoteRequest { Audience = "gewerbe", Plz = "63599", City = "Biebergemünd", Name = "Max", Email = "x@y.de", Phone = "06050", ConsentPrivacy = true, Services = { "wallbox" } };
        var token = p.Protect(q);
        var q2 = p.Unprotect(token);
        await Assert.That(q2!.Audience).IsEqualTo("gewerbe");
        await Assert.That(q2.Services).Contains("wallbox");
    }

    [Test]
    public async Task Unprotect_TamperedToken_ReturnsNull()
    {
        var p = NewProtector();
        var q = new QuoteRequest { Name = "Max", Email = "x@y.de", Phone = "060", ConsentPrivacy = true, Services = { "wallbox" }, Plz = "63599", City = "B" };
        var token = p.Protect(q);
        var tampered = token[..^4] + "AAAA";
        await Assert.That(p.Unprotect(tampered)).IsNull();
    }
}
```

- [ ] **Step 2: Implement**

```csharp
using System.Text.Json;
using ETD.Web.Models;
using Microsoft.AspNetCore.DataProtection;

namespace ETD.Web.Services;

public sealed class WizardStateProtector
{
    private readonly IDataProtector protector;

    public WizardStateProtector(IDataProtectionProvider provider)
    {
        protector = provider.CreateProtector("ETD.Wizard.v1");
    }

    public string Protect(QuoteRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        return protector.Protect(json);
    }

    public QuoteRequest? Unprotect(string? token)
    {
        if (string.IsNullOrEmpty(token)) return null;
        try
        {
            var json = protector.Unprotect(token);
            return JsonSerializer.Deserialize<QuoteRequest>(json);
        }
        catch
        {
            return null;
        }
    }
}
```

- [ ] **Step 3: Run tests, pass**

Run: `dotnet test tests/ETD.Web.Tests --filter WizardStateProtectorTests`
Expected: 2/2 passing.

- [ ] **Step 4: Register in DI (`Program.cs`)**

Add to `Program.cs` after `builder.AddServiceDefaults()`:

```csharp
builder.Services.AddDataProtection();
builder.Services.AddSingleton<ETD.Web.Services.WizardStateProtector>();
```

- [ ] **Step 5: Commit**

```bash
git add src/ETD.Web/Services/WizardStateProtector.cs tests/ETD.Web.Tests/WizardStateProtectorTests.cs src/ETD.Web/Program.cs
git commit -m "feat: WizardStateProtector for signed wizard state"
```

---

### Task 3.3: PriceEstimator with tests

**Files:**
- Create: `src/ETD.Web/Services/PriceEstimator.cs`
- Create: `src/ETD.Web/Models/PriceEstimate.cs`
- Create: `tests/ETD.Web.Tests/PriceEstimatorTests.cs`

- [ ] **Step 1: Failing tests**

```csharp
using ETD.Web.Services;

namespace ETD.Web.Tests;

public class PriceEstimatorTests
{
    [Test]
    public async Task Wallbox_11kw_10m_Returns1500to2200()
    {
        var e = PriceEstimator.EstimateWallbox(kw: 11, distanceMeters: 10);
        await Assert.That(e.LowEuro).IsGreaterThanOrEqualTo(1500);
        await Assert.That(e.HighEuro).IsLessThanOrEqualTo(2400);
    }

    [Test]
    public async Task Wallbox_22kw_30m_HigherThan11kw_10m()
    {
        var a = PriceEstimator.EstimateWallbox(11, 10);
        var b = PriceEstimator.EstimateWallbox(22, 30);
        await Assert.That(b.HighEuro).IsGreaterThan(a.HighEuro);
    }

    [Test]
    public async Task Pv_30sqm_NoStorage_ReturnsRange()
    {
        var e = PriceEstimator.EstimatePv(areaSqm: 30, withStorage: false);
        await Assert.That(e.LowEuro).IsGreaterThan(0);
        await Assert.That(e.HighEuro).IsGreaterThan(e.LowEuro);
    }

    [Test]
    public async Task Pv_WithStorage_MoreExpensive()
    {
        var no = PriceEstimator.EstimatePv(30, false);
        var yes = PriceEstimator.EstimatePv(30, true);
        await Assert.That(yes.HighEuro).IsGreaterThan(no.HighEuro);
    }
}
```

- [ ] **Step 2: Implement model + service**

`src/ETD.Web/Models/PriceEstimate.cs`:

```csharp
namespace ETD.Web.Models;

public sealed record PriceEstimate(int LowEuro, int HighEuro, string Disclaimer = "Verbindliches Angebot erst nach Vor-Ort-Termin");
```

`src/ETD.Web/Services/PriceEstimator.cs`:

```csharp
using ETD.Web.Models;

namespace ETD.Web.Services;

public static class PriceEstimator
{
    // Base: 1200 € for an 11 kW wallbox plus standard cable run of 5 m.
    // Each extra kW adds ~30 €, each metre over 5 adds ~30 €.
    public static PriceEstimate EstimateWallbox(int kw, int distanceMeters)
    {
        kw = Math.Clamp(kw, 4, 22);
        distanceMeters = Math.Clamp(distanceMeters, 1, 50);
        var low = 1200 + (kw - 11) * 30 + Math.Max(0, distanceMeters - 5) * 30;
        var high = low + 400 + (kw > 11 ? 200 : 0) + (distanceMeters > 15 ? 300 : 0);
        return new PriceEstimate(Math.Max(low, 1100), Math.Max(high, low + 300));
    }

    // PV: ~1300 €/kWp installed (rule of thumb 2024), 5 kWp ≈ 30 m². Storage adds ~6000-9000 €.
    public static PriceEstimate EstimatePv(int areaSqm, bool withStorage)
    {
        areaSqm = Math.Clamp(areaSqm, 10, 200);
        var kwp = Math.Round(areaSqm / 6.0, MidpointRounding.AwayFromZero);
        var low = (int)(kwp * 1100);
        var high = (int)(kwp * 1500);
        if (withStorage)
        {
            low += 6000;
            high += 9000;
        }
        return new PriceEstimate(low, high);
    }
}
```

- [ ] **Step 3: Run tests — pass**

Run: `dotnet test tests/ETD.Web.Tests --filter PriceEstimatorTests`
Expected: 4/4 passing.

- [ ] **Step 4: Commit**

```bash
git add src/ETD.Web/Services/PriceEstimator.cs src/ETD.Web/Models/PriceEstimate.cs tests/ETD.Web.Tests/PriceEstimatorTests.cs
git commit -m "feat: PriceEstimator with non-binding ranges for wallbox + PV"
```

---

### Task 3.4: SubmissionRateLimiter

**Files:**
- Create: `src/ETD.Web/Services/SubmissionRateLimiter.cs`
- Create: `tests/ETD.Web.Tests/SubmissionRateLimiterTests.cs`

- [ ] **Step 1: Failing tests**

```csharp
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
```

- [ ] **Step 2: Implement**

```csharp
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
```

- [ ] **Step 3: Register in DI** — add to `Program.cs`:

```csharp
builder.Services.AddSingleton(new ETD.Web.Services.SubmissionRateLimiter(maxPerHour: 5));
```

- [ ] **Step 4: Run tests + commit**

```bash
dotnet test tests/ETD.Web.Tests --filter SubmissionRateLimiterTests
git add src/ETD.Web/Services/SubmissionRateLimiter.cs tests/ETD.Web.Tests/SubmissionRateLimiterTests.cs src/ETD.Web/Program.cs
git commit -m "feat: per-IP rate limiter (5/hour) for wizard submissions"
```

---

### Task 3.5: Email service (IQuoteMailer + SmtpQuoteMailer)

**Files:**
- Create: `src/ETD.Web/Services/IQuoteMailer.cs`
- Create: `src/ETD.Web/Services/SmtpQuoteMailer.cs`
- Modify: `src/ETD.Web/Program.cs`
- Modify: `src/ETD.Web/appsettings.json`

- [ ] **Step 1: Interface**

`src/ETD.Web/Services/IQuoteMailer.cs`:

```csharp
using ETD.Web.Models;

namespace ETD.Web.Services;

public interface IQuoteMailer
{
    Task SendAsync(QuoteRequest request, CancellationToken ct = default);
}
```

- [ ] **Step 2: Implementation (MailKit + MimeKit)**

Add MailKit reference to `src/ETD.Web/ETD.Web.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="MailKit" />
</ItemGroup>
```

`src/ETD.Web/Services/SmtpQuoteMailer.cs`:

```csharp
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
```

- [ ] **Step 3: Register in DI**

Add to `Program.cs`:

```csharp
builder.Services.Configure<ETD.Web.Services.SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddSingleton<ETD.Web.Services.IQuoteMailer, ETD.Web.Services.SmtpQuoteMailer>();
```

- [ ] **Step 4: Default config**

Edit `src/ETD.Web/appsettings.json` to add:

```json
"Smtp": {
  "Host": "localhost",
  "Port": 1025,
  "From": "anfrage@elektrotechnikdesch.de",
  "To": "mail@ElektroTechnikDesch.de",
  "UseTls": false
}
```

In prod, `Smtp__Host`, `Smtp__Port`, `Smtp__User`, `Smtp__Pass`, `Smtp__UseTls` come from environment variables (set via K8s `Secret` referenced by Aspire).

- [ ] **Step 5: Commit**

```bash
git add src/ETD.Web/Services/IQuoteMailer.cs src/ETD.Web/Services/SmtpQuoteMailer.cs src/ETD.Web/Program.cs src/ETD.Web/appsettings.json src/ETD.Web/ETD.Web.csproj
git commit -m "feat: SMTP-based quote mailer (owner + customer copy)"
```

---

### Task 3.6: Wizard step 1 — `/angebot` (Was?)

**Files:**
- Create: `src/ETD.Web/Components/Pages/Angebot/Was.razor`

- [ ] **Step 1: Add wizard CSS**

Append to `src/ETD.Web/wwwroot/css/components.css`:

```css
.wizard { max-width: 760px; margin: 0 auto; padding: 48px var(--gutter) 96px; }
.wizard .stepper { display: flex; gap: 6px; margin-bottom: 20px; }
.wizard .stepper > div { flex: 1; height: 4px; border-radius: 2px; background: var(--line); }
.wizard .stepper > div.done, .wizard .stepper > div.active { background: var(--red); }
.wizard .step-meta { display: flex; justify-content: space-between; font-size: 11px; color: var(--muted); margin-bottom: 24px; text-transform: uppercase; letter-spacing: 0.06em; font-weight: 700; }
.wizard label { display: block; font-size: 13px; font-weight: 700; color: var(--ink-2); margin-bottom: 6px; margin-top: 16px; }
.wizard .service-tiles { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 8px; }
.wizard .tile { display: flex; align-items: center; gap: 10px; padding: 12px 14px; background: #fff; border: 1px solid var(--line); border-radius: 8px; cursor: pointer; user-select: none; font-size: 13px; font-weight: 600; }
.wizard .tile input { display: none; }
.wizard .tile:has(input:checked) { background: var(--red-soft); border-color: var(--red); }
.wizard .tile-icon { width: 24px; height: 24px; background: var(--red-soft); color: var(--red); border-radius: 6px; display: flex; align-items: center; justify-content: center; font-size: 13px; flex-shrink: 0; }
.wizard .tile:has(input:checked) .tile-icon { background: var(--red); color: #fff; }
.wizard .chips { display: flex; gap: 8px; }
.wizard .chip { padding: 8px 16px; border: 1px solid var(--line); background: #fff; border-radius: 999px; font-size: 13px; cursor: pointer; }
.wizard .chip:has(input:checked) { background: var(--red); color: #fff; border-color: var(--red); }
.wizard .chip input { display: none; }
.wizard input[type="text"], .wizard input[type="email"], .wizard input[type="tel"], .wizard textarea, .wizard select { width: 100%; padding: 10px 12px; border: 1px solid var(--line); border-radius: 6px; font-size: 14px; font-family: inherit; background: #fff; }
.wizard input:focus, .wizard textarea:focus, .wizard select:focus { outline: none; border-color: var(--red); box-shadow: 0 0 0 3px rgba(220,31,38,0.1); }
.wizard textarea { resize: vertical; min-height: 90px; }
.wizard .nav-btns { display: flex; gap: 10px; margin-top: 28px; }
.wizard .nav-btns .btn { flex: 1; justify-content: center; }
.wizard .field-error { color: var(--red); font-size: 12px; margin-top: 4px; }
.wizard .honeypot { position: absolute; left: -9999px; height: 0; width: 0; overflow: hidden; }
```

- [ ] **Step 2: Write `Was.razor`**

```razor
@page "/angebot"
@using System.ComponentModel.DataAnnotations
@using ETD.Web.Models
@using ETD.Web.Services
@inject WizardStateProtector Protector
@inject NavigationManager Nav

<PageTitle>Angebot anfragen — Schritt 1 — ETD</PageTitle>

<section class="wizard fade-in">
    <div class="stepper">
        <div class="active"></div><div></div><div></div><div></div>
    </div>
    <div class="step-meta"><span>Schritt 1 von 4</span><span>Was brauchen Sie?</span></div>
    <h1 style="margin-bottom:8px">Was brauchen Sie?</h1>
    <p style="color:var(--muted);margin-bottom:24px">Mehrfach-Auswahl möglich. Wenn unsicher: zur nächsten Frage springen — wir klären das im Telefon­gespräch.</p>

    <EditForm Model="model" OnValidSubmit="HandleNext" FormName="step1" Enhance>
        <DataAnnotationsValidator />

        <div class="service-tiles">
            @foreach (var s in ServiceCatalog.All)
            {
                <label class="tile">
                    <input type="checkbox" name="model.Services" value="@s.Slug" checked="@model.Services.Contains(s.Slug)" />
                    <span class="tile-icon">@s.Icon</span>
                    <span>@s.Title</span>
                </label>
            }
        </div>
        <ValidationMessage For="() => model.Services" class="field-error" />

        <label style="margin-top:24px">Für wen?</label>
        <div class="chips">
            <label class="chip"><InputRadio Name="audience" Value="@("privat")" @bind-Value="model.Audience" /> Privat</label>
            <label class="chip"><InputRadio Name="audience" Value="@("gewerbe")" @bind-Value="model.Audience" /> Gewerbe</label>
        </div>

        <label style="margin-top:24px">Weitere Hinweise (optional)</label>
        <InputTextArea @bind-Value="model.Notes" placeholder="Was sollen wir noch wissen? (Termin, Anzahl Räume, Sondersituationen ...)" />

        <div class="nav-btns">
            <button type="submit" class="btn btn-primary">Weiter →</button>
        </div>
    </EditForm>
</section>

@code {
    [SupplyParameterFromForm] private QuoteRequest model { get; set; } = new();

    protected override void OnInitialized()
    {
        if (model is null || string.IsNullOrEmpty(model.Audience))
            model = new QuoteRequest { Audience = "privat" };
    }

    private void HandleNext()
    {
        var hasPrice = model.Services.Contains("wallbox") || model.Services.Contains("photovoltaik");
        var next = hasPrice ? "/angebot/preisrahmen" : "/angebot/zeit-ort";
        Nav.NavigateTo($"{next}?s={Uri.EscapeDataString(Protector.Protect(model))}");
    }
}
```

- [ ] **Step 3: Verify in browser** — open `/angebot`, select Wallbox + Photovoltaik, click Next → should land on `/angebot/preisrahmen?s=...`. Select only E-Check → `/angebot/zeit-ort?s=...`.

- [ ] **Step 4: Commit**

```bash
git add src/ETD.Web/Components/Pages/Angebot/Was.razor src/ETD.Web/wwwroot/css/components.css
git commit -m "feat: wizard step 1 (Was?) with conditional next step"
```

---

### Task 3.7: Wizard step 2 — Preisrahmen (conditional)

**Files:**
- Create: `src/ETD.Web/Components/Pages/Angebot/Preisrahmen.razor`

- [ ] **Step 1: Write the page**

```razor
@page "/angebot/preisrahmen"
@using ETD.Web.Models
@using ETD.Web.Services
@inject WizardStateProtector Protector
@inject NavigationManager Nav

<PageTitle>Angebot anfragen — Schritt 2 — ETD</PageTitle>

<section class="wizard fade-in">
    <div class="stepper">
        <div class="done"></div><div class="active"></div><div></div><div></div>
    </div>
    <div class="step-meta"><span>Schritt 2 von 4</span><span>Preisrahmen (unverbindlich)</span></div>

    @if (model is null)
    {
        <p>Bitte mit <a href="/angebot">Schritt 1</a> beginnen.</p>
    }
    else
    {
        <h1 style="margin-bottom:8px">Erste Kostenschätzung</h1>
        <p style="color:var(--muted);margin-bottom:18px">Nur ein Richtwert. Verbindlich nach Vor-Ort-Termin.</p>

        <EditForm Model="model" OnValidSubmit="HandleNext" FormName="step2" Enhance>
            <input type="hidden" name="s" value="@token" />

            @if (model.Services.Contains("wallbox"))
            {
                <label>Wallbox-Leistung: @(model.WallboxKw ?? 11) kW</label>
                <InputNumber @bind-Value="model.WallboxKw" min="4" max="22" step="1" />

                <label>Entfernung zum Zählerschrank: @(model.WallboxDistanceMeters ?? 5) m</label>
                <InputNumber @bind-Value="model.WallboxDistanceMeters" min="1" max="50" step="1" />

                @{ var wb = PriceEstimator.EstimateWallbox(model.WallboxKw ?? 11, model.WallboxDistanceMeters ?? 5); }
                <div style="margin:14px 0;padding:14px 18px;background:var(--red-soft);border-left:4px solid var(--red);border-radius:8px">
                    <div style="font-size:11px;font-weight:700;color:var(--red);letter-spacing:0.08em;text-transform:uppercase">Wallbox · Richtwert (zzgl. MwSt.)</div>
                    <div style="font-size:24px;font-weight:900;color:var(--red);margin-top:4px;letter-spacing:-0.02em">~ @($"{wb.LowEuro:N0} – {wb.HighEuro:N0}") €</div>
                </div>
            }

            @if (model.Services.Contains("photovoltaik"))
            {
                <label>Geschätzte Dachfläche: @(model.PvAreaSqm ?? 30) m²</label>
                <InputNumber @bind-Value="model.PvAreaSqm" min="10" max="200" step="5" />

                <label>Mit Batteriespeicher?</label>
                <div class="chips">
                    <label class="chip"><InputRadio Name="pvStor" Value="@false" @bind-Value="model.PvWithStorage" /> Nein</label>
                    <label class="chip"><InputRadio Name="pvStor" Value="@true" @bind-Value="model.PvWithStorage" /> Ja</label>
                </div>

                @{ var pv = PriceEstimator.EstimatePv(model.PvAreaSqm ?? 30, model.PvWithStorage ?? false); }
                <div style="margin:14px 0;padding:14px 18px;background:var(--red-soft);border-left:4px solid var(--red);border-radius:8px">
                    <div style="font-size:11px;font-weight:700;color:var(--red);letter-spacing:0.08em;text-transform:uppercase">Photovoltaik · Richtwert (zzgl. MwSt.)</div>
                    <div style="font-size:24px;font-weight:900;color:var(--red);margin-top:4px;letter-spacing:-0.02em">~ @($"{pv.LowEuro:N0} – {pv.HighEuro:N0}") €</div>
                </div>
            }

            <p style="font-size:12px;color:var(--muted);margin-top:8px">Diese Schätzung ersetzt kein Angebot. Verbindlich erst nach Vor-Ort-Termin.</p>

            <div class="nav-btns">
                <button type="button" class="btn btn-ghost" @onclick="HandleBack">← Zurück</button>
                <button type="submit" class="btn btn-primary">Weiter →</button>
            </div>
        </EditForm>
    }
</section>

@code {
    [SupplyParameterFromQuery(Name = "s")] private string? token { get; set; }
    [SupplyParameterFromForm] private QuoteRequest? model { get; set; }

    protected override void OnParametersSet()
    {
        if (model is null) model = Protector.Unprotect(token);
        model ??= new QuoteRequest();
        model.WallboxKw ??= 11;
        model.WallboxDistanceMeters ??= 5;
        model.PvAreaSqm ??= 30;
        model.PvWithStorage ??= false;
    }

    private void HandleNext()
    {
        Nav.NavigateTo($"/angebot/zeit-ort?s={Uri.EscapeDataString(Protector.Protect(model!))}");
    }

    private void HandleBack() => Nav.NavigateTo("/angebot");
}
```

- [ ] **Step 2: Verify** — From step 1, select Wallbox → step 2 shows wallbox sliders + estimate. Select PV only → step 2 shows PV. Select both → both. Estimate updates after each save (post-back). Click Next → /angebot/zeit-ort.

- [ ] **Step 3: Commit**

```bash
git add src/ETD.Web/Components/Pages/Angebot/Preisrahmen.razor
git commit -m "feat: wizard step 2 (Preisrahmen) with live wallbox + pv estimate"
```

---

### Task 3.8: Wizard step 3 — Zeit & Ort

**Files:**
- Create: `src/ETD.Web/Components/Pages/Angebot/ZeitOrt.razor`

- [ ] **Step 1: Write**

```razor
@page "/angebot/zeit-ort"
@using ETD.Web.Models
@using ETD.Web.Services
@inject WizardStateProtector Protector
@inject NavigationManager Nav

<PageTitle>Angebot anfragen — Schritt 3 — ETD</PageTitle>

<section class="wizard fade-in">
    <div class="stepper">
        <div class="done"></div><div class="done"></div><div class="active"></div><div></div>
    </div>
    <div class="step-meta"><span>Schritt 3 von 4</span><span>Wann & wo?</span></div>

    @if (model is null)
    {
        <p>Bitte mit <a href="/angebot">Schritt 1</a> beginnen.</p>
    }
    else
    {
        <h1 style="margin-bottom:24px">Wann & wo?</h1>
        <EditForm Model="model" OnValidSubmit="HandleNext" FormName="step3" Enhance>
            <DataAnnotationsValidator />

            <label>Wunsch-Zeitrahmen</label>
            <div class="chips" style="flex-wrap:wrap">
                <label class="chip"><InputRadio Name="tf" Value="@("asap")" @bind-Value="model.Timeframe" /> So bald wie möglich</label>
                <label class="chip"><InputRadio Name="tf" Value="@("2w")" @bind-Value="model.Timeframe" /> Innerhalb 2 Wochen</label>
                <label class="chip"><InputRadio Name="tf" Value="@("4w")" @bind-Value="model.Timeframe" /> Innerhalb 4 Wochen</label>
                <label class="chip"><InputRadio Name="tf" Value="@("flex")" @bind-Value="model.Timeframe" /> Flexibel</label>
            </div>

            <label style="margin-top:24px">PLZ</label>
            <InputText @bind-Value="model.Plz" maxlength="5" inputmode="numeric" placeholder="63599" />
            <ValidationMessage For="() => model.Plz" class="field-error" />

            <label style="margin-top:16px">Ort</label>
            <InputText @bind-Value="model.City" placeholder="Biebergemünd" />
            <ValidationMessage For="() => model.City" class="field-error" />

            <div class="nav-btns">
                <button type="button" class="btn btn-ghost" @onclick="HandleBack">← Zurück</button>
                <button type="submit" class="btn btn-primary">Weiter →</button>
            </div>
        </EditForm>
    }
</section>

@code {
    [SupplyParameterFromQuery(Name = "s")] private string? token { get; set; }
    [SupplyParameterFromForm] private QuoteRequest? model { get; set; }

    protected override void OnParametersSet()
    {
        if (model is null) model = Protector.Unprotect(token);
        model ??= new QuoteRequest();
        if (string.IsNullOrEmpty(model.Timeframe)) model.Timeframe = "flex";
    }

    private void HandleNext()
    {
        Nav.NavigateTo($"/angebot/kontakt?s={Uri.EscapeDataString(Protector.Protect(model!))}");
    }

    private void HandleBack()
    {
        var hasPrice = model!.Services.Contains("wallbox") || model.Services.Contains("photovoltaik");
        Nav.NavigateTo(hasPrice ? "/angebot/preisrahmen" : "/angebot");
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add src/ETD.Web/Components/Pages/Angebot/ZeitOrt.razor
git commit -m "feat: wizard step 3 (Zeit & Ort)"
```

---

### Task 3.9: Wizard step 4 — Kontakt + submit handler

**Files:**
- Create: `src/ETD.Web/Components/Pages/Angebot/Kontakt.razor`

- [ ] **Step 1: Write the final-step component**

```razor
@page "/angebot/kontakt"
@using System.ComponentModel.DataAnnotations
@using ETD.Web.Models
@using ETD.Web.Services
@inject WizardStateProtector Protector
@inject NavigationManager Nav
@inject IQuoteMailer Mailer
@inject SubmissionRateLimiter Limiter
@inject IHttpContextAccessor Http
@inject ILogger<Kontakt> Log

<PageTitle>Angebot anfragen — Schritt 4 — ETD</PageTitle>

<section class="wizard fade-in">
    <div class="stepper">
        <div class="done"></div><div class="done"></div><div class="done"></div><div class="active"></div>
    </div>
    <div class="step-meta"><span>Schritt 4 von 4</span><span>Kontakt</span></div>

    @if (model is null)
    {
        <p>Bitte mit <a href="/angebot">Schritt 1</a> beginnen.</p>
    }
    else
    {
        <h1 style="margin-bottom:24px">Ihre Kontaktdaten</h1>
        <EditForm Model="model" OnValidSubmit="HandleSubmit" FormName="step4" Enhance>
            <DataAnnotationsValidator />

            <label>Name</label>
            <InputText @bind-Value="model.Name" placeholder="Max Muster" />
            <ValidationMessage For="() => model.Name" class="field-error" />

            <label style="margin-top:16px">E-Mail</label>
            <InputText @bind-Value="model.Email" type="email" placeholder="max@example.de" />
            <ValidationMessage For="() => model.Email" class="field-error" />

            <label style="margin-top:16px">Telefon</label>
            <InputText @bind-Value="model.Phone" type="tel" placeholder="06050 9062874" />
            <ValidationMessage For="() => model.Phone" class="field-error" />

            <!-- Honeypot -->
            <div class="honeypot" aria-hidden="true">
                <label>Firma (bitte leer lassen)</label>
                <InputText @bind-Value="model.CompanyName2" tabindex="-1" autocomplete="off" />
            </div>

            <label style="display:flex;gap:10px;align-items:flex-start;margin-top:18px">
                <InputCheckbox @bind-Value="model.ConsentPrivacy" />
                <span style="font-size:13px;color:var(--ink-2)">Ich habe die <a href="/datenschutz" target="_blank">Datenschutzerklärung</a> gelesen und stimme der Verarbeitung meiner Daten zur Bearbeitung meiner Anfrage zu.</span>
            </label>
            <ValidationMessage For="() => model.ConsentPrivacy" class="field-error" />

            @if (errorMessage is not null)
            {
                <p class="field-error" style="margin-top:12px">@errorMessage</p>
            }

            <div class="nav-btns">
                <button type="button" class="btn btn-ghost" @onclick="HandleBack">← Zurück</button>
                <button type="submit" class="btn btn-primary" disabled="@submitting">Anfrage absenden →</button>
            </div>

            <p style="font-size:12px;color:var(--muted);margin-top:14px;text-align:center">Verschlüsselte Übertragung · keine Weitergabe an Dritte</p>
        </EditForm>
    }
</section>

@code {
    [SupplyParameterFromQuery(Name = "s")] private string? token { get; set; }
    [SupplyParameterFromForm] private QuoteRequest? model { get; set; }
    private bool submitting;
    private string? errorMessage;

    protected override void OnParametersSet()
    {
        if (model is null) model = Protector.Unprotect(token);
        model ??= new QuoteRequest();
    }

    private async Task HandleSubmit()
    {
        // Honeypot — silently succeed without sending
        if (!string.IsNullOrWhiteSpace(model!.CompanyName2))
        {
            Log.LogWarning("Honeypot triggered on quote submission");
            Nav.NavigateTo("/angebot/erfolg");
            return;
        }

        var ip = Http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (!Limiter.TryConsume(ip))
        {
            errorMessage = "Zu viele Anfragen in kurzer Zeit. Bitte versuchen Sie es in einer Stunde erneut oder rufen Sie uns an.";
            return;
        }

        submitting = true;
        try
        {
            await Mailer.SendAsync(model!);
            Nav.NavigateTo("/angebot/erfolg");
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Mailer failed");
            errorMessage = "Der Versand ist fehlgeschlagen. Bitte rufen Sie uns direkt an: 06050 9062874.";
            submitting = false;
        }
    }

    private void HandleBack() => Nav.NavigateTo("/angebot/zeit-ort");
}
```

- [ ] **Step 2: Register `IHttpContextAccessor`** — add to `Program.cs`:

```csharp
builder.Services.AddHttpContextAccessor();
```

- [ ] **Step 3: Commit**

```bash
git add src/ETD.Web/Components/Pages/Angebot/Kontakt.razor src/ETD.Web/Program.cs
git commit -m "feat: wizard step 4 (Kontakt) with submit + honeypot + rate-limit"
```

---

### Task 3.10: Erfolg page

**Files:**
- Create: `src/ETD.Web/Components/Pages/Angebot/Erfolg.razor`

- [ ] **Step 1: Write**

```razor
@page "/angebot/erfolg"

<PageTitle>Anfrage gesendet — ETD</PageTitle>

<section class="container fade-in" style="padding:96px 0;text-align:center;max-width:680px;margin:0 auto">
    <div style="width:80px;height:80px;background:var(--red);color:#fff;border-radius:50%;display:flex;align-items:center;justify-content:center;font-size:36px;margin:0 auto 24px;box-shadow:var(--shadow-red)">✓</div>
    <div class="eyebrow">Anfrage gesendet</div>
    <h1 style="margin-bottom:14px">Vielen Dank!</h1>
    <p style="font-size:1.0625rem;color:var(--ink-2)">Ihre Anfrage ist bei uns angekommen. Wir melden uns innerhalb von 24 Stunden bei Ihnen — sehr wahrscheinlich deutlich schneller.</p>
    <p style="color:var(--muted);margin-top:16px">Wenn es eilt, rufen Sie uns direkt an: <a href="tel:+4960509062874" style="font-weight:700">06050 9062874</a></p>

    <div style="margin-top:32px;display:flex;gap:12px;justify-content:center;flex-wrap:wrap">
        <a href="/" class="btn btn-ghost">Zur Startseite</a>
        <a href="/leistungen" class="btn btn-primary">Weitere Leistungen</a>
    </div>
</section>

<script>
    // Clear the wizard state from localStorage on the success page
    try { localStorage.removeItem('etd.wizard.v1'); } catch (e) { }
</script>
```

- [ ] **Step 2: Commit**

```bash
git add src/ETD.Web/Components/Pages/Angebot/Erfolg.razor
git commit -m "feat: wizard success page + clear localStorage"
```

---

### Task 3.11: E2E test — full wizard submission via Mailpit

**Files:**
- Create: `tests/ETD.E2E/PlaywrightFixture.cs`
- Create: `tests/ETD.E2E/WizardFlowTests.cs`

- [ ] **Step 1: Playwright fixture (boots Aspire host + installs browsers)**

```csharp
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Playwright;
using TUnit.Core.Interfaces;

namespace ETD.E2E;

public sealed class AspireFixture : IAsyncInitializer, IAsyncDisposable
{
    public DistributedApplication? App { get; private set; }
    public string WebBaseUrl { get; private set; } = "";
    public string MailpitUiUrl { get; private set; } = "";
    public IBrowser? Browser { get; private set; }

    public async Task InitializeAsync()
    {
        Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });

        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.ETD_AppHost>();
        App = await builder.BuildAsync();
        await App.StartAsync();

        WebBaseUrl = await App.GetEndpointAsync("etd-web", "http");
        MailpitUiUrl = await App.GetEndpointAsync("mailpit", "ui");

        var pw = await Playwright.CreateAsync();
        Browser = await pw.Chromium.LaunchAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (Browser is not null) await Browser.CloseAsync();
        if (App is not null) await App.DisposeAsync();
    }
}
```

> **Note:** `App.GetEndpointAsync` signature varies between Aspire releases. If this exact API is not available in 13.3, use `App.GetEndpoint("etd-web", "http").ToString()` or read the resource URL from the resource notifications. The test executor must adjust to whatever the 13.3 testing API surfaces.

- [ ] **Step 2: Wizard flow test**

```csharp
using Microsoft.Playwright;

namespace ETD.E2E;

[ClassDataSource<AspireFixture>(Shared = SharedType.PerTestSession)]
public class WizardFlowTests(AspireFixture fx)
{
    [Test]
    public async Task FullWizardSubmission_DeliversTwoMailsToMailpit()
    {
        var page = await fx.Browser!.NewPageAsync(new() { BaseURL = fx.WebBaseUrl });

        await page.GotoAsync("/angebot");
        await page.GetByText("E-Mobilität & Wallbox").ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Weiter" }).ClickAsync();

        // Step 2 — Preisrahmen
        await page.GetByRole(AriaRole.Button, new() { Name = "Weiter" }).ClickAsync();

        // Step 3 — Zeit & Ort
        await page.GetByText("So bald wie möglich").ClickAsync();
        await page.GetByLabel("PLZ").FillAsync("63599");
        await page.GetByLabel("Ort").FillAsync("Biebergemünd");
        await page.GetByRole(AriaRole.Button, new() { Name = "Weiter" }).ClickAsync();

        // Step 4 — Kontakt
        await page.GetByLabel("Name").FillAsync("Max Tester");
        await page.GetByLabel("E-Mail").FillAsync("max@tester.example");
        await page.GetByLabel("Telefon").FillAsync("01601234567");
        await page.GetByText("Ich habe die Datenschutzerklärung").ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Anfrage absenden" }).ClickAsync();

        await page.WaitForURLAsync("**/angebot/erfolg");
        await Assert.That(await page.GetByText("Vielen Dank!").IsVisibleAsync()).IsTrue();

        // Verify the mails arrived in Mailpit
        using var http = new HttpClient { BaseAddress = new Uri(fx.MailpitUiUrl) };
        var messages = await http.GetStringAsync("/api/v1/messages");
        await Assert.That(messages).Contains("Max Tester");
        await Assert.That(messages).Contains("Ihre Anfrage bei Elektrotechnik Desch");
    }
}
```

- [ ] **Step 3: Run the E2E test**

Run: `dotnet test tests/ETD.E2E`
Expected: 1/1 passing. If Playwright browsers aren't installed yet, the fixture installs them — first run is slow.

- [ ] **Step 4: Commit**

```bash
git add tests/ETD.E2E/
git commit -m "test: E2E wizard flow with Mailpit assertion"
```

---

## Phase 4 — Deploy

### Task 4.1: Health endpoint + Dockerfile

**Files:**
- Modify: `src/ETD.ServiceDefaults/Extensions.cs` (likely already maps health endpoints — verify)
- Modify: `src/ETD.Web/Program.cs`
- Create: `Dockerfile`
- Create: `.dockerignore`

- [ ] **Step 1: Verify `/health` and `/alive` are mapped via `MapDefaultEndpoints`**

Look at `src/ETD.ServiceDefaults/Extensions.cs` — the scaffold maps `/health` and `/alive` by default in development only. For prod, replace the guard with a permanent map:

```csharp
public static WebApplication MapDefaultEndpoints(this WebApplication app)
{
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/alive", new HealthCheckOptions { Predicate = r => r.Tags.Contains("live") });
    return app;
}
```

- [ ] **Step 2: Write `Dockerfile`**

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["global.json", "."]
COPY ["src/ETD.ServiceDefaults/ETD.ServiceDefaults.csproj", "src/ETD.ServiceDefaults/"]
COPY ["src/ETD.Web/ETD.Web.csproj", "src/ETD.Web/"]
RUN dotnet restore "src/ETD.Web/ETD.Web.csproj"
COPY src/ src/
WORKDIR /src/src/ETD.Web
RUN dotnet publish -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "ETD.Web.dll"]
```

- [ ] **Step 3: Write `.dockerignore`**

```
**/bin
**/obj
**/.vs
**/.git
.gitignore
.dockerignore
Dockerfile
deploy/
tests/
docs/
.superpowers/
```

- [ ] **Step 4: Build + smoke-test locally**

```bash
docker build -t etd-web:dev .
docker run --rm -p 8080:8080 etd-web:dev &
sleep 5
curl -fsS http://localhost:8080/health
docker kill $(docker ps -q --filter ancestor=etd-web:dev)
```

Expected: 200 + body "Healthy".

- [ ] **Step 5: Commit**

```bash
git add Dockerfile .dockerignore src/ETD.ServiceDefaults/Extensions.cs
git commit -m "feat: Dockerfile + health endpoints"
```

---

### Task 4.2: Aspire publish target & generated manifests

**Files:**
- Modify: `src/ETD.AppHost/AppHost.cs` (image name + ingress hostname)
- Create: `deploy/kubernetes/.gitkeep`

- [ ] **Step 1: Configure the Web project for K8s publishing**

Update `AppHost.cs` `AddProject<Projects.ETD_Web>("etd-web")` call to:

```csharp
var web = builder.AddProject<Projects.ETD_Web>("etd-web")
    .WithEnvironment("Smtp__From", "anfrage@elektrotechnikdesch.de")
    .WithEnvironment("Smtp__To", "mail@ElektroTechnikDesch.de")
    .WithExternalHttpEndpoints()
    .PublishAsContainer()
    .WithImageName("etd-web")           // ghcr.io/<owner>/etd-web (registry from env config)
    .WithReplicas(2);
```

Plus an annotation that the Kubernetes target picks up for ingress:

```csharp
web.PublishAsKubernetesService(svc =>
{
    svc.Ingress(ingress =>
    {
        ingress.Host("www.elektrotechnikdesch.de");
        ingress.WithTls("etd-web-tls", clusterIssuer: "letsencrypt-prod");
    });
});
```

> **Note:** Exact API names may differ between Aspire 13.x minors. The executor should consult `Aspire.Hosting.Kubernetes` 13.3 API docs and adjust — the *intent* is: tell Aspire's K8s publisher to produce Deployment/Service/Ingress YAML with TLS via cert-manager.

- [ ] **Step 2: Run `aspire publish` locally**

```bash
dotnet run --project src/ETD.AppHost -- --publisher kubernetes --output-path deploy/kubernetes
```

Expected: YAML files appear under `deploy/kubernetes/`. Validate with `kubeval` or `kubectl apply --dry-run=client -f deploy/kubernetes/`.

- [ ] **Step 3: Commit the output for transparency**

```bash
git add deploy/kubernetes/ src/ETD.AppHost/AppHost.cs
git commit -m "feat: aspire publish target generates K8s manifests with ingress + TLS"
```

---

### Task 4.3: GitHub Actions CI workflow

**Files:**
- Create: `.github/workflows/ci.yml`

- [ ] **Step 1: Write**

```yaml
name: ci

on:
  pull_request:
  push:
    branches: [main]

jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'

      - name: Install Aspire workload
        run: dotnet workload install aspire

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore

      - name: Unit tests
        run: dotnet test tests/ETD.Web.Tests --configuration Release --no-build --verbosity normal

      - name: Install Playwright browsers
        run: pwsh -Command "Microsoft.Playwright.Program.Main(@('install', '--with-deps', 'chromium'))"

      - name: E2E tests
        run: dotnet test tests/ETD.E2E --configuration Release --no-build --verbosity normal
```

- [ ] **Step 2: Commit**

```bash
git add .github/workflows/ci.yml
git commit -m "ci: GitHub Actions workflow for build + tests"
```

---

### Task 4.4: GitHub Actions deploy workflow

**Files:**
- Create: `.github/workflows/deploy.yml`

- [ ] **Step 1: Write**

```yaml
name: deploy

on:
  push:
    branches: [main]
  workflow_dispatch:

permissions:
  contents: read
  packages: write

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}/etd-web

jobs:
  build-push:
    runs-on: ubuntu-latest
    outputs:
      image-tag: ${{ steps.meta.outputs.tags }}
      image-digest: ${{ steps.build.outputs.digest }}
    steps:
      - uses: actions/checkout@v4
      - uses: docker/setup-buildx-action@v3
      - name: Login to GHCR
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}
      - id: meta
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
          tags: |
            type=sha,prefix=
            type=ref,event=branch
      - id: build
        uses: docker/build-push-action@v6
        with:
          context: .
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}

  publish-manifests:
    needs: build-push
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'
      - run: dotnet workload install aspire
      - run: dotnet run --project src/ETD.AppHost -- --publisher kubernetes --output-path deploy/kubernetes-out
      - name: Substitute image tag
        run: |
          sed -i "s|image: etd-web.*|image: ${{ needs.build-push.outputs.image-tag }}|g" deploy/kubernetes-out/*.yaml
      - uses: actions/upload-artifact@v4
        with:
          name: k8s-manifests
          path: deploy/kubernetes-out

  apply:
    needs: publish-manifests
    runs-on: ubuntu-latest
    steps:
      - uses: actions/download-artifact@v4
        with:
          name: k8s-manifests
          path: deploy/kubernetes-out
      - name: Set up kubectl
        uses: azure/setup-kubectl@v4
      - name: Write kubeconfig
        run: |
          mkdir -p ~/.kube
          echo "${{ secrets.CIVO_KUBECONFIG }}" | base64 -d > ~/.kube/config
          chmod 600 ~/.kube/config
      - run: kubectl apply -f deploy/kubernetes-out
      - run: kubectl rollout status deployment/etd-web --timeout=180s
```

- [ ] **Step 2: Required secrets**

Document the required GitHub repository secret:
- `CIVO_KUBECONFIG`: base64-encoded kubeconfig file for the CIVO cluster, scoped to a service account with deploy permission in the `default` (or chosen) namespace.

- [ ] **Step 3: Commit**

```bash
git add .github/workflows/deploy.yml
git commit -m "ci: deploy workflow — build image, generate manifests, apply to CIVO"
```

---

### Task 4.5: Privacy E2E test — no external requests

**Files:**
- Create: `tests/ETD.E2E/PrivacyTests.cs`

- [ ] **Step 1: Write**

```csharp
using Microsoft.Playwright;

namespace ETD.E2E;

[ClassDataSource<AspireFixture>(Shared = SharedType.PerTestSession)]
public class PrivacyTests(AspireFixture fx)
{
    [Test]
    public async Task Home_NeverRequestsExternalOrigin()
    {
        var page = await fx.Browser!.NewPageAsync(new() { BaseURL = fx.WebBaseUrl });
        var ownOrigin = new Uri(fx.WebBaseUrl).Host;

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

        await page.GotoAsync("/", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Assert.That(external).IsEmpty();
    }
}
```

- [ ] **Step 2: Run + commit**

```bash
dotnet test tests/ETD.E2E --filter PrivacyTests
git add tests/ETD.E2E/PrivacyTests.cs
git commit -m "test: assert home page makes zero external network calls"
```

---

### Task 4.6: README

**Files:**
- Create: `README.md`

- [ ] **Step 1: Write**

```markdown
# ETD — Elektrotechnik Desch website

Static SSR Blazor site for Elektrotechnik Desch, deployed to a CIVO Kubernetes cluster in Frankfurt.

## Local dev

```bash
dotnet run --project src/ETD.AppHost
```

Opens the Aspire dashboard. The web app runs at the URL Aspire prints; Mailpit (catches all outgoing mail in dev) at port 8025 on the host shown in the dashboard.

## Tests

```bash
dotnet test tests/ETD.Web.Tests      # unit tests (TUnit)
dotnet test tests/ETD.E2E             # E2E with Playwright + Aspire test host
```

## Deploy

Pushing to `main` triggers `.github/workflows/deploy.yml`:

1. Build container, push to `ghcr.io/<repo>/etd-web:<sha>`
2. Generate K8s manifests via `aspire publish --publisher kubernetes`
3. `kubectl apply` against the CIVO cluster (kubeconfig in `CIVO_KUBECONFIG` secret)

## Required GitHub secrets

| Secret | Description |
|---|---|
| `CIVO_KUBECONFIG` | base64-encoded kubeconfig for a service account with deploy rights in the target namespace |

## Required cluster prerequisites

| Component | Notes |
|---|---|
| nginx-ingress | The Aspire-generated ingress uses standard `nginx` annotations |
| cert-manager | The ingress references the `letsencrypt-prod` ClusterIssuer for TLS |

## Production environment variables

| Variable | Description |
|---|---|
| `Smtp__Host` | e.g. `mail.elektrotechnikdesch.de` |
| `Smtp__Port` | typically 587 |
| `Smtp__User` | SMTP login |
| `Smtp__Pass` | SMTP password — store as K8s `Secret` |
| `Smtp__UseTls` | `true` in prod |
| `Smtp__From` | `anfrage@elektrotechnikdesch.de` |
| `Smtp__To` | `mail@ElektroTechnikDesch.de` |

See [`docs/superpowers/specs/2026-05-16-etd-website-design.md`](docs/superpowers/specs/2026-05-16-etd-website-design.md) for the full design.
```

- [ ] **Step 2: Commit**

```bash
git add README.md
git commit -m "docs: README with run/test/deploy instructions"
```

---

## Self-review (run by the implementer before finishing)

Spot-check the produced site once everything above is committed:

1. **All 18 routes return 200:** `/`, `/leistungen`, `/leistungen/{slug}` × 7, `/gewerbe`, `/notdienst`, `/referenzen`, `/ueber-uns`, `/angebot`, `/angebot/erfolg`, `/kontakt`, `/impressum`, `/datenschutz`, `/cookies`, `/sitemap.xml`, `/robots.txt`. Verified via a small script or by hand.
2. **404 page** renders the styled MainLayout (not a raw 404 from ASP.NET).
3. **Lighthouse scores** on `/` and on `/leistungen/wallbox` ≥ 95 / 100 / 100 / 100. Run via `npx lighthouse` against a deployed preview or local prod build.
4. **Privacy E2E** passes (zero external requests).
5. **Full wizard E2E** passes (Mailpit sees both mails).
6. **No `Console.WriteLine`, no `TODO`, no commented-out code** in committed sources. Grep before declaring done.
7. **Restart-during-wizard scenario:** start the site, fill steps 1-3, kill the process, restart, navigate back via browser back — `localStorage` must still let the user resume. Manually verified (or add an additional Playwright test that kills the container between steps).

If any of the above fails, fix it in a follow-up commit before merging.

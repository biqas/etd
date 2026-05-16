# ETD Website — Design Spec

**Date:** 2026-05-16
**Status:** Draft for review
**Replaces:** https://www.elektrotechnikdesch.de (current WordPress site)

## 1. Goal

Build a new public website for **Elektrotechnik Desch** (ETD), a master electrician business in Biebergemünd, Germany. The site must:

- Project a professional, trustworthy master-craftsman image
- Serve both private and commercial customers equally
- Generate qualified quote requests via a guided wizard
- Showcase services with strong imagery
- Be fully GDPR-compliant by design, with no infringing brand imagery
- Run on the customer's own CIVO Kubernetes cluster in Frankfurt
- Be implementable in `.NET 10` / `C# 13` using Blazor static SSR (no SignalR circuit, no WebSockets) so the site never times out when the connection breaks

## 2. Business identity

| Field | Value |
|---|---|
| Trade name | Elektrotechnik Desch |
| Owner | me. Christoph Desch (Elektrotechnikermeister) |
| Address | Von-Cancrin-Straße 22, 63599 Biebergemünd |
| Phone | +49 6050 9062874 |
| Email | mail@ElektroTechnikDesch.de |
| USt-IdNr. | DE330546558 |
| Steuernummer | 019 811 62300 |
| Memberships / certs | Innungsmitglied · Sachkundezertifikat A1 · Betriebszertifizierung gemäß §6 ChemKlimaschutzV |
| Brand color | Red `#DC1F26` (primary), `#A8161C` (hover), `#FEF2F2` (soft) |
| Ink color | `#0E1116` (ink), `#2C333D` (ink-2), `#6B7280` (muted) |
| Typeface | Inter (self-hosted woff2, weights 400 / 600 / 700 / 800 / 900) |
| Logo | Text mark "ETD" in red square (interim — replace with provided logo when available) |
| Audiences | Private homeowners AND commercial customers, weighted equally |

## 3. Services

### 3.1 Top-tier services (each gets its own landing page)

| Slug | Name | Notes |
|---|---|---|
| `elektroinstallation` | Elektroinstallation (Alt- & Neubau) | Zählerschrank, Unterverteilung, Schalter, Steckdosen, Lampen, Herd, Wallbox-Vorbereitung |
| `smart-home-knx` | Smart Home / KNX | Lichtsteuerung, Heizung, Jalousien, Szenen, herstellerunabhängig |
| `photovoltaik` | Photovoltaik & Speicher | Anlagen, Speicher, Energiemanagement, Anmeldung beim Netzbetreiber |
| `wallbox` | E-Mobilität / Wallbox | Installation, Lastmanagement, KfW-Förderberatung |
| `klimatechnik` | Klima- & Kältetechnik | mit Sachkundezertifikat A1 + §6 ChemKlimaschutzV |
| `e-check` | E-Check / DGUV V3 | Prüfung ortsfester und ortsveränderlicher Anlagen |
| `sicherheit` | Sicherheitstechnik | Alarmanlagen, Videoüberwachung, Brandmeldetechnik, Zutritt |

### 3.2 Secondary services (grouped as anchored sections on `/leistungen`)

- Sat- & Antennenanlagen (Sat ZF, Unicable, SAT>IP)
- Beleuchtung / LED-Planung
- Netzwerk & EDV (LAN, WLAN, Patchschränke)
- Türsprechanlagen (Ritto Twinbus, Video-Türsprechanlagen)

Each gets an in-page anchor, a brief description, and a CTA to the quote wizard. No standalone landing page (search volume too low to justify maintenance overhead).

### 3.3 Other paths

- `/notdienst` — short, prominent landing page that puts the phone number above the fold; no form
- `/gewerbe` — overview of commercial-focused offerings (Wartung, Schaltanlagen, E-Check, Maschinenanschluss)

## 4. Visual design

### 4.1 Style direction: Bold & Industrial

- Dark hero backgrounds (`#0E1116` base, `#1A1F2A` accent gradient)
- Heavy display typography (Inter Black for headlines, weight 700–900)
- Red signal accents (no decorative red — red always means "this is important / clickable")
- Hard geometric shapes; rounded radii only on cards / buttons (radius 8 px buttons, 12–16 px cards)
- Subtle radial-glow accents at hero edges; no gradient mush across content

### 4.2 Hero layout: Full-Bleed Hero Photo

- Full-width hero image (real photo of installation work / Schaltschrank / Smart-Home scene)
- Dark overlay (`linear-gradient(rgba(14,17,22,0.7), rgba(14,17,22,0.85))`) for text legibility
- Headline + sub + 2 buttons ("Angebot anfragen" primary, "Leistungen" ghost)
- Trust bar at hero bottom: 4 stats (years, projects, "Innung", "24/7 Notdienst")
- Top-right nav: logo · main links · phone number CTA

### 4.3 Page sections beyond hero (recurring patterns)

- **Service grid** — 3-column card grid, icon + title + 1 line + arrow
- **Trust block** — Christoph's photo (or silhouette placeholder until photo provided) + bullet list of certifications, with logos of Innung etc. (subject to permission)
- **Reference strip** — horizontal scroller with project tiles (Holzhaus + others to be added)
- **CTA banner** — full-bleed red `#DC1F26` band with "Jetzt Angebot anfragen" button — appears as the final block of every page

### 4.4 Motion

- Fade-up on scroll (Intersection Observer, 14 px translate, 500 ms ease-out)
- Hover lift on cards (`translateY(-4px)` + soft shadow)
- Button hover: brighten + tighten letter-spacing
- 250 ms `cubic-bezier(.2,.8,.2,1)` is the default transition curve
- No auto-rotating sliders, no parallax, no modal popups

## 5. Information architecture

```
/                                Startseite (Hero, Top-6, Trust, Refs, CTA)
/leistungen                      Übersicht aller Leistungen
/leistungen/elektroinstallation  Detail-Landing
/leistungen/smart-home-knx       Detail-Landing
/leistungen/photovoltaik         Detail-Landing
/leistungen/wallbox              Detail-Landing
/leistungen/klimatechnik         Detail-Landing
/leistungen/e-check              Detail-Landing
/leistungen/sicherheit           Detail-Landing
                                 (sat, beleuchtung, netzwerk, sprechanlagen
                                  = anchors on /leistungen)
/gewerbe                         Gewerbe-Hub
/notdienst                       Notdienst-Mini-Page
/referenzen                      Projektgalerie + Holzhaus-Case
/ueber-uns                       Christoph + Zertifikate
/angebot                         Wizard (4 steps)
/angebot/erfolg                  Confirmation page
/kontakt                         Adresse, Karte (static image), Öffnungszeiten
/impressum                       Pflichtseite
/datenschutz                     Pflichtseite
/cookies                         Cookie-Einstellungen (minimal)
```

Total: 15 content pages + 3 mandatory legal pages + the wizard success page (`/angebot/erfolg`).

Each leaf page must include a final-CTA band linking to `/angebot`.

## 6. Quote wizard (`/angebot`)

A 4-step form plus a success page. Rendered as static SSR (one POST per step, server holds no in-memory state — wizard state is serialized to localStorage in the browser + hidden form fields). Server only sees the full submission on the final step.

### Step 1 — "Was brauchen Sie?"
- Multi-select grid of all service tiles (the 7 top + 4 secondary, total 11 tiles)
- Audience chip: Privat | Gewerbe (single-select)
- Free-text "Weitere Hinweise" (optional, 500 chars)

### Step 2 — Preisrahmen (optional, shown only if Step 1 includes Wallbox or Photovoltaik)
- For Wallbox: kW slider (3.7–22 kW) + Entfernung-zum-Zählerschrank slider (1–30 m) → display non-binding range
- For Photovoltaik: dachfläche slider (10–100 m²) + speicher yes/no → display range
- Always shows the disclaimer: "Verbindliches Angebot erst nach Vor-Ort-Termin"
- Step is skipped entirely if Wallbox and PV are not selected

### Step 3 — Wann & Wo
- Wunsch-Zeitraum: "So bald wie möglich" / "Innerhalb 2 Wochen" / "Innerhalb 4 Wochen" / "Flexibel" (single-select radio)
- PLZ + Ort (text inputs, PLZ validated to 5 digits)

### Step 4 — Kontakt
- Name (required), E-Mail (required, validated), Telefon (required for callbacks)
- Checkbox: "Ich habe die Datenschutzerklärung gelesen und stimme zu" (required)
- Honeypot field (`field_name_2`, hidden via CSS, must be empty on submit) for spam protection
- Server-side rate limit: 5 submissions per IP per hour

### Success page (`/angebot/erfolg`) — not a wizard step
- Reached only after Step 4 submission validates successfully
- Shows submission summary + "Wir melden uns innerhalb 24 h"
- The same controller action that renders this page triggers two emails: one to `mail@ElektroTechnikDesch.de` (full submission), one auto-confirmation to the customer
- Reloading the success page is safe (idempotent — re-submission is blocked by the same per-IP rate limit and by clearing the wizard state from localStorage on success)

### Persistence model
- No database. Submission lives only in the email sent.
- Wizard intermediate state is held in `localStorage` (key `etd.wizard.v1`) + signed hidden form fields, so the user can refresh / back-button without losing data.
- Server state is per-request only. If the server restarts mid-flow, the next POST simply re-validates the submitted hidden state and continues. No timeout possible.

## 7. Technical architecture

### 7.1 Stack

| Layer | Choice | Rationale |
|---|---|---|
| Runtime | .NET 10 | User-mandated |
| Language | C# 13 | User-mandated |
| Web framework | Blazor with static server-side rendering (`@rendermode InteractiveServer` is NOT used) | User-mandated: no SignalR circuit, no WebSocket, no timeout when connection drops |
| Forms | Blazor `EditForm` with `[SupplyParameterFromForm]` and Enhanced Form Navigation | Static SSR submits via POST; enhanced nav gives SPA-like UX without circuit |
| Orchestration | .NET Aspire 13.3.0 | User-mandated; replaces hand-written Helm |
| K8s manifest generation | `aspire publish` to Kubernetes target (Aspire 13.3 publishes manifests directly via its publishing infrastructure — `Aspire.Hosting.Kubernetes` package or equivalent extension) | User-mandated: no Helm |
| Container registry | GitHub Container Registry (`ghcr.io/<owner>/etd-web`) | Free for public repos; easy GHA auth |
| Cluster | CIVO Kubernetes, Frankfurt region | User-provided |
| Ingress | nginx-ingress (assumed already in cluster) | Standard CIVO setup |
| TLS | cert-manager + Let's Encrypt (HTTP-01 via the Ingress) | Free, automated rotation |
| Mail (dev) | Mailpit container, orchestrated by Aspire | Local mails visible at `http://localhost:8025` |
| Mail (prod) | SMTP to `mail.ElektroTechnikDesch.de` (existing domain mailbox) | No external SaaS, no extra cost, DSGVO satisfied |
| Logging | stdout JSON to k8s logs (collected by CIVO's default stack) | Keep it simple in v1 |
| Tests | TUnit (unit) + Playwright (E2E against the Aspire-orchestrated stack) | Repo conventions and Aspire test integration |

### 7.2 Repository layout

```
etd/
├── docs/                              # Specs and notes
├── src/
│   ├── ETD.Web/                       # Blazor Static SSR project
│   │   ├── Components/                # Razor components (Pages + Layout + Shared)
│   │   ├── Services/                  # Mail, validation, wizard state
│   │   ├── Resources/                 # Service catalog, FAQ data (JSON or static C#)
│   │   ├── wwwroot/
│   │   │   ├── fonts/                 # Self-hosted Inter
│   │   │   ├── img/                   # WebP-optimized images
│   │   │   ├── css/                   # Compiled CSS (or Tailwind output)
│   │   │   └── favicon.svg
│   │   └── Program.cs
│   ├── ETD.Web.Tests/                 # TUnit unit tests
│   └── ETD.AppHost/                   # Aspire 13.3 AppHost
│       └── Program.cs                 # Models Web + Mailpit (dev) + publishing config
├── tests/
│   └── ETD.E2E/                       # Playwright E2E suite
├── .github/workflows/
│   ├── ci.yml                         # Build + test on PR
│   └── deploy.yml                     # main → build, push, aspire publish, kubectl apply
├── deploy/
│   └── kubernetes/                    # Output of `aspire publish` (committed for transparency)
└── ETD.sln
```

### 7.3 Aspire AppHost responsibilities

- Define the `ETD.Web` project as a `ProjectResource`
- Add a `MailpitResource` (container) wired to the web app in dev only
- Define environment variables: SMTP host/port/user/from, base URL, recaptcha-off / honeypot toggle
- Configure container image name & tag for publishing
- Configure the Kubernetes publishing target with namespace, ingress hostname, TLS issuer name, and resource limits/requests

### 7.4 Container

- `Dockerfile` is a standard multi-stage `mcr.microsoft.com/dotnet/sdk:10.0` → `mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled`
- Runs as non-root
- Health check at `/healthz` (simple 200 from `MapHealthChecks`)
- Listens on port `8080`

### 7.5 Kubernetes resources (generated by Aspire publish)

- `Deployment` for `etd-web` — 2 replicas, rolling update, readiness + liveness probes on `/healthz`
- `Service` (ClusterIP)
- `Ingress` for `www.elektrotechnikdesch.de` (and apex redirect) with cert-manager annotation for Let's Encrypt
- `Secret` `etd-smtp` mounted as env vars (created by hand or via SealedSecrets — not stored in git)
- `ConfigMap` `etd-config` for non-secret runtime config

Resource requests/limits to start: `100m/256Mi` request, `500m/512Mi` limit. Tune after first prod week.

### 7.6 CI/CD

`ci.yml` (PRs):
- Restore, build, run TUnit tests, run Playwright headless against Aspire dev stack
- Fail on any test or lint failure

`deploy.yml` (push to main):
1. Build & test as above
2. `docker build` & push to `ghcr.io/<owner>/etd-web:${{ github.sha }}`
3. `aspire publish` to `deploy/kubernetes/` (using image tag as input)
4. `kubectl apply -k deploy/kubernetes/` against CIVO cluster (kubeconfig from secret)
5. Wait for rollout to complete, fail otherwise

## 8. Imagery and content policy

### 8.1 Allowed image sources

1. **Photographs reused from the current site** (since they belong to ETD): Holzhaus reference, customer-installation photos, certificates. Re-export at `2400 px` long edge and convert to WebP.
2. **Custom SVG illustrations** generated as part of this project for service icons, hero mood graphics, and section dividers. All committed to `wwwroot/img/illustrations/` with a `.attribution` file.
3. **Royalty-free CC0** stock (Unsplash, Pexels) only for hero backgrounds where no in-house photo exists. Each file must have an accompanying `<filename>.license.txt` recording source URL, photographer, license, and download date.

### 8.2 Prohibited

- Logos of Gira, Legrand, Ritto and other brands **unless** Christoph has obtained explicit written permission. Mention these brands **only as text** ("Wir verbauen Gira-Schalterprogramme") in v1.
- Any image whose provenance is unclear.
- Stock photos depicting identifiable third parties without model release.

### 8.3 Image pipeline

- Source images live under `assets/source/` (outside the build) — never deployed.
- A small build-time step (`dotnet tool` or `nuke` task) converts each into 3 WebP variants (480 / 1024 / 2048 px) and writes to `wwwroot/img/`.
- Markup uses `<picture>` with `srcset` + `loading="lazy"` + width/height set, except for the hero image (eager-loaded).

## 9. Privacy and compliance (DSGVO)

### 9.1 Default behaviors

- Inter typeface is self-hosted (no Google Fonts CDN call)
- The "Kontakt" page shows a **static map image** (rendered server-side from OpenStreetMap or a one-off Google Static Maps export with archived license terms) + a "Route in Google Maps öffnen" link. No iframe embed.
- Zero analytics in v1. No GA, no Plausible, no Matomo. Server access logs (anonymized IPs) only.
- No third-party scripts of any kind. No CDN dependencies at runtime.

### 9.2 Cookies

- Wizard state lives in `localStorage`, which is functionally essential and does not require a consent banner under TTDSG §25 (2).
- No tracking cookies, no advertising cookies. Therefore: no consent banner is needed by default. A minimal "Cookies"-Info page is still provided to be transparent.
- If a feature is added later that needs consent (e.g. embedded video), a TCF-compliant banner must be added at that time.

### 9.3 Mandatory pages

- `/impressum` — populated with the values in §2 plus required text per TMG §5 and DL-InfoV
- `/datenschutz` — covers: server logs, form data handling, TLS, hosting in Frankfurt, no third-party transfers, rights under DSGVO Art. 15-22, contact for data subject requests
- `/cookies` — explains the localStorage usage, why no consent is required, and how to clear it

### 9.4 Logs

- Ingress logs configured with IP anonymization (`/24` truncation) before they hit persistent storage
- Application logs never include form payloads (only the fact that a submission occurred + the submission ID)
- Mail attachments / form submissions are not persisted in the application — they live only in the inbox

## 10. Acceptance criteria

- All 15 content pages + 3 legal pages + wizard success page render and return HTTP 200
- Lighthouse on the homepage scores ≥ 95 in Performance, ≥ 100 in Accessibility, ≥ 100 in SEO, ≥ 100 in Best Practices
- The quote wizard end-to-end test passes in Playwright (full submission from `/` to received email in Mailpit dev)
- Server restart during wizard does not lose user data (verified by Playwright test that kills the container between steps)
- All site fonts and assets are served from the same origin (verified by network panel snapshot in E2E test)
- No external network calls from any page (verified by a Playwright assertion: no request to a hostname other than the site's own)
- DSGVO pages contain the values in §2 verbatim
- `aspire publish` produces a manifest that `kubeval`-validates clean
- Container starts under 5 s and serves the first request within 2 s of startup

## 11. Phasing

| Phase | Duration | Deliverables |
|---|---|---|
| 1 — Foundation | ~3 working days | Aspire solution, Blazor Static SSR project, design tokens, layout components (header/footer/hero frame), CI pipeline running |
| 2 — Content | ~5 working days | All 14 content pages + 3 legal pages with real copy and imagery |
| 3 — Conversion | ~3 working days | Quote wizard (all 5 steps), validation, SMTP service, Mailpit local, honeypot + rate-limit |
| 4 — Go-Live | ~2 working days | Aspire publish → CIVO deploy, cert-manager, DNS cutover, Lighthouse pass, sitemap.xml + robots.txt |

Total: ~13 working days of focused implementation.

## 12. Out of scope (v1)

- Blog / Ratgeber section
- AGB page (only required if direct online sales — none here)
- Logo redesign — current "ETD" red square is interim
- Multi-language (German only in v1)
- Customer login / appointment booking calendar
- Live preisrechner outside Wallbox / PV
- Embedded video, embedded Google reviews, embedded social
- Analytics
- A/B testing infrastructure

## 13. Open questions to confirm before implementation

1. Does Christoph already have written brand-logo permissions from Gira / Legrand / Ritto? If yes, we can include logos in v1; if not, text-only mentions are used.
2. Does the existing domain mailbox `mail@ElektroTechnikDesch.de` support SMTP submission with SPF/DKIM/DMARC properly configured? If not, we add Brevo or a similar EU provider as a fallback later — not in v1.
3. Is the CIVO cluster's nginx-ingress and cert-manager already installed? If not, add a `phase 0` to set them up.
4. PV and Wärmepumpen — are these full services offered, or "auf Anfrage / Kooperationspartner"? Default: full service is shown; switch to "auf Anfrage" in copy if confirmed otherwise.
5. Does Christoph have any existing project photos beyond Holzhaus we can use, or do we need to commission new photography in phase 2?

# ETD Webseite — Handover

**Stand:** 2026-05-17
**Branch:** `feat/initial-website` (3 commits ahead of `origin/feat/initial-website`)
**Working tree:** clean
**Letzter Commit:** `2a82f8a fix(logo): filament als M-Form mit 2 Pfeil-Peaks + Stem + Basis`

Dieses Dokument fasst den vollständigen Stand der ETD-Webseite zusammen, damit ein anderer AI-Agent (oder Mensch) die Arbeit ohne Reibung übernehmen kann.

---

## 1. Projekt-Übersicht

**Kunde:** Elektrotechnik Desch (ETD), Meisterbetrieb für Elektro-, Kälte- und Klimatechnik in Biebergemünd / Spessart, Hessen.

**Tech-Stack:**
- .NET 10 / C# 13
- Blazor Static Server-Side Rendering (kein SignalR, kein WebSocket, keine Interactive-Sessions — bewusst, damit Seiten nicht "wegen Server-Verbindung verloren" sind)
- .NET Aspire 13.3.0 für Orchestrierung
- Aspirate 9.1.0 für K8s-Manifest-Generierung (statt Helm)
- Deploy-Ziel: CIVO Kubernetes in Frankfurt + nginx-ingress + cert-manager
- TUnit + Playwright für Tests
- MailKit/MimeKit für SMTP (Dev: Mailpit via Aspire-Container, Prod: STRATO)

**Repo-Root:** `/Users/saqib.javed/Work/github/biqas/etd`

**Wichtige Datei:** `ETD.slnx` (nicht `ETD.sln` — .NET 10 SLNX-Format)

---

## 2. Projektstruktur

```
src/
├── ETD.AppHost/                          # Aspire Orchestrator
│   └── AppHost.cs                        # Mailpit-Container, ETD.Web mit 2 Replicas
├── ETD.ServiceDefaults/                  # Standard Aspire Service-Defaults
└── ETD.Web/                              # Blazor Static SSR
    ├── appsettings.json                  # Dev-Defaults (Mailpit localhost:1025)
    ├── Program.cs                        # DI, MapStaticAssets(), MapRazorComponents
    ├── Components/
    │   ├── App.razor + Routes.razor + _Imports.razor
    │   ├── Layout/
    │   │   ├── MainLayout.razor          # Hauptlayout mit Header/Footer
    │   │   ├── Header.razor              # NEU: BrandLogo + Burger + Nav
    │   │   ├── Footer.razor              # NEU: BrandLogo (dark) + HWK-Hinweis
    │   │   └── CtaBanner.razor           # End-of-page CTA für die meisten Seiten
    │   ├── Shared/
    │   │   ├── BrandLogo.razor           # NEU: SVG-Logo (icon/icon-dark/full)
    │   │   ├── ServiceIcon.razor         # Lucide-Style SVG-Icons mit Hover-Animation
    │   │   ├── ServiceCard.razor
    │   │   ├── HeroFullBleed.razor       # Hero auf Home
    │   │   ├── ServiceDetailHero.razor   # Hero auf Service-Detail-Seiten
    │   │   ├── TrustBar.razor
    │   │   └── KnxRichContent.razor      # KNX-Deep-Dive Content (lange Datei!)
    │   └── Pages/
    │       ├── Home.razor                # /
    │       ├── Leistungen.razor          # /leistungen (Übersicht aller Services)
    │       ├── ServiceDetail.razor       # /leistungen/{slug}
    │       ├── Gewerbe.razor             # /gewerbe (Wartungsverträge ENTFERNT)
    │       ├── Notdienst.razor           # /notdienst
    │       ├── Referenzen.razor          # /referenzen (NEU: 11 echte Fotos + Moosmann)
    │       ├── UeberUns.razor            # /ueber-uns (TODO: Werkstatt + Zertifikate)
    │       ├── Kontakt.razor             # /kontakt
    │       ├── Impressum.razor / Datenschutz.razor / Cookies.razor
    │       ├── SmartHomeKnxBauherr.razor # NEU: /leistungen/smart-home-knx/bauherr
    │       └── Angebot/
    │           ├── Was.razor             # Step 1: Service-Auswahl (Multi-Select Tiles)
    │           ├── Preisrahmen.razor     # NEU: Step 2 — REINER Feature-Multi-Select, KEINE Slider, KEINE Preise mehr
    │           ├── ZeitOrt.razor         # Step 3: PLZ/Ort/Zeitrahmen
    │           ├── Kontakt.razor         # Step 4: Kontaktdaten + Submit
    │           └── Erfolg.razor          # Danke-Seite nach Submit
    ├── Models/
    │   └── QuoteRequest.cs               # Wizard-Modell (mit SelectedFeatures statt alten Param-Feldern!)
    ├── Services/
    │   ├── ServiceCatalog.cs             # Service-Definitionen (Klima HWK + CAT7 hier!)
    │   ├── ServiceParams.cs              # Wird nicht mehr aktiv genutzt (nach Wizard-Refactor)
    │   ├── PriceEstimator.cs             # DEAD CODE — kann gelöscht werden
    │   ├── WizardStateProtector.cs       # DataProtection-signierter Wizard-State
    │   ├── SubmissionRateLimiter.cs      # In-Memory per-IP
    │   ├── IQuoteMailer.cs / SmtpQuoteMailer.cs  # E-Mail-Versand (NEU: SelectedFeatures formatiert)
    └── wwwroot/
        ├── css/
        │   ├── tokens.css                # Variablen, --gutter responsive (32→20→16)
        │   ├── base.css                  # Reset, container, h1/h2 clamp
        │   ├── components.css            # ALLES andere — sehr lang
        │   └── site.css
        ├── js/
        │   └── animations.js             # IntersectionObserver fade-up. Estimator-Code ENTFERNT.
        ├── fonts/inter-*.woff2           # 5 Weights, GDPR-konform selbst gehostet
        └── img/legacy/                   # 31 Bilder von alter Webseite + legacy-images.md Index
tests/
├── ETD.Web.Tests/                        # TUnit Unit-Tests
└── ETD.E2E/                              # Playwright E2E mit Aspire-Test-Fixture
deploy/
└── kubernetes/                           # Aspirate-generierte Manifeste + hand-written ingress.yaml
```

---

## 3. Was wurde gemacht (Chronologisch, letzte zuerst)

### `2a82f8a` — Filament-Fix
- Glühdraht im Logo war als einfaches Polygon — jetzt 3 Shapes (Basis-Bar + Stem + 5-Punkt-M-Polygon)
- **User hatte die M-Form 2× moniert** — bitte verifizieren ob die aktuelle Lösung passt (siehe Section "OFFENE PUNKTE")

### `4726603` — Großer Multi-Topic-Commit (48 Dateien)
1. **Logo neu (`BrandLogo.razor`)**: 3 Varianten — `icon` (Header), `icon-dark` (Footer auf dunkel), `full` (für E-Mail/Print). Dunkelrote Lampe + transluzente blaue Schneeflocke + Swoosh im Hintergrund.
2. **Klimatechnik**: HWK-Eintragung im Kälte-Anlagenbauer-Handwerk erwähnt (`ServiceCatalog.cs`, Slug `klimatechnik`)
3. **Netzwerk**: "mindestens CAT 7" statt CAT 6/7 (`ServiceCatalog.cs`, Slug `netzwerk`)
4. **Wartungsverträge-Kachel auf `/gewerbe` entfernt**
5. **Moosmann Zerspanung** als Featured-Referenz mit Link `https://moosmann-zerspanung-stage-fe7dcae185f98.webflow.io/`
6. **Wizard-Refactor Step 2** — KOMPLETT umgebaut:
   - Alle Slider (Wallbox kW, PV-Fläche, KNX-Räume etc.) ENTFERNT
   - Alle automatischen Preisschätzungen ENTFERNT
   - Stattdessen: Multi-Select-Checkbox-Liste pro Service mit den Bullets aus `ServiceCatalog`
   - `QuoteRequest.SelectedFeatures` (List&lt;string&gt;) ersetzt alle service-spezifischen Param-Properties
   - Format: `"service-slug::feature-text"` pro Eintrag
7. **31 Bilder von alter WordPress-Seite geholt** nach `wwwroot/img/legacy/` mit Index `legacy-images.md`. 11 Privat-Referenzen, 4 Zertifikate, Werkstatt-Foto, Gira-Schalterbilder, Bosch-Hausgeräte.
8. **Referenzen-Seite zeigt jetzt die 11 echten Privat-Fotos** in Galerie-Grid statt Platzhaltern.
9. **STRATO SMTP** vorbereitet via `dotnet user-secrets` (Credentials NIE im Repo — siehe Section "E-Mail" unten).

### `5686d97` — Responsive Overhaul + Bauherr-Subpage
- Burger-Menü (Pure-CSS Drawer, Checkbox-Hack — Static SSR safe)
- `--gutter` responsive (32 → 20 → 16 px)
- EN15232-Tabelle stackt auf Mobile (war Killer-Overflow)
- Sektorkopplung-SVG-Labels überlappungsfrei
- Kontakt OSM-Map + Adresse stacken
- Footer Touch-Targets WCAG-konform
- Bauherr-Pakete bei 768-1099px: 2-Col Layout
- **NEUE Subpage `/leistungen/smart-home-knx/bauherr`** mit 7 Sektionen, 3 Paketen (Basic/Komfort/Premium) mit konkreten Geräteliste + KfW-Förder-Rechnung

### Vor `5686d97` — KNX Content (3 Commits)
- `a1ff6cd`: Use-Case-Inseln mit Geräten/Preisen, Sektorkopplung-Sektion, EN15232+Förderung-Sektion (KfW 458 bis 70%, BAFA 15-20%), Mythen-FAQ Accordion, Hersteller-Logos
- `ab33db8` + `7f5d8fc`: Room-Scenes und Persona-Illustrationen im Schema-Stil, Überlappungen behoben

---

## 4. Was ist OFFEN / pending

### 🟡 1. Logo-Filament — verifizieren mit User
Der User wollte den Glühdraht "wie ein gestrecktes M". Aktueller Stand (Commit `2a82f8a`):
- `wwwroot/Components/Shared/BrandLogo.razor`
- Icon-Variante Polygon: `15,22 19,8 22,16 26,8 29,22` (peaks at top, V-valley deep)
- Plus Basis-Rect bei `y=30-33` und Stem-Rect bei `15,22 width=14 height=8`
- Vom Repository nicht 100% bestätigt dass User happy ist — eventuell Feinjustierung nötig.

**Zum Verifizieren:** Vergleich mit Referenzbild das User mehrfach geschickt hat (zuletzt mit Untertitel "like this idot:"). Bild zeigt: 2 spitze Pfeil-Peaks oben + V-Valley dazwischen + einheitlicher Stem + horizontale Basis-Bar.

### 🔴 2. Sanierer-Subpage (NICHT gemacht — User hat es explizit verschoben)
- Path: `/leistungen/smart-home-knx/sanierer`
- Pendant zu Bauherr-Subpage
- User-Auswahl früher: "Tief: 3 Pakete mit konkreten Gerätelisten je Paket, Förder-Rechnung, Phasen-Timeline, Mini-Case-Study"
- Inhaltlich: Bestandshaus / Modernisierung / Hybrid wired+RF / schrittweiser Ausbau
- Vorbild für Struktur: `src/ETD.Web/Components/Pages/SmartHomeKnxBauherr.razor`
- Förderung-Schwerpunkt: KfW 458 (Wärmepumpe + KNX als Umfeldmaßnahme bis 70%) + BAFA BEG EM 15-20%
- CSS-Klassen müssten neu sein (`.sanierer-...`) oder Bauherr-Klassen wiederverwenden + umbenennen

### 🟡 3. Über-uns-Seite mit echten Bildern
- Datei existiert: `src/ETD.Web/Components/Pages/UeberUns.razor` (aktueller Stand prüfen)
- Verfügbare Assets in `wwwroot/img/legacy/`:
  - `ueberuns-werkstatt-IMG_20250911.jpg` (823×1024) — Werkstatt-Foto Sept 2025
  - `zert-innungsmitgliedschaft.jpg` (724×1024) — Innungs-Urkunde
  - `zert-qb-christoph-desch.png` (724×1024) — Qualifikations-Zertifikat
  - `zert-chemklimaschutzv.png` (724×1024) — ChemKlimaschutzV-Betriebszertifikat
- Vorschlag: Werkstatt-Foto + 3 Zertifikate als Trust-Signal-Galerie

### 🟡 4. Home-Hero mit echtem Foto
- Aktuell wird `hero-cropped-IMG_20200508_120948.jpg` (1500×1000) noch nicht eingesetzt
- Könnte den abstrakten Hero auf `/` ersetzen

### 🟡 5. Gira-Schalter-Bilder und Bosch-Hausgeräte
- 6 Gira-Schalterbilder (Standard 55, E2, E3 — Schalter + Steckdose je) — könnten auf KNX- oder Elektroinstallation-Seite gezeigt werden
- 4 Bosch-Hausgeräte-Bilder — User hat keine separate Bosch-Service-Seite, eventuell auf Home oder weglassen
- seano-Banner: alte Marke, vermutlich nicht mehr aktiv → ignorieren

### 🟢 6. Aufräumen (nicht dringend)
- `Services/PriceEstimator.cs` ist Dead Code seit Wizard-Refactor — kann gelöscht werden
- `Services/ServiceParams.cs` wird nicht mehr genutzt für Routing — kann gelöscht werden
- Alte `referenz-04-portrait-766x1024.jpg` in legacy/ ist Duplikat zu `referenz-04.jpg`

---

## 5. Wichtige technische Punkte / Gotchas

### Blazor Static SSR Eigenheiten
- **Kein Interactive Mode** — alle Formulare nutzen `<EditForm ... Enhance>` mit `OnSubmit` (NICHT `OnValidSubmit`, das hat Edge-Cases mit nullable Form-Bindings)
- **NRE-Trap bei `SupplyParameterFromForm`**: Form-Binder kann `null` in das Property schreiben bei initialer GET. Pattern siehe `Angebot/Was.razor`:
  ```csharp
  private QuoteRequest _model = new QuoteRequest { Audience = "privat" };
  [SupplyParameterFromForm(FormName = "step1")]
  private QuoteRequest currentModel
  {
      get => _model;
      set { if (value is not null) _model = value; }
  }
  ```
- **Inline `<script>` tags** re-executen NICHT nach Enhanced Navigation. Globale Init in `js/animations.js` via `Blazor.addEventListener('enhancedload', ...)`. Page-spezifische Scripts müssen sich selbst neu-initialisieren auf das Event.
- **MapStaticAssets() ist Pflicht** in `Program.cs` für `/_framework/blazor.web.js` (sonst 404)

### Static Asset Caching
- Wenn man Razor-Files ändert während der Server läuft, muss man **kompilieren UND Server neu starten**. `dotnet build` allein reicht nicht — der laufende Server cached die kompilierten Outputs.
- Hot Reload (`dotnet watch run`) funktioniert mit Vorbehalt, aber für saubere Tests Server immer kalt starten.

### Wizard-State zwischen Schritten
- Wird als Token (verschlüsselt + signiert via DataProtection) in der URL übergeben: `?s=...`
- Bei POST aus Step N: vorheriger Token kommt als hidden `prevToken`-Feld mit, wird im `OnParametersSet` aus dem Token rehydriert und in das neue Model gemergt.
- Siehe `Angebot/Kontakt.razor` Zeile 84-113 für das Pattern.

### Responsive Breakpoints (konsistent)
- `980px` — Hero-Grids stacken (Bauherr-Hero, KNX-Hero, Sektor-Diagramm-Grid)
- `820px` — Persona-Pfade, Kontakt-Layout
- `768px` — Burger-Menü greift, `--gutter: 20px`
- `640px` — KNX-Sektor-Stats 1-Col, EN15232-Row stackt
- `540px` — Förderung-Karten 1-Col, Hero-Stats 1-Col
- `414px` — `--gutter: 16px`

---

## 6. E-Mail-Konfiguration (STRATO)

**Dev (Default in `appsettings.json`):** Mailpit Container auf `localhost:1025` (kein Auth, kein TLS)

**Prod-Config über `dotnet user-secrets` für `ETD.Web` (NIEMALS IM REPO):**
```
Smtp:Host         smtp.strato.de
Smtp:Port         465
Smtp:UseTls       true
Smtp:User         info@elektrotechnikdesch.de
Smtp:Pass         (im User-Secret — vom User bereitgestellt)
Smtp:From         info@elektrotechnikdesch.de
Smtp:To           info@elektrotechnikdesch.de
```

**Lokales Setzen:**
```bash
dotnet user-secrets set "Smtp:Pass" '<PASSWORT>' --project src/ETD.Web
```
Secrets liegen in `~/.microsoft/usersecrets/ded5845c-5166-4ef8-83fc-8792e8ec5332/secrets.json`

**Production / K8s:** Env-Variablen mit Doppel-Underscore-Pattern:
- `Smtp__Host`, `Smtp__Port`, `Smtp__User`, `Smtp__Pass`, `Smtp__UseTls`, `Smtp__From`, `Smtp__To`
- Auf CIVO als Kubernetes-Secret mounten und an den Container injecten.

**Mailer-Code:** `Services/SmtpQuoteMailer.cs` — sendet 2 Mails: Owner-Notification + Customer-Bestätigung. Owner-Mail enthält `SelectedFeatures` gruppiert nach Service.

---

## 7. Wichtige Konventionen / Entscheidungen

- **Keine Cloud-Abhängigkeiten in Branding**: User legt Wert darauf dass KNX/Smart-Home-Inhalte das "läuft lokal, keine Cloud"-Argument betonen
- **GDPR-konform**: Inter-Schrift selbst gehostet (keine Google Fonts), keine Google Maps (eigener OSM-Static-PNG), keine Analytics
- **Mobile-First**: Responsive-Audit (Commit `5686d97`) hat alle Bruchstellen behoben
- **Deutsche Soft-Hyphens** (`&shy;`) für lange Komposita inkonsistent eingesetzt — könnte systematischer werden
- **Logo-Farben**:
  - Dunkelrot Vorlage: `#6B1015` (full variant)
  - Heller-Rot für Header-Icon-Sichtbarkeit: `#A8161C`
  - Footer-Dark-Variant: `#DC1F26` (brand-rot)
  - Schneeflocken-Blau Vorlage: `#1E5A8A`
  - Header-Variante heller: `#2178B8`
  - Footer-Dark: `#7BB8E0`

---

## 8. Build- und Run-Befehle

```bash
# Build
dotnet build /Users/saqib.javed/Work/github/biqas/etd/ETD.slnx

# Run nur die Web-App (ohne Aspire)
dotnet run --project src/ETD.Web --urls http://localhost:5050

# Voll mit Aspire (Mailpit-Container etc.)
dotnet run --project src/ETD.AppHost

# Tests
dotnet test src/ETD.Web.Tests
dotnet test tests/ETD.E2E

# User-Secrets ansehen / setzen
dotnet user-secrets list --project src/ETD.Web
dotnet user-secrets set "<key>" "<value>" --project src/ETD.Web
```

---

## 9. Bekannte / wahrscheinliche User-Frustrationspunkte

Der User hat über die Session mehrfach Frust geäußert. Konkrete Punkte:
1. **Logo-Iterationen**: User musste mehrfach intervenieren (Referenzbild liefern, Klarstellungen zum M-Glühdraht). Aktueller Stand könnte immer noch nicht 100% passen — siehe Section 4.1.
2. **Tiefe der Inhalte**: User war früher unzufrieden mit oberflächlichem KNX-Inhalt → daraufhin große Recherche und Content-Vertiefung (Commit `a1ff6cd`). Sollte jetzt erfüllt sein.
3. **Wizard war zu kompliziert**: User wollte Slider/Preise raus → Wizard-Refactor (Commit `4726603` enthält das).

**Empfehlung für übernehmenden Agent:** Bevor du irgendwas Großes anpackst, **frag den User WAS GENAU der Schmerzpunkt jetzt ist** und arbeite STRENG am konkret genannten Problem. Nicht-trivial: das Logo wurde mehrfach iteriert ohne dass es 100% getroffen hat — wenn der User damit weiter unzufrieden ist, **frag nach welcher Aspekt konkret nicht passt** (Form? Farbe? Größe? Proportion?) statt zu vermuten.

---

## 10. Wichtige externe Quellen / Recherche-Ergebnisse

Bereits recherchiert und in den Inhalt eingearbeitet (Stand 2026):
- **DIN EN 15232-1:2017-12** — GA-Klassen A/B/C/D, Wohngebäude Heizenergie −20…−30% bei Klasse A
- **KfW 458** — Heizungsförderung bis 70%, KNX als Umfeldmaßnahme zur WP förderfähig
- **KfW 261** — Wohngebäude-Kredit, bis 45% Tilgungszuschuss je Effizienzhaus-Level
- **BAFA BEG EM** — 15-20% (mit iSFP) auf Smart-Home-Steuerung
- **§35c EStG** — 20% Steuerbonus, max. 40.000€
- **§14a EnWG** seit 2024 — steuerbare Wallbox/WP/Speicher >4.2kW Pflicht
- **ETS6 Lizenzpreise** — Pro ~1000€, Home 350€, Demo kostenlos
- **Typische Projektpreise** — RMH Basic 15-22k€, EFH Komfort 28-38k€, Premium 48-75k€
- **Konkrete Gerätepreise**: MDT GT II Smart 150-250€, Theben HMT 6 245-295€, Weinzierl 731 IF 130-200€, Elsner Suntracer KNX pro 1800-2300€

Diese Zahlen stehen auf der KNX-Hauptseite + Bauherr-Subpage.

---

## 11. Nächste konkrete Schritte (Vorschlag)

Wenn ich übernehmen würde, in dieser Reihenfolge:

1. **User fragen**: Was ist JETZT konkret unzufrieden? Logo? Anderes? Was ist die Top-Priorität?
2. **Falls Logo immer noch nicht passt**: Referenzbild vom User holen und **Pixel-Vergleich** machen. Nicht raten. Eventuell SVG-Pfad direkt vom User holen lassen oder Vektor-Datei (Illustrator/Inkscape) anfordern und mit `svgo` minifizieren.
3. **Sanierer-Subpage bauen** (Pendant zu Bauherr, Struktur ist klar in `SmartHomeKnxBauherr.razor`)
4. **Über-uns-Seite**: Werkstatt-Foto + 3 Zertifikat-Scans als Trust-Galerie
5. **Home-Hero**: echtes Werkstatt-Foto einbinden
6. **Aufräumen**: PriceEstimator.cs + ServiceParams.cs löschen

---

## 12. Verifikation vor jedem Commit

```bash
# Build über die ganze Solution
dotnet build /Users/saqib.javed/Work/github/biqas/etd/ETD.slnx

# Visual check: Server starten + Playwright-Screenshots bei 414/768/1400px
dotnet run --project src/ETD.Web --urls http://localhost:5050

# Mindestens prüfen:
# /
# /leistungen
# /leistungen/smart-home-knx
# /leistungen/smart-home-knx/bauherr
# /angebot (Wizard Step 1)
# /angebot/preisrahmen (Wizard Step 2 — Multi-Select)
# /referenzen
# /kontakt
```

Kein horizontaler Scroll bei keiner Viewport-Breite — sonst Regression vom Responsive-Audit.

---

**Ende Handover.**

Falls der übernehmende Agent Fragen zur Geschichte einzelner Entscheidungen hat: die Git-Commit-Messages sind ausführlich und enthalten den Reasoning-Trail.

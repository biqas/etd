using ETD.Web.Models;

namespace ETD.Web.Services;

public static class ServiceCatalog
{
    public static IReadOnlyList<ServiceItem> All { get; } = new ServiceItem[]
    {
        new("elektroinstallation", "Elektroinstallation", "zap",
            "Alt- und Neubau, Sanierung, Verteilung, Stromkreise.",
            "Wir installieren saubere Elektroanlagen für Privat- und Gewerbekunden — vom Zählerschrank bis zur letzten Steckdose. Auch Sanierungen im Bestand führen wir staubarm und termingerecht durch.",
            new[] { "Zählerschrank & Unterverteilung", "Schalter, Steckdosen, Lampen", "Herd- und Wallbox-Anschluss", "Sanierung bewohnter Räume" }),

        new("smart-home-knx", "Smart Home & KNX", "home",
            "Lichtsteuerung, Heizung, Jalousien, Szenen — herstellerunabhängig.",
            "KNX ist der offene Standard für Gebäudeautomation. Wir planen, programmieren und montieren herstellerunabhängig — Sie sind nicht an eine App oder Cloud gebunden.",
            new[] { "Lichtsteuerung & Szenen", "Heizungs- und Jalousie-Steuerung", "Anwesenheits- & Energie-Reports", "Erweiterbar für 30+ Jahre" }),

        new("photovoltaik", "Photovoltaik & Speicher", "sun",
            "PV-Anlagen, Batteriespeicher, Energiemanagement.",
            "Wir planen und installieren PV-Anlagen mit und ohne Speicher. Anmeldung beim Netzbetreiber und Marktstammdatenregister übernehmen wir komplett.",
            new[] { "Dachflächen-Auslegung", "Batteriespeicher-Integration", "Anmeldung Netzbetreiber & MaStR", "Energiemanagement / EVU-Box" },
            HasPriceEstimator: true),

        new("wallbox", "E-Mobilität & Wallbox", "plug-zap",
            "Wallbox-Installation, Lastmanagement, Förderberatung.",
            "Vom 11 kW-Standard bis zur 22 kW-Lösung mit Lastmanagement. Wir kümmern uns um die elektrische Anbindung, Anmeldung und Förderfähigkeit.",
            new[] { "11 kW oder 22 kW", "Lastmanagement bei mehreren Boxen", "Anmeldung beim Netzbetreiber", "KfW-Förderberatung" },
            HasPriceEstimator: true),

        new("klimatechnik", "Klima- und Kältetechnik", "snowflake",
            "Klimaanlagen Privat & Gewerbe — eingetragen in der Handwerksrolle für Kälte-Anlagenbauer.",
            "Wir installieren und warten Klimaanlagen für Wohn- und Geschäftsräume. Für das Kälte-Anlagenbauer-Handwerk in der Handwerksrolle der HWK Wiesbaden eingetragen, mit Sachkundezertifikat A1 und Betriebszertifizierung gemäß §6 ChemKlimaschutzV — alles aus einer Hand statt zwei Gewerken.",
            new[] { "Eingetragen in der Handwerksrolle HWK Wiesbaden — Kälte-Anlagenbauer", "Multi-Split & VRF-Systeme", "Wohnraum- und Gewerbekühlung", "Wartung & jährliche Dichtheitsprüfung (Pflicht ab 5 t CO₂-Äquivalent)", "Inbetriebnahme & Anmeldung beim Umweltbundesamt" }),

        new("e-check", "E-Check / DGUV V3", "shield-check",
            "Gesetzlich vorgeschriebene Prüfung ortsfester und ortsveränderlicher Anlagen.",
            "Als Arbeitgeber sind Sie verpflichtet, elektrische Anlagen regelmäßig prüfen zu lassen. Wir führen E-Checks normgerecht durch und stellen das Prüfprotokoll aus.",
            new[] { "Ortsfeste Anlagen (DIN VDE 0100-600 / 0105-100)", "Ortsveränderliche Geräte (DGUV V3 § 5)", "Wiederholungsprüfungen", "Versicherungsrelevantes Protokoll" }),

        new("sicherheit", "Sicherheitstechnik", "shield",
            "Alarmanlagen, Videoüberwachung, Brandmeldetechnik, Zutritt.",
            "Schutz für Haus, Hof und Betrieb. Wir planen vor-Ort, installieren und schulen Sie auf das System ein.",
            new[] { "Funk- und Hybrid-Alarmanlagen", "IP-Videoüberwachung (DSGVO-konform)", "Brandmeldetechnik nach DIN 14675", "Zutrittskontrolle" }),

        new("sat", "Sat- und Antennenanlagen", "satellite",
            "Sat ZF, Unicable, SAT>IP — auch im Bestand.",
            "Klassische ZF mit Multischalter, Einkabel-Lösung im Bestand, oder SAT>IP fürs ganze Haus über das Netzwerk.",
            new[] { "Sat ZF mit Multischalter", "Unicable (Einkabel-Lösung)", "SAT>IP (TV über LAN/WLAN)", "Antennenmontage" },
            IsTopTier: false),

        new("beleuchtung", "Beleuchtung & LED", "lightbulb",
            "Lichtplanung Wohn- und Gewerbe, LED-Sanierung, Außenbeleuchtung.",
            "Gute Beleuchtung ist mehr als hell — wir planen Lichtszenen, sanieren auf LED und sparen damit nachhaltig Stromkosten.",
            new[] { "Lichtplanung mit DIALux", "LED-Sanierung im Bestand", "Außen- und Fassadenbeleuchtung", "Notbeleuchtung / Fluchtweg" },
            IsTopTier: false),

        new("netzwerk", "Netzwerk & EDV", "network",
            "Strukturierte Verkabelung mit mindestens CAT 7, WLAN, Patchschränke.",
            "Mindestens CAT 7-Verkabelung für Neubau und Bestand — zukunftssicher für 10-Gigabit. Patchfelder, WLAN-Ausleuchtung und Glasfaser-Vorbereitung aus einer Hand.",
            new[] { "Mindestens CAT 7 strukturierte Verkabelung (10 GbE-fähig)", "Patchfelder & 19″-Schränke", "WLAN-Ausleuchtung mit Heatmap", "Glasfaser-Vorbereitung" },
            IsTopTier: false),

        new("sprechanlagen", "Türsprechanlagen", "phone",
            "Video-Türsprechanlagen, Ritto Twinbus, Briefkastenanlagen.",
            "Wir verbauen klassische Audio- und moderne Video-Sprechanlagen — auch nachrüstbar im Bestand.",
            new[] { "Ritto Twinbus", "Video-Türsprechanlagen", "Briefkastenanlagen", "Smartphone-Integration" },
            IsTopTier: false),
    };

    public static IReadOnlyList<ServiceItem> TopServices { get; } = All.Where(s => s.IsTopTier).ToList();

    public static IReadOnlyList<ServiceItem> SecondaryServices { get; } = All.Where(s => !s.IsTopTier).ToList();

    public static ServiceItem? FindBySlug(string slug) => All.FirstOrDefault(s => s.Slug == slug);
}

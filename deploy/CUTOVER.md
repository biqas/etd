# DNS-Cutover: elektrotechnikdesch.de → ETD-Cluster

Diese Datei ist die **Schritt-für-Schritt-Anleitung für Christoph**, um die
Live-Domain `elektrotechnikdesch.de` (+ `www.elektrotechnikdesch.de`) auf
die neue Website umzuhängen. Es muss **nur der A-Record** geändert werden
— alles auf der Server-Seite ist schon vorbereitet.

## Wo Christoph einsteigt

DNS-Verwaltung bei seinem Hoster (vermutlich Strato — aktueller A-Record
`elektrotechnikdesch.de → 81.169.145.88` zeigt auf Strato-IP-Bereich).

## Was geändert wird — drei A-Records

| Record-Typ | Name                              | Bisher          | Neu                  |
|------------|-----------------------------------|-----------------|----------------------|
| A          | `elektrotechnikdesch.de` (`@`)    | 81.169.145.88   | **74.220.27.209**    |
| A          | `www.elektrotechnikdesch.de`      | 81.169.145.88   | **74.220.27.209**    |
| A          | `dl.elektrotechnikdesch.de`       | (Strato)        | **74.220.27.209**    |

Der `dl.`-Record sorgt dafür, dass alte Download-Links aus der
Email-Signatur weiter funktionieren — z.B.
`http://dl.elektrotechnikdesch.de/Datenschutzerklärung.pdf` wird auf
dem Cluster auf `/dl/Datenschutzerklärung.pdf` umgeleitet (intern via
Traefik AddPrefix-Middleware).

**TTL:** falls möglich vorher auf 300 s (5 min) reduzieren — sonst kann
die Umstellung bis zu 24 h dauern.

## Was NICHT geändert wird

- **MX-Records** (Mail) bleiben bei Strato — `mail@ElektroTechnikDesch.de`
  läuft unverändert über die alten Mailserver. Der Webserver versendet
  nur via SMTP, empfängt keine Mails.
- **TXT/SPF/DKIM** bleiben bei Strato.
- Nameserver-Eintrag bleibt bei Strato — wir tauschen nur zwei Records,
  nicht die ganze Zone.

## Was nach der Umstellung passiert (automatisch)

1. DNS-Propagation: 1–15 Min bei TTL 300 s, sonst bis zu Stunden.
2. cert-manager im Cluster sieht, dass die Domain jetzt auf uns zeigt,
   startet automatisch die Let's-Encrypt-HTTP-01-Challenge.
3. Innerhalb von 1–2 Min ist das HTTPS-Zertifikat aktiv.
4. `https://elektrotechnikdesch.de` zeigt die neue Seite.
5. `https://www.elektrotechnikdesch.de` macht **301-Redirect** auf die
   apex-Domain (so verlangt's Google für Canonical-SEO).

## Vorab-Test (vor DNS-Umstellung)

Funktioniert die neue Seite schon — ohne dass die Domain umgezogen ist?
Ja. Mit `curl --resolve` umgeht man DNS und hittet den Cluster direkt:

```bash
curl -k --resolve elektrotechnikdesch.de:443:74.220.27.209 \
  https://elektrotechnikdesch.de/
# → HTTP 200, Seite kommt korrekt zurück
```

Zertifikatsfehler (`TRAEFIK DEFAULT CERT`) ist erwartet — solange DNS
nicht zeigt, kann LetsEncrypt nicht validieren. Sobald DNS umgehängt ist,
löst sich das von selbst.

## Verifikation NACH der Umstellung

```bash
# 1. DNS-Propagation prüfen
dig +short elektrotechnikdesch.de
# → muss 74.220.27.209 ausgeben

# 2. HTTPS + Zertifikat prüfen
curl -I https://elektrotechnikdesch.de/
# → HTTP/2 200, cert ist von Let's Encrypt

# 3. www-Redirect prüfen
curl -I https://www.elektrotechnikdesch.de/
# → HTTP/2 301, Location: https://elektrotechnikdesch.de/
```

Cluster-seitig (nur für mich):

```bash
kubectl -n etd-dev get certificate
# elektrotechnikdesch-de-tls       True
# www-elektrotechnikdesch-de-tls   True
```

## Rollback

Falls etwas hakt:

1. A-Record zurück auf `81.169.145.88` → alte Strato-Seite läuft sofort
   wieder (außer die Strato-Hosting-Pakete sind schon gekündigt).
2. TTL 300 s bedeutet: Rollback ≤ 5 Min wirksam.

## Was schon im Cluster steht

```yaml
# deploy/kubernetes/ingress-prod.yaml
- Ingress: etd-web-prod        → elektrotechnikdesch.de       (kein BasicAuth)
- Ingress: etd-web-prod-www    → www.elektrotechnikdesch.de   (301 → apex)
- Middleware: etd-www-to-apex  → www → apex Redirect-Regel
- Certificate: zwei wartende cert-manager-Anforderungen
```

Cluster-IP für A-Record: **`74.220.27.209`** (Traefik LoadBalancer im
CIVO-k3s `bits-prod`).

## Erreichbar bleibt während dem Umzug

- `https://etd.it-blue.net` (Dev-/Preview-Umgebung mit BasicAuth) — läuft
  parallel weiter, unabhängig von der DNS-Umstellung. Kann nach erfolgter
  Migration entfernt werden, wenn nicht mehr gebraucht.

# ETD — Elektrotechnik Desch website

Static SSR Blazor site for Elektrotechnik Desch (Meisterbetrieb in Biebergemünd), deployed to a CIVO Kubernetes cluster in Frankfurt.

## Local development

```bash
dotnet run --project src/ETD.AppHost
```

Opens the Aspire dashboard. The web app runs at the URL Aspire prints; Mailpit (which catches all outgoing mail in dev) is available at the port shown in the dashboard.

## Tests

```bash
dotnet test tests/ETD.Web.Tests   # unit tests (TUnit)
dotnet test tests/ETD.E2E         # E2E with Playwright + Aspire test host
```

The E2E suite boots the Aspire AppHost in test mode (Mailpit container included) and drives the wizard flow with Playwright, then asserts both customer and owner emails arrived in Mailpit.

## Build & deploy

Pushing to `main` triggers `.github/workflows/deploy.yml`:

1. Build container, push to `ghcr.io/<repo>/etd-web:<sha>`
2. Generate K8s manifests via Aspirate (community CLI tool, pinned in `.config/dotnet-tools.json`)
3. Patch the image tag, then `kubectl apply -k` against the CIVO cluster (kubeconfig in `CIVO_KUBECONFIG` secret)

## Required GitHub secrets

| Secret | Description |
|---|---|
| `CIVO_KUBECONFIG` | base64-encoded kubeconfig for a service account with deploy rights in the `etd` namespace |

## Required cluster prerequisites

| Component | Notes |
|---|---|
| nginx-ingress | The Aspirate-generated ingress uses standard nginx annotations |
| cert-manager | The ingress references the `letsencrypt-prod` ClusterIssuer for TLS |
| Kubernetes secret `etd-smtp` in namespace `etd` | Keys: `host`, `port`, `user`, `pass`, `use-tls`. Used by the Web container to send outbound mail via `mail.elektrotechnikdesch.de`. |

## Production environment variables

These are injected into the Web container by the K8s Deployment (from the `etd-smtp` Secret):

| Variable | Description |
|---|---|
| `Smtp__Host` | e.g. `mail.elektrotechnikdesch.de` |
| `Smtp__Port` | typically 587 |
| `Smtp__User` | SMTP login |
| `Smtp__Pass` | SMTP password |
| `Smtp__UseTls` | `true` in prod |
| `Smtp__From` | `anfrage@elektrotechnikdesch.de` |
| `Smtp__To` | `mail@ElektroTechnikDesch.de` |

## Project structure

- `src/ETD.ServiceDefaults` — shared Aspire defaults (health endpoints, telemetry)
- `src/ETD.Web` — Blazor Static SSR app (no SignalR, no WebSocket)
- `src/ETD.AppHost` — Aspire 13.3 AppHost with Mailpit dev SMTP
- `tests/ETD.Web.Tests` — TUnit unit tests
- `tests/ETD.E2E` — Playwright E2E tests against the Aspire test host
- `deploy/kubernetes` — K8s manifests (Aspirate output + hand-written ingress)
- `docs/superpowers/specs` — design spec
- `docs/superpowers/plans` — implementation plan

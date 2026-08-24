# GitHub Actions Secrets & Variables (Triple A)

Configure under **Settings → Secrets and variables → Actions** in the `triple-a` GitHub repository.

---

## 1. GitHub Actions Secrets (Encrypted)

These secrets are used by the auto-deploy workflow (`.github/workflows/deploy-master.yml`) to inject production/staging environment variables into `web.config` via `infra/deploy/iis/set-api-iis-environment.ps1`.

| Secret Name | Description / Example |
|:---|:---|
| `DB_CONNECTION_STRING` | Complete SQL Server connection string:<br>`Server=sql.devson.co.za;Database=KPW_MoveWell;User Id=kpw_app;Password=...;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=False;Packet Size=4096;Connection Timeout=60;` |
| `JWT_KEY` | 256-bit symmetric signing key for JWT authentication (e.g. `rnm%80^!cwtAfU*TYe3wzEutDCEYdYrS`) |
| `SENDGRID_API_KEY` | SendGrid API key (starts with `SG.`) |
| `GEMINI_API_KEY` | *(Optional)* Google Gemini API key for AI audio transcription & assistant |

---

## 2. GitHub Actions Variables (Configuration)

Repository variables override workflow defaults when set.

| Variable Name | Default | Purpose |
|:---|:---|:---|
| `IIS_SITE_NAME` | `KPW` | Documentation / IIS site name |
| `API_APP_POOL` | `KPW` | API app pool name for deploy script |
| `PORTAL_APP_POOL` | `KPW` | Portal site app pool name |
| `APP_APP_POOL` | `KPW` | Owner app site app pool name |
| `LANDING_APP_POOL` | `KPW` | www site app pool for landing deploy |
| `API_BASE_URL` | `https://mytriplea.co.za` | Baked into portal build as `VITE_API_BASE_URL` |
| `API_HEALTH_URL` | `https://mytriplea.co.za/api/health` | Post-deploy health check |
| `PORTAL_PUBLIC_URL` | `https://mytriplea.co.za/portal` | Password-reset / invite links for physio |
| `OWNER_APP_PUBLIC_URL` | `https://mytriplea.co.za/app` | Password-reset / invite links for owners |
| `DEPLOY_BACKUP_RETENTION` | `1` | API backups under `C:\WebApps\TripleA\_backups` |
| `DEPLOY_ASPNETCORE_ENVIRONMENT` | `Staging` | Set in published `web.config` |
| `SENDGRID_PROVIDER` | `SendGrid` | Email delivery provider (`SendGrid` or `Logging`) |
| `SENDGRID_FROM_EMAIL` | `noreply@mytriplea.co.za` | Verified sender email address |
| `SENDGRID_FROM_NAME` | `Triple A` | Sender display name |
| `FLUTTER_ROOT` | `C:\flutter` | Flutter SDK folder on the runner |

---

## 3. Local Development (`.env`)

For local development, copy `.env.example` to `.env` in the root repository folder. The `.NET` API will automatically load this `.env` file upon startup.

> [!CAUTION]
> Never commit `.env` or files containing plain-text passwords / API keys to git. `.env` is listed in `.gitignore`.

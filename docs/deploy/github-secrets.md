# GitHub Actions variables (Triple A)

Configure under **Settings → Secrets and variables → Actions** for the `triple-a` repository.

**This project does not use GitHub Actions secrets.** The API deploy is `dotnet publish` → robocopy. Configuration and the GCP credentials JSON are included in publish output from [`backend-api-dot-net/KPW.Api/`](../../backend-api-dot-net/KPW.Api/) (same as local publish).

## API configuration (publish output)

| Item | Source |
|------|--------|
| `appsettings.json`, `appsettings.Staging.json` | Published with the API (`/p:EnvironmentName=Staging`) |
| `web.config` | Published with the API |
| `devson-development-6d4da133b74e.json` | Published when present next to the project on the build machine (see `KPW.Api.csproj` `CopyToPublishDirectory`) |

No workflow step patches connection strings, JWT, or credentials. Ensure the GCP JSON exists on the runner under `backend-api-dot-net/KPW.Api/` before API jobs run (gitignored; same as local dev).

## Variables (optional)

Repository variables override workflow defaults when set.

| Name | Default | Purpose |
|------|---------|---------|
| `IIS_SITE_NAME` | `KPW` | Documentation / future IIS automation |
| `API_APP_POOL` | `KPW` | API app pool name for deploy script |
| `PORTAL_APP_POOL` | `KPW` | Portal site app pool name |
| `APP_APP_POOL` | `KPW` | Owner app site app pool name |
| `LANDING_APP_POOL` | `KPW` | www site app pool for landing deploy |
| `API_BASE_URL` | `https://www.mytriplea.co.za` | Baked into portal build as `VITE_API_BASE_URL` |
| `API_HEALTH_URL` | `https://www.mytriplea.co.za/api/health` | Post-deploy health check |
| `PORTAL_PUBLIC_URL` | `https://app.mytriple.co.za` | Documented public portal URL |
| `OWNER_APP_PUBLIC_URL` | `https://owner.mytriplea.co.za` | Documented owner app URL |
| `DEPLOY_BACKUP_RETENTION` | `1` | API backups under `C:\WebApps\TripleA\_backups` |
| `DEPLOY_ASPNETCORE_ENVIRONMENT` | `Staging` | Documented; set in published `web.config` |

## Portal and app builds

Portal: `API_BASE_URL` → `VITE_API_BASE_URL`, `vite build --base /`.

Owner app: `--dart-define=ENV=staging`, `--base-href /`.

No GitHub secrets are required for any deploy job.

# Deploy main to IIS (Triple A)

Self-hosted GitHub Actions deploy for **mytriplea.co.za**. The runner is installed on the same Windows IIS host as LandDiary. GitHub queues jobs; the runner polls outbound and runs checkout, build, and local filesystem copy.

## Architecture

```
Push to main / manual dispatch
        ↓
GitHub queues jobs
        ↓
Self-hosted runner on IIS host
        ↓
C:\WebApps\TripleA\ (www root) | api | portal | app
        ↓
IIS sites (subdomain bindings)
```

| Public URL | Physical folder | Content |
|------------|-----------------|---------|
| `https://mytriplea.co.za/` | `C:\WebApps\TripleA\` (site root) | Gateway (`site-landing/`) |
| `https://mytriplea.co.za/api/` | `C:\WebApps\TripleA\api` | Published `KPW.Api` |
| `https://app.mytriplea.co.za/` | `C:\WebApps\TripleA\portal` | Vue physio portal (`dist/`) |
| `https://owner.mytriplea.co.za/` | `C:\WebApps\TripleA\app` | Flutter owner web (`build/web/`) |

Portal and owner app are served on **their own subdomains** (not `/portal/` or `/app/` paths). Production builds use base path `/`.

The **www** site physical path is `C:\WebApps\TripleA\`. Landing deploy mirrors only root files from `site-landing/` and excludes `api`, `portal`, `app`, and `_backups` so sibling folders are not deleted.

## Workflow file

[`.github/workflows/deploy-master.yml`](../../.github/workflows/deploy-master.yml)

### Triggers

| Trigger | Behaviour |
|---------|-----------|
| Push to `main` | Deploy changed targets only |
| Actions → Deploy Master to IIS → Run workflow | Manual; optional force-all or pick `api`, `portal`, `app`, `landing` |
| Commit message contains `[skip ci]` | Skips automatic runs (manual still works) |

### Jobs

1. **detect-changes** — path-based diff decides which targets to deploy
2. **deploy-landing** — static gateway (`site-landing/`) to `C:\WebApps\TripleA\`
3. **deploy-portal** — Vue physio portal (`physio-portal-vue3/`)
4. **deploy-app** — Flutter owner web app (`owner-app-flutter/`)
5. **deploy-api** — .NET API (`backend-api-dot-net/KPW.Api/`)

All jobs use `runs-on: self-hosted` with no extra labels. Only the IIS machine should be registered for this repo, or add runner labels and scope jobs accordingly.

### Path rules

| Changed path | Deploys |
|--------------|---------|
| `site-landing/**` | landing |
| `physio-portal-vue3/**` | portal |
| `backend-api-dot-net/**` | API |
| `owner-app-flutter/**` | app |
| `infra/deploy/iis/**` or `.github/workflows/deploy-master.yml` | all four |
| docs, root files | none |

## Runner setup

Install the [GitHub Actions runner](https://docs.github.com/en/actions/hosting-your-own-runners/managing-self-hosted-runners) on the IIS host and register it against this repository (or your org).

Recommended service account: dedicated deploy user with rights to:

- Stop/start/recycle IIS app pools for Triple A sites
- Write under `C:\WebApps\TripleA\`
- Run `robocopy`

### Required software on the runner host

| Tool | Purpose |
|------|---------|
| Git | checkout |
| Node.js 24 | portal build |
| .NET SDK 9.x | API publish |
| Flutter SDK | owner web build |
| IIS + URL Rewrite | hosting |
| [.NET 9 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/9.0) | ASP.NET Core Module |

The runner polls GitHub outbound. No inbound ports from GitHub are required.

If no runner is online, jobs remain **Queued** indefinitely.

### NTFS permissions (fix robocopy ERROR 5)

The runner service account must have **Modify** on `C:\WebApps\TripleA\` and subfolders (`api`, `portal`, `app`, `_backups`). LandDiary’s runner already has access to `C:\WebApps\LandDiary\`; Triple A needs the same for its folder.

On the IIS server, **run PowerShell as Administrator**:

```powershell
# 1. Find which account runs the triple-a runner
Get-CimInstance Win32_Service |
  Where-Object { $_.Name -like 'actions.runner.*' } |
  Select-Object Name, StartName, State

# 2. Grant Modify (replace StartName from step 1, e.g. NT SERVICE\actions.runner... or DOMAIN\svc-deploy)
$runner = 'NT SERVICE\actions.runner.triple-a-iis'   # <-- your runner's StartName
icacls C:\WebApps\TripleA /grant "${runner}:(OI)(CI)M" /T

# 3. Quick write test (optional — run as the runner user or re-run deploy)
New-Item -ItemType File -Path C:\WebApps\TripleA\.write-test -Force
Remove-Item C:\WebApps\TripleA\.write-test -Force
```

If `C:\WebApps\TripleA` does not exist yet:

```powershell
New-Item -ItemType Directory -Path C:\WebApps\TripleA\api, C:\WebApps\TripleA\portal, C:\WebApps\TripleA\app, C:\WebApps\TripleA\_backups -Force
```

The runner also needs permission to **stop/start the KPW app pool** (same as LandDiary’s deploy account — often membership in `IIS_IUSRS` is not enough; use a dedicated deploy user in the local Administrators group or delegate app pool rights).

## IIS layout

Create IIS sites/bindings before the first deploy:

1. **mytriplea.co.za** (and **www** redirect) — site physical path `C:\WebApps\TripleA\`; child application `api` → `C:\WebApps\TripleA\api`
2. **app.mytriplea.co.za** — site physical path `C:\WebApps\TripleA\portal`
3. **owner.mytriplea.co.za** — site physical path `C:\WebApps\TripleA\app`

API app pool: **No Managed Code**. Portal and owner sites can use a static-friendly pool or share the API pool via repo variables.

Gateway files (`site-landing/`) deploy via **deploy-landing** to `C:\WebApps\TripleA\` (no build step; static copy with excluded sibling folders).

## Deploy scripts

| Script | Role |
|--------|------|
| [`infra/deploy/iis/deploy-iis.ps1`](../../infra/deploy/iis/deploy-iis.ps1) | Stop pool, optional `app_offline.htm`, `robocopy /MIR` (optional `/XD` excludes), start pool |
| [`infra/deploy/iis/rollback-iis.ps1`](../../infra/deploy/iis/rollback-iis.ps1) | Restore latest API backup from `C:\WebApps\TripleA\_backups` |

API deploy keeps one backup by default (`DEPLOY_BACKUP_RETENTION=1`). Portal, app, and landing skip backup (rebuilt from git). Landing deploy does not use `app_offline.htm` (would take `/api` offline on www).

### Manual API rollback

On the IIS host:

```powershell
cd C:\actions-runner\_work\<repo>\<repo>   # or your runner work folder
.\infra\deploy\iis\rollback-iis.ps1
```

## Secrets and variables

See [`github-secrets.md`](github-secrets.md). API config and GCP credentials come from `dotnet publish` output (appsettings + project folder JSON on the runner).

## Manual publish (fallback)

From repo root on any build machine:

```powershell
.\scripts\publish-iis.ps1 -Clean
```

Then copy `publish\iis\` contents to `C:\WebApps\TripleA\` (gateway at root; `api`, `portal`, `app` subfolders).

## Post-deploy checklist

- [ ] `https://mytriplea.co.za/` loads gateway
- [ ] `https://mytriplea.co.za/api/health` returns 200
- [ ] `https://app.mytriplea.co.za/` loads physio login
- [ ] `https://owner.mytriplea.co.za/` loads owner login
- [ ] `POST https://www.mytriplea.co.za/api/auth/login` returns tokens

Full staging notes: [`backend-api-dot-net/docs/IIS_STAGING.md`](../backend-api-dot-net/docs/IIS_STAGING.md).

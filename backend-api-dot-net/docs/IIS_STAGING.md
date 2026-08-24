# IIS Staging — mytriplea.co.za

Single IIS site with path-based applications (LandDiary-style):

| Public URL | Physical folder | Content |
|------------|-----------------|---------|
| `https://mytriplea.co.za/` | `C:\WebApps\TripleA\` (site root) | Static gateway (`site-landing/`) — deployed by CI |
| `https://mytriplea.co.za/api` | `C:\WebApps\TripleA\api` | Published `KPW.Api` |
| `https://mytriplea.co.za/portal` | `C:\WebApps\TripleA\portal` | Vue physio portal (`dist/`) |
| `https://mytriplea.co.za/app` | `C:\WebApps\TripleA\app` | Flutter owner web (`build/web/`) |

API routes are `/api/pets`, `/api/auth/login`, etc. Controllers have no `api/` prefix; IIS application `/api` supplies it.

Portal is built with Vite `base: '/portal/'`. Owner web uses `--base-href /app/`.

**Automated deploy:** push to `main` or run **Deploy Master to IIS** in GitHub Actions. See [`docs/deploy/iis-master.md`](../../docs/deploy/iis-master.md).

---

## Prerequisites (VPS)

1. **Windows Server** with IIS enabled
2. **[.NET 9 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/9.0)** (includes ASP.NET Core Module)
3. **IIS URL Rewrite** module
4. **SQL Server** (local on VPS or reachable instance)
5. **HTTPS** certificate for `mytriplea.co.za`
6. DNS **A records** for `www`, `owner`, and `app` → VPS public IP

---

## 1. SQL database

Create database and login (if not already done) using [`docs/sql/create_kpw_app_login.sql`](sql/create_kpw_app_login.sql) on **sql.devson.co.za**, then set the connection string in your local `.env` (or in GitHub Secrets / IIS environment variables for deployments).

If `dotnet ef database update` fails with **Login failed for user 'kpw_app'**, the password in `.env` does not match the SQL login. Fix one side:

- **Reset SQL login** (run as sysadmin on `sql.devson.co.za`):
  ```sql
  ALTER LOGIN [kpw_app] WITH PASSWORD = N'<password>';
  ```
- **Or** update `.env` to match whatever password was set when `create_kpw_app_login.sql` was run.

Test credentials in SSMS or Azure Data Studio before running EF again. You can also pass a connection string explicitly:
```powershell
dotnet ef database update --project KPW.Infrastructure --startup-project KPW.Api `
  --connection "Server=sql.devson.co.za;Database=KPW_MoveWell;User Id=kpw_app;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

Connection string (shared Dev / QA):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=sql.devson.co.za;Database=KPW_MoveWell;User Id=kpw_app;Password=...;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

Apply migrations from your dev machine or the VPS:

```powershell
cd backend-api-dot-net
$env:ASPNETCORE_ENVIRONMENT = "Staging"
dotnet ef database update --project KPW.Infrastructure --startup-project KPW.Api
```

**Fresh database:** run the command above on an empty `KPW_MoveWell` database to apply all migrations including seed data.

**Existing staging database:** run `dotnet ef database update` to apply the latest migration (`ExpandStagingSeedData`).

**Database has tables but migration history is empty** (error: *"There is already an object named 'Clinics'"*):

1. Verify schema includes `MessageThreads` and `ExerciseSessionLogs` (see [`docs/sql/baseline_ef_migrations.sql`](sql/baseline_ef_migrations.sql)).
2. Run that baseline script on `KPW_MoveWell` to record migrations already applied.
3. Run `dotnet ef database update` again — only pending migrations (e.g. `ExpandStagingSeedData`) will run.

**Nuclear option:** drop and recreate `KPW_MoveWell`, then run `dotnet ef database update` on an empty database.

If you have conflicting manual data after baselining, drop and recreate the database, then re-apply all migrations.

Seed data is defined in [`KPW.Infrastructure/Data/DatabaseSeeder.cs`](../KPW.Infrastructure/Data/DatabaseSeeder.cs).

---

## Quick publish (all apps + gateway)

From the repo root:

```powershell
.\scripts\publish-iis.ps1 -Clean
```

Output:

```
publish/iis/
  index.html, styles.css, favicon.svg, web.config  -> C:\WebApps\TripleA\ (www site root)
  api/      -> IIS application /api on www
  portal/   -> IIS application /portal
  app/      -> IIS application /app
```

Copy each folder to `C:\WebApps\TripleA\` and bind to the matching IIS site. Optional: `-ApiBaseUrl https://mytriplea.co.za` (default).

For production-like staging on the IIS host, prefer the GitHub Actions workflow (`.github/workflows/deploy-master.yml`) which copies directly to `C:\WebApps\TripleA\{api,portal,app}`.

Source for the gateway page: [`site-landing/`](../../site-landing/) (no build step). Gateway links point to `/app/` and `/portal/`.

---

## 2. Publish API (manual)

On build machine:

```powershell
cd backend-api-dot-net
dotnet publish KPW.Api/KPW.Api.csproj -c Release -o .\publish\api
```

Copy `publish\api\*` to `C:\WebApps\TripleA\api`.

### GCP credentials (video + AI)

The same service account JSON used locally (`KPW.Api/devson-development-6d4da133b74e.json`) is:

- **Copied into `publish/iis/api/`** when you run `.\scripts\publish-iis.ps1` (if the file exists on your build machine)
- **Auto-detected at startup** — `Program.cs` sets `GOOGLE_APPLICATION_CREDENTIALS` to the full path when the file sits next to `KPW.Api.dll`
- **Set in `web.config`** for IIS (`ASPNETCORE_ENVIRONMENT=Staging` and relative credentials filename)

After deploy, confirm the file exists on the server:

```
C:\WebApps\TripleA\api\devson-development-6d4da133b74e.json
```

**Do not commit** the JSON key to git. If missing from publish output, copy it manually to the API folder on the server.

Optional IIS app pool override (Advanced Settings → Environment Variables):

- `ASPNETCORE_ENVIRONMENT` = `Staging`
- `GOOGLE_APPLICATION_CREDENTIALS` = `C:\WebApps\TripleA\api\devson-development-6d4da133b74e.json`

### Staging configuration

`appsettings.json`, `appsettings.Staging.json`, `web.config`, and the GCP credentials JSON are published with the API (`dotnet publish /p:EnvironmentName=Staging`). No GitHub Actions secrets or extra copy steps in CI.

**Do not** set `Hosting:PathBase` on IIS — the `/api` application path is applied automatically by the ASP.NET Core Module.

### IIS: API application

1. Site **TripleA** — physical path `C:\WebApps\TripleA\` (gateway at root; CI **deploy-landing**)
2. Gateway files deploy automatically from `site-landing/` or via `.\scripts\publish-iis.ps1`
3. Add **Application** alias `api`, physical path `C:\WebApps\TripleA\api`
4. App pool: **No Managed Code**, .NET CLR version empty
5. Ensure `web.config` from publish exists (created by Hosting Bundle)

---

## 3. Build physio portal

```powershell
cd physio-portal-vue3
npm ci --include=dev
$env:NODE_ENV = "development"
$env:VITE_API_BASE_URL="https://mytriplea.co.za"
npm run build
```

Copy `dist\*` to `C:\WebApps\TripleA\portal`.

`public/web.config` is included in the build for SPA routing under `/portal/`.

### IIS: portal application

- Alias: `portal`
- Physical path: `C:\WebApps\TripleA\portal`

---

## 4. Build owner app (Flutter web)

```powershell
cd owner-app-flutter
flutter pub get
flutter build web --release --base-href /app/ --dart-define=ENV=staging
```

Copy `build\web\*` to `C:\WebApps\TripleA\app` (includes `web.config` from `web/`).

### IIS: owner app application

- Alias: `app`
- Physical path: `C:\WebApps\TripleA\app`

---

## 5. Local development (unchanged URLs)

- API: `https://localhost:7112/api/...` via `Hosting:PathBase` in [`appsettings.Development.json`](../KPW.Api/appsettings.Development.json)
- Vue: `npm run dev` → `http://localhost:5287/portal/` (Vite base `/portal/`)
- Flutter: `flutter run -d chrome` with default dev API `https://localhost:7112`

| Max | Canine (German Shepherd) | Lameness | Lameness Rehab |
| Bella | Canine (Beagle) | Weight / mobility | Weight & Mobility Plan |
| Whiskers | Feline (DSH) | Arthritis | Arthritis Care |
| Milo | Feline (Maine Coon) | Post-operative | (no active programme) |

### What each screen shows

| Screen | Seed highlights |
|--------|-----------------|
| Physio patients list | 6 pets |
| Physio dashboard | 3 appointments on 27 Jul 2026; 3+ pending video reviews |
| Physio appointments | Scheduled, completed, and cancelled across pets |
| Physio messages | Threads for Buddy, Luna, Max, Bella, Whiskers |
| Physio video approvals | Pending, processing, ready, reviewed, and failed submissions |
| Owner pets | 6 pets with varied conditions |
| Owner tracking | 14-day improving trends for Buddy, Luna, Max |
| Owner reminders | Upcoming appointments + incomplete exercise sessions on anchor day |
| Owner messages | Multi-turn conversations with physio |
| Owner video inbox | Reviewed feedback and pending uploads |
| Exercise library | 8 exercises (canine + feline, multiple conditions) |

---

## 7. Smoke checklist

After deploy:

- [ ] `https://mytriplea.co.za/` loads gateway with Owner App and Physio Portal buttons
- [ ] Gateway buttons navigate to `/app/` and `/portal/`
- [ ] `https://mytriplea.co.za/portal/` loads physio login
- [ ] `https://mytriplea.co.za/app/` loads owner login
- [ ] Physio login → dashboard, patients, appointments, messages, videos
- [ ] Owner login → pets, exercises, tracking, upload video, messages, reminders
- [ ] `https://mytriplea.co.za/api/health` returns 200
- [ ] API auth: `POST https://mytriplea.co.za/api/auth/login` returns tokens
- [ ] No double `/api/api` in browser network tab
- [ ] Video upload / AI chat (requires valid GCP credentials on API pool)

---

## Troubleshooting

| Issue | Check |
|-------|--------|
| 404 on `/` | Gateway files at www site physical path; `index.html` in `web.config` defaultDocument |
| 404 on `/api/pets` | API app alias is `api` on www site; app pool running; `web.config` present |
| 404 on portal routes | URL Rewrite installed; `web.config` in portal folder rewrites to `/portal/index.html` |
| 404 on owner routes | Same for `/app/` and `/app/index.html` |
| CORS errors | `ASPNETCORE_ENVIRONMENT=Staging`; origin `https://mytriplea.co.za` |
| 500 on API | Event Viewer / stdout logs; connection string; migrations applied |
| EF: object already exists (Clinics) | DB has schema but no migration history — run [`baseline_ef_migrations.sql`](sql/baseline_ef_migrations.sql) then `dotnet ef database update` |
| EF: Login failed for user kpw_app | Password in appsettings ≠ SQL login — sync via `ALTER LOGIN` or update appsettings (see SQL section above) |
| 500.19 duplicate mimeMap | Remove `<staticContent>` mimeMap blocks from site/portal/app `web.config` — IIS already registers `.json`, `.woff`, `.svg` |
